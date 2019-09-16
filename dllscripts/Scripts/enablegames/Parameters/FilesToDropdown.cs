using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FilesToDropdown : MonoBehaviour {

	List<string> fileNames = new List<string>();
	public GameObject dropObj;

	// Use this for initialization
	void Start () {
		GetFileNames ();
	}

	public void GetFileNames () {
		fileNames.Clear ();
		DirectoryInfo info = new DirectoryInfo(GameParameters.SettingsFolder);
		if (info.Exists == false) {
			info.Create();
		}

		foreach (string file in System.IO.Directory.GetFiles(GameParameters.SettingsFolder)) {
			fileNames.Add (sepFile (file));
		}
		fileNames = fileNames.Distinct ().ToList ();
		dropObj.GetComponent<Dropdown> ().ClearOptions ();
		dropObj.GetComponent<Dropdown>().AddOptions (fileNames);
	}

	string sepFile (string str) {
		int i;
		int f = str.IndexOf ("\\AbleSettings") + 14;
		str = str.Substring (f, str.Length - f);
		if (str.Contains (".Warmup")) {
			i = str.IndexOf (".Warmup");
		} else if (str.Contains (".Conditioning")) {
			i = str.IndexOf (".Conditioning");
		} else if (str.Contains (".Cooldown")) {
			i = str.IndexOf (".Cooldown");
		} else {
			return "";
		}
		if (i > 0) {
			return str.Substring (0, i);
		}
		return "";
	}
}
