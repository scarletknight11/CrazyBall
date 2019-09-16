#region copyright
/*
* Copyright (C) 2017 EnAble Games LLC - All Rights Reserved
* Unauthorized copying of these files, via any medium is strictly prohibited
* Proprietary and confidential
*/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Skeleton data.  Calculates and holds all skeleton data which is needed by suki, etc.
/// Calculates joint angles, min and max values,etc.
/// </summary>
public class SkeletonData : MonoBehaviour
{
    public Animator animatorComponent; ///if null, gets component from GameObject 
    public AvatarSkeleton avatarSkeleton;
	private bool moving = false;
	public bool Moving
	{
		get
		{
			return moving;
		}
		set  //no setting
		{
		}
	}
    private bool was_moving = false;
    public bool updateMinMax = false;
    public bool resetMinMax = false;



    //old general calculations
    private Vector2[] BoneNumDOF;  //The bone number in [0] (defined in AvatarSkeleton.cs) and how many degree of freedoms joint has in [1]
    private Vector3[] angles;     //array of angles
    private Vector3[] anglesMin;
    private Vector3[] anglesMax;

    private Vector3[] localAngles;

    //new calculations based on Arun's biomechanic defs
    public string[] boneNames;     //array of body part names (mecanim joint names)
    public Vector3[] bonePos;     //array of positions
    public Vector3[] bonePosMin;
    public Vector3[] bonePosMax;
    public string[] jointNames;     //array of body part names (mecanim joint names)
    public float[] jointAng;     //array of angles
    public float[] jointAngMin;
    public float[] jointAngMax;

    private Transform parent, child;
    private int randomBuffer;
    private bool initted = false;

    //    private Vector3[] jointDOF;
    //private string[] pointNames;

