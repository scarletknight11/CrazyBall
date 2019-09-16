using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using SocketIO;

public class EnableAPI : MonoBehaviour
{
    public SocketIOComponent socket;
    public bool RunTest;

    public string Game = "EGDL";
    public string Version = "1.0.1504";

    private const string port = ":2505";
    private const string localEndpoint = "http://localhost" + port + "/";
    private const string testEndpoint = "http://enabledevs.com" + port + "/";
    private const string prodEndpoint = "https://portal.enablegames.com/";

    // TODO: tie to SKU
    private string endpoint = prodEndpoint;
    private const string kinecticEndpoint = "http://localhost:9004/";

    private bool isFetchingData = false;
    public float kinecticInterval = 1.0f;
    public delegate void NewBodyData(Dictionary<string, object> data);
    public event NewBodyData NewBodyDataEvent;
    private APIService kinecticService;

    private APIService apiService;
    public APIService api_Service
    {
        get
        {
            return apiService;
        }
    }
    private string sessionID;
    public string SessionID
    {
        get
        {
            return this.sessionID;
        }
    }
    private Dictionary<string, string> parameters;
    public bool HasParameter(string paramName)
    {
        if (null == parameters)
        {
            return false;
        }
        return parameters.ContainsKey(paramName);
    }
    public string GetParameter(string paramName)
    {
        if (!HasParameter(paramName))
        {
            return string.Empty;
        }
        return parameters[paramName];
    }
    /// <summary>
    /// returns a refrence to the parameters object within this class
    /// </summary>
    public Dictionary<string, string> GetAllParameters()
    {
        if (apiService.IsAuthenticated)
        {
            GetGameParameters(Game.ToLower(), "warmup");
        }
        return parameters;
    }

    private Action<APIService.APIError, APIService.IResponse> callback;

    // collection of server endpoints
    private Dictionary<string, string> endpoints = new Dictionary<string, string> {
        {"Authenticate", "auth/login"},
        {"Session", "api/sessions"},
        {"Settings", "api/settings"},
        {"Test", "api/test"},
        {"Kinematic", "api/kinematic"},
        {"Schemas", "api/schemas" }
    };

