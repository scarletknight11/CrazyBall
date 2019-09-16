using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using FullSerializer;

public class KinecticBody : MonoBehaviour {

    // input data from the motion sensor
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;

    public bool UseKinectic;

    // Use this for initialization
    void Start() {
       // if (HelloKinect.Device == HelloKinect.InputDevice.Kinectic) {
            EnableAPI.Instance.NewBodyDataEvent += NewBodyDataEventHandler;
       // }
    }

    void NewBodyDataEventHandler(Dictionary<string, object> data) {
        leftHand.transform.localPosition = Vector3.Lerp(leftHand.transform.localPosition, ObjectToVector(data["HandLeft"]), 1f);
        rightHand.transform.localPosition = Vector3.Lerp(rightHand.transform.localPosition, ObjectToVector(data["HandRight"]), 1f);
        leftFoot.transform.localPosition = Vector3.Lerp(leftFoot.transform.localPosition, ObjectToVector(data["FootLeft"]), 1f);
        rightFoot.transform.localPosition = Vector3.Lerp(rightFoot.transform.localPosition, ObjectToVector(data["FootRight"]), 1f);
    }

    Vector3 ObjectToVector(object obj) {
        Vector3 ret = new Vector3();
        try {
            //IDictionary<string, object> pos = obj as IDictionary<string, object>;
            Dictionary<string, fsData> pos = obj as Dictionary<string, fsData>;
            if (null != pos) {
                object x = pos["x"].AsDouble;
                object y = pos["y"].AsDouble;
                ret.x = Convert.ToSingle(x);
                ret.y = Convert.ToSingle(y);
            }
        } catch (Exception e) {
            ret.x = 0f;
            ret.y = 0f;
        }
       // ret.z = Game.GameZ;
        return ret;
    }

}
