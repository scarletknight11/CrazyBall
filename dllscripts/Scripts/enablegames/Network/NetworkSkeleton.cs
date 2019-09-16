
using System;
using System.ComponentModel;
using UnityEngine;

//using System;
//using UnityEngine;
using System.Collections.Generic;
//using UnityEngine.Networking;
#if ENABLE_UNET

namespace UnityEngine.Networking
{
//    [RequireComponent(typeof(Animator))]
//    [AddComponentMenu("Network/NetworkSkeleton")]
//	[NetworkSettings(channel=2)]
[AddComponentMenu("Network/NetworkSkeleton")]
[RequireComponent(typeof(NetworkManager))]
[EditorBrowsable(EditorBrowsableState.Never)]

public class NetworkSkeleton : NetworkBehaviour
    {
        public class BoneInfo
        {
            //			public Transform	m_bone;
            public bool valid;
            public Vector3 m_TargetSyncPosition;
            public Quaternion m_TargetSyncRotation3D;
        }

        const short SkeletonMsg = 199;

        [SerializeField]
        Transform m_Target;

        Transform[] m_Bones;

        [SerializeField]
        float m_SendInterval = 0.1f;
        [SerializeField]
        NetworkTransform.AxisSyncMode m_SyncRotationAxis = NetworkTransform.AxisSyncMode.AxisXYZ;
        [SerializeField]
        NetworkTransform.CompressionSyncMode m_RotationSyncCompression = NetworkTransform.CompressionSyncMode.None;

        [SerializeField]
        float m_InterpolateRotation = 0.5f;
        [SerializeField]
        float m_InterpolateMovement = 0.5f;

        [SerializeField]
        int m_SyncLevel = -1;  //-1 means used Avatar defintion rather than transform hierarchy

        [SerializeField]
        int m_NetworkChannel = Channels.DefaultUnreliable;

        // movement smoothing
        public BoneInfo[] m_BoneInfos;

        float m_LastClientSyncTime; // last time client received a sync from server
        float m_LastClientSendTime; // last time client send a sync to server
		float m_LastClientUpdated; // last time client send a sync to server

        const float k_LocalRotationThreshold = 0.00001f;

        public int numBones;
        public int binarySize;

        NetworkWriter m_LocalTransformWriter;

        // settings
        public Transform target { get { return m_Target; } set { m_Target = value; OnValidate(); } }
        public float sendInterval { get { return m_SendInterval; } set { m_SendInterval = value; } }
        public NetworkTransform.AxisSyncMode syncRotationAxis { get { return m_SyncRotationAxis; } set { m_SyncRotationAxis = value; } }
        public NetworkTransform.CompressionSyncMode rotationSyncCompression { get { return m_RotationSyncCompression; } set { m_RotationSyncCompression = value; } }
        public float interpolateRotation { get { return m_InterpolateRotation; } set { m_InterpolateRotation = value; } }
        public float interpolateMovement { get { return m_InterpolateMovement; } set { m_InterpolateMovement = value; } }
        public int syncLevel { get { return m_SyncLevel; } set { m_SyncLevel = value; } }
        public bool globalData = false;
        public bool usePositions = false;
        public Vector3 posOffset;
        // runtime data
        public float lastSyncTime { get { return m_LastClientSyncTime; } }

        [SerializeField] public Camera mainCamera;
        [SerializeField] public Transform head;

        private Vector3 headAdjust;
        public bool moving;
		//bool newData = false;
        void OnValidate()
        {
            if (m_SendInterval < 0)
            {
                m_SendInterval = 0;
            }

            if (m_SyncRotationAxis < NetworkTransform.AxisSyncMode.None || m_SyncRotationAxis > NetworkTransform.AxisSyncMode.AxisXYZ)
            {
                m_SyncRotationAxis = NetworkTransform.AxisSyncMode.None;
            }

            if (interpolateRotation < 0)
            {
                interpolateRotation = 0.01f;
            }
            if (interpolateRotation > 1.0f)
            {
                interpolateRotation = 1.0f;
            }

            if (interpolateMovement < 0)
            {
                interpolateMovement = 0.01f;
            }

            if (interpolateMovement > 1.0f)
            {
                interpolateMovement = 1.0f;
            }
        }



        /// <summary>
        /// ///////////////////////////////////////////////////////START OF NEW CODE
        /// </summary>
        /// 
        // The body root node
        protected Transform bodyRoot;

        // Avatar's offset/parent object that may be used to rotate and position the model in space.
        protected GameObject offsetNode;

        // Variable to hold all them bones. It will initialize the same size as initialRotations.
        protected Transform[] bones;