    private Dictionary<string, int> bodyDict;
    /*
	private Vector3 Neck;
	private Vector3 Head;
	private Vector3 L_Shoulder_Flexion;
	private Vector3 L_Shoulder_Abduction;
	private Vector3 L_Elbow_Flexion;
	private Vector3 R_Shoulder_Flexion;
	private Vector3 R_Shoulder_Abduction;
	private Vector3 R_Elbow_Flexion;
	private Vector3 L_Hip_Flexion;
	private Vector3 L_Hip_Abduction;
	private Vector3 L_Hip_Rotation;
	private Vector3 R_Hip_Flexion;
	private Vector3 R_Hip_Abduction;
	private Vector3 R_Hip_Rotation;
	private Vector3 R_Knee_Flexion;
	private Vector3 L_Knee_Flexion;
	private Vector3 Spine_Flexion;
	private Vector3 Spine_Rotation;
*/
    public static void GetSkeletonData(GameObject mygameObject, ref SkeletonData skeletonData)
    {
        if (skeletonData == null)
        {
            skeletonData = mygameObject.GetComponent<SkeletonData>();
            if (skeletonData == null)
            {
                print("SkeletonData: No SkeletonData component.  Attempting to add.");
                skeletonData = mygameObject.AddComponent<SkeletonData>() as SkeletonData;
            }
            if (skeletonData == null)
            {
                print("SkeletonData: No SkeletonData component could be added");
            }
            else
            {
                print("TrackAvatar: Skeleton found.");
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        //        jointDOF = new Vector3[] {        };
        jointNames = new string[] {
            "Neck_Flexion",
            "Neck_LateralFlexion",
            "L_Shoulder_Flexion",
            "L_Shoulder_Abduction",
            "L_Shoulder_HorizontalAbduction",
            "L_Elbow_Flexion",
            "R_Shoulder_Flexion",
            "R_Shoulder_Abduction",
            "R_Shoulder_HorizontalAbduction",
            "R_Elbow_Flexion",
            "L_Hip_Flexion",
            "L_Hip_Abduction",
            "L_Hip_Rotation",
            "R_Hip_Flexion",
            "R_Hip_Abduction",
            "R_Hip_Rotation",
            "R_Knee_Flexion",
            "L_Knee_Flexion",
            "Spine_Flexion",
            "Spine_LateralFlexion",
            "Spine_Rotation"
        };
        int numJoints;
        numJoints = jointNames.Length;
        jointAng = new float[numJoints];
        jointAngMin = new float[numJoints];
        jointAngMax = new float[numJoints];

        bodyDict = new Dictionary<string, int>();
        int ii = 0;

        //bodyDict maps jointDOFNames to index of JointAng
        foreach (string t in jointNames)
        {
            //print("bodyDict add:" + jointNames[ii]);
            bodyDict.Add(jointNames[ii], ii);
            ii++;
        }
        AvatarSkeleton.GetAvatarSkeleton(gameObject, ref avatarSkeleton);

        if (animatorComponent == null)
            animatorComponent = gameObject.GetComponent<Animator>();

        numJoints = avatarSkeleton.boneIndexCount();
        // print("nJ=" + numJoints);
        localAngles = new Vector3[numJoints];

        //        jointNames = new string[numJoints];
        boneNames = new string[numJoints];
        bonePos = new Vector3[numJoints];
        bonePosMin = new Vector3[numJoints];
        bonePosMax = new Vector3[numJoints];


        for (int i = 0; i < numJoints; i++)
        {
            int boneIndex = i;
            string jointName = avatarSkeleton.boneIndex2Name(boneIndex);
            //print(" joint mecanim name=" + jointName);
            boneNames[i] = jointName;
        }


        resetMinMax = true;
		updateMinMax = true;//false;
        Update();
        moving = false;
        initted = true;

    }

    ///Calculate the dot product as an angle
    public static float DotProductAngle(Vector3 vec1, Vector3 vec2)
    {

        double dot;
        double angle;

        //get the dot product
        dot = Vector3.Dot(vec1, vec2);

        //Clamp to prevent NaN error. Shouldn't need this in the first place, but there could be a rounding error issue.
        if (dot < -1.0f)
        {
            dot = -1.0f;
        }
        if (dot > 1.0f)
        {
            dot = 1.0f;
        }

        //Calculate the angle. The output is in radians
        //This step can be skipped for optimization...
        angle = Mathf.Acos((float)dot);

        return (float)angle;
    }

    ///
    float SignedProjectedAngle(Vector3 a, Vector3 b, Vector3 planeNormal)
    {
        //        Vector3 projA = a - planeNormal * Vector3.Dot(a, planeNormal);
        Vector3 projA = Vector3.ProjectOnPlane(a, planeNormal);

        projA.Normalize();
        return SignedAngle(projA, b, planeNormal);
    }

    /*
	 * Use cross product of the two vectors to get the normal of the plane formed by the two vectors. 
	 */
    float SignedAngle(Vector3 a, Vector3 b)
    {
        float vcos = Vector3.Dot(a, b);
        //Vector3 cross = Vector3.Cross(a, b);
        float angle;
        angle = Mathf.Acos(vcos);
        //print ("Signed angle : a = " + a + ", b = " + b + ",angle=" + angle);
        return angle;
    }


    //signed angle between two vectors with sign determined by planeNormal (a.b is positive when aXb is in planenormal direction
    /*
	 * Use cross product of the two vectors to get the normal of the plane formed by the two vectors. 
	 * Then check the dotproduct between that and the original plane normal to see if they are facing the same direction.
	 */
    float SignedAngle(Vector3 a, Vector3 b, Vector3 planeNormal)
    {
        float vcos = Vector3.Dot(a, b);
        Vector3 cross = Vector3.Cross(a, b);
        float angle;
        if (planeNormal != Vector3.zero)
            angle = Mathf.Atan2(Vector3.Dot(cross, planeNormal), vcos);
        else
            angle = Mathf.Atan2(1.0f, vcos);
        return angle;
    }

    private static float deg90 = 1.570796326794897f;

    ///Calculate the angle between a vector and a plane. The plane is made by a normal vector.
    ///Output is in radians.
    public static float AngleVectorPlane(Vector3 vector, Vector3 normal)
    {

        float dot;
        float angle;

        //calculate the the dot product between the two input vectors. This gives the cosine between the two vectors
        dot = Vector3.Dot(vector, normal);

        //this is in radians
        angle = (float)Mathf.Acos(dot);

        return deg90 - angle; //90 degrees - angle
    }

    ///Convert a plane defined by 3 points to a plane defined by a vector and a point. 
    ///The plane point is the middle of the triangle defined by the 3 points.
    public static Vector3 NormSub(Vector3 pointA, Vector3 pointB)
    {

        //Make two vectors from the 2 input points, originating from point A
        Vector3 AB = pointA - pointB;
        AB.Normalize();
        return AB;
    }

    ///Convert a plane defined by 3 points to a plane defined by a vector and a point. 
    ///The plane point is the middle of the triangle defined by the 3 points.
    public static Vector3 PlaneFrom3Points(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {

        Vector3 planeNormal = Vector3.zero;
        //planePoint = Vector3.zero;

        //Make two vectors from the 3 input points, originating from point A
        Vector3 AB = pointB - pointA;
        Vector3 AC = pointC - pointA;

        //Calculate the normal
        planeNormal = Vector3.Normalize(Vector3.Cross(AB, AC));
        return planeNormal;
    }

    float brange(float val)
    {
        if (val > 180.0f)
        {
            val = val - 360f;
        }
        return val;
    }


    Vector3 GetJointPosition(HumanBodyBones bi)
    {
        //    bi = HumanBodyBones.LeftLowerArm;
        Transform joint = animatorComponent.GetBoneTransform(bi);
        //        print("GJP:bi=" + bi);
        Vector3 pos = joint.position;
        return pos;
    }


    /// <summary>
    /// Save body angles as current min, max
    /// </summary>
    /// <param name="val">current joint angles</param>
    /// <param name="min">current minimum values</param>
    /// <param name="max">current maximum values</param>
    void initMinMaxF(float val, ref float min, ref float max)
    {
        min = val;
        max = val;
    }


    /// <summary>
    /// Update body angle min, max values if needed
    /// </summary>
    /// <param name="val">current joint angles</param>
    /// <param name="min">current minimum values</param>
    /// <param name="max">current maximum values</param>
    void saveMinMaxF(float val, ref float min, ref float max)
    {
        const float BADDATA_MIN = -30.0f;  // reject angles below this value
                                           //print ("val=" + val);
        if (val < min)
        {
            if (val < BADDATA_MIN)
                return;
			if (!moving) {
				//print ("changing moving: val=" + val + ", min=" + min);
				moving = true;
			}
            min = val;
        }
        //		print ("val=" + val);
        if (val > max)
        {
			if (!moving) {
				moving = true;
				//print ("changing moving: val=" + val + ", max=" + max);
			}
            max = val;
        }

    }

    /// <summary>
    /// Save body angles as current min, max
    /// </summary>
    /// <param name="val">current joint angles</param>
    /// <param name="min">current minimum values</param>
    /// <param name="max">current maximum values</param>
    void initMinMaxV3(Vector3 val, ref Vector3 min, ref Vector3 max)
    {
        for (int i = 0; i < 3; i++)
        {
            min[i] = val[i];
            max[i] = val[i];
        }
    }


    /// <summary>
    /// Update body angle min, max values if needed
    /// </summary>
    /// <param name="val">current joint angles</param>
    /// <param name="min">current minimum values</param>
    /// <param name="max">current maximum values</param>
    void saveMinMaxV3(Vector3 val, ref Vector3 min, ref Vector3 max)
    {
        const float BADDATA_MIN = -30.0f;  // reject angles below this value
        for (int i = 0; i < 3; i++)
        {
            if (val[i] < min[i])
            {
//                if (val[i] < BADDATA_MIN)
//                    continue;
				if (!moving) {
					//print ("changing moving: val=" + val + ", min=" + min);
					moving = true;
				}
                min[i] = val[i];
            }
            if (val[i] > max[i])
            {
				if (!moving) {
					moving = true;
					//print ("changing moving: val=" + val + ", max=" + max);
				}
                max[i] = val[i];
            }
        }
    }

    void SetJointDOFAngle(string dofname, float angle)
    {
        jointAng[bodyDict[dofname]] = angle;
    }

    /// <summary>
    /// Calculate current joint angles and update min/max if necessary.
    /// This function performs two types of calcuations.  The first is based on individual biomechanical definitions (thanks Arun).
    /// The second method is a generic calculation based on the position of the parant and child bones and the spine for a reference frame.
    /// </summary>
    void CalculateBodyAngles()
    {

        //float angles[2];
        //int DOFs = 2;

        //bool flipped = false;  //flipped = false for LEFT side
        HumanBodyBones bi;

        Vector3 pHips = GetJointPosition(HumanBodyBones.Hips);
        Vector3 pSpine = GetJointPosition(HumanBodyBones.Spine);
        Vector3 pChest = GetJointPosition(HumanBodyBones.Chest);
        Vector3 pLUA = GetJointPosition(HumanBodyBones.LeftUpperArm);
        Vector3 pRUA = GetJointPosition(HumanBodyBones.RightUpperArm);
        Vector3 pLLA = GetJointPosition(HumanBodyBones.LeftLowerArm);
        Vector3 pRLA = GetJointPosition(HumanBodyBones.RightLowerArm);
        Vector3 pLH = GetJointPosition(HumanBodyBones.LeftHand);
        Vector3 pRH = GetJointPosition(HumanBodyBones.RightHand);
        Vector3 pLUL = GetJointPosition(HumanBodyBones.LeftUpperLeg);
        Vector3 pRUL = GetJointPosition(HumanBodyBones.RightUpperLeg);
        Vector3 pLLL = GetJointPosition(HumanBodyBones.LeftLowerLeg);
        Vector3 pRLL = GetJointPosition(HumanBodyBones.RightLowerLeg);
        Vector3 pLF = GetJointPosition(HumanBodyBones.LeftFoot);
        Vector3 pRF = GetJointPosition(HumanBodyBones.RightFoot);

        Vector3 pelvisPlane = PlaneFrom3Points(pHips, pLUL, pRUL);
        Vector3 chestPlane = PlaneFrom3Points(pChest, pLUA, pRUA);
        Vector3 bLUArm = NormSub(pLUA, pLLA);
		//print ("points: pLUA=" + pLUA);
		//print ("points: pLLA=" + pLLA);
		//print ("points: pLH=" + pLH);
        Vector3 bRUArm = NormSub(pRUA, pRLA);
        Vector3 bLLArm = NormSub(pLLA, pLH);
        Vector3 bRLArm = NormSub(pRLA, pRH);
        Vector3 bLULeg = NormSub(pLUL, pLLL);
        Vector3 bRULeg = NormSub(pRUL, pRLL);
        Vector3 bLLLeg = NormSub(pLLL, pLF);
        Vector3 bRLLeg = NormSub(pRLL, pRF);


        Vector3 pCSh = (pRUA + pLUA) / 2.0f;  //center/midpoint between shoulders
        Vector3 bSh = NormSub(pRUA, pLUA);    //bone between shoulders
        Vector3 bHips = NormSub(pRUL, pLUL);    //bone between left and right hip

        Vector3 bUSpine = NormSub(pCSh, pChest);    //upper spine
        Vector3 bLSpine = NormSub(pChest, pSpine);  //lower spine
        Vector3 bSpine = NormSub(pCSh, pSpine);     //full spine
        Vector3 bForward = Vector3.Cross(bHips, bLSpine);  //body-forward direction


        //SPINE
        float _SpineFlexion = SignedProjectedAngle(bSpine, Vector3.up, -bHips) * Mathf.Rad2Deg;
        float _SpineLateralFlexion = SignedProjectedAngle(bSpine, Vector3.up, pelvisPlane) * Mathf.Rad2Deg;
        float _SpineRotation = SignedAngle(bSh, bHips, bLSpine) * Mathf.Rad2Deg;
        SetJointDOFAngle("Spine_Flexion", _SpineFlexion);
        SetJointDOFAngle("Spine_LateralFlexion", _SpineLateralFlexion);
        SetJointDOFAngle("Spine_Rotation", _SpineRotation);


        //SHOULDERS

        //project on Transverse (X-Z horiz divide upper-lower) plane
        Vector3 pbLUArm_T = Vector3.ProjectOnPlane(bLUArm, bUSpine);
        float MpbLUArm_T = pbLUArm_T.magnitude; pbLUArm_T.Normalize();
        Vector3 pbRUArm_T = Vector3.ProjectOnPlane(bRUArm, bUSpine);
        float MpbRUArm_T = pbRUArm_T.magnitude; pbRUArm_T.Normalize();

        //project on Sagittal (Y-Z divide left-right sides) plane
        Vector3 pbLUArm_S = Vector3.ProjectOnPlane(bLUArm, bSh);//project on horiz plane
        float MpbLUArm_S = pbLUArm_S.magnitude; pbLUArm_S.Normalize();
        Vector3 pbRUArm_S = Vector3.ProjectOnPlane(bRUArm, bSh);
        float MpbRUArm_S = pbRUArm_S.magnitude; pbRUArm_S.Normalize();

        //project on Frontal (X-Y divide front-back sides) plane
        //print("bLUArm" + bLUArm);
        Vector3 pbLUArm_F = Vector3.ProjectOnPlane(bLUArm, chestPlane);//project on horiz plane
        //print("pbLUArm" + pbLUArm_F);
        float MpbLUArm_F = pbLUArm_F.magnitude; pbLUArm_F.Normalize();
        Vector3 pbRUArm_F = Vector3.ProjectOnPlane(bRUArm, chestPlane);
        float MpbRUArm_F = pbRUArm_F.magnitude; pbRUArm_F.Normalize();


        //SHOULDERS

        //Flexion
        float L_ShoulderFlexion = SignedAngle(pbLUArm_S, bUSpine, bSh) * MpbLUArm_S * Mathf.Rad2Deg;
        float R_ShoulderFlexion = SignedAngle(pbRUArm_S, bUSpine, bSh) * MpbRUArm_S * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Shoulder_Flexion", L_ShoulderFlexion);
        SetJointDOFAngle("R_Shoulder_Flexion", R_ShoulderFlexion);

        //Abduction
        //print("pbLUArm_F" + pbLUArm_F);
        //print("bUSpine" + bUSpine);
        //print("SAShAb"+SignedAngle(pbLUArm_F, bUSpine, -chestPlane));
        float L_ShoulderAbduction = SignedAngle(pbLUArm_F, bUSpine, -chestPlane) * MpbLUArm_F * Mathf.Rad2Deg;
        float R_ShoulderAbduction = -SignedAngle(pbRUArm_F, bUSpine, -chestPlane) * MpbRUArm_F * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Shoulder_Abduction", L_ShoulderAbduction);
        SetJointDOFAngle("R_Shoulder_Abduction", R_ShoulderAbduction);

        //Horiz Ab
        float L_ShoulderHAb = SignedAngle(pbLUArm_T, bSh, -bUSpine) * MpbLUArm_T * Mathf.Rad2Deg;
        float R_ShoulderHAb = -SignedAngle(pbRUArm_T, -bSh, -bUSpine) * MpbRUArm_T * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Shoulder_HorizontalAbduction", L_ShoulderHAb);
        SetJointDOFAngle("R_Shoulder_HorizontalAbduction", R_ShoulderHAb);

        //ELBOWS
        float L_ElbowFlexion = SignedAngle(bLUArm, bLLArm) * Mathf.Rad2Deg;
		//print ("Lelbow: bLUarm=" + bLUArm);
		//print ("Lelbow: bLLarm=" + bLLArm);
		//print ("Lelbow: Flex=" + L_ElbowFlexion);
        float R_ElbowFlexion = SignedAngle(bRUArm, bRLArm) * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Elbow_Flexion", L_ElbowFlexion);
        SetJointDOFAngle("R_Elbow_Flexion", R_ElbowFlexion);


        //KNEES
        float L_KneeFlexion = SignedAngle(bLULeg, bLLLeg) * Mathf.Rad2Deg;
        float R_KneeFlexion = SignedAngle(bRULeg, bRLLeg) * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Knee_Flexion", L_KneeFlexion);
        SetJointDOFAngle("R_Knee_Flexion", R_KneeFlexion);


        //HIPS
        //project on Transverse (X-Z horiz divide upper-lower) plane
        Vector3 pbLULeg_T = Vector3.ProjectOnPlane(bLULeg, bLSpine);
        float MpbLULeg_T = pbLULeg_T.magnitude; pbLULeg_T.Normalize();
        Vector3 pbRULeg_T = Vector3.ProjectOnPlane(bRULeg, bLSpine);
        float MpbRULeg_T = pbRULeg_T.magnitude; pbRULeg_T.Normalize();

        //project on Sagittal (Y-Z divide left-right sides) plane
        Vector3 pbLULeg_S = Vector3.ProjectOnPlane(bLULeg, bHips);//project on horiz plane
        float MpbLULeg_S = pbLULeg_S.magnitude; pbLULeg_S.Normalize();
        Vector3 pbRULeg_S = Vector3.ProjectOnPlane(bRULeg, bHips);
        float MpbRULeg_S = pbRULeg_S.magnitude; pbRULeg_S.Normalize();

        //project on Frontal (X-Y divide front-back sides) plane
        Vector3 pbLULeg_F = Vector3.ProjectOnPlane(bLULeg, pelvisPlane);//project on horiz plane
        float MpbLULeg_F = pbLULeg_F.magnitude; pbLULeg_F.Normalize();
        Vector3 pbRULeg_F = Vector3.ProjectOnPlane(bRULeg, pelvisPlane);
        float MpbRULeg_F = pbRULeg_F.magnitude; pbRULeg_F.Normalize();

        //HIP FLEXION/EXTENSION
        //180- spine-hip-knee angle



        //Right Arun's
        //float R_HipFlexion = AngleVectorPlane(bRULeg, -pelvisPlane) * Mathf.Rad2Deg;
        //Left Arun's
        //float L_HipFlexion = AngleVectorPlane(bLULeg, -pelvisPlane) * Mathf.Rad2Deg;

        //Flexion
        float L_HipFlexion = SignedAngle(pbLULeg_S, bLSpine, bHips) * MpbLULeg_S * Mathf.Rad2Deg;
        float R_HipFlexion = SignedAngle(pbRULeg_S, bLSpine, bHips) * MpbRULeg_S * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Hip_Flexion", L_HipFlexion);
        SetJointDOFAngle("R_Hip_Flexion", R_HipFlexion);

        //Abduction
        float L_HipAbduction = SignedAngle(pbLULeg_F, bLSpine, pelvisPlane) * MpbLULeg_F * Mathf.Rad2Deg;
        float R_HipAbduction = -SignedAngle(pbRULeg_F, bLSpine, pelvisPlane) * MpbRULeg_F * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Hip_Abduction", L_HipAbduction);
        SetJointDOFAngle("R_Hip_Abduction", R_HipAbduction);

        //Rotation
        float L_HipRot = SignedAngle(pbLULeg_T, -pelvisPlane, bLSpine) * MpbLULeg_T * Mathf.Rad2Deg;
        float R_HipRot = -SignedAngle(pbRULeg_T, -pelvisPlane, bLSpine) * MpbRULeg_T * Mathf.Rad2Deg;
        SetJointDOFAngle("L_Hip_Rotation", L_HipRot);
        SetJointDOFAngle("R_Hip_Rotation", R_HipRot);


        //Calculate min-max
        for (int ii = 0; ii < jointAng.Length; ii++)
        {
            //should be able to use jointDOF array to map name to Vector3 field, but not working, so using a bunch of conditionals...YUCK
            int boneIndex = ii;
			//            if (/*!moving ||*/ (!was_moving && moving))  //something moved for first time, so record all positions as the new min and max
			if (resetMinMax)
			{
				initMinMaxF(jointAng[boneIndex], ref jointAngMin[boneIndex], ref jointAngMax[boneIndex]);
			}else
            if (updateMinMax)
                saveMinMaxF(jointAng[boneIndex], ref jointAngMin[boneIndex], ref jointAngMax[boneIndex]);
        }
    }

    void SaveBodyPositions()
    {

        Transform joint;

        //Calculate joint angles based on parent-child positions instead of custom definitions above.
        int numJoints = avatarSkeleton.boneIndexCount();
        //print("NumJoints=" + numJoints);
        for (int i = 0; i < numJoints; i++)
        {
            int boneIndex = i;
            joint = avatarSkeleton.boneIndex2Transform(boneIndex);
            string jointName = avatarSkeleton.boneIndex2Name(boneIndex);
            //print("boneIndex = " + boneIndex);
            //print(" joint = " + joint.name + ", body part=" + jointName);

            bonePos[i] = joint.position;
            if (i == 7)
            {
                //Debug.Log(bonePos[i]);
            }
			if (resetMinMax)
			{
				//                print("sbp:moved!");
				//                           print(" joint = " + joint.name + ", body part=" + jointName);
				initMinMaxV3(bonePos[boneIndex], ref bonePosMin[boneIndex], ref bonePosMax[boneIndex]);
			}else
            if (updateMinMax)
                saveMinMaxV3(bonePos[boneIndex], ref bonePosMin[boneIndex], ref bonePosMax[boneIndex]);
            //if (!was_moving && moving)  //something moved for first time, so record all positions as the new min and max
        }
    }

    void Update()
    {
		//print ("Update1: wasmoving=" + moving);
        was_moving = moving;
        CalculateBodyAngles();
        SaveBodyPositions();
		//print ("Update2: moving=" + moving);
        resetMinMax = false;
		if (!was_moving && moving)
			resetMinMax = true;  //it moved,so now save initial position
        //        Debug.Log(boneNames[0]);
        //        Debug.Log(boneNames[1]);
        //        Debug.Log(boneNames[2]);
    }
}

/////////////////// OLD CODE ///////////////////////////////

//        print("bUSpine=" + bUSpine);
//        print("bLSpine=" + bLSpine);
//print("bForward=" + bForward);
//        print("pChest=" + pChest);
//        print("pLUA=" + pLUA);
//        print("pRUA=" + pRUA);
//        print("chestPlane=" + chestPlane);
//        print("bRUArm=" + bRUArm);
//        print("bLArm=" + bLArm);
//        print("bSh=" + bSh);
//        print("bUSpine=" + bUSpine);
//        print("pbRUArm=" + pbRUArm);
//        print("pbLArm=" + pbLArm);
//        R_ShoulderFlexion = SignedProjectedAngle(bRUArm, bSh, bUSpine) * Mathf.Rad2Deg;
//        L_ShoulderFlexion = SignedProjectedAngle(bLArm, bSh, bUSpine) * Mathf.Rad2Deg;
//float L_ShoulderFlexion = AngleVectorPlane(bLArm, chestPlane) * Mathf.Rad2Deg;  //ARUN's def
//float R_ShoulderFlexion = AngleVectorPlane(bRUArm, chestPlane) * Mathf.Rad2Deg; //ARUN's def
//print("LL joint = " + joint.name);
//print("l_elbow=" + L_Elbow);

/*
        //HIP AB/ADDUCTION
//90- Lhip to RHip to Knee
Vector3 hipV = NormSub(pRUL, pLUL);
//float R_HipAB = deg90 - DotProductAngle(hipV, bRleg);
//float L_HipAB = deg90 - DotProductAngle(hipV, bLleg);
float R_HipAB = Vector3.Angle(hipV, bRULeg) - 90.0f;  //these are perp to each other when angle = 0, so subtract 90 deg
float L_HipAB = 90.0f - Vector3.Angle(hipV, bLULeg);
SetJointDOFAngle("L_Hip_Abduction", L_HipAB);
SetJointDOFAngle ("R_Hip_Abduction", R_HipAB);
*/

/*        DOFs = 1;s
        flipped = false;
        bi = HumanBodyBones.LeftLowerArm;
        joint = animatorComponent.GetBoneTransform(bi);
		Vector3 L_Elbow=
        CalculateJointAngle(joint, DOFs, null, null, flipped);
        flipped = true;
        bi = HumanBodyBones.RightLowerArm;
        joint = animatorComponent.GetBoneTransform(bi);
		Vector3 R_Elbow=
        CalculateJointAngle(joint, DOFs, null, null, flipped);
	SetJointDOFAngle("L_Elbow_Flexion", L_Elbow[0]);
	SetJointDOFAngle("R_Elbow_Flexion", R_Elbow[0]);
*/
/*
//KNEES
DOFs = 1;
flipped = false;
bi = HumanBodyBones.LeftLowerLeg;
joint = animatorComponent.GetBoneTransform(bi);
Vector3 L_Knee=
	CalculateJointAngle(joint, DOFs, null, null, flipped);
flipped = true;
bi = HumanBodyBones.RightLowerLeg;
joint = animatorComponent.GetBoneTransform(bi);
Vector3 R_Knee=
	CalculateJointAngle(joint, DOFs, null, null, flipped);
SetJointDOFAngle("L_Knee_Flexion", L_Knee[0]);
SetJointDOFAngle("R_Knee_Flexion", R_Knee[0]);
*/
/*
Transform joint, frameJoint, overrideParentJoint = null;
DOFs = 2;
flipped = false;
bi = HumanBodyBones.LeftUpperArm;
joint = animatorComponent.GetBoneTransform(bi);
frameJoint = animatorComponent.GetBoneTransform(HumanBodyBones.Spine);
//overrideParentJoint = animatorComponent.GetBoneTransform(HumanBodyBones.RightUpperArm);
Vector3 L_Shoulder=
	CalculateJointAngle(joint, DOFs, frameJoint, overrideParentJoint, flipped);

DOFs = 2;
flipped = true;
bi = HumanBodyBones.RightUpperArm;
//overrideParentJoint = animatorComponent.GetBoneTransform(HumanBodyBones.LeftUpperArm);
joint = animatorComponent.GetBoneTransform(bi);
Vector3 R_Shoulder=
	CalculateJointAngle(joint, DOFs, frameJoint, overrideParentJoint, flipped);
//        L_Shoulder[1] = L_ShoulderFlexion;  //overwrite 
//        R_Shoulder[1] = R_ShoulderFlexion;
SetJointDOFAngle("L_Shoulder_Abduction", L_Shoulder[0]);  //should prob replace with Arun's def
SetJointDOFAngle("R_Shoulder_Abduction", R_Shoulder[0]);
*/
/*
    //Min-Max calculations
    saveMinMax(Head, ref HeadMin, ref HeadMax);
    saveMinMax(Neck, ref NeckMin, ref NeckMax);
    saveMinMax(Spine, ref SpineMin, ref SpineMax);
    saveMinMax(L_Shoulder, ref L_ShoulderMin, ref L_ShoulderMax);
    saveMinMax(R_Shoulder, ref R_ShoulderMin, ref R_ShoulderMax);
    saveMinMax(L_Elbow, ref L_ElbowMin, ref L_ElbowMax);
    saveMinMax(R_Elbow, ref R_ElbowMin, ref R_ElbowMax);
    saveMinMax(L_Hip, ref L_HipMin, ref L_HipMax);
    saveMinMax(R_Hip, ref R_HipMin, ref R_HipMax);
    saveMinMax(L_Knee, ref L_KneeMin, ref L_KneeMax);
    saveMinMax(R_Knee, ref R_KneeMin, ref R_KneeMax);


    if ((!was_moving && moving))  //something moved for first time, so record all positions as the new min and max
    {
        initMinMax(Head, ref HeadMin, ref HeadMax);
        initMinMax(Neck, ref NeckMin, ref NeckMax);
        initMinMax(Spine, ref SpineMin, ref SpineMax);
        initMinMax(L_Shoulder, ref L_ShoulderMin, ref L_ShoulderMax);
        initMinMax(R_Shoulder, ref R_ShoulderMin, ref R_ShoulderMax);
        initMinMax(L_Elbow, ref L_ElbowMin, ref L_ElbowMax);
        initMinMax(R_Elbow, ref R_ElbowMin, ref R_ElbowMax);
        initMinMax(L_Hip, ref L_HipMin, ref L_HipMax);
        initMinMax(R_Hip, ref R_HipMin, ref R_HipMax);
        initMinMax(L_Knee, ref L_KneeMin, ref L_KneeMax);
        initMinMax(R_Knee, ref R_KneeMin, ref R_KneeMax);

    }
    */
/*
//Calculate joint angles based on parent-child positions instead of custom definitions above.
for (int i = 0; i < BoneNumDOF.Length; i++)
//    for (int boneIndex = 0; boneIndex < BoneNumDOF.Length; boneIndex++)
{
    int boneIndex = i;
    Vector2 boneV = BoneNumDOF[boneIndex];
    int bone = (int)boneV[0];
    print("boneIndex = " + bone);
    joint = avatarSkeleton.boneIndex2Transform(bone);
    print(" joint = " + joint.name);
    //DOFs = jointDOFs[i];
    DOFs = (int)boneV[1];
    flipped = false;
    frameJoint = null;
    overrideParentJoint = null;
    if (DOFs == 2)
    {
        frameJoint = avatarSkeleton.boneIndex2Transform(1);  //use spine for frameJoint
    }
    if (bone >= 11) { flipped = true; }
    CalculateJointAngle(joint, DOFs, frameJoint, overrideParentJoint, flipped, ref angles[boneIndex]);
    saveMinMax(angles[boneIndex], ref anglesMin[boneIndex], ref anglesMax[boneIndex]);
    if ( (!was_moving && moving))  //something moved for first time, so record all positions as the new min and max
    {
        initMinMax(angles[boneIndex], ref anglesMin[boneIndex], ref anglesMax[boneIndex]);
    }
}
*/
/*
//Get local joint rotations
//print("getSkelData");
EnableBody ebody = avatarSkeleton.GetSkeletonData(true);
Dictionary<string, EnableJoint> joints = ebody.Joints;
//estimatedRootOffset = newEstimatedRootOffset;


int ii = 0;
foreach (string jointName in joints.Keys)
{
    EnableJoint j = joints[jointName];
    localAngles[ii] = j.orientation.eulerAngles;
    ii++;

}
*/

/*
// dictionary for looking up which avatar bones we are interested in.  For now, we are only interested in the ones that kinect provides.
private readonly Dictionary<string, Vector3> boneName2value = new Dictionary<string, Vector3>
{
	//        {"Hips",    Hips},
	{"Spine",   Spine},
	{"Chest",   Chest},
	{"Neck",    Neck},
	{"Head",    Head},

	{"LeftUpperArm",      L_Shoulder},
	{"LeftLowerArm",      L_Elbow},

	//        {"LeftHand",          L_Wrist},
	//        {"LeftIndexProximal", LeftIndexProximal},
	//        {"LeftIndexIntermediate",  LeftIndexIntermediate},
	//        {"LeftThumbProximal", LeftThumbProximal},

	{"RightUpperArm",  R_Shoulder},
	{"RightLowerArm",  R_Elbow},
	//        {"RightHand",  R_Wrist},
	//        {"RightIndexProximal",  RightIndexProximal},
	//        {"RightIndexIntermediate",  RightIndexIntermediate},
	//        {"RightThumbProximal",  RightThumbProximal},

	{"LeftUpperLeg",  L_Hip},
	{"LeftLowerLeg",  L_Knee},
	//        {"LeftFoot",  LeftFoot},
	//        {"LeftToes",  LeftToes},

	{"RightUpperLeg",  R_Hip},
	{"RightLowerLeg",  R_Knee},
	//        {"RightFoot",  RightFoot},
	//        {"RightToes",  RightToes},

	//        {"LeftShoulder",  LeftShoulder},
	//        {"RightShoulder",  RightShoulder},
};

*/
/*
public Vector3 NeckMin;
public Vector3 HeadMin;
public Vector3 L_ShoulderMin;
public Vector3 L_ElbowMin;
public Vector3 R_ShoulderMin;
public Vector3 R_ElbowMin;
public Vector3 R_HipMin;
public Vector3 L_HipMin;
public Vector3 R_KneeMin;
public Vector3 L_KneeMin;
public Vector3 SpineMin;

public Vector3 NeckMax;
public Vector3 HeadMax;
public Vector3 L_ShoulderMax;
public Vector3 L_ElbowMax;
public Vector3 R_ShoulderMax;
public Vector3 R_ElbowMax;
public Vector3 R_HipMax;
public Vector3 L_HipMax;
public Vector3 R_KneeMax;
public Vector3 L_KneeMax;
public Vector3 SpineMax;


/// <summary>
/// Calculates the joint angle.
/// </summary>
/// <param name="joint">Joint.</param>
/// <param name="DOFs">DOF for joint.</param>
/// <param name="frameJoint">Frame joint.</param>
/// <param name="overrideParentJoint">Override parent joint.</param>
/// <param name="flipped">If set to <c>true</c> flipped.</param>
/// <param name="angles">Angles.</param>
Vector3 CalculateJointAngle(
	Transform joint,
	int DOFs,
	Transform frameJoint,                     ///grandparent (usually) joint to define rotation planes
	Transform overrideParentJoint,            ///sometimes (i.e. clavicle)parent is not best ref for direction(i.e.shouder-to-shoulder)
	bool flipped)
//     ref Vector3 angles)
{
	Vector3 angles = Vector3.zero;
	Transform parent = joint.parent;
	if (overrideParentJoint)
	{
		parent = overrideParentJoint;
	}
	Transform child = joint.GetChild(0);

	if (!initted)
		return angles;
	Vector3 parentRot = joint.position - parent.position;
	Vector3 boneDir = child.position - joint.position;
	parentRot.Normalize();
	boneDir.Normalize();
	float vcos = Vector3.Dot(parentRot, boneDir);
	float rad = Mathf.Acos(vcos);
	//float deg = rad * Mathf.Rad2Deg;
	//Debug.Log("Angle:" + rad + " radians (" + deg + " degrees)");	

	//parent orientation used to determine components of anglular motion of child relative to it
	for shoulder, green is in direction of bone, blue is up, and red in z direction away from camera
	//This might need to be changed to not use bone orientations at all, if possible, since axis might depend on avatar.
	//Or, this might need to use relative orientatios from T-Pose, as determined by AvatarSkeleton
	//
//                Vector3 pVAPlaneNormal = parent.right;      //R - clavical backward, normal to calc sign for vert abduction
//                Vector3 pBoneDir = parent.up;               //G  - clavical bone dir
 //               Vector3 pHAPlaneNormal = parent.forward;   //B - clavical UP, rotation plane normal for horizontal abduction
        //
	Vector3 pBoneDir = parentRot;               //G  - clavical bone dir
	
    //      if (flipped) {
    //        pBoneDir = -pBoneDir;
    //        pVAPlaneNormal = -pVAPlaneNormal;
    //        pHAPlaneNormal = -pHAPlaneNormal;
    //    }



	if (DOFs == 1)
	{
		Vector3 pHAPlaneNormal = Vector3.Cross(boneDir, pBoneDir);
		float simpleAngle = SignedAngle(boneDir, pBoneDir, pHAPlaneNormal);
		//if (flipped)
		//	simpleAngle = -simpleAngle;
		float degSA = simpleAngle * Mathf.Rad2Deg;
		//Debug.Log("V_ABD:" + vertAbduct + " radians (" + degVA + " degrees)");	
		//textMesh.text = label + " "  + degSA.ToString("F2") + " degrees";

		angles[0] = degSA;
	}
	if (DOFs == 2)
	{
		Vector3 gpBoneDir = parent.position - frameJoint.position;   //vector to define planes for rotational components (i.e. spine to clav)
		gpBoneDir.Normalize();
		Vector3 pVAPlaneNormal = Vector3.Cross(pBoneDir, gpBoneDir);  //rotation plane normal to calc sign for vert abduction
		Vector3 pHAPlaneNormal = gpBoneDir;                          //rotation plane normal for horizontal abduction
		//        Debug.Log ("parentRot = " + parentRot + ", pBoneDir = " + pBoneDir);
		//        Debug.Log ("parentRot = " + parentRot + ", boneDir = " + boneDir);
		//        Debug.Log ("VAplaneNormal = " + pVAPlaneNormal + ", pBoneDir = " + pBoneDir + ", pHAPlaneNormal = " + pHAPlaneNormal);

		//shoulder horiz Abduction (rotation in horizontal plane (UP normal vector)
		float horizAbduct = -SignedProjectedAngle(boneDir, pBoneDir, pHAPlaneNormal);
		if (flipped)
			horizAbduct = -horizAbduct;
		float degHA = horizAbduct * Mathf.Rad2Deg;
		//if (flipped)
		//    horizAbduct = -horizAbduct;

		//pRotPlaneNormal = parent.right;             //R - clavical backward, normal to calc sign for vert abduction
		float vertAbduct = SignedAngle(boneDir, -pHAPlaneNormal, -pVAPlaneNormal);
		//if (flipped)
		//    vertAbduct = -vertAbduct;
		float degVA = vertAbduct * Mathf.Rad2Deg;
		//Debug.Log("H_ABD:" + horizAbduct + " radians (" + degHA + " degrees)");	
		//textMesh.text = label + DOFslabel[1] + " "  + degHA.ToString("F2") + " degrees"; //"H_ABD:" 
		//Debug.Log("V_ABD:" + vertAbduct + " radians (" + degVA + " degrees)");	
		//textMesh.text = label +DOFslabel[0]  + " "  + degVA.ToString("F2") + " degrees"; //"V_ABD:"

		angles[0] = degVA;
		angles[1] = degHA;
	}
	if (DOFs == 3)  //twist
	{
		//float twist = 0;
		Vector3 jRot = joint.localEulerAngles;
		angles[0] = brange(jRot.x);
		angles[1] = brange(jRot.y);
		angles[2] = brange(jRot.z);
	}
	return angles;
}

*/
