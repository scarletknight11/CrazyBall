using UnityEngine;
using System.Collections;

public class SessionReader : MonoBehaviour {
	public TextAsset jsonFile;

	private Session sess;

	public Session GetSession() {
		if(sess == null) {
			sess = JSONSerializer.Deserialize(typeof(Session),jsonFile.text) as Session;
		}
		return sess;
	}
}
