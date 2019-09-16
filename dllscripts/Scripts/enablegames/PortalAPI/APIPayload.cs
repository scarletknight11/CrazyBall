using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using FullSerializer;

public partial class APIService : ScriptableObject {
	// json payload sent or received from an API call
	public interface IPayload {
		// a string dictionary of the payload data
		Dictionary<string, object> Data { get; }
		// a byte array serialization of the payload data
		byte[] Serialize();
	}

	// encpasulates a json payload for requests and reponses
	public class APIPayload : IPayload {
		// key value pairs representing the JSON payload
		private Dictionary<string, object> data;

		// constructors
		private APIPayload() {
			this.data = new Dictionary<string, object>();
		}
		public APIPayload(Dictionary<string, object> payloadData)
			: this() {
			if (null != payloadData) {
				// deep clone of the input data
				foreach (KeyValuePair<string, object> payload in payloadData) {
					this.data.Add(payload.Key, payload.Value);
				}
			}
		}
		public static object getValueFromFsData(fsData dat){
			if(dat.IsBool)				{return dat.AsBool;}
			else if(dat.IsDictionary)	{return dat.AsDictionary;}
			else if(dat.IsDouble)		{return dat.AsDouble;}
			else if(dat.IsInt64)		{return dat.AsInt64;}
			else if(dat.IsList)			{return dat.AsList;}
			else if(dat.IsString)		{return dat.AsString;}
			else						{return null; }
		}
		public APIPayload(string json)
			: this() {
            //Hashtable response = null;
            Dictionary<string, fsData> response=null;
			if (!string.IsNullOrEmpty(json)) {
				try {
                    //response = (Hashtable)MiniJSON.jsonDecode(json);
                    response = fsJsonParser.Parse(json).AsDictionary;
					// MiniJSON returns null if the json couldn't be decoded
					//TODO: write an equivalent version of this for FullSerializer
					if (null == response) {
						// find and throw decoding error
						/*
						int errorLocation = MiniJSON.getLastErrorIndex();
						string errorSnippet = MiniJSON.getLastErrorSnippet();
						throw new ArgumentException("Error when deserializing JSON - loc #" + errorLocation + ": \"" + errorSnippet + "\"");
                        /*/
						throw new ArgumentException("Error when deserializing JSON - something went wrong with FullSerializer, response is null");
						//*/
					} else {
						// deep clone of input data
						foreach (KeyValuePair<string, fsData> kvp in response) {
							this.data.Add(kvp.Key, getValueFromFsData(kvp.Value));
							/*
							object val=getValueFromFsData(kvp.Value);
							Debug.Log("adding key \"" + kvp.Key + "\" with value \"" + val.ToString() + "\" that has the type \"" + val.GetType()+'"');//*/
						}
					}
				} catch (Exception) {
					Debug.LogWarning("Cannot decode json: " + json);
					//throw ex;
				}
			}
		}

		#region IPayload

		// provides a read-only list of payload data
		public Dictionary<string, object> Data {
			get {
				Dictionary<string, object> ret = new Dictionary<string, object>();
				// deep clone of the headers so the user cannot alter the original
				foreach (KeyValuePair<string, object> payloads in data) {
					ret.Add(payloads.Key, payloads.Value);
				}
				return ret;
			}
		}

		// convert payload to a byte array suitable for transmitting via HTTP
		public byte[] Serialize() {
			byte[] ret = null;
			if (data.Count > 0) {
				try {
					//TODO: replace this with FullSerializer equivalent
					string json = MiniJSON.jsonEncode(data);
					//string json=JSONSerializer.Serialize(typeof(Dictionary<string, object>),data);//!IMPORTANT! this line does NOT work because this does not prittify the result
					// MiniJSON library returns null on encode error
					if (string.IsNullOrEmpty(json)) {
						throw new Exception("JSON encoding error");
					}
					//string json = SerializeHelper(data);
					ret = Encoding.UTF8.GetBytes(json);
				} catch (Exception e) {
					Debug.LogError(e.Message);
				}
			}
			return ret;
		}

		#endregion
	}
}
