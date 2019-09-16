using UnityEngine;
using System.Collections;
using enableGame;
using UnityEngine.Networking;


public class GlobalObjectMover : MonoBehaviour
{

    /// <summary>
    /// This class moves the mazes down and destroys them when they pass a certain threshold
    /// </summary>

    egFloat speed = 0.5f; //movement speed

    float destroyThreshold = -10.0f;

    void Awake()
    {
        VariableHandler.Instance.Register(CBParameterStrings.ENEMY_AI, speed);  
    }

    void FixedUpdate()
    {
        transform.position -= new Vector3(0, 0, Time.deltaTime * speed);

        //Destroy it if it's out of screen view
        if (transform.position.z < destroyThreshold)
         modifyLevelDifficulty();
    }

    void modifyLevelDifficulty()
    {
       //increase difficulty by increasing movement speed
       speed++;   
    }
}
