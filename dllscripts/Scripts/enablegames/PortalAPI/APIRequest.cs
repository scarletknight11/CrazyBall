using System;
using System.Text;
using UnityEngine;

public partial class APIService : ScriptableObject {
	// a request made to the API server
	public interface IRequest {
		RequestMethod Method { get; }
		string URI { get; }
		IHeaders Headers { get; }
		byte[] PushData { get; }
	}

	// HTTP verbs for contacting the API server
	public enum RequestMethod {
		GET,
		POST,
		PUT,
		DELETE
	};

	// encapsulates a request to the API server, only usable by the APIService
	private class APIRequest : IRequest {
		private readonly RequestMethod httpMethod;
		private readonly string server;
		private readonly string function;
		private readonly APIHeaders headers;
		private readonly APIPayload payload;

		// private constructor, for use by descendant classes
		protected APIRequest(RequestMethod httpMethod, string server, string function, APIHeaders headers) {
			this.httpMethod = httpMethod;
			this.server = server;
			this.function = function;
			this.headers = headers;
		}

		// constructor - only APIService can create
		public APIRequest(RequestMethod httpMethod, string server, string function, APIHeaders headers, APIPayload payload)
			: this(httpMethod, server, function, headers) {
			this.payload = payload;
		}

		#region IRequest

		// get the HTTP method for the request
		public RequestMethod Method {
			get {
				return httpMethod;
			}
		}

		// get the URI for the request
		public string URI {
			get {
				return Uri.EscapeUriString(server + function);
			}
		}

		// get a read-only version of the headers
		public IHeaders Headers {
			get {
				return this.headers;
			}
		}

		// get a read-only version of the post data
		virtual public byte[] PushData {
			get {
				return this.payload.Serialize();
			}
		}

		#endregion
	}

	// a request with a raw JSON string for push data, instead of an APIPayload
	private class APIRequestJSON : APIRequest {
		private readonly string rawJSON;

		public APIRequestJSON(RequestMethod httpMethod, string server, string function, APIHeaders headers, string jsonPayload)
			: base(httpMethod, server, function, headers) {
			this.rawJSON = jsonPayload;
		}

		// get a read-only version of the post data
		public override byte[] PushData {
			get {
				return Encoding.UTF8.GetBytes(this.rawJSON);
			}
		}
	}
}
