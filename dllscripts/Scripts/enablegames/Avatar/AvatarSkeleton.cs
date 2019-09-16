#region copyright
/*
* Copyright (C) 2017 EnAble Games LLC - All Rights Reserved
* Unauthorized copying of these files, via any medium is strictly prohibited
* Proprietary and confidential
* fullserializer by jacobdufault is provided under the MIT license.
*/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using FullSerializer;
using DG.Tweening;

public class AvatarSkeleton : MonoBehaviour
{
    public bool globalData = true;
    public Animator animatorComponent; ///if null, gets component from GameObject 
    public class BoneInfo
    {
        public Transform m_bone;
        public Vector3 m_TargetSyncPosition;
        public Quaternion m_TargetSyncRotation3D;
    }
    private BoneInfo[] m_BoneInfos;
    private Transform[] m_Bones;
    private int numBones;
    private bool firstPositionSet = false;
    private Vector3 firstPosition;

    // Avatar's offset/parent object that may be used to rotate and position the model in space.
    public GameObject offsetNode;

    // Variable to hold all them bones. It will initialize the same size as initialRotations.
    protected Transform[] bones;
    protected string[] boneNames;

    // Rotations of the bones when in T-Pose
    protected Quaternion[] initialRotations;
    protected Quaternion[] initialLocalRotations;
    // Initial position and rotation of the body
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

    //OLD STUFF
    //    Transform m_Target;
    //    public Transform target { get { return m_Target; } set { m_Target = value;  } }
    //   int m_SyncLevel = 0;
    //    private GameObject bodyPrefab;
    //    private AvatarController avatarController;
    //    The body root node
    //    protected Transform bodyRoot;

    public static void GetAvatarSkeleton(GameObject mygameObject, ref AvatarSkeleton avatarSkeleton)
    {
        if (avatarSkeleton == null)
        {
            //            avatarSkeleton = GameObject.FindObjectOfType<AvatarSkeleton>();
            avatarSkeleton = mygameObject.GetComponent<AvatarSkeleton>();
            if (avatarSkeleton == null)
            {
                print("AvatarSkeleton: No avatar Skeleton component.  Attempting to add.");
                avatarSkeleton = mygameObject.AddComponent<AvatarSkeleton>() as AvatarSkeleton;
            }
            if (avatarSkeleton == null)
            {
                print("TrackAvatar: No avatar Skeleton could be added");
            }
            else
            {
                print("TrackAvatar: Skeleton found.");
            }
        }
    }

    /// Use this for initialization
    void Awake()
    {
        {
            m_Bones = BuildBonesFromAvatar().ToArray();
            //print("TA:Afer Build. m_Bones=" + m_Bones.Length);
            /*
            for (int i = 0; i < m_Bones.Length; i++)
            {
                var bone = m_Bones[i];
                //if (bone)
                //    print("builtbones[" + i + "]=" + bone.name);
            }
            */
            //globalData = true;
        }
        /*
        m_BoneInfos = new BoneInfo[m_Bones.Length];
        for (int i = 0; i < m_Bones.Length; i++)
        {
            m_BoneInfos[i] = new BoneInfo();
            m_BoneInfos[i].m_bone = m_Bones[i];
        }
        */
        // cache these to avoid per-frame allocations.
        numBones = m_Bones.Length;

        //headAdjust = new Transform();
    }

    // dictionary for looking up which avatar bones we are interested in.  For now, we are only interested in the ones that kinect provides.
    private Dictionary<string, int> boneName2BoneIndex = new Dictionary<string, int>
    {

    };