        // Rotations of the bones when the Kinect tracking starts.
        protected Quaternion[] initialRotations;
        protected Quaternion[] initialLocalRotations;
        // Initial position and rotation of the transform
        protected Vector3 initialPosition;
        protected Quaternion initialRotation;

        // If the bones to be mapped have been declared, map that bone to the model.
        protected virtual void MapBones()
        {
            //Debug.Log("MapBones");
            // make OffsetNode as a parent of model transform.
            offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
            offsetNode.transform.position = transform.position;
            offsetNode.transform.rotation = transform.rotation;
            offsetNode.transform.parent = transform.parent;
            offsetNode.transform.localScale = Vector3.one;
            //Debug.Log("map bones 2");

            // take model transform as body root
            transform.parent = offsetNode.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            bodyRoot = transform;

            var animatorComponent = GetComponent<Animator>();
            //Debug.Log("map bones 3, bl="+ bones.Length);
            // get bone transforms from the animator component
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            //            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                //                if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                //                    continue;
                //Debug.Log("map addbone1");
                HumanBodyBones bi = boneIndex2MecanimMap[boneIndex];
                //Debug.Log("map addbone:bi=" + bi);
                bones[boneIndex] = animatorComponent.GetBoneTransform(bi);
                /*
                if (bones[boneIndex] != null)
                    Debug.Log("====================================add bone[" + boneIndex + "][bi=" + bi + "]=" + bones[boneIndex].name);
                else
                    Debug.Log("=======================NULL:" + boneIndex);
                */
            }
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


        // Capture the initial rotations of the bones
        protected void GetInitialRotations()
        {
            moving = false;
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

        // builds bones list (Transforms) from the humanoid avatar representation.
        //Because this code was merged in, bones is an array while bonesl is a list.  
        //This should be cleaned up to only use one data representation.
        List<Transform> BuildBonesFromAvatar()
        {
            Debug.Log("BuildBonesFromAvatar");
            // check for double start
            if (bones != null)
                return null;

            // inits the bones array
            int nb = boneIndex2MecanimMap.Count;
            bones = new Transform[nb];//27

            // Initial rotations and directions of the bones.
            initialRotations = new Quaternion[bones.Length];
            initialLocalRotations = new Quaternion[bones.Length];

            // Map bones to the points the Kinect tracks
            MapBones();

            // Get initial bone rotations
            GetInitialRotations();

            List<Transform> bonesl = new List<Transform>();
            //bonesl.Add(root);
            //Debug.Log("BuildBonesFromAvatarL="+ bones.Length);
            for (int i = 0; i < bones.Length; i++)
            {
                //bones[i] = target;
                bonesl.Add(bones[i]);
            }
            return bonesl;
        }

        // builds bones recursively until it reaches SyncLevel
        List<Transform> BuildBones(Transform root, int level)
        {
            Debug.Log("BuildBones");
            List<Transform> bones = new List<Transform>();
            bones.Add(root);

            if (level >= m_SyncLevel)
                return bones;

            var children = GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.parent == root)
                {
                    // this is a direct child
                    var subchildren = BuildBones(child, level + 1);
                    foreach (var sub in subchildren)
                    {
                        bones.Add(sub);
                    }
                }
            }
            return bones;
        }

        void Awake()
        {
            //print("NetworkSkeleton:Awake");
            if (m_SyncLevel < 0)  // -1 indicates use avatar insteaad of transform hierarchy
            {
                m_Bones = BuildBonesFromAvatar().ToArray();
                //print("Afer Build. m_Bones=" + m_Bones.Length);
                for (int i = 0; i < m_Bones.Length; i++)
                {
                    var bone = m_Bones[i];
                    if (!bone)
                        continue;
                    //print("builtbones[" + i + "]=" + bone.name);
                }
                //globalData = true;
            }
            else
            {
                if (m_SyncLevel == 0)
                {
                    m_Bones = m_Target.GetComponentsInChildren<Transform>();
                }
                else
                {
                    m_Bones = BuildBones(m_Target, 0).ToArray();
                }
            }

            m_BoneInfos = new BoneInfo[m_Bones.Length];
            for (int i = 0; i < m_Bones.Length; i++)
            {
                m_BoneInfos[i] = new BoneInfo();
                m_BoneInfos[i].valid = (m_Bones[i] != null);
                //m_BoneInfos[i].m_bone = m_Bones[i];
            }

            // cache these to avoid per-frame allocations.
            if (localPlayerAuthority)
            {
                m_LocalTransformWriter = new NetworkWriter();
            }
            numBones = m_Bones.Length;

            NetworkServer.RegisterHandler(SkeletonMsg, HandleSkeleton);
            //headAdjust = new Transform();

        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            //print ("################################################OnSerialize:1");
            if (initialState)
            {
                // dont send in initial data. size is likely too large for default channel
                return true;
            }

            if (syncVarDirtyBits == 0)
            {
                writer.WritePackedUInt32(0);
                return false;
            }

            // dirty bits
            writer.WritePackedUInt32(1);

            SerializeModeTransform(writer);
            return true;
        }

