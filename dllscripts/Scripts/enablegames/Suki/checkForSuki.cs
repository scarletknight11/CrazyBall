using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class checkForSuki : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        string path = Path.Combine(Application.streamingAssetsPath, "Suki");

        bool b = Directory.Exists(path);
        if (b) {
            gameObject.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
