using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {
	public GameObject bulletPrefab;
	public Transform bulletSpawn;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {  
		if (!isLocalPlayer) {
//			print ("PC:Update:isLocal=" + isLocalPlayer);
			return;
		}
//		print ("PC:Update:isLocal=" + isLocalPlayer);
//		print ("player controller");
		float x = Input.GetAxis ("Horizontal") * Time.deltaTime * 150.0f;
		float z = Input.GetAxis ("Vertical") * Time.deltaTime * 3.0f;
		transform.Rotate (0, x, 0); 
		transform.Translate (0, 0, z);

		if (Input.GetKeyDown (KeyCode.Space)) {
			Fire ();
		}
	}

	void Fire()
	{
		//Create bullet from prefab
		GameObject bullet = (GameObject)Instantiate(bulletPrefab,bulletSpawn.position,bulletSpawn.rotation);

		//Add velocity
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6.0f;

		//Destroy after 2 secs
		Destroy(bullet,2);

	}

	public override void OnStartLocalPlayer()
	{
		GetComponent<MeshRenderer> ().material.color = Color.blue;
	}
}
