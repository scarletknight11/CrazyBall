using System.Collections;
using System.Collections.Generic;
using eaglib;
//EG REQUIRED
using UnityEngine;
using enableGame;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{

    //player note from the game over panel
    //in Citadel we store the game information such as score and stats in different placese, we may want to have a script 
    //or a singleton that store all those information (so the tracker footer can retrieve those information directly by it)

    public string Notes;
    //public int score = 0;
 
    //private Page currentPage = Page.PLAY;

    public static bool isPaused;
    //public GameObject pausePanel;
    public string MenuScene = "eag_MainMenu";
    public string GameScene = "eag_KickCubeGame";

    bool gameStarted = false;
    bool gameOver = false;

    public SkeletonData Skeleton;
    public NetworkSkeleton netskeleton;
    private Suki.SukiInput suki = null;

    public Transform PlayerObject;

    //egFloat,etc. are custom variables that can be attached to parameters in the settings menu and portal
    egFloat Speed = 1.0f;       //speed of player
    egFloat Gravity = -9.80f;   //falling cylinder's gravity (-1.0 is unity default)
    egInt GameLength = 300;   //in seconds
    egFloat maxSize = 0.1f;

    float startTime; //start time of game

    //Use this for initialization
    void Awake()
    {
        //initialize SUKI
        if (Skeleton == null)
            Skeleton = GameObject.Find("Tracking Avatar").GetComponent<SkeletonData>();
        if (netskeleton == null)
            netskeleton = GameObject.Find("Tracking Avatar").GetComponent<NetworkSkeleton>();

        suki = Suki.SukiInput.Instance;
        suki.Skeleton = Skeleton;

        suki.Skeleton.Moving = false;

        suki.Skeleton.resetMinMax = true;

        //connect the client skeleton to the server skeleton (running in the enablegames launcher app)
        NetworkClientConnect.Instance.Connect();


        //Bind Speed to the variable "STARTING SPEED" from the settings menu
        //NOTE:Binding will be skipped if ParameterHandler not loaded (i.e. running this scene 
        //without first running MainMenu scene
        VariableHandler.Instance.Register(CBParameterStrings.STARTING_SPEED, Speed);
        VariableHandler.Instance.Register(CBParameterStrings.BALL_SIZE_WIDTH, maxSize);
        VariableHandler.Instance.Register(egParameterStrings.GAME_LENGTH, GameLength);
        //PlayerPrefs.SetString(egParameterStrings.LAUNCHER_ADDRESS, "10.0.0.62");


        PlayerObject.localScale += new Vector3(0, maxSize, 0);
        print("Speed=" + Speed);
        print("GameLength=" + GameLength);

    }

    //Update is called once per frame
    void Update()
    {
        //print("Gravity=" + Gravity);
        // Return to main menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //disconnect the skeleton from the server
            //NetworkClientConnect.Instance.Disconnect();
            //Application.LoadLevel(1);
           // EndGame();
        }
    }

    private float timeSinceLastLaneMove = 0;

    private void FixedUpdate()
    {
        if (gameOver)
            return;
        if (!gameStarted)
        {  //is level just starting?
            gameStarted = true;
            startTime = Time.time;
            //egBeginSession (); 
            Tracker.Instance.BeginTracking();
            print("BEGIN TRACKING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
        float duration = Time.time - startTime;
        if (duration >= GameLength)  //is game time over?
            //showGameOverPanel();
            timeSinceLastLaneMove += Time.deltaTime;

        // no-op if SUKI is not currently giving us input data
        //print("Game:FixedUpdate:" + suki.Updating);

        if (netskeleton && netskeleton.moving)
        {
            suki.Skeleton.Moving = true;
            suki.Skeleton.resetMinMax = true;
        }

        if (!suki.Updating)
        {
            print("Game:suki not updating.");
            return;
        }

        if (suki.RangeExists("placement"))
        {
            // we can use a range value as a placement to move left and right
            float range = suki.GetRange("placement");
            // convert 0f to 1f to -1f to 1f
            float xPercent = (range * 2) - 1f;
            //print("placement mode:" + range + ":" + xPercent);
            // add a deadzone of +/- %
            float deadzone = 0.2f;
            if (xPercent > -deadzone && xPercent < deadzone)
            {
                xPercent = 0f;
            }
            // move the object
            Vector3 pos = PlayerObject.transform.localPosition;  //REPLACE PlayerObject with whatever object or vector you want to be updated
            pos.x = pos.x + (xPercent * Speed / 20); // we use speed as a position scaler
            PlayerObject.transform.localPosition = pos;
        }
        //shoulder profile is set as "joystick"
        //In X-Z movement, can be used for X-movement togther with "placement" for Z.
        if (suki.RangeExists("joystick"))
        {
            // we can use a range value as a placement to move left and right
            float range = suki.GetRange("joystick");
            // convert 0f to 1f to -1f to 1f
            float xPercent = (range * 2) - 1f;
            //print("joysick mode:" + range + ":" + xPercent);
            // move the object
            float deadzone = 0.2f;
            if (xPercent > -deadzone && xPercent < deadzone)
            {
                xPercent = 0f;
            }
            Vector3 pos = PlayerObject.transform.localPosition; //REPLACE PlayerObject with whatever object or vector you want to be updated
            pos.z = pos.z + (xPercent * Speed / 25); // we use speed as position scaler
            PlayerObject.transform.localPosition = pos;
        }

        if (suki.SignalExists("moveLeft") && suki.SignalExists("moveRight"))
        {
            // we can use a pair of triggers to move left or move right
            bool moveLeft = suki.GetSignal("moveLeft");
            bool moveRight = suki.GetSignal("moveRight");

            Vector3 pos = PlayerObject.transform.localPosition;

            // only if there is a direction to move, and it's been some time since our last move
            // Instead of changing the speed of the movement here we change the pause between movements
            if ((!moveLeft && !moveRight) || (moveLeft && moveRight) || (timeSinceLastLaneMove < 1 / Speed)) // we use speed as a time scaler
            {
                return;
            }
            else if (moveLeft)
            {
                pos.x = (pos.x - 0.2f);
            }
            else if (moveRight)
            {
                pos.x = (pos.x + 0.2f);
            }
            PlayerObject.transform.localPosition = pos;
            timeSinceLastLaneMove = 0f;
        }
        if (suki.Location2DExists("leftfoot"))
        {
            Vector3 pos = PlayerObject.transform.localPosition; //REPLACE PlayerObject with whatever object or vector you want to be updated
            Vector2 fpos = suki.GetLocation2D("leftfoot");
            // convert 0f to 1f to -1f to 1f
            float xPercent = (fpos.x * 2) - 1f;
            float yPercent = (fpos.y * 2) - 1f;
            pos.x = (xPercent * Speed * 4); // we use speed as position scaler
            pos.y = (xPercent * Speed * 4); // we use speed as position scaler
            PlayerObject.transform.localPosition = pos;

            //pos.x = pos.x + (fpos.x * Speed/40); // we use speed as position scaler
            //PlayerObject.transform.position = Vector3.Lerp(LeftFoot.transform.position, new Vector3(newX, newY, newZ), 1f);
        }
        if (suki.Location2DExists("rightfoot"))
        {
            Vector3 pos = PlayerObject.transform.localPosition; //REPLACE PlayerObject with whatever object or vector you want to be updated
            Vector2 fpos = suki.GetLocation2D("rightfoot");
            // convert 0f to 1f to -1f to 1f
            float xPercent = (fpos.x * 2) - 1f;
            float yPercent = (fpos.y * 2) - 1f;
            pos.x = (xPercent * Speed * 4); // we use speed as position scaler
            pos.y = (yPercent * Speed * 4); // we use speed as position scaler
            PlayerObject.transform.localPosition = pos;
        }
        checkRange();
    }

    void checkRange()
    {
        float maxX = 4f, maxY = 3f;
        Vector3 pos = PlayerObject.transform.localPosition;
        //REPLACE PlayerObject with whatever object or vector you want to be updated
        if (pos.x > maxX)
            pos.x = maxX;
        if (pos.x < -maxX)
            pos.x = -maxX;
        if (pos.y > maxY)
            pos.y = maxY;
        if (pos.y < -maxY)
            pos.y = -maxY;
        PlayerObject.transform.localPosition = pos;
    }

    public void PauseGame()
    {
        print("Game is Paused...");
        isPaused = true;
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
        AudioListener.volume = 0;
    }

    /// <summary>
    /// Unpauses game time and audio
    /// </summary>
	public void UnPauseGame()
    {
        print("Unpause");
        isPaused = false;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        AudioListener.volume = 1.0f;
    }

    /// <summary>
    /// Quit the current gamesession (tracking).
    /// </summary>
	public void EndGame()
    {
        PauseGame();
        gameOver = true;
        UnPauseGame();  //must start up unity time again so DOTweens work
        egEndSession();
    }

    /// <summary>
    /// End the session and Load the main menu scene.
    /// </summary>
	public void MainMenu()
    {
        print("MainMenu");
        UnPauseGame();  //must start up unity time again so DOTweens work
        egEndSession();

#if UNITY_ANDROID
        SceneManager.LoadScene(MenuScene);
#else
        SceneManager.LoadScene(MenuScene);
#endif
    }

    /// <summary>
    /// End current session and Reload game
    /// </summary>
	public void ReloadGame()
    {
        print("ReloadGame");
        UnPauseGame();  //must start up unity time again so DOTweens work
        egEndSession();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Loads next game level.
    /// </summary>
	public void NextGame()
    {
        print("NextGame");
        UnPauseGame();  //must start up unity time again so DOTweens work
        egEndSession();
        SceneManager.LoadScene(GameScene);
    }

    void egEndSession()
    {
        Tracker.Instance.Interrupt((int)egEvent.Type.CustomEvent, "GameEnd");
        Tracker.Instance.StopTracking();
        NetworkClientConnect.Instance.Disconnect(); // this will disconnect form the avatar server! remember to disconnect each time you change the time scale or you change scene
    }
}