    private static EnableAPI instance;
    public static EnableAPI Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject();
                EnableAPI i = go.AddComponent<EnableAPI>() as EnableAPI;
                // EnableAPI.Awake() is called here, which will set instance
                return i;
            }
            return instance;
        }
    }

    void Awake()
    {
        Debug.Log("EnableAPI:Awake:1");
        if (instance != null)
        {
            Debug.Log("EnableAPI:Awake:2");
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Debug.Log("EnableAPI:Awake:3");
            instance = this;
        }
        Debug.Log("EnableAPI:Awake:4");

        this.gameObject.name = "_EnableAPI(Singleton)";
        Debug.Log("EnableAPI:Awake:4.1");
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("EnableAPI:Awake:4.1.1");

        //#if !UNITY_ANDROID || UNITY_EDITOR
        if (Application.platform != RuntimePlatform.Android) {
            Debug.Log("EnableAPI:Awake:4.2");
            // read from command line which endpoint we should hit
            String[] arguments = Environment.GetCommandLineArgs();
            foreach (string arg in arguments)
            {
                Debug.Log("EnableAPI:Awake:4.3");
                // if the debug flag was given, switch to local endpoint  
                if (arg.ToLower().StartsWith("/d"))
                {
                    endpoint = localEndpoint;
                    break;
                }
            }
        }
//#endif
        Debug.Log("EnableAPI:Awake:5");
        apiService = ScriptableObject.CreateInstance<APIService>();
        Debug.Log("EnableAPI:Awake:6");
        if (!apiService)
            return;
        Debug.Log("EnableAPI:Awake:7");
        apiService.Init(endpoint);
        Debug.Log("EnableAPI:Awake:8");
        sessionID = string.Empty;
        parameters = new Dictionary<string, string>();
        if (socket != null)
        {
            socket.On("open", SocketOpen);
            socket.On("error", SocketError);
            socket.On("new object", SocketNewObject);
            socket.On("close", SocketClose);
        }
        if (RunTest)
        {
            callback = APICallback;
            StartCoroutine(TestAPI());
        }
        else
        {
            callback = APICallback;
            //Authenticate("morr@enabledevs.com", "test", "Kollect");
        }
        if (apiService.IsAuthenticated)
        {
            GetGameParameters(Game.ToLower(), "warmup");
        }
    }

    public delegate void NewObjectSpawnedEvent(Vector2 pos);
    public event NewObjectSpawnedEvent NewObjectSpawned;

    public void SocketOpen(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
    }

    public void SocketError(SocketIOEvent e)
    {
       // Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
    }

    public void SocketNewObject(SocketIOEvent e)
    {
        if (null != NewObjectSpawned)
        {
            float x = e.data["x"].f;
            float y = e.data["y"].f;
            Vector2 pos = new Vector2(x, y);
            NewObjectSpawned(pos);
        }
        Debug.Log("[SocketIO] New Object received: " + e.name + " " + e.data);
    }

    public void SocketClose(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
    }

    // test the backend
    private void TestEndpoint()
    {
        if (!apiService)
            return;
        APIService.IRequest req = apiService.CreateRequest(APIService.RequestMethod.GET, this.endpoints["Test"], null);
        CallAPI(req, this.callback, false);
    }

    // retrieve kinematic info from the Kinectic server
    private IEnumerator FetchKinecticData()
    {
        Action<APIService.APIError, APIService.IResponse> GetData = (error, response) => {
            if (null != error)
            {
                Debug.LogWarning(error.Message);
                isFetchingData = false;
                return;
            }
            if (null != response)
            {
                Dictionary<string, object> payloadData = response.Payload.Data;
                if (null != payloadData)
                {
                    if (null != NewBodyDataEvent)
                    {
                        NewBodyDataEvent(payloadData);
                    }
                }
            }
        };
        while (isFetchingData)
        {
            yield return new WaitForSeconds(kinecticInterval);
            if (null != NewBodyDataEvent)
            {
                APIService.IRequest req = kinecticService.CreateRequest(APIService.RequestMethod.GET, this.endpoints["Kinematic"], null);
                StartCoroutine(kinecticService.SendRequest(req, GetData));
            }
        }
    }

    // authenticate the user
    public void Authenticate(string username, string password, string game, Action externalCallback = null)
    {
        Debug.Log("Authenticate:Logging into " + username);
        if (!apiService)
            return;
        Debug.Log("Authenticate:Setting up game parameter fetch ");
        Action<APIService.APIError, APIService.IResponse> OnComplete = (error, response) => {
            this.callback(error, response);
            if (apiService.IsAuthenticated)
            {
                Debug.Log("Authenticate:OnComplete:Calling GetGameParameters.");
                GetGameParameters(game.ToLower(), "warmup");
                Debug.Log("Authenticate:OnComplete:Called GetGameParameters.");
            }
            externalCallback();
        };
        Dictionary<string, object> payload = new Dictionary<string, object>();
        payload.Add("email", username);
        payload.Add("password", password);
        Debug.Log("Authenticate:Calling Authenticate request.");

        APIService.IRequest req = apiService.CreateRequest(APIService.RequestMethod.POST, this.endpoints["Authenticate"], payload);
        CallAPI(req, OnComplete, false);
        Debug.Log("Authenticate:Called Authenticate request.");
    }

    // create a new session
    public void CreateNewSession(string game, string version)
    {
        if (!apiService)
            return;
        Dictionary<string, object> payload = new Dictionary<string, object>();
        payload.Add("timeStart", EnableAPI.GetTimestamp());
        payload.Add("game", game.ToLower());
        payload.Add("version", version.ToLower());
        payload.Add("location", "Home");

        APIService.IRequest req = apiService.CreateRequest(APIService.RequestMethod.POST, this.endpoints["Session"], payload);
        CallAPI(req, this.callback, true);
    }

    // add kinematic data to a session
    public void PushSessionData(string serializedData)
    {
        if (!apiService)
            return;
        if (string.IsNullOrEmpty(this.sessionID))
        {
            //Debug.LogError("Cannot push session data to a null session");
            return;
        }

        APIService.IRequest req = apiService.CreateRequestJSON(APIService.RequestMethod.POST, this.endpoints["Session"] + "/" + this.sessionID, serializedData);
        CallAPI(req, this.callback, true);
    }

    // retrieve this user's parameter settings for this game
    public void GetGameParameters(string gameName, string phase)
    {
        if (!apiService)
            return;
        Action<APIService.APIError, APIService.IResponse> SetParams = (error, response) => {
            this.callback(error, response);
            if (null != response)
            {
                Dictionary<string, object> payloadData = response.Payload.Data;
                if (payloadData.ContainsKey("data"))
                {
                    // data field should be stringified json for the data payload
                    // TODO: when we change APIPayload to use FS, update this
                    APIService.APIPayload paramData = new APIService.APIPayload(payloadData["data"].ToString());
                    if (null != paramData.Data)
                    {
                        foreach (KeyValuePair<string, object> kvp in paramData.Data)
                        {
                            string key = kvp.Key.ToLower();
                            if (!parameters.ContainsKey(key))
                            {
                                parameters.Add(key, kvp.Value.ToString());
                            }
                            else
                            {
                                parameters[key] = kvp.Value.ToString();
                            }
                            //Debug.Log(key + ":" + kvp.Value.ToString());
                        }
                    }
                }
            }
        };
        string url = this.endpoints["Settings"] + "?stringify=true&game_id=" + gameName.ToLower() + "&phase=" + phase.ToLower();

        APIService.IRequest req = apiService.CreateRequest(APIService.RequestMethod.GET, url, null);
        CallAPI(req, SetParams, true);
    }

    // retrieve this user's extents for this schema
    public void GetSchemaExtents(string schemaID, Action<bool, Dictionary<string, float>> SetExtents)
    {
        if (!apiService)
            return;
        Action<APIService.APIError, APIService.IResponse> GetExtents = (error, response) => {
            this.callback(error, response);
            bool extentsFound = false;
            Dictionary<string, float> ret = new Dictionary<string, float>();
            if (null != response)
            {
                // aggregate repeated code into a temp function
                Action<object, string> ParseToRet = (payloadDict, value) => {
                    Dictionary<string, object> payload = (Dictionary<string, object>)payloadDict;
                    if (payload.ContainsKey(value))
                    {
                        if (ret.ContainsKey(value))
                        {
                            ret[value] = Convert.ToSingle(payload[value]);
                        }
                        else
                        {
                            ret.Add(value, Convert.ToSingle(payload[value]));
                        }
                    }
                };
                // parse the response data as either scalar or vector data
                Dictionary<string, object> payloadData = response.Payload.Data;
                if (payloadData.ContainsKey("min"))
                {
                    ParseToRet(payloadData, "min");
                    ParseToRet(payloadData, "max");
                    extentsFound = true;
                }
                else if (payloadData.ContainsKey("xMin"))
                {
                    ParseToRet(payloadData, "xMin");
                    ParseToRet(payloadData, "xMax");
                    ParseToRet(payloadData, "yMin");
                    ParseToRet(payloadData, "yMax");
                    extentsFound = true;
                }
            }
            // call back to the schema object
            SetExtents(extentsFound, ret);
        };

        string url = this.endpoints["Schemas"] + "/" + schemaID + "/extents";
        APIService.IRequest req = apiService.CreateRequest(APIService.RequestMethod.GET, url, null);
        CallAPI(req, GetExtents, true);
    }

    // set new schema extents
    public void SetSchemaExtents(string schemaID, Dictionary<string, object> values)
    {
        if (!apiService)
            return;
        Dictionary<string, object> payload = new Dictionary<string, object>();
        if (values.ContainsKey("min"))
        {
            payload.Add("scalar", values);
        }
        else if (values.ContainsKey("xMax"))
        {
            payload.Add("location2d", values);
        }
        else
        {
            return;
        }

        string url = this.endpoints["Schemas"] + "/" + schemaID + "/extents";
        APIService.IRequest req = apiService.CreateRequest(APIService.RequestMethod.POST, url, payload);
        CallAPI(req, this.callback, true);
    }

    // general function for calling the API
    private void CallAPI(APIService.IRequest req, Action<APIService.APIError, APIService.IResponse> callback, bool requireAuthentication)
    {
        if (!apiService)
            return;
        if (requireAuthentication && !apiService.IsAuthenticated)
        {
            //Debug.LogError("User is not authenticated.");
            return;
        }
        //Debug.Log(req.Method + " : " + req.URI);
        StartCoroutine(apiService.SendRequest(req, callback));
    }

    void APICallback(APIService.APIError error, APIService.IResponse response)
    {
        if (!apiService)
            return;
        if (null != error)
        {
            Debug.LogWarning(error.Message);
            return;
        }
        Dictionary<string, object> payloadData = response.Payload.Data;
        if (payloadData.ContainsKey("session_id"))
        {
            sessionID = payloadData["session_id"].ToString();
            //Debug.Log("Session ID Saved: " + sessionID);
        }
    }

    public static string GetTimestamp()
    {
        //return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString();
        return DateTime.Now.ToString("o");
    }

    #region testing

    // runs a series of tests for the api
    private IEnumerator TestAPI()
    {

        Authenticate("morr@enabledevs.com", "test", "Kollect");
        yield return new WaitForSeconds(2f);
        //TestEndpoint();
        //yield return new WaitForSeconds(2f);
        CreateNewSession(EnableAPI.Instance.Game, EnableAPI.Instance.Version);
        yield return new WaitForSeconds(2f);

        EnableString handString1 = new EnableString("(2.5, 2.5, 5.0) (2.0, 2.0, 5.0)");
        TrackerMessage handData1 = new TrackerMessage("Hand Game Position", handString1);
        PushSessionData(handData1.SerializedValue);

        EnableString handString2 = new EnableString("(1.5, 3.5, -5.0) (3.0, 3.0, 5.0)");
        TrackerMessage handData2 = new TrackerMessage("Hand Game Position", handString2);
        PushSessionData(handData2.SerializedValue);
        /*
		// create test session data json
		Dictionary<string, object> postData = new Dictionary<string, object>();
		Dictionary<string, object> handData = new Dictionary<string, object>();
		Dictionary<string, object> skeletonData = new Dictionary<string, object>();
		Dictionary<string, object> jointDictionary = new Dictionary<string, object>();
		Dictionary<string, object> spinebaseData = new Dictionary<string, object>();
		Dictionary<string, object> spinemidData = new Dictionary<string, object>();
		Dictionary<string, object> position1 = new Dictionary<string, object>();
		Dictionary<string, object> orientation1 = new Dictionary<string, object>();
		Dictionary<string, object> position2 = new Dictionary<string, object>();
		Dictionary<string, object> orientation2 = new Dictionary<string, object>();

		Action ClearDBs = () => {
			postData.Clear();
			handData.Clear();
			skeletonData.Clear();
			jointDictionary.Clear();
			spinebaseData.Clear();
			spinemidData.Clear();
			position1.Clear();
			orientation1.Clear();
			position2.Clear();
			orientation2.Clear();
		};
		
		ClearDBs();
		handData.Add("Value", "(2.5, 2.5, 5.0) (2.0, 2.0, 5.0)");
		handData.Add("$type", "EnableString");
		postData.Add("timestamp", GetTimestamp());
		postData.Add("key", "Hand Game Position");
		postData.Add("value", handData);
		PushSessionData(postData);
		yield return new WaitForSeconds(.3f);

		ClearDBs();
		handData.Add("Value", "(1.5, 3.5, -5.0) (3.0, 3.0, 5.0)");
		handData.Add("$type", "EnableString");
		postData.Add("timestamp", GetTimestamp());
		postData.Add("key", "Hand Game Position");
		postData.Add("value", handData);
		PushSessionData(postData);
		yield return new WaitForSeconds(.3f);
		
		ClearDBs();
		position1.Add("x", 1.1);
		position1.Add("y", 2.2);
		position1.Add("z", 3.3);
		orientation1.Add("x", 0.1);
		orientation1.Add("y", 0.2);
		orientation1.Add("z", 0.3);
		orientation1.Add("w", 0.4);
		spinebaseData.Add("position", position1);
		spinebaseData.Add("orientation", orientation1);
		position2.Add("x", 6.6);
		position2.Add("y", 7.7);
		position2.Add("z", 8.8);
		orientation2.Add("x", 0.6);
		orientation2.Add("y", 0.7);
		orientation2.Add("z", 0.8);
		orientation2.Add("w", 0.9);
		spinemidData.Add("position", position2);
		spinemidData.Add("orientation", orientation2);
		skeletonData.Add("SpineBase", spinebaseData);
		skeletonData.Add("SpineMid", spinemidData);
		jointDictionary.Add("jointDictionary", skeletonData);
		postData.Add("timestamp", GetTimestamp());
		postData.Add("key", "Player Skeleton");
		postData.Add("value", jointDictionary);
		PushSessionData(postData);
		*/
    }

    void DebugCallback(APIService.APIError error, APIService.IResponse response)
    {
        if (null != error)
        {
            //Debug.Log(error.Message);
            return;
        }

        Dictionary<string, object> payloadData = response.Payload.Data;
        if (payloadData.ContainsKey("session_id"))
        {
            sessionID = payloadData["session_id"].ToString();
            //Debug.Log("Session ID Saved: " + sessionID);
        }

        //Debug.Log("HEADERS");
        foreach (KeyValuePair<string, string> header in response.Headers.Data)
        {
            //Debug.Log(header.Key + " : " + header.Value);
        }
        //Debug.Log("PAYLOAD DATA");
        foreach (KeyValuePair<string, object> data in response.Payload.Data)
        {
            //Debug.Log(data.Key + " : " + data.Value.ToString());
        }

        //	Debug.Log("User is Authenticated: " + apiService.IsAuthenticated);
    }

    #endregion
}