    /// If the bones to be mapped have been declared, map that bone to the model.
    protected virtual void MapBones()
    {
        /*
        Debug.Log("map bones 1");
        // make OffsetNode as a parent of model transform.
        offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
        offsetNode.transform.position = transform.position;
        offsetNode.transform.rotation = transform.rotation;
        offsetNode.transform.parent = transform.parent;
        offsetNode.transform.localScale = Vector3.one;

        // take model transform as body root
        transform.parent = offsetNode.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        bodyRoot = transform;
        */

        if (animatorComponent == null)
            animatorComponent = GetComponent<Animator>();
        //Debug.Log("map bones 3, bl=" + bones.Length);
        // get bone transforms from the animator component
        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            //if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
            //   continue;
            HumanBodyBones bi = boneIndex2MecanimMap[boneIndex];
            //print("BoneIndex2Mecanim[" + boneIndex + "]=" + bi.ToString());
            boneName2BoneIndex.Add(bi.ToString(), boneIndex);    //make map so we can use JSON name to find avatar bone index

            bones[boneIndex] = animatorComponent.GetBoneTransform(bi);
            boneNames[boneIndex] = bi.ToString();
            /* 
            if (bones[boneIndex] != null)
                Debug.Log("TA=======================add bone[" + boneIndex + "][bi=" + boneNames[boneIndex] + "]=" + bones[boneIndex].name);
            else
                Debug.Log("TA=======================NULL:" + boneIndex);
            */
        }
    }

    public string boneIndex2Name(int boneIndex)
    {
        HumanBodyBones bi = boneIndex2MecanimMap[boneIndex];
        //print("BoneIndex2Mecanim[" + boneIndex + "]=" + bi.ToString());
        return bi.ToString();
    }

    public Transform boneIndex2Transform(int boneIndex)
    {
        HumanBodyBones bi = boneIndex2MecanimMap[boneIndex];
        //print("BoneIndex2Mecanim[" + boneIndex + "]=" + bi.ToString());
        return animatorComponent.GetBoneTransform(bi);
    }

    public int boneIndexCount()
    {
        return boneIndex2MecanimMap.Count;
    }

    // dictionary for looking up which avatar bones we are interested in.  For now, we are only interested in the ones that kinect provides.
    private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
    {
        {0, HumanBodyBones.Hips},
        {1, HumanBodyBones.Spine},
                {2, HumanBodyBones.Chest},
        {3, HumanBodyBones.Neck},
        {4, HumanBodyBones.Head},

        {5, HumanBodyBones.LeftUpperArm},
        {6, HumanBodyBones.LeftLowerArm},
        {7, HumanBodyBones.LeftHand},
        {8, HumanBodyBones.LeftIndexProximal},
        {9, HumanBodyBones.LeftIndexIntermediate},
        {10, HumanBodyBones.LeftThumbProximal},

        {11, HumanBodyBones.RightUpperArm},
        {12, HumanBodyBones.RightLowerArm},
        {13, HumanBodyBones.RightHand},
        {14, HumanBodyBones.RightIndexProximal},
        {15, HumanBodyBones.RightIndexIntermediate},
        {16, HumanBodyBones.RightThumbProximal},

        {17, HumanBodyBones.LeftUpperLeg},
        {18, HumanBodyBones.LeftLowerLeg},
        {19, HumanBodyBones.LeftFoot},
        {20, HumanBodyBones.LeftToes},

        {21, HumanBodyBones.RightUpperLeg},
        {22, HumanBodyBones.RightLowerLeg},
        {23, HumanBodyBones.RightFoot},
        {24, HumanBodyBones.RightToes},

        {25, HumanBodyBones.LeftShoulder},
        {26, HumanBodyBones.RightShoulder},
    };



    /// <summary>
    /// Capture the initial rotations of the bones
    /// </summary>
    protected void GetInitialRotations()
    {
        // save the initial rotation
        if (offsetNode != null)
        {
            initialPosition = offsetNode.transform.position;
            initialRotation = offsetNode.transform.rotation;

            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;


            transform.rotation = Quaternion.identity;
        }

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                initialRotations[i] = bones[i].rotation; // * Quaternion.Inverse(initialRotation);
                initialLocalRotations[i] = bones[i].localRotation;
            }
        }

        // Restore the initial rotation
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }



    /// <summary>
    /// Builds bones list (Transforms) from the humanoid avatar representation.
    /// Because this code was merged in, bones is an array while bonesl is a list.  
    /// This should be cleaned up to only use one data representation.
    /// </summary>
    /// <returns></returns>
    List<Transform> BuildBonesFromAvatar()
    {
        //Debug.Log("AvatarSkeleton:BuildBonesFromAvatar");
        // check for double start
        if (bones != null)
            return null;

        // inits the bones array
        bones = new Transform[27];
        boneNames = new string[27];

        // Initial rotations and directions of the bones.
        initialRotations = new Quaternion[bones.Length];
        initialLocalRotations = new Quaternion[bones.Length];

        // Map bones to the points the Kinect tracks
        //Debug.Log("BuildBonesFromAvatar:MapBones");
        MapBones();
        //Debug.Log("BuildBonesFromAvatar:Bones Mapped.");

        // Get initial bone rotations
        GetInitialRotations();
        //Debug.Log("BuildBonesFromAvatar:InitialRotations Set");


        List<Transform> bonesl = new List<Transform>();
        //            bonesl.Add(root);
        //Debug.Log("BuildBonesFromAvatar:Number of Bones=" + bones.Length);
        for (int i = 0; i < bones.Length; i++)

        {
            bonesl.Add(bones[i]);
        }

        return bonesl;
    }

    /// <summary>
    /// Gets avatar's joint positions and orientations relative to initial T-Pose.
    /// Needs to cache ebody so only calculated if needed.
    /// </summary>
    /// <param name="global">Only global is currently supported</param>
    /// <returns></returns>
    public EnableBody GetSkeletonData(bool global)
    {
        //Debug.Log("AvatarSkeleton:GetSkeletonData");
        EnableBody ebody = new EnableBody();
        for (int i = 0; i < bones.Length; i++)
        {
            var bone = bones[i];
            if (!bone)
                continue;
            // position
            Vector3 jointPosition;
            if (globalData)
                jointPosition = bone.position;
            else
                jointPosition = bone.localPosition;

            //            writer.Write(jointPosition);
            Quaternion jointRotation;

            if (global)
            {
                jointRotation = bone.rotation * Quaternion.Inverse(initialRotations[i]);
                //                jointRotation = Quaternion.Inverse(initialRotations[i]) * bone.rotation;
            }
            else
            {
                jointRotation = bone.localRotation * Quaternion.Inverse(initialLocalRotations[i]);
                //intRotation = Quaternion.Inverse(initialLocalRotations[i]) * bone.localRotation;
            }

            //var name = boneNames[i];
            //var jointName = name;
            //Debug.Log("avatar bone [" + i + "]=" + name);
            /*
            if (name == "aLeftShoulder")
            {
                print("GET ROTATIONS[" + jointName + "]bi=" + name);
                print("GET ROTATIONS[" + jointName + "]lrot=" + bone.localRotation.eulerAngles);
                print("GET ROTATIONS[" + jointName + "]init=" + initialLocalRotations[i].eulerAngles);
                print("GET ROTATIONS[" + jointName + "]targ= " + jointRotation.eulerAngles);
                print("GET ROTATIONS[" + jointName + "]trans=" + bone.name);
            }
            */
            //            Debug.Log("AvatarSkeleton:bn=" + boneNames[i] + ",jp=" +  jointPosition + ", jr=" + jointRotation);

            ebody.AddJoint(boneNames[i], jointPosition, jointRotation);
        }
        //        Debug.Log("AvatarSkeleton:returning SkeletonData");
        return ebody;
    }



    //    public void SetFromBody(EnableBody body, ReplayEventData data, float speed = 1.0f, bool usePositionData = false)
    /// <summary>
    /// Sets the Avatar's joints using EnableBody as relative-to-Tpose rotation data.
    /// </summary>
    /// <param name="body">T-pose offset rotation data</param>
    /// <param name="duration">How quickly to get to final pose</param>
    /// <param name="speed">Multiplier to duration</param>
    /// <param name="usePositionData">Whether to use or ignore joint positions (and only use orientations)</param>
    public void SetFromBody(EnableBody body, float duration, float speed = 1.0f, bool usePositionData = false, bool mirror = false)
    {
        Dictionary<string, EnableJoint> joints = body.Joints;
        //estimatedRootOffset = newEstimatedRootOffset;
        //print("SetFromBody: duration=" + duration + ", speed = " + speed);
        foreach (string jointName in joints.Keys)
        {
            //print ("======================SetFromBody jointName = " + jointName);
            //Transform bp = bodyDict[jointName];
            /*
                        Transform bp;
                        //lookup if JSON data is one of the joints we are tracking
                        if (bodyDict.TryGetValue(jointName, out bp))
                        {
                        }
                        else
                        {
                            continue;
                        }
                        */
            string jn = jointName;
            //print("jn=" + jn);
            if (mirror == true)
            {
                if (jointName.Contains("Left"))
                {
                    //print("replacing " + jn);
                    jn = jointName.Replace("Left", "Right");
                }
                else if (jointName.Contains("Right"))
                {
                    jn = jointName.Replace("Right", "Left");
                }
            }


            EnableJoint j = joints[jn];
            //if (j.orientation == null) {
            //	print ("NULL+++++++++++++++++++++");
            //}
            JointSet(j, jointName, duration / speed, usePositionData, mirror);
        }
        /*
		footPositionCurrent = (leftFootPosition.y < rightFootPosition.y ? leftFootPosition : rightFootPosition);
		leftFootDown = (leftFootPosition.y < rightFootPosition.y);
		int footDown = (leftFootDown ? 1 : 0);
		if (lastFootDown != footDown) {  //switched feet
			footPositionInit = (leftFootDown ? leftFootPosition : rightFootPosition) ;
			//print ("switched feet, leftdown=" + leftFootDown + ", l=" + leftFootPosition + ", r=" + rightFootPosition);
			if (lastFootDown == -1) {  //get offset to move foot position to startPosition
				startOffset = startPosition - footPositionInit;
			}
			lastFootDown = footDown;
		}
		newEstimatedRootOffset = footPositionCurrent - footPositionInit ; //move body 
        */
    }

    /// <summary>
    /// Used to kill any tweens still going on (i.e. if speed is too fast for previous tween to complete or sample rate was too high)
    /// </summary>
    /// <param name="bp"></param>
    void tweenDone(Transform bp)
    {
        //   bp.DOKill();
        //print("tween done! bp="+ bp.name + ",rot=" + bp.rotation);
        //bp.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        /*
        if (bp.name == "mixamorig:RightShoulder")
        {
            for (int i= 0;i<1000; i++)
            {
                print("tween loop "+i);
            }
        }
        */
    }


    /// <summary>
    /// Set the Avatar's joint position and orientation
    /// </summary>
    /// <param name="aJoint">values to set the joint to</param>
    /// <param name="jointName">name of the avatar joint to set</param>
    /// <param name="duration">how long it takes joint to interpolate to new position</param>
    /// <param name="usePositionData">whether to set joints to positons (scale skeleton to bone lenghts)</param>
	void JointSet(EnableJoint aJoint, string jointName, float duration, bool usePositionData, bool mirror)
    {
        //        Transform bp = bodyDict[jointName];
        Quaternion targetRotation, offsetNodeRotation;
        offsetNodeRotation = Quaternion.identity;
        int boneInd;
        //        int boneInd = boneName2BoneIndex[jointName];
        if (!boneName2BoneIndex.TryGetValue(jointName, out boneInd))
        {
            print("JointSet : " + jointName + " not found!");

        }
        //print("JointSet : " + jointName + ", boneInd: " + boneInd);
        //       print("JointSet id= " + boneInd);
        Transform bp = bones[boneInd];  //bone on avatar to set
        if (bp == null)
        {
            //print("bone NULL: " + jointName);
            return;
        }
        // Kinect2AvatarRot(aJoint.orientation, bp);
        //bp.rotation = Quaternion.Slerp(bp.rotation, targetRotation, smoothFactor * Time.deltaTime);


        //    Quaternion targetRotation = bp.rotation;
        //    var boneInfo = m_BoneInfos[i];
        if (offsetNode != null)
        {
            offsetNodeRotation = offsetNode.transform.rotation;
        }
        var rot = aJoint.orientation;  //global orientation
        if (mirror == true)
        {
            //flip quaternion
            rot.y = -rot.y;
            rot.z = -rot.z;
            //			rot.y = -rot.y;
            //			rot.x = -rot.x;
            //			rot.x = -rot.x;
            //			rot.z = -rot.z;
        }

        rot = offsetNodeRotation * rot;  //since we set global rotations, all parent transforms are ignored unless offsetNode set

        if (globalData)
        {
            targetRotation = rot * initialRotations[boneInd];
            if (jointName == "aLeftShoulder")
            {
                print("SET ROTATIONS[" + jointName + "]bi=" + boneInd.ToString());
                print("SET ROTATIONS[" + jointName + "]targ=" + rot.eulerAngles);
                print("SET ROTATIONS[" + jointName + "]init=" + initialLocalRotations[boneInd].eulerAngles);
                print("SET ROTATIONS[" + jointName + "]Ltarg= " + targetRotation.eulerAngles);
                print("SET ROTATIONS[" + jointName + "]trans=" + bp.name);
            }

        }
        else
        {
            //targetRotation = rot * initialLocalRotations[boneInd]; 
            Quaternion iq2 = initialLocalRotations[boneInd];
            Quaternion lq2 = Quaternion.Inverse(iq2) * rot * iq2;
            targetRotation = iq2 * lq2;
            /*
            if (jointName == "aLeftShoulder")
            {
                print("SET LOCAL ROTATIONS[" + jointName + "]bi=" + boneInd.ToString());
                print("SET LOCAL ROTATIONS[" + jointName + "]targ=" + rot.eulerAngles);
                print("SET LOCAL ROTATIONS[" + jointName + "]init=" + initialLocalRotations[boneInd].eulerAngles);
                print("SET LOCAL ROTATIONS[" + jointName + "]Ltarg= " + targetRotation.eulerAngles);
                print("SET LOCAL ROTATIONS[" + jointName + "]trans=" + bp.name);
            }*/
        }

        //        bp.localPosition = Vector3.Lerp(bp.localPosition, targetPosition , m_InterpolateMovement);
        //        bp.localRotation = Quaternion.Slerp(bp.localRotation, targetRotation, m_InterpolateRotation);
        if (globalData)
        {

            Vector3 ang = targetRotation.eulerAngles;
            float duration2 = duration * 1.5f; //hack... For some reason, tween is stopping well before next one starts without this hack
                                               //print("duration2="+duration2);
                                               /*
                                               if (jointName == "LeftShoulder")
                                               {
                                                   print("DORotate: ang=" + ang + ", duration = " + duration2);
                                                   print("SET ROTATIONS[" + jointName + "]bi=" + boneInd.ToString());
                                                   print("SET ROTATIONS[" + jointName + "]targ=" + rot.eulerAngles);
                                                   print("SET ROTATIONS[" + jointName + "]init=" + initialLocalRotations[boneInd].eulerAngles);
                                                   print("SET ROTATIONS[" + jointName + "]Ltarg= " + targetRotation.eulerAngles);
                                                   print("SET ROTATIONS[" + jointName + "]trans=" + bp.name);
                                               }*/
                                               //bp.transform.rotation = targetRotation;
                                               //            bp.DORotate(ang, duration2).OnComplete(tweenDone);
            bp.DOKill();  //kill any tweens still going (in case speed was increased or we jumped in time)
            bp.DORotate(ang, duration2).OnComplete(() => tweenDone(bp));
        }
        else
        {
            float m_InterpolateRotation = 0.5f;
            bp.localRotation = Quaternion.Slerp(bp.localRotation, targetRotation, m_InterpolateRotation);
        }
        Vector3 targetPosition, offsetNodePosition, offsetNodeScale;
        offsetNodePosition = Vector3.zero;
        if (offsetNode != null)
        {
            offsetNodePosition = offsetNode.transform.position;
            offsetNodeScale = offsetNode.transform.localScale;
        }
        //bp.DOLocalMove(Kinect2AvatarPos(aJoint.position, rootToGround), duration);
        if (usePositionData || (jointName == "Hips"))
        {  //move body parts in global space if flag is set
            if (jointName == "Chest")
            {
                return;
            }
            if (false) // PJD  Check if ok to put this back!!! usePositionData)
            {
                targetPosition = aJoint.position + offsetNodePosition;
            }
            else
            {
                //firstPosition is a hack if no offset node is set for the figure not to move globally from where it is initially placed.
                if (!firstPositionSet)
                {
                    if (offsetNode != null)
                    {
                        firstPosition = Vector3.zero;
                        //print("+++++++++++++++FPSet1");
                    }
                    else
                    {
                        firstPosition = aJoint.position - bp.position;// aJoint.position;
                        //print("+++++++++++++++FPSet2");
                    }
                    firstPositionSet = true;
                    //print("+++++++++++++++FPSet");
                }

                //                targetPosition = aJoint.position - firstPosition + offsetNodePosition.Scale(offsetNodeScale);
                targetPosition = aJoint.position - firstPosition;
                //print("targ pos1=" + targetPosition);
                targetPosition = offsetNode.transform.TransformPoint(targetPosition);
                //print("targ pos2=" + targetPosition);

            }
            if (mirror)
                targetPosition.x = -targetPosition.x;
            bp.DOMove(targetPosition, duration); //move relative to spine base
        }
    }
}
