using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestroy : MonoBehaviour {

    public GameObject explosionPrefab;

    private Vector3 respawnPosition;
    private Quaternion respawnRotation;


    // Use this for initialization
    void Start () {
        respawnPosition = transform.position;
        respawnRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update () {
        if (explosionPrefab != null)
        {
          Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            Destroy(gameObject);
        }

    }

    public void updateRespawn(Vector3 newRespawnPosition, Quaternion newRespawnRotation)
    {
        respawnPosition = newRespawnPosition;
        respawnRotation = newRespawnRotation;
    }

}
