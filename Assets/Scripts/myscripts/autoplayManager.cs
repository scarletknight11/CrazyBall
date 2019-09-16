using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using enableGame;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
 

public class autoplayManager : MonoBehaviour {

   // public PauseManager pm;


   ////public Text finishPanelNumber;
   // public Text startingPanelNumber;

   // public GameObject finishPanelPlayBtn;
   // public GameObject startingPanelPlayBtn;
   // public GameObject gameOverPanel;


   // private egBool autoplay = new egBool();

   // int cd = 5;

   // bool countdown = false;

   // // Use this for initialization
   // void Awake()
   // {
   //     VariableHandler.Instance.Register(ParameterStrings.AUTOPLAY, autoplay);
   //     cd = 5;
   //     StartCoroutine(cooldown(startingPanelNumber));
   //    // GetComponent<Text>().text = ((int)PlayerPrefs.GetInt("playerProgress", 1)).ToString();
   // }


   // public void startingPanel()
   // {
   //     if (autoplay)
   //     {
   //         startingPanelPlayBtn.SetActive(false);
   //         cd = 5;
   //         StartCoroutine(cooldown(startingPanelNumber));
   //         //StartCoroutine(startAutoplay(1));
   //     }
   //     else
   //     {
   //         startingPanelNumber.gameObject.SetActive(false);
   //     }
   // }

   // public IEnumerator cooldown(Text tm)
   // {


   //     float start = Time.realtimeSinceStartup;
   //     int time = cd;
   //     while (Time.realtimeSinceStartup < start + cd)
   //     {
   //         countdown = true;

   //         tm.text = Mathf.Round(((start + cd) - Time.realtimeSinceStartup)).ToString();
   //         yield return null;
   //     }
   //     countdown = false;
   //     SceneManager.LoadScene("MainMenu Scene");
   // }
}
