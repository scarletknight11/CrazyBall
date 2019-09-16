using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// This class creates sessions for AbleGames games.
/// This class will connect & create a session for logging data and results.
/// </summary>
public class SessionCreator : MonoBehaviour {
    private static SessionCreator instance;

    private bool confirmed = false;

    private string sessionName = string.Empty;
    public string SessionName {
        get {
            return sessionName;
        }
        set {
            sessionName = value;
        }
    }

    private Session currentSession;
    public Session CurrentSession {
        get {
            if (currentSession == null) {
                SetDefaultSession();
            }
            return currentSession; }
    }
    /// <summary>
    /// This accessor allows users to access the session creator without dragging a prefab into their scene.
    /// </summary>
    public static SessionCreator Instance {
        get {
            if (instance == null) {
                GameObject go = new GameObject();
                instance = go.AddComponent<SessionCreator>() as SessionCreator;
                go.name = "_AbleSessionCreator(Singleton)";
            }
            return instance;
        }
    }

    void Awake() {
        if (instance == null || instance == this)
            instance = this;
        else {
            DestroyImmediate(this);
        }
    }

    /// <summary>
    /// Makes a session with 'name'
    /// The singleton then stays alive between scenes
    /// </summary>
    public void CreateSession() {
        confirmed = true;
		print ("CreateSession");
        currentSession = new Session(sessionName);
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Quick way to make a new session--if you don't want to set up the UI.
    /// </summary>
    public void SetDefaultSession() {
        currentSession = new Session();
    }

}
