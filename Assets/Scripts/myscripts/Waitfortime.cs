using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Waitfortime : MonoBehaviour {


	// Use this for initialization
	void Start () {
        StartCoroutine(Test());
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator Test()
    {
        yield return new WaitForSeconds(2);
        Application.LoadLevel("eag_MainMenu");
    }

}
