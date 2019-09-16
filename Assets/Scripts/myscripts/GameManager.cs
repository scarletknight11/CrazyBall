using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {

    //public static GameManager gm;

    [Tooltip("If not set, the player will default to the gameObject tagged as Player.")]
    public GameObject player;

    public bool canBeatLevel = false;

    //Gamevver state
    public static bool gameOver;            //Gameover plane object
    private bool gameOverFlag;              //Run the gameover sequence just once


    //public GameObject mainCanvas;
    //public Text mainScoreDisplay;
    public GameObject gameOverCanvas;
    //public Text gameOverScoreDisplay;

    //[Tooltip("Only need to set if canBeatLevel is set to true.")]
    //public GameObject beatLevelCanvas;

    void Awake()
    {
        gameOverCanvas.SetActive(false);
        gameOver = false;
        gameOverFlag = false;
    }


    //void Start() {
    //    if (gm == null)
    //        gm = gameObject.GetComponent<GameManager>();

    //    if (player == null) {
    //        player = GameObject.FindWithTag("Player");
    //    }

    //    // make other UI inactive
    //    gameOverCanvas.SetActive(false);
    //    if (canBeatLevel)
    //        beatLevelCanvas.SetActive(false);
    //}

    void Update()
    {
        if (gameOver)
        {
            if (!gameOverFlag)
            {
                gameOverFlag = true;
                //show gameover menu
                processGameover();
            }
            return;
        }
    }

    void processGameover()
    {
        gameOverCanvas.SetActive(true);
    }
}
