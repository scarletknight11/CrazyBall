using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using enableGame;
using UnityEngine.Networking; 


public class cloneobjects : MonoBehaviour {

    public egFloat cloneInterval = 3.0f;         //clone maze and enenmyball every N seconds
    private Vector3 startPoint;             //starting point of the clones object

    //maze & enemyball creation flag
    private bool createMaze;                //can we clone a new maze?
    private bool createEnemyBall;           //can we clone a new enemyball?

     
    //maze types
    public GameObject[] maze;				//Array of all available mazes

    // Use this for initialization
    void Start () {
        VariableHandler.Instance.Register(CBParameterStrings.CLONE, cloneInterval);
        createMaze = true;          //allow maze creation
        createEnemyBall = true;     //allow enemyball creation

    }


    // Update is called once per frame
    void Update () {
        //if we are allowed to spawn a maze
        if (createMaze) { 
            cloneMaze();
       } //else {
        	//if we are allowed to spawn enemyBall
        	//if(createEnemyBall)
              //  cloneEnemyBall();
          //  }
    }

    public void cloneMaze()
    {
        createMaze = false;
        startPoint = new Vector3(Random.Range(-1.0f, 1.0f), 0.5f, 7);
        Instantiate(maze[Random.Range(0, maze.Length)], startPoint, Quaternion.Euler(new Vector3(0, 0, 0)));
        StartCoroutine(reactiveMazeCreation());
    }

    ////enable this controller to be able to clone maze objects again
    IEnumerator reactiveMazeCreation()
    {
        yield return new WaitForSeconds(cloneInterval);
        createMaze = true;
    }
}
