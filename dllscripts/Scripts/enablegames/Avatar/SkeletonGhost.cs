using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonGhost : MonoBehaviour {
    public Animator animatorComponent;      ///if null, gets component from GameObject 
    public AvatarSkeleton avatarSkeleton;   ///if null, gets component from GameObject 
    public AvatarSkeleton Ghost;    /// body that joint info is copied to
    bool usePositionData = false; /// whether to copy position data as well as orientations
    //public AvatarSkeleton avatarSkeletonGhost2;
    public bool moving = false;
    private bool initted = false;
	public bool mirror = false;
    // Use this for initialization
    void Start() {
        if (avatarSkeleton == null)
        {
            //            avatarSkeleton = GameObject.FindObjectOfType<AvatarSkeleton>();
            avatarSkeleton = gameObject.GetComponent<AvatarSkeleton>();
            if (avatarSkeleton == null)
            {
                print("TrackAvatar: No avatar Skeleton component.  Attempting to add.");
                avatarSkeleton = gameObject.AddComponent<AvatarSkeleton>() as AvatarSkeleton;
            }
            if (avatarSkeleton == null)
            {
                print("TrackAvatar: No avatar Skeleton");
                //                return new EnableString("Invalid tracking data. No avatar Skeleton");
            }
        }
        if (animatorComponent == null)
            animatorComponent = gameObject.GetComponent<Animator>();

        moving = false;
        initted = true;

    }

    /// <summary>
    /// </summary>
    void CopySkeletonData()
    {
        
        //print("getSkelData");
        EnableBody ebody = avatarSkeleton.GetSkeletonData(true);
        Dictionary<string, EnableJoint> joints = ebody.Joints;
        //estimatedRootOffset = newEstimatedRootOffset;

        float duration = 0.0f;
		float speed = 1.0f;//SessionReplay.timeScale;
        if (Ghost)
        {
//            print("\n--GHOST1--\n");
            Ghost.SetFromBody(ebody, duration, speed, usePositionData,mirror);
        }
    }

    void Update()
    {
       CopySkeletonData();
    }
}