        //send transform data out
        void SerializeModeTransform(NetworkWriter writer)
        {
            int start = writer.Position;
            int i = 0;
            Quaternion relativeRotation;
            Vector3 relativePosition;
            print("Serialize: numbones=" + m_Bones.Length);
            foreach (var bone in m_Bones)
            {
                if (!bone)
                {  //send bad quaternion if no bone
                    Debug.Log("server skipping bone " + i + ",bone=" + boneIndex2MecanimMap[i]);
                    i++;
                    relativeRotation = Quaternion.identity;
                    relativePosition = Vector3.zero;
                    relativePosition.x = 111f;
                    relativePosition.y = 222f;
                    relativePosition.z = 333f;
                    writer.Write(relativePosition);
                    if (m_SyncRotationAxis != NetworkTransform.AxisSyncMode.None)
                    {
                        NetworkTransform.SerializeRotation3D(writer, relativeRotation, syncRotationAxis, rotationSyncCompression);
                    }
                    continue;
                }
                // position
                if (globalData)
                    relativePosition = bone.position;
                else
                    relativePosition = bone.localPosition;

                writer.Write(relativePosition);

                if (globalData)
                {
                    //                    relativeRotation = Quaternion.Inverse(initialRotations[i]) * bone.rotation;
                    relativeRotation = bone.rotation * Quaternion.Inverse(initialRotations[i]);
                }
                else
                {
                    relativeRotation = bone.localRotation; // * Quaternion.Inverse(initialLocalRotations[i++]);
                }
                // rotation
                if (m_SyncRotationAxis != NetworkTransform.AxisSyncMode.None)
                {
                    NetworkTransform.SerializeRotation3D(writer, relativeRotation, syncRotationAxis, rotationSyncCompression);
                }
                if (i == -1)
                {
                    print("server processing bone " + i + ",bone=" + boneIndex2MecanimMap[i]);
                    print("server processing bone " + i + ",rot=" + relativeRotation);
                    print("server processing bone " + i + ",pos=" + relativePosition);
                }
                i++;
            }

            int sz = writer.Position - start;
            if (sz > 1400 && binarySize == 0)
            {
                // this is only generated once.
                Debug.LogWarning("NetworkSkeleton binary serialization size is very large:" + sz + ". Consider reducing the number of levels being synchronized.");
            }
            binarySize = sz;
        }


        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            //print ("OnDeser========================================");
            if (initialState)
                return;

            if (isServer && NetworkServer.localClientActive)
                return;

            if (reader.ReadPackedUInt32() == 0)
                return;

            UnserializeModeTransform(reader, initialState);

