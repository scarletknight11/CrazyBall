using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;
using DG.Tweening;

/// <summary>
/// UI Element. Default for generic/quick setup.
/// </summary>
public class SessionCreatorPanel : MonoBehaviour {

    public delegate void SessionEvent( );
    public static event SessionEvent OnSessionLoaded;

    [SerializeField]
    private InputField textField;
	[SerializeField]
	private GameObject dropDownText;
	public FilesToDropdown ftd; 
	private static float TWEEN_TIME = .2f;
	private GamePanel gamePanel;

	void Start () {
        print("SessionCreatorPanel:Start");
		gamePanel = this.gameObject.GetComponent<GamePanel> ();
	}

	/*
	void Awake () {
		this.transform.localScale = Vector3.zero;
		this.transform.DOScale(1f,TWEEN_TIME);
	}

    public void Dismiss() {
		this.transform.DOScale(0f,TWEEN_TIME);
		StartCoroutine (Disable ());
    }

	IEnumerator Disable () {
		yield return new WaitForSeconds(TWEEN_TIME);
		UnityEngine.Object.Destroy(this.gameObject);
	}
	*/

    /// <summary>
    /// Save currently set parameters into settings folder
    /// </summary>
    public void SaveParameters() {
        DirectoryInfo info = new DirectoryInfo(GameParameters.SettingsFolder);
        if (info.Exists == false) {
            info.Create();
        }
        string playerID = textField.text;
        GameParameterCreator[] gameCreators = GetGameParameterCreators();
        foreach (GameParameterCreator gpc in gameCreators) {
            if (null == gpc.Parameters) {
                gpc.SetUp();
            }
            GameParameters gameParams = gpc.Parameters;
            string filename = Path.Combine(GameParameters.SettingsFolder, playerID + "." + gpc.ParameterKey + GameParameters.FileExtension);
            string serializedParameters = gameParams.Serialize();
            using (StreamWriter sw = new StreamWriter(filename)) {
                sw.Write(serializedParameters);
            }
        }
        SessionCreator.Instance.SessionName = playerID;
		ftd.GetFileNames ();  //populate the dropdown list
		gamePanel.Hide();
    }

    /// <summary>
    /// Load saved parameters from SettingsFolder
    /// </summary>
    public void LoadParameters() {
        //string playerID = textField.text;
		string playerID = dropDownText.GetComponent<Text>().text;
        GameParameterCreator[] gameCreators = GetGameParameterCreators();
        ParameterHandler.Instance.ResetParameters();
        bool success = true;
        foreach (GameParameterCreator gpc in gameCreators) {
            gpc.Reset();
            string filename = Path.Combine(GameParameters.SettingsFolder, playerID + "." + gpc.ParameterKey + GameParameters.FileExtension);
            try {
                string serializedParameters = string.Empty;
                using (StreamReader sr = new StreamReader(filename)) {
                    serializedParameters = sr.ReadToEnd();
                }
                if (!string.IsNullOrEmpty(serializedParameters)) {
                    GameParameters gp = (GameParameters)JSONSerializer.Deserialize(typeof(GameParameters),serializedParameters);
                    gpc.Parameters = gp;
                    gpc.LoadIntoParamHandler(gp);
                }
            } catch (Exception e) {
                Debug.Log(e.Message);
                success = false;
            }
        }
        if(success)
        {
            Toast.MakeToast("Profile loaded.");
            
            if( OnSessionLoaded  != null )
            {
                OnSessionLoaded( );
            }
        }
        else
        {
            Toast.MakeToast("Error, profile not loaded.");
        }
        SessionCreator.Instance.SessionName = playerID;
		gamePanel.Hide ();
    }

    private GameParameterCreator[] GetGameParameterCreators() {
        return GameCreatorPanel.Instance.GameParameterCreators;
    }

}
