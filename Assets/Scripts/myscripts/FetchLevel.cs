using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FetchLevel : MonoBehaviour {

	/// <summary>
	/// Get player progress.
	/// </summary>

	void Awake () {
        Text t = GetComponent<Text>();
 
        if (t != null)
            t.text = ((int)PlayerPrefs.GetInt("playerProgress", 1)).ToString();

    }

}
