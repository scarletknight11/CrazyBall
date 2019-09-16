using FullSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public partial class APIService : ScriptableObject
{
    // the server that hosts the API
    public string Server { get; private set; }
    // the authentication token used as a header on all API calls
    private string authenticationToken;

    public bool IsAuthenticated
    {
        get
        {
            // TODO: check to make sure token isn't expired
            return !string.IsNullOrEmpty(authenticationToken);
        }
    }

    // encapsulates an error in an API request
    public class APIError
    {
        public readonly string Message;

        public APIError(string message)
        {
            this.Message = message;
        }
    }

    // constructor
    public void Init(string server)
    {
        this.Server = server;
        // check to see if a token was passed via command line
        //#if !UNITY_ANDROID || UNITY_EDITOR
        if (Application.platform != RuntimePlatform.Android)
        {
            String[] arguments = Environment.GetCommandLineArgs();
            foreach (string arg in arguments)
            {
                if (arg.ToLower().StartsWith("/t:"))
                {
                    // clip the flag off the string and save as token
                    string token = arg.Substring(3);
                    // not a true validation, but see if token is well-formed enough to give us a username
                    string tokenUser = GetUserIDFromJWT(token);
                    if (!string.IsNullOrEmpty(tokenUser))
                    {
                        authenticationToken = token;
                        //Debug.Log("Token read: " + authenticationToken);
                        //Debug.Log("User ID: " + tokenUser);
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(authenticationToken))
            {
                //Debug.Log("No token read from command line");
            }
        }
//#endif

    }

    // extracts the user ID from a JWT
    public string GetUserIDFromJWT(string token)
    {
        string ret = string.Empty;
        string[] parts = token.Split('.');
        if (parts.Length == 3)
        {
            // extract the json payload
            var payload = parts[1];
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            // convert to json object and retrieve user id
            Dictionary<string, fsData> tokenData = fsJsonParser.Parse(payloadJson).AsDictionary;
            if (tokenData.ContainsKey("user_id"))
            {
                ret = tokenData["user_id"].AsString;
            }
        }
        return ret;
    }

    // From JWS spec RFC7515
    public static byte[] Base64UrlDecode(string arg)
    {
        string s = arg;
        s = s.Replace('-', '+'); // 62nd char of encoding
        s = s.Replace('_', '/'); // 63rd char of encoding
        switch (s.Length % 4) // Pad with trailing '='s
        {
            case 0: break; // No pad chars in this case
            case 2: s += "=="; break; // Two pad chars
            case 3: s += "="; break; // One pad char
            default:
                throw new System.Exception("Illegal base64url string!");
        }
        return Convert.FromBase64String(s); // Standard base64 decoder
    }

    // create headers for a request, injecting authentication token
    private Dictionary<string, string> GenerateHeaders(RequestMethod httpMethod)
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();
        // add headers, including authentication token if it exists
        ret.Add("Content-type", "application/json");
        if (!string.IsNullOrEmpty(authenticationToken))
        {
            ret.Add("Authorization", "Bearer " + authenticationToken);
        }
        // Unity WWW interprets HTTP Methods as follows:
        // GET - new WWW without postData defined
        // POST - new WWW with postData defined
        // PUT - use POST with X-HTTP-Method-Override:PUT header
        // DELETE - use GET with X-HTTP-Method-Override:DELETE header
        else if (httpMethod == RequestMethod.PUT)
        {
            ret.Add("X-HTTP-Method-Override", "PUT");
        }
        else if (httpMethod == RequestMethod.DELETE)
        {
            ret.Add("X-HTTP-Method-Override", "DELETE");
        }

        return ret;
    }

    // create a request for the API service with a dictionary of push data
    public IRequest CreateRequest(RequestMethod httpMethod, string function, Dictionary<string, object> data)
    {
        Dictionary<string, string> headers = GenerateHeaders(httpMethod);

        // if GET, be sure we are submitting no data, or else it will be reinterpreted as POST
        if (httpMethod == RequestMethod.GET)
        {
            data = null;
        }
        else
        {
            // if we do not send in post data, Unity WWW will reinterpret as GET
            // be sure that we're submitting postdata, even if it's this dummy value
            if (null == data || data.Count <= 0)
            {
                data = new Dictionary<string, object>();
                data.Add("forcePost", "1");
            }
        }

        return new APIRequest(httpMethod, this.Server, function, new APIHeaders(headers), new APIPayload(data));
    }

    // create a request for the API service with a serialized JSON string for push data
    public IRequest CreateRequestJSON(RequestMethod httpMethod, string function, string data)
    {
        // if GET request, no JSON data, or empty JSON object, just use the standard method
        if (httpMethod == RequestMethod.GET || string.IsNullOrEmpty(data) || data.Replace(" ", "").Equals("{}"))
        {
            return CreateRequest(httpMethod, function, null);
        }
        Dictionary<string, string> headers = GenerateHeaders(httpMethod);

        return new APIRequestJSON(httpMethod, this.Server, function, new APIHeaders(headers), data);
    }

    // create a response from the API service, intercepting any authentication tokens
    private IResponse CreateResponse(Dictionary<string, string> headers, string json)
    {
        APIResponse ret = new APIResponse(headers, json);

        // check to see if a new authentication token has been returned
        Dictionary<string, object> payload = ret.Payload.Data;
        if (payload.ContainsKey("token"))
        {
            string token = payload["token"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                authenticationToken = token;
            }
        }

        return ret;
    }

    // perform a request to the API server and return the response in the Callback
    public IEnumerator SendRequest(IRequest request, Action<APIError, IResponse> callback)
    {
        // only send if we can confirm network availability
        if (NetworkReachability.NotReachable == Application.internetReachability)
        {
            if (null != callback)
            {
                callback(new APIError("Network not available"), null);
            }
            yield break;
        }

        // send request to server
        using (WWW connection = new WWW(request.URI, request.PushData, request.Headers.Data))
        {
            yield return connection;
            // function will resume here when the response is received
            if (null != connection.error)
            {
                if (null != callback)
                {
                    callback(new APIError(connection.error.ToString()), null);
                }
                yield break;
            }

            // create a response object from the connection info and forward to callback
            IResponse response = CreateResponse(connection.responseHeaders, connection.text);

            if (null != callback)
            {
                callback(null, response);
            }
        }
    }
}
