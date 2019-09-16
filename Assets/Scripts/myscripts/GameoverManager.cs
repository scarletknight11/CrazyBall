using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using enableGame;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using FullSerializer;


public class GameoverManager : MonoBehaviour {

	/// <summary>
	/// Gameover manager class. 
	/// 
	/// </summary>
	
	public GameObject scoreText;					//gameobject which shows the score on screen
	public GameObject bestScoreText;                //gameobject which shows the best saved score on screen

    public AudioClip menuTap;						//touch sound

	private bool canTap;							//prevents double click on buttons
	private float buttonAnimationSpeed = 9.0f;      //button scale animation speed

    [SerializeField] private TextMesh uiText;
    [SerializeField] private float mainTimer;

    private float timer = 5;
    private bool canCount = true;
    private bool doOnce = false;




    void Awake () {
		canTap = true;
		//Set the new score on the screen
		scoreText.GetComponent<TextMesh>().text = LifeManager.playerScore.ToString();
		bestScoreText.GetComponent<TextMesh>().text = PlayerPrefs.GetInt("bestScore").ToString();
        //Sets the countdown after game is over
        timer = mainTimer;
        canCount = true;
        doOnce = false;
    }


	void Update () {
        if (canTap)
            StartCoroutine(tapManager());
    }


    ///***********************************************************************
    /// Manage user taps on gameover buttons
    ///***********************************************************************
    private RaycastHit hitInfo;
    private Ray ray;
    IEnumerator tapManager() {

        yield return new WaitForSeconds(3);

        if (timer <= 0.0f && !doOnce)
        {
            canCount = false;
            doOnce = true;
            uiText.text = "5";
            uiText.text = "4";
            uiText.text = "3";
            uiText.text = "2";
            uiText.text = "1";
            uiText.text = "0";
            saveScore();
            SceneManager.LoadScene("Game");
            Tracker.Instance.Interrupt((int)egEvent.Type.CustomEvent, "GameEnd");
            Tracker.Instance.StopTracking();
            NetworkClientConnect.Instance.Disconnect(); // this will disconnect form the avatar server! remember to disconnect each time you change the time scale or you change scene
            Tracker.Instance.StopTracking();
        }
        while (timer >= 0.0f && canCount)
        {

            timer -= Time.realtimeSinceStartup;
            uiText.text = timer.ToString("F");

            if (Time.realtimeSinceStartup <= 0.0f && !doOnce)
            {
                canCount = false;
                doOnce = true;
                uiText.text = "0.00";
                timer = 0.0f;
            }
        }

        //Mouse of touch?
        if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)
            ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
        else if (Input.GetMouseButtonUp(0))
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
            yield break;

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject objectHit = hitInfo.transform.gameObject;
            switch (objectHit.name)
            {
                case "retryButton":
                    playSfx(menuTap);                                   //play audioclip
                    saveScore();                                        //save players best and last score
                    StartCoroutine(animateButton(objectHit));           //animate the button
                    yield return new WaitForSeconds(0.4f);              //Wait
                    //Application.LoadLevel(Application.loadedLevelName); 
                    Tracker.Instance.Interrupt((int)egEvent.Type.CustomEvent, "GameEnd");
                    Tracker.Instance.StopTracking();
                    NetworkClientConnect.Instance.Disconnect(); // this will disconnect form the avatar server! remember to disconnect each time you change the time scale or you change scene
                    Tracker.Instance.StopTracking();
                    Application.LoadLevel(3);
                   //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;
                case "menuButton":
                    playSfx(menuTap);
                    saveScore();
                    StartCoroutine(animateButton(objectHit));
                    yield return new WaitForSeconds(0.4f);
                    Tracker.Instance.Interrupt((int)egEvent.Type.CustomEvent, "GameEnd");
                    Tracker.Instance.StopTracking();
                    NetworkClientConnect.Instance.Disconnect(); // this will disconnect form the avatar server! remember to disconnect each time you change the time scale or you change scene
                    Tracker.Instance.StopTracking();
                    Application.LoadLevel(2);
                    //SceneManager.LoadScene("MainMenu Scene");
                    break;
            }
        }
    }


    ///***********************************************************************
    /// Save player score
    ///***********************************************************************
    void saveScore() {
		//immediately save the last score
		PlayerPrefs.SetInt("lastScore", LifeManager.playerScore);
		//check if this new score is higher than saved bestScore.
		//if so, save this new score into playerPrefs. otherwise keep the last bestScore intact.
		int lastBestScore;
		lastBestScore = PlayerPrefs.GetInt("bestScore");
		if(LifeManager.playerScore > lastBestScore)
			PlayerPrefs.SetInt("bestScore", LifeManager.playerScore);
	}


    ///***********************************************************************
    /// Animate buttons on touch
    ///***********************************************************************
    IEnumerator animateButton(GameObject _btn)
    {
        canTap = false;
        Vector3 startingScale = _btn.transform.localScale;
        Vector3 destinationScale = startingScale * 1.1f;

        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime * buttonAnimationSpeed;
            _btn.transform.localScale = new Vector3(Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
                                                    _btn.transform.localScale.y,
                                                    Mathf.SmoothStep(startingScale.z, destinationScale.z, t));
            yield return 0;
        }

        float r = 0.0f;
        if (_btn.transform.localScale.x >= destinationScale.x)
        {
            while (r <= 1.0f)
            {
                r += Time.deltaTime * buttonAnimationSpeed;
                _btn.transform.localScale = new Vector3(Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
                                                        _btn.transform.localScale.y,
                                                        Mathf.SmoothStep(destinationScale.z, startingScale.z, r));
                yield return 0;
            }
        }

        if (r >= 1)
            canTap = true;
    }
   

    ///***********************************************************************
    /// IPlay audioclip
    ///***********************************************************************
    void playSfx(AudioClip _sfx) {
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}
}
