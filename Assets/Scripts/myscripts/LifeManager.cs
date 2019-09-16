using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using enableGame;

public class LifeManager : MonoBehaviour {

    //public PlayerManager pm;
    public static int playerScore;          //player score
    public GameObject scoreTextDynamic;     //gameobject which shows the score on screen
    public egFloat lives = 1;
    public GameObject pause;
 



    //Text theText;
    //string theText;


    //Use this for initialization
    void Awake() {
        playerScore = 0;
        //theText = GetComponent<Text>();
        VariableHandler.Instance.Register(CBParameterStrings.HEALTH, lives);
        print("Health " + lives);
    }

    //Update is called once per frame
    void Update()
    {
        //theText.text = " " + startingLives; //"x " +
       if (!GameController.gameOver)
        if (lives > 0 && lives <= 3) {
            calculateScore();
        } else if(lives > 3 && lives <= 5) {
            calculateScore2();
        } else if (lives > 5 && lives <= 7) {
            calculateScore3();
        } else if (lives > 7 && lives <= 9) {
            calculateScore4();
        } else if (lives == 10) {
            calculateScore5();
        }
    }

    void calculateScore() {
        if (!PauseManager.isPaused) {
            playerScore += (int)(GameController.currentLevel * Mathf.Log(GameController.currentLevel + 10, 2));
            scoreTextDynamic.GetComponent<TextMesh>().text = playerScore.ToString();
        }
    }

    void calculateScore2() {
        if (!PauseManager.isPaused) {
            playerScore += (int)(GameController.currentLevel * Mathf.Log(GameController.currentLevel + 200, 300/lives));
            scoreTextDynamic.GetComponent<TextMesh>().text = playerScore.ToString();
        }
    }

    void calculateScore3() {
        if (!PauseManager.isPaused) {
            playerScore += (int)(GameController.currentLevel * Mathf.Log(GameController.currentLevel + 4000, 4000/(7/lives)));
            scoreTextDynamic.GetComponent<TextMesh>().text = playerScore.ToString();
        }
    }

    void calculateScore4() {
        if (!PauseManager.isPaused) {
            playerScore += (int)(GameController.currentLevel * Mathf.Log(GameController.currentLevel + 50000, 50000/(9/lives)));
            scoreTextDynamic.GetComponent<TextMesh>().text = playerScore.ToString();
        }
    }

    void calculateScore5() {
        if (!PauseManager.isPaused) {
            playerScore += (int)(GameController.currentLevel * Mathf.Log(GameController.currentLevel + 600000, 600000/(10/lives)));
            scoreTextDynamic.GetComponent<TextMesh>().text = playerScore.ToString();
        }
    }

    public void GiveLife() {
        lives++;
    }

    public void TakeLife() {
        lives--;
        if (lives <= 0) {
            pause.SetActive(false);
            lives = 0;
            GameController.gameOver = true;
         }
    }
}
