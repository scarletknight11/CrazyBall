using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using enableGame;
using UnityEngine.Networking;

public class TestingScaler : MonoBehaviour {

    Vector3 minScale;
    public Vector3 maxScale;
    //public bool repeatable;
    
    
    IEnumerator Start()
    {
        minScale = transform.localScale;
        //yield return RepeatLerp(maxScale);
        yield return null;
    }

    public IEnumerator RepeatLerp(Vector3 a, Vector3 b, float time)
    {
        float i = 0.0f;
        transform.localScale = Vector3.Lerp(a, b, i);
        yield return null;
    }

    
 
}
