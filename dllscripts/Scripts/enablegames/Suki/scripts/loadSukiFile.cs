/*
 * using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

//loads a Suki JSON file based on parameter change.
public class loadSukiFile : MonoBehaviour {

    Text text;
	string output = "no file";

    void Subscribe()
    {
        ThemeController.OnThemeChange += ThemeController_OnThemeChange;
    }

    void UnSubscribe()
    {
        ThemeController.OnThemeChange -= ThemeController_OnThemeChange;
    }
    // Use this for initialization
    void Start () {
        text = textRT.gameObject.GetComponent<Text>();
	}

    void Update()
    {
        output = (fb.outputFile == null) ? "cancel" : fb.outputFile.FullName; //full path name
        if (output != "cancel")
        {
            if (text)
            {
                text.text = Path.GetFileName(output);
                print("Text=" + text.text);
            }
            else
            {
                print("no text");
            }
            Suki.SukiSchemaList.Instance.Reset();
            Suki.SukiSchemaList.Instance.AddFile(output);
        }
    }
}
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;


public class SukiProfileController : MonoBehaviour
{
    #region Properties
    public static string ActiveProfile
    {
        get
        {
            if (instance != null)
            {
                return instance.activeProfile;
            }
            else
            {
                return "default"; // default theming
            }
        }
    }
    private static ProfileController instance;
    private string activeProfile = "default";

    public delegate void ProfileEvent(Profile newProfile);
    public static event ProfileEvent OnProfileChange;


    public delegate void ProfileRequest();

    public static string SukiPath
    {
        get { return ActiveProfile.ToString() + "/suki"; }
    }


    #endregion

    #region Monobehaviour callbacks

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        Subscribe();

        ActivateProfile(MenuKollectibleSpawner.kollectProfile);
    }


    void OnDestroy()
    {
        UnSubscribe();
    }

    #endregion

    #region Events

    void Subscribe()
    {
        SessionCreatorPanel.OnSessionLoaded += OnSessionLoaded;
        GameParameterCreator.SetUpComplete += onSetUpComplete;
        Game.OnParameterChange.AddListener(OnParameterUpdated);
        ParameterWidget.OnParameterChanged += OnParamChange;
    }

    void UnSubscribe()
    {
        SessionCreatorPanel.OnSessionLoaded -= OnSessionLoaded;
        GameParameterCreator.SetUpComplete -= onSetUpComplete;
        Game.OnParameterChange.RemoveListener(OnParameterUpdated);
        ParameterWidget.OnParameterChanged -= OnParamChange;
    }


    void OnSessionLoaded()
    {
        Game.ActiveParameters = ParameterHandler.Instance.GetParameters(GameCreatorPanel.Instance.GameParameterCreators[0].ParameterKey);
        UpdateBackgroundKollect();
    }

    void OnParamChange(string paramName)
    {
        if (paramName == ParameterStrings.Profile )
        {
            OnChangeSelectedProfile(paramName);
        }
    }

    void OnParameterUpdated()
    {
            UpdateProfile();
    }

    #endregion


    #region Utility

    string GetValueFromParameter(string paramType)
    {
        return ((StringListParameter)Game.ActiveParameters.GetParameter(paramType)).Value;
    }

    Profile ConvertStringToProfile(string param)
    {
        return (Profile)System.Enum.Parse(typeof(Profile), param);
    }

    string GetProfileObjectName()
    {
        return Game.ActiveParameters.Name + " Panel/ScrollView/Image/Profile-Widget";
    }

    bool GetBoolParamState(string paramToCheck)
    {
        return ((BoolParameter)Game.ActiveParameters.GetParameter(paramToCheck)).Value;
    }

    #endregion

    #region Core Implementation

    void GameParameterChange(string paramType, Action<Profile> onSuccess)
    {
        string param = GetValueFromParameter(paramType);
        Profile oldProfile = activeProfile;
        activeProfile = ConvertStringToProfile(param);

        if (activeProfile != oldProfile)
        {
            if (onSuccess != null)
            {
                onSuccess(activeProfile);
            }
        }
    }

    void ChangeProfile(Profile Profile)
    {
        kollectProfile = Profile;
        hazardProfile = Profile;
        ActivateProfile(Profile);
    }


    public static void ActivateProfile(Profile aProfile)
    {
        instance.activeProfile = aProfile;
        if (OnProfileChange != null)
        {
            OnProfileChange(aProfile);
        }
    }

    void UpdateProfile()
    {
        GameParameterChange(ParameterStrings.Profile, ChangeProfile);
    }

/*
    public static void SetProfileForPhase()
    {
        if (instance.GetBoolParamState(ParameterStrings.SELECT_BG_SEPARATELY))
        {
            instance.OnSelectBGKollectChange(true);
        }
        else
        {
            instance.OnSelectBGKollectChange(false);
        }
    }
    */
/*
    
    #endregion

    #region UI callbacks
    void ChangeProfileWidgetStatus(bool status)
    {
        if (GameObject.Find(GetProfileObjectName()) != null)
        {
            GameObject ProfileObject = GameObject.Find(GetProfileObjectName());
            ProfileObject.SetActive(status);
        }
    }


    void OnChangeSelectedProfile(string paramName)
    {
        if (paramName == ParameterStrings.Profile)
        {
                UpdateProfile();
        }
    }
    #endregion






}
*/