            m_LastClientSyncTime = Time.time;
        }

        void UnserializeModeTransform(NetworkReader reader, bool initialState)
        {
            //IMPORTANT: Time critical: Don't put print statements in loop!  Makess Unity hang.
            //print("SN:Unserialize numbones= "+ m_Bones.Length);
            if (hasAuthority)
            {
                // this component must read the data that the server wrote, even if it ignores it.
                // otherwise the NetworkReader stream will still contain that data for the next component.

                for (int i = 0; i < m_Bones.Length; i++)
                {
                    // position
                    reader.ReadVector3();

                    if (syncRotationAxis != NetworkTransform.AxisSyncMode.None)
                    {
                        NetworkTransform.UnserializeRotation3D(reader, syncRotationAxis, rotationSyncCompression);
                    }
                }
                return;
            }

            for (int i = 0; i < m_Bones.Length; i++)
            {
                var boneInfo = m_BoneInfos[i];

                // position
                Vector3 pos = reader.ReadVector3();
                boneInfo.m_TargetSyncPosition = pos;

                // rotation
                if (syncRotationAxis != NetworkTransform.AxisSyncMode.None)
                {
                    var rot = NetworkTransform.UnserializeRotation3D(reader, syncRotationAxis, rotationSyncCompression);
                    //rot = Quaternion.identity;
                    boneInfo.m_TargetSyncRotation3D = rot;
                    if (pos.x == 111 && pos.z == 333)  //this joint was not sent, so skip
                    {
						//Debug.Log ("unserialize count = " + pos.y);
                        boneInfo.valid = false;
                    }
                }
            }
        }
        public int first = 2;
        public Vector3 camOffset;
        void FixedUpdate()
        {

            if (isServer)
            {
                //print ("NetworkSkeleton:FixedUpdateServer!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                FixedUpdateServer();
            }
            if (isClient)
            {
                //print ("NetworkSkeleton:FixedUpdateClient!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Vector3 headPos;
                if (head != null)
                {
                    headPos = head.position;
                }
                /* The following is for debugging hololens version, putting cubes where head position is
				if (false) { // --first > 0) {
					//print ("!!!!!!!!!!!!!!!!!\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!head postion[" + first + "] = " + headPos);
					//first = false;
					float temp = m_InterpolateMovement;
					m_InterpolateMovement = 0.9f;
					FixedUpdateClient();
					m_InterpolateMovement = temp;
					GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					cube.transform.localScale = new Vector3 (0.1f-first*0.005f, 0.1f-first*0.005f, 0.1f-first*0.005f);
					cube.transform.position = headPos;
					if (first == 1) {
						//mainCamera.transform.position = headPos;
						camOffset = headPos;
						print ("=================\n=====================================camOffset = " + camOffset);
					}
				} else*/
                {
                    //mainCamera.transform.position += camOffset;
                    FixedUpdateClient();
                }
            }
        }

        void FixedUpdateServer()
        {
            if (syncVarDirtyBits != 0)
                return;

            // dont run if network isn't active
            if (!NetworkServer.active)
                return;

            // dont run if we haven't been spawned yet
            if (!isServer)
                return;

            // dont' auto-dirty if no send interval
            if (GetNetworkSendInterval() == 0)
                return;

            // This will cause transform to be sent
            SetDirtyBit(1);
        }

        private int count = 30; //used so we don't print debug every frame

        //set bone orientations based on data read from packet
        void FixedUpdateClient()
        {
            //            print("!!!!!FixedUpdateClient:0 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            // dont run if we haven't received any sync data
            if (m_LastClientSyncTime == 0)
                return;
			//if (m_LastClientSyncTime == m_LastClientUpdated)  //maybe use this for optimzation so network data only processed once
			//	return;
			m_LastClientUpdated = m_LastClientSyncTime;
            // dont run if network isn't active
            if (!NetworkServer.active && !NetworkClient.active)
                return;

            // dont run if we haven't been spawned yet
            if (!isServer && !isClient)
                return;

            // dont run if not expecting continuous updates
            if (GetNetworkSendInterval() == 0)
                return;

            // dont run this if this client has authority over this player object
            if (hasAuthority)
                return;
            /*
            //offset root so head is at camera
            if (mainCamera == null)
            {
                camOffset = Vector3.zero;
            }
            else
            {
                camOffset = head.position - mainCamera.transform.position + headAdjust;
            }
            */
            // interpolate on client

            count--;
            int nb = m_Bones.Length;
            moving = false;
            for (int i = 0; i < nb; i++)
            {
                var bone = m_Bones[i];
                if (!bone)
                    continue;
                var boneInfo = m_BoneInfos[i];
                HumanBodyBones bi = boneIndex2MecanimMap[i];
                if (!boneInfo.valid)
                {
					//if (i==0)
					//	print("processing packet " + boneInfo.m_TargetSyncPosition.y);
                    //print("client skipping bone " + i + ",bone=" + boneIndex2MecanimMap[i]);
                    continue;
                }
                if (i == -1)
                {
                    print("client processing bone " + i + ",bone=" + boneIndex2MecanimMap[i]);
                    print("client processing bone " + i + ",rot=" + boneInfo.m_TargetSyncRotation3D);
                    print("client processing bone " + i + ",pos=" + boneInfo.m_TargetSyncPosition);
                }
                Vector3 targetPos = boneInfo.m_TargetSyncPosition;
                /*
				if (i == 0) {
					print("=====================camoffset="+camOffset);
					print (bone.name+"1=" + bone.localPosition);
					targetPos += bone.localPosition;
					print ("targetPos1=" + targetPos);
					targetPos -= camOffset;
					print ("targetPos2=" + targetPos);
				}*/

                if (globalData)
                {
//                    if (count < 0)
                    {
                        //                        print("curr position[" + i + "]=" + bone.name + ":" + bone.position);
                        //                        print("set position: " + bone.name + ":" + boneInfo.m_TargetSyncPosition);
                        //                        print("set rotation: " + bone.name + ":" + boneInfo.m_TargetSyncRotation3D);
                    }

                    //check if figure is in T-pose (no rotations)
                    float dot = Quaternion.Dot(boneInfo.m_TargetSyncRotation3D, Quaternion.identity);
                    if (Math.Abs(dot)<0.9)
                    {
//                                                print("set rotation: " + bone.name + ":" + boneInfo.m_TargetSyncRotation3D);
//                        print("set rotation E: " + bone.name + ":" + Quaternion.Dot(boneInfo.m_TargetSyncRotation3D, Quaternion.identity));
                        moving = true;
                    }
                    //                    bone.rotation = Quaternion.Slerp(bone.rotation, boneInfo.m_TargetSyncRotation3D, m_InterpolateRotation);
                    Quaternion newRotation = boneInfo.m_TargetSyncRotation3D * initialRotations[i];
                    bone.rotation = newRotation;
                    //bone.rotation = Quaternion.Slerp(bone.rotation, newRotation, m_InterpolateRotation);
                    if (usePositions) //if (bi != HumanBodyBones.Chest && bi != HumanBodyBones.Spine)
                    {
                        bone.position = Vector3.Lerp(bone.position, boneInfo.m_TargetSyncPosition, m_InterpolateMovement);
                    }
                    else
                    {
                        if (bi == HumanBodyBones.Hips)
                        {
                            Vector3 pos = bone.position;
                            pos = boneInfo.m_TargetSyncPosition + posOffset;
                            bone.position = pos;
                        }
                    }
                }
                else
                {
                    //bone.localPosition = Vector3.Lerp(bone.localPosition, boneInfo.m_TargetSyncPosition, m_InterpolateMovement);
                    //bone.localRotation = Quaternion.Slerp(bone.localRotation, boneInfo.m_TargetSyncRotation3D, m_InterpolateRotation);
                    bone.localPosition = Vector3.Lerp(bone.localPosition, targetPos, m_InterpolateMovement);
                    bone.localRotation = Quaternion.Slerp(bone.localRotation, boneInfo.m_TargetSyncRotation3D, m_InterpolateRotation);
                }
            }
            if (count < 0)
                count = 30;
            //print("NetworkSkeleton:Moving=" + moving);
        }


        // --------------------- local transform sync  ------------------------
        private float adjustSign = 1.0f;
        public float adjustRes = 0.01f;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                adjustSign = 1.0f;
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                adjustSign = -1.0f;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                headAdjust.x += adjustRes * adjustSign;
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                headAdjust.y += adjustRes * adjustSign;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                headAdjust.z += adjustRes * adjustSign;
            }
            if (!hasAuthority)
                return;

            if (!localPlayerAuthority)
                return;

            if (NetworkServer.active)
                return;

            if (Time.time - m_LastClientSendTime > GetNetworkSendInterval())
            {
                SendTransform();
                m_LastClientSendTime = Time.time;
            }
        }

        bool HasMoved()
        {
            //TODO - idle animation make this useless?
            return true;
        }

        [Client]
        void SendTransform()
        {
            //			print ("SendTransform");
            if (!HasMoved() || ClientScene.readyConnection == null)
            {
                return;
            }

            m_LocalTransformWriter.StartMessage(SkeletonMsg);
            m_LocalTransformWriter.Write(netId);
            SerializeModeTransform(m_LocalTransformWriter);

            m_LocalTransformWriter.FinishMessage();

            ClientScene.readyConnection.SendWriter(m_LocalTransformWriter, GetNetworkChannel());
        }

        static internal void HandleSkeleton(NetworkMessage netMsg)
        {
            NetworkInstanceId netId = netMsg.reader.ReadNetworkId();

            GameObject foundObj = NetworkServer.FindLocalObject(netId);
            if (foundObj == null)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkSkeleton no gameObject"); }
                return;
            }

            NetworkSkeleton sk = foundObj.GetComponent<NetworkSkeleton>();
            if (sk == null)
            {
                if (LogFilter.logError) { Debug.LogError("NetworkSkeleton null target"); }
                return;
            }

            if (!netMsg.conn.clientOwnedObjects.Contains(netId))
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NetworkSkeleton netId:" + netId + " is not for a valid player"); }
                return;
            }

            sk.UnserializeModeTransform(netMsg.reader, false);
            sk.m_LastClientSyncTime = Time.time;
        }

        //        public override int GetNetworkChannel()

        public int myGetNetworkChannel()
        {
            return m_NetworkChannel;
        }
        public override float GetNetworkSendInterval()
        {
            return m_SendInterval;
        }
        void OnGui()
        {
            print("NS");
        }
    }
}
#endif //ENABLE_UNET