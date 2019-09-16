using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LiveManager : MonoBehaviour {

    public float healthPoints = 1f;
    public float respawnHealthPoints = 1f;      //base health points

    public int numberOfLives = 1;
    private LifeManager lifeSystem;

    // Use this for initialization
    void Start ()
    {
      lifeSystem = FindObjectOfType<LifeManager>();
    }

  //  void Update()
  //  {
        // if (healthPoints <= 0)
        //{              
        //    numberOfLives--;
        //}
   // }

    public void LiveDamage(float amount)
    {
        healthPoints = healthPoints - amount;
        lifeSystem.TakeLife();
    }
}
