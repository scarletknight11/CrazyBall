using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControlScript : MonoBehaviour {

    public GameObject live;
    public static int health;

	// Use this for initialization
	void Start () {
        health = 1;
        live.gameObject.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
