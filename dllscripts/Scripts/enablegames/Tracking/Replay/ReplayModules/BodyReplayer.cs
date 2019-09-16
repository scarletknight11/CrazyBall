using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BodyReplayer : MonoBehaviour {
	public Transform SpineBase, SpineMid, Neck, Head, ShoulderLeft, ElbowLeft, WristLeft, HandLeft,
	ShoulderRight, ElbowRight, WristRight, HandRight, HipLeft, KneeLeft, AnkleLeft, FootLeft, HipRight,
	KneeRight, AnkleRight, FootRight, SpineShoulder, HandTipLeft, ThumbLeft, HandTipRight, ThumbRight;
	// Use this for initialization


	private Dictionary<string, Transform> bodyDict;

	private Transform[] bodyPoints;
	private string[] pointNames;
	void Start () {
		bodyPoints = new Transform[] {
			SpineBase, SpineMid, Neck, Head, ShoulderLeft, ElbowLeft, WristLeft, HandLeft,
			ShoulderRight, ElbowRight, WristRight, HandRight, HipLeft, KneeLeft, AnkleLeft, FootLeft, HipRight,
			KneeRight, AnkleRight, FootRight, SpineShoulder, HandTipLeft, ThumbLeft, HandTipRight, ThumbRight
		};
		pointNames = new string[] {
			"SpineBase","SpineMid","Neck","Head","ShoulderLeft","ElbowLeft","WristLeft","HandLeft",
			"ShoulderRight","ElbowRight","WristRight","HandRight","HipLeft","KneeLeft","AnkleLeft","FootLeft","HipRight",
			"KneeRight","AnkleRight","FootRight","SpineShoulder","HandTipLeft","ThumbLeft", "HandTipRight", "ThumbRight"
		};
		bodyDict = new Dictionary<string, Transform>();
		int i = 0;
		foreach(Transform t in bodyPoints) {
			bodyDict.Add(pointNames[i],t);
			//t.localPosition = Vector3.zero;
			//t.localRotation = new Quaternion(0f,1f,0f,0f);
			i++;
		}
		SessionReplay.OnReplayTransition += HandleOnReplayTransition;
	}

	void OnDestroy() {
		SessionReplay.OnReplayTransition -= HandleOnReplayTransition;
	}

	void HandleOnReplayTransition (ReplayEventData data) {
		TrackerMessage bodyMessage = SessionAnalyzer.LookupMessage(data.To.messages,"Player Skeleton");
		if(bodyMessage != null) {
			EnableBody body = bodyMessage.Value as EnableBody;
			SetFromBody(body, data);
		}
	}

	void SetFromBody(EnableBody body, ReplayEventData data) {
		Dictionary<string, EnableJoint> joints = body.Joints;
		foreach(string jointName in joints.Keys) {
			Transform bp = bodyDict[jointName];
			EnableJoint j = joints[jointName];
			bp.DOLocalRotate(j.orientation.eulerAngles, data.Duration);
			bp.DOMove(j.position, data.Duration);
		}
	}
}
