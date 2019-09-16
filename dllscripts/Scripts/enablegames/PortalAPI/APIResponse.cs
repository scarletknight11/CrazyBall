using System.Collections.Generic;
using UnityEngine;

public partial class APIService : ScriptableObject {
	// a request made to the API server
	public interface IResponse {
		StatusCode Status { get; }
		IHeaders Headers { get; }
		IPayload Payload { get; }
	}

	// HTTP status codes returned by the API
	public enum StatusCode {
		Invalid = 0,
		OK = 200,
		Created = 201,
		NoContent = 204,
		BadSyntax = 400,
		Unauthorized = 401,
		NotFound = 404,
		BadSemantics = 422,
		ServerError = 500
	};

	// encapsulates the response from the API server
	private class APIResponse : IResponse {
		// success, message, data, and raw json returned from the server
		private readonly StatusCode status;
		private readonly APIHeaders headers;
		private readonly APIPayload payload;

		// constructor - only APIService can create
		public APIResponse(Dictionary<string, string> headers, string json) {
			this.headers = new APIHeaders(headers);
			this.payload = new APIPayload(json);
			this.status = this.headers.GetStatusCode();
		}

		#region IResponse

		public StatusCode Status {
			get {
				return this.status;
			}
		}
		public IHeaders Headers {
			get {
				return this.headers;
			}
		}
		public IPayload Payload {
			get {
				return this.payload;
			}
		}

		#endregion
	}
}
