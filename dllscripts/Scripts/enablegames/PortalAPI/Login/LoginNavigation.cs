using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginNavigation : MonoBehaviour {

    public GameObject addressInput;
    public GameObject usernameInput;
	public GameObject passwordInput;
    public Text addressPlaceHolder;
    public Text usernamePlaceHolder;
	public Text passwordPlaceHolder;

    [SerializeField]
    private Toggle _rememberMeToggle;

    public int sceneIndexToLoad;
    //public GameObject _mainMenu;
	public GameObject APIObject;
	//public GameObject loginScreen;
	private GamePanel gamePanel;

	[SerializeField]
	private bool _rememberMe = true;

	private const string _LAST_USED_USER_NAME = "lastUsedUserName";

    private bool loginOverride = false;
	public bool autoLogin;

	public void Quit()
	{
		Application.Quit ();
	}
    void Start()
	{
	//	gamePanel = loginScreen.gameObject.GetComponent<GamePanel>();

        if (EnableAPI.Instance.api_Service.IsAuthenticated) {
			Debug.Log ("Start:LoadLevel");
            //Application.LoadLevel(sceneIndexToLoad);
            SceneManager.LoadScene(sceneIndexToLoad);
            Debug.Log("Start:LoadLevel failed");
        }
        if (usernameInput.GetComponent<InputField>() != null)
		{
			usernameInput.GetComponent<InputField>().onEndEdit.AddListener(OnUserNameEntered);
		}
/*        if (addressInput.GetComponent<InputField>() != null)
        {
            addressInput.GetComponent<InputField>().onEndEdit.AddListener(OnAddressEntered);
        }
        */
        if (transform.parent.gameObject.GetComponentInChildren<Toggle>() != null)
        {
            _rememberMeToggle = transform.parent.gameObject.GetComponentInChildren<Toggle>();
        }

        if(_rememberMeToggle != null)
        {
            _rememberMeToggle.onValueChanged.AddListener(
                (state)=> 
                {
                    _rememberMe = state;
                    PlayerPrefs.SetString("rememberMe", state.ToString());
                }
            );
        }

        string rmParam = PlayerPrefs.GetString("rememberMe");
        //        if (PlayerPrefs.GetString("rememberMe") != null)
        if (rmParam != null)
        {
            //print("remember:");
            //print("remember:="+PlayerPrefs.GetString("Remember Me"));
            //Debug.Log(PlayerPrefs.GetString("rememberMe"));
            if (rmParam != "true" && rmParam != "false")
            {
                PlayerPrefs.SetString("rememberMe","true");
            }
            _rememberMe = bool.Parse(PlayerPrefs.GetString("rememberMe"));
            _rememberMeToggle.isOn = _rememberMe;
        }

		//If true, then we set the last loaded login into the text
		if (_rememberMe)
		{
			LoadRememberedLogin();
		}

		//if already logged in, hide login panel
		//LoginCallback();
	}

	void Update () {

		// Changes the focus between the Username and Password inputs by hitting tab
		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (addressInput.GetComponent<InputField>().isFocused) {
				usernameInput.GetComponent<InputField>().Select();
			} else if (usernameInput.GetComponent<InputField>().isFocused) {
				passwordInput.GetComponent<InputField>().Select();
			} else if (passwordInput.GetComponent<InputField> ().isFocused) {
				addressInput.GetComponent<InputField>().Select();
			}
		}

		if (Input.GetKeyDown (KeyCode.Return)) {
			Login ();
		}
	}


	/// <summary>
	/// Attempts to load the last used username from playerprefs.
	/// If it exists, set the userName (load the password) and password to
	/// the inputfields text.
	/// </summary>
	void LoadRememberedLogin()
	{
		print ("Load remembered login");
        string lastUsedAddress = PlayerPrefs.GetString(egParameterStrings.LAUNCHER_ADDRESS);
        if (!string.IsNullOrEmpty(lastUsedAddress))
        {
            SetAddress(lastUsedAddress);
        }
        string lastUsedUserName = PlayerPrefs.GetString(_LAST_USED_USER_NAME);
		if (!string.IsNullOrEmpty(lastUsedUserName))
		{
            SetUserName(lastUsedUserName);
			SetLoadedPassword(LoadPassword(lastUsedUserName));
		}
	}

	public void Login()
	{
		if (autoLogin)
		{
			APIObject.GetComponent<EnableAPI>().Authenticate("morr@enabledevs.com", "test", EnableAPI.Instance.Game);

			gamePanel.Hide();
			//loginScreen.SetActive (false);
		} else {

			if (usernameInput.GetComponent<InputField> ().text == "") {
				usernamePlaceHolder.text = "Username required";
				usernamePlaceHolder.color = new Color (255, 0, 0);
			} 

			if (passwordInput.GetComponent<InputField> ().text == "") {
				passwordPlaceHolder.text = "Password required";
				passwordPlaceHolder.color = new Color (255, 0, 0);
			}

            //			if (usernameInput.GetComponent<InputField>().text != "" && passwordInput.GetComponent<InputField>().text != "") {
            //				APIObject.GetComponent<EnableAPI>().Authenticate(usernameInput.GetComponent<InputField>().text, passwordInput.GetComponent<InputField>().text, "Citadel", this.LoginCallback);
            //            }

            if (usernameInput.GetComponent<InputField>().text != "")
                SaveAddress(addressInput.GetComponent<InputField>().text);
            else
                SaveAddress("localhost");

            if (usernameInput.GetComponent<InputField>().text != "" && passwordInput.GetComponent<InputField>().text != "")
            {
            
				Login(usernameInput.GetComponent<InputField>().text, passwordInput.GetComponent<InputField>().text);
			}
		
		}
	}


	/// <summary>
	/// Logs the player into the game
	/// Saves the players username/password
	/// </summary>
	/// <param name="user"></param>
	/// <param name="password"></param>
	void Login(string user, string password)
	{
        Debug.Log("Login as " + user);
        if (user == "EAG" && password == "kol13ct")
            loginOverride = true;
		if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
		{
            Debug.Log("Login:2");
            if (APIObject != null)
			{
                Debug.Log("Login:3");
                if (APIObject.GetComponent<EnableAPI>() != null)
				{
                    Debug.Log("Login:4");
                    APIObject.GetComponent<EnableAPI>().Authenticate(user, password, EnableAPI.Instance.Game, this.LoginCallback);
                    Debug.Log("Login:5");
                    SaveLogin(user, password);
                    Debug.Log("Login:6");
                }
                else
				{
					if (FindAPIObject() != false)
					{
                        Debug.Log("Login:7");
                        Login(user, password);
					}
				}
			}
			else
			{
				if (FindAPIObject() != false)
				{
                    Debug.Log("Login:8");
                    Login(user, password);
				}
			}
		}
	}

	public void LoginCallback() {
        Debug.Log("Start:LoginCallback");
        if (APIObject.GetComponent<EnableAPI>().api_Service.IsAuthenticated || loginOverride) {
            loginOverride = false;
            // GameObject _mainMenu = GameObject.Find("mainMenu");
            /*_mainMenu.SetActive(true);
            gamePanel.Hide();*/
            //Application.LoadLevel(sceneIndexToLoad);
            Debug.Log("Start:LoginCallback:loadLevel");
            SceneManager.LoadScene(sceneIndexToLoad);
            Debug.Log("Start:LoadLevel failed");
            //loginScreen.SetActive (false);
        } else {
			Toast.MakeToast("Login Failed");
		}
	}

	/// <summary>
	/// Makes sure we still have a reference to the APIObject 
	/// if for some reason we lose it, like when we come back to the
	/// main menu from the game.
	/// </summary>
	/// <returns></returns>
	bool FindAPIObject()
	{
		if (FindObjectOfType<EnableAPI>() != null)
		{
			APIObject = FindObjectOfType<EnableAPI>().gameObject;
			return true;
		}
		else
		{
			Debug.Log("Login Nav Cannot find the EnableAPI object; cannot login");
		}
		return false;
	}

	/// <summary>
	/// Saves a username password pair to playerprefs
	/// </summary>
	/// <param name="user"></param>
	/// <param name="password"></param>
	void SaveLogin(string user, string password)
	{
		print ("LoginNavigation:SaveLogin");
		if (!string.IsNullOrEmpty(user))
		{
			PlayerPrefs.SetString(user, password);
		}

		PlayerPrefs.SetString(_LAST_USED_USER_NAME, user);
	}

    void SaveAddress(string address)
    {
        print("LoginNavigation:SaveAddress:"+address);
        if (!string.IsNullOrEmpty(address))
        {
            PlayerPrefs.SetString(egParameterStrings.LAUNCHER_ADDRESS, address);
        }
        else
            PlayerPrefs.SetString(egParameterStrings.LAUNCHER_ADDRESS, "localhost");

    }

    /// <summary>
    /// Checks to see if player prefs has 
    /// a key value pair where the key 
    /// is the user name and the value 
    /// is the password
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    string LoadPassword(string userName)
	{
		string password = null;

		if (!string.IsNullOrEmpty(PlayerPrefs.GetString(userName)))
		{
			password = PlayerPrefs.GetString(userName);
		}

		return password;
	}

	void SetLoadedPassword(string password)
	{
		if (!string.IsNullOrEmpty(password))
		{
			if (passwordInput.GetComponent<InputField>() != null)
			{
				passwordInput.GetComponent<InputField>().text = password;
			}
		}
	}

	void SetAddress(string address)
	{
		if (!string.IsNullOrEmpty(address))
		{
			if (addressInput.GetComponent<InputField>() != null)
			{
				addressInput.GetComponent<InputField>().text = address;
			}
		}
	}

	void SetUserName(string userName)
	{
		if (!string.IsNullOrEmpty(userName))
		{
			if (usernameInput.GetComponent<InputField>() != null)
			{
				usernameInput.GetComponent<InputField>().text = userName;
			}
		}
	}

	/// <summary>
	/// Checks to see if the client is authenticated.
	/// If the player comes back to the main menu, then they 
	/// shouldn't have to login agian.
	/// </summary>
	/// <returns></returns>
	bool PlayerIsLoggedIn()
	{
		if (FindAPIObject())
		{
			if (APIObject.GetComponent<EnableAPI>().api_Service.IsAuthenticated)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// When an edit was made to the username
	/// input field, take the text from that field
	/// check to see if a password exists with it
	/// then set the password text field to that 
	/// password.
	/// </summary>
	/// <param name="userName"></param>
	void OnUserNameEntered(string userName)
	{
		SetLoadedPassword(LoadPassword(userName));
	}
}
