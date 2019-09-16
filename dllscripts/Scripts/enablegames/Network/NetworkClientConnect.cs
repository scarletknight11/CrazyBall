using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientConnect: ScriptableObject/*: MonoBehaviour*/ 
{

    private static NetworkClientConnect instance = null;
	public NetworkManager manager = null;
	public string address="localhost";
    public static NetworkClientConnect Instance {
        get {
			//Debug.Log ("getting NCC Instance" + instance);
            if (instance == null) {
                instance = ScriptableObject.CreateInstance<NetworkClientConnect>();
                    //new NetworkClientConnect();
				//Debug.Log ("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++MAKING NEW NETWORK CLIENT!!!"+instance);
            }
            return instance;
        }
    }

	// Use this for initialization
	void Start () {
		//Debug.Log("NetworkClientonnect Awake");
       // Connect();

		GetManager();
    }

	private NetworkManager GetManager()
	{
		if (manager != null)
			return manager;
		if (GameObject.Find ("NetworkManager") != null) {
			manager = GameObject.Find ("NetworkManager").GetComponent<NetworkManager> ();
		}
		return manager;
	}
	public void Connect(string str=null)
	{
        Debug.Log("CONNECTING");
		if (GetManager() == null)
			return;
		Debug.Log("CONNECTING2:"+str);
		if (str!=null)
			address = str;
		manager.networkAddress = address;
        manager.StartClient();
		Debug.Log("CONNECTING3");
            //            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().client.Disconnect();
    }
    public void Disconnect() {
        Debug.Log("DISCONNECT");
		if (GetManager() == null)
			return;
		NetworkClient client = manager.client;
		if (client!=null)
			client.Disconnect();
    }
    

    void onDestroy()
    {
		//Debug.Log ("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++NCC:Destroy" + instance);
        Disconnect();
    }
}
