using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public partial class APIService : ScriptableObject
{
    // headers sent or received from an API call
    public interface IHeaders
    {
        // a string dictionary of the headers
        Dictionary<string, string> Data { get; }
    }

    // provides a wrapper for headers
    private class APIHeaders : IHeaders
    {
        // key value pairs storing the header info
        private Dictionary<string, string> headers;

        // constructors
        private APIHeaders()
        {
            this.headers = new Dictionary<string, string>();
        }
        public APIHeaders(Dictionary<string, string> headers)
            : this()
        {
            // deep clone of the input headers
            foreach (KeyValuePair<string, string> header in headers)
            {
                this.headers.Add(header.Key, header.Value);
            }
        }

        // get status code from header
        public StatusCode GetStatusCode()
        {
            StatusCode ret = StatusCode.Invalid;
            // search for the status header
            string statusText = string.Empty;
            if (headers.ContainsKey("STATUS"))
            {
                Regex regex = new Regex("^(HTTP|http)/(1|2)\\.\\d (\\d{3})(.|\\s)+$");
                Match match = regex.Match(headers["STATUS"]);
                if (match.Success)
                {
                    statusText = match.Groups[3].Value;
                }
            }
            else
            {
                throw new Exception("Status not found in response headers");
            }
            // attempt to convert the status text into a StatusCode
            bool success = false;
            try
            {
                ret = (StatusCode)Enum.Parse(typeof(StatusCode), statusText);
                if (Enum.IsDefined(typeof(StatusCode), ret))
                {
                    success = true;
                }
            }
            catch (ArgumentException) { }
            if (!success)
            {
                ret = StatusCode.Invalid;
                throw new Exception("Unrecognized response HTTP Status Code: " + statusText);
            }

            return ret;
        }

        #region IHeader

        // provides a read-only list of headers
        public Dictionary<string, string> Data
        {
            get
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                // deep clone of the headers so the user cannot alter the original
                foreach (KeyValuePair<string, string> header in headers)
                {
                    ret.Add(header.Key, header.Value);
                }
                return ret;
            }
        }

        #endregion

    }
}
