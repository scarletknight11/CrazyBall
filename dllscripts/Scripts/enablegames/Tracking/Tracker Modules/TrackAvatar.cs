using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using FullSerializer;

public class TrackAvatar : TrackerComponent
{
    private GameObject bodyPrefab;
    public AvatarSkeleton avatarSkeleton;

    // Use this for initialization
    void Awake()
    {
        /* switch (PlayerPrefs.GetString("TrackAvatar"))
        {
            case "onTick":
                Tracker.Instance.AddTickModule(new TrackerModule("Avatar Skeleton", skeletonData));
                break;
            case "onEvent":
                TrackModuleOnEvent(new TrackerModule("Avatar Skeleton", skeletonData));
                break;
            default:
                TrackModuleOnEvent(new TrackerModule("Avatar Skeleton", skeletonData));
                break;
        }*/
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
            else
            {
                print("TrackAvatar: Skeleton found.");
            }
            //          bodyPrefab = avatarSkeleton.gameObject;
        }
        //      int playerIndex = avatarSkeleton.playerIndex;
        Tracker.Instance.AddTickModule(new TrackerModule("Avatar Skeleton", skeletonData));
    }
    /*    string jointString(KinectInterop.JointType j, Vector3 position, Quaternion rotation)
        {
            print(string.Format("{0} : [ Position . {1} ] [ Rotation . {2} ]", j.ToString(), position.ToString(), rotation.ToString()));
            return string.Format("{0} : [ Position . {1} ] [ Rotation . {2} ]", j.ToString(), position.ToString(), rotation.ToString());
        }
    */

    //    Array allJoints = Enum.GetValues(typeof(KinectInterop.JointType));
    public EnableSerializableValue skeletonData()
    {
        //        EnableBody ebody = new EnableBody();
        //        return ebody;
        //print("TrackAvatar:skeletonData");
        EnableBody ebody = avatarSkeleton.GetSkeletonData(true);
        //print("TrackAvatar:GetSkeletonData returned: " + ebody);
        return ebody;
    }
}
