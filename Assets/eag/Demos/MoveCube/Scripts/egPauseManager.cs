using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class egPauseManager : MonoBehaviour {

    public GameObject pauseButton;
    public GameObject retryButton;
    public GameObject NextButton;
    public GameObject MenuButton;
 

    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public Game gameManager;
    public static bool isPaused;        //is game already paused?

    /// <summary>
    /// This class manages pause and unpause states.
    /// </summary> 

    public enum Page {PLAY, PAUSE}
    private Page currentPage = Page.PLAY;

    void Awake()
    {
        isPaused = false;
        if (pausePanel)
            pausePanel.SetActive(false);
    }


    void Update()
    {

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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadGame();
        }
    }


    private RaycastHit hitInfo;
    private Ray ray;
    void touchManager()
    {

        //Mouse of touch?
        if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)
            ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
        else if (Input.GetMouseButtonUp(0))
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
            return;

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject objectHit = hitInfo.transform.gameObject;
            print("objectHit=" + objectHit.name);
           // switch (objectHit)//(objectHit.name)
            {
                if (objectHit == pauseButton)
            {//:// "pauseBtn":
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
                //break;
            }
            else
                if (objectHit == retryButton)
                {//:// "retryButtonPause":
                    ReloadGame();
                    //break;
                }
                else
                if (objectHit == MenuButton)
                { // "menuButtonPause":
                    MainMenu();
                    //break;
                }
               //case QuitButton: "retryButtonPause":
               // EndGame();
               // break;

            }
        }
    }

    public void PauseGame()
    {
        print("Game is Paused...");
        isPaused = true;
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
        AudioListener.volume = 0;
        if (pausePanel)
            pausePanel.SetActive(true);
        currentPage = Page.PAUSE;
    }

    public void UnPauseGame()
    {
        currentPage = Page.PLAY;
        gameManager.UnPauseGame();
        if (pausePanel)
            pausePanel.SetActive(false);
 
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

    public void NextGame()
    {
        gameManager.NextGame();
    }


}
