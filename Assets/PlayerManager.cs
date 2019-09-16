using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using enableGame;

public class PlayerManager : MonoBehaviour {

    /// <summary>
    /// Main Player manager class.
    /// We check all player collisions here.
    /// We also calculate the score in this class. 
    /// </summary>

 
    //public float delayBeforeDestroy = 0.0f;
    //public static int playerScore;			//player score
	//public GameObject scoreTextDynamic;     //gameobject which shows the score on screen
    public GameObject explosionPrefab;
    private LifeManager lifesystem;

 


    void Awake() {
		//playerScore = 0;
        lifesystem = FindObjectOfType<LifeManager>();
        //Disable screen dimming on mobile devices
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
		//Player the game with a fixed framerate in all platforms
		Application.targetFrameRate = 60;

        //store initial position as respawn location
        //respawnPosition = transform.position;
        //respawnRotation = transform.rotation;

      }

    //Update is called once per frame
    void Update() {
        //lifesystem.Update();
   //     if (!GameController.gameOver)
			//calculateScore();
    }
	
	
	///***********************************************************************
	/// calculate player's score
	/// Score is a combination of gameplay duration (while player is still alive) 
	/// and a multiplier for the current level.
	///***********************************************************************
	public void calculateScore() {
		//if(!PauseManager.isPaused) {
			//playerScore += (int)( GameController.currentLevel * Mathf.Log(GameController.currentLevel + 1, 2) );
			//scoreTextDynamic.GetComponent<TextMesh>().text = playerScore.ToString();
		//}
	}


	///***********************************************************************
	/// Collision detection and management
	///***********************************************************************
	void OnCollisionEnter(Collision c) {

		//collision with mazes and enemyballs leads to a sudden gameover
		if(c.gameObject.tag == "Maze") {
			print ("Game Over");
            //GameController.gameOver = true;
            Instantiate(explosionPrefab, transform.position, transform.rotation);
            //transform.position = respawnPosition;   // reset the player to respawn position
            //transform.rotation = respawnRotation;
            lifesystem.TakeLife();
        }

        if (c.gameObject.tag == "enemyBall") {
			Destroy(c.gameObject);
		}
	}

	void playSfx(AudioClip _sfx) {
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}

   // public void updateRespawn(Vector3 newRespawnPosition, Quaternion newRespawnRotation) {
       // respawnPosition = newRespawnPosition;
        //respawnRotation = newRespawnRotation;
    //}
}
