using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using enableGame;
using UnityEngine.Networking;

public class PauseManager : MonoBehaviour {
	
	/// <summary>
	/// This class manages pause and unpause states.
	/// </summary> 


	public static bool isPaused;		//is game already paused?
	public GameObject pausePanel;       //we move this plane over all other elements in the game to simulate the pause
    public GameObject pauseButton;
    public GameObject retryButton;
    public GameObject NextButton;
    public GameObject MenuButton;
    public GameObject gameOverPanel;
    public Game gameManager;




    public enum Page {PLAY, PAUSE}
	private Page currentPage = Page.PLAY;
	
	
	void Awake() {		
		isPaused = false;
		Time.timeScale = 1.0f;
		if(pausePanel)
			pausePanel.SetActive(false); 
	}
	
	
	void Update() {

		//touch control
		touchManager();

        //optional pause
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            //PAUSE THE GAME
            switch (currentPage)
            {
                case Page.PLAY:
                    PauseGame();
                    break;
                case Page.PAUSE:
                    UnPauseGame();
                    break;
                default:
                    currentPage = Page.PLAY;
                    break;
            }
        }

        //debug restart
        if (Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	
	private RaycastHit hitInfo;
	private Ray ray;
	void touchManager() {
		
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonUp(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;
		
		if (Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			switch(objectHit.name) {
				case "pauseBtn":
					switch (currentPage) {
						case Page.PLAY: 
							PauseGame(); 
							break;
						case Page.PAUSE: 
							UnPauseGame(); 
							break;
						default: 
							currentPage = Page.PLAY; 
							break;
						}
					break;

                case "retryButtonPause":
                    UnPauseGame();
                    Tracker.Instance.Interrupt((int)egEvent.Type.CustomEvent, "GameEnd");
                    Tracker.Instance.StopTracking();
                    NetworkClientConnect.Instance.Disconnect();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;

                    //case "menuButtonPause":
                    //	UnPauseGame();
                    //                Tracker.Instance.Interrupt((int)egEvent.Type.CustomEvent, "GameEnd");
                    //                Tracker.Instance.StopTracking();
                    //                NetworkClientConnect.Instance.Disconnect();
                    //                SceneManager.LoadScene ("Menu");
                    //                break;
            }	
		}
	}
    public void autoplay()
    {
        UnPauseGame();
#if UNITY_ANDROID
        SceneManager.LoadScene("MainGame Scene_vr");
#else
        SceneManager.LoadScene("MainGame Scene_pc");
#endif
    }


    //public void autostart()
    //{
    //    playSfx(tapSfx);
    //    UnPauseGame();
    //    if (Skeleton != null)
    //    {
    //        Skeleton.moving = true;
    //    }
    //    targetPlane.SetActive(false);
    //}

    public void PauseGame()
    {
        print("Game is Paused...");
        isPaused = true;
        Time.timeScale = 0;
        AudioListener.volume = 0;
        if (pausePanel)
            pausePanel.SetActive(true);
        currentPage = Page.PAUSE;
    }

    public void UnPauseGame()
    {
        print("Unpause");
        isPaused = false;
        Time.timeScale = 1.0f;
        AudioListener.volume = 1.0f;
        if (pausePanel)
            pausePanel.SetActive(false);
        currentPage = Page.PLAY;
    }

    public void EndGame()
    {
        if (pausePanel)
            pausePanel.SetActive(false);
        if (gameOverPanel)
            gameOverPanel.SetActive(true);
        pauseButton.SetActive(false);
        gameManager.EndGame();
    }

    public void MainMenu()
    {
        gameManager.MainMenu();
    }

    public void ReloadGame()
    {
        gameManager.ReloadGame();
    }



}
