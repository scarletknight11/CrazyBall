using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
//using Windows.Kinect;

namespace Suki
{

    public class SukiInput : MonoBehaviour
    {

        private List<SukiSchema> schemas;
        private SukiData data;

        private static SukiInput instance;
		public static WWW GetWWW(string url)
		{

			WWW www = new WWW (url);

			//WaitForSeconds w;
			while (!www.isDone) {
//				w = new WaitForSeconds (0.1f);
			}
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.Log(www.error);
			}

			return www; 
		}

		public static SukiInput Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    // the next line will call Awake() on the new component
                    SukiInput i = go.AddComponent<SukiInput>() as SukiInput;
					instance = i;
                    return i;
                }
                return instance;
            }
        }

        private SkeletonData skeleton = null;
        public SkeletonData Skeleton
        {
            get
            {
                return skeleton;
            }
            set
            {
                skeleton = value;
            }
        }

        private bool updating = false;
        public bool Updating
        {
            get
            {
                return updating;
            }
        }

        //private bool bodyFound = false;
        private bool syncing = false;
        private IEnumerator SyncUser()
        {
            yield return new WaitForSeconds(3f);
            syncing = false;
        }

        private void Awake()
        {
            schemas = new List<SukiSchema>();
            data = SukiData.Instance;
            // in the event this is running again, we need to clear existing schemas
            data.Reset();

            ReadConfigFiles();
        }

        private void FixedUpdate()
        {
            // suki does not operate until we have linked up a skeleton
            if ((!skeleton) || (!skeleton.Moving))

            {
                updating = false;
                return;
            }


            /*            // suki does not operate until we have linked up a skeleton
                        if (!skeleton) {
                            updating = false;
                            return;
                        }
                        // suki does not operate until a body has been found
            // TODO: assumes attached Kinect. Needs to be updated for Network Skeleton
            /*
                        if (KinectManager.Instance.GetUsersCount() < 1) {
                            bodyFound = false;
                            updating = false;
                            return;
                        } else if (!bodyFound) {
                            // the first frame in which a body is valid, start our sync
                            bodyFound = true;
                            syncing = true;
                            StartCoroutine(SyncUser());
                        }
                        // if currently syncing the user (waiting until avatar matches user pose)
                        if (syncing) {
                            updating = false;
                            return;
                        }
			*/
            // provide skeleton to each schema for processing this frame
            foreach (SukiSchema schema in schemas)
            {
				if (skeleton.gameObject.activeInHierarchy)
                schema.Execute(skeleton);
            }
            // indicate that SUKI is active
            updating = true;
        }

        private void OnDestroy()
        {
        }

      /*  public void ReadConfigFiles()
        {
            List<string> schemaFiles = SukiSchemaList.Instance.GetAllSchemaFiles();
            if (schemaFiles.Count <= 0)
            {
                //				throw new System.Exception("No Schema Files specified");

                print("No Schema Files specified.  Loading default.");
                schemaFiles.Add("suki/default.suki.json");
            }
            foreach (string filename in schemaFiles)
                {
                Debug.Log("Loading SUKI JSON file: " + filename);
                string json = File.ReadAllText(filename);
                SukiSchemaInfo newSchemaInfo = SukiSchemaInfo.Deserialize(json);
                SukiSchema newSchema = SukiSchema.CreateSchema(newSchemaInfo);
                schemas.Add(newSchema);
                }
        }*/


        public void ReadConfigFiles()
        {
            List<string> schemaFiles = SukiSchemaList.Instance.GetAllSchemaFiles();
            if (schemaFiles.Count <= 0)
            {
                    print("No Schema Files specified.  Loading default.");
                    //                    string defaultSuki = "suki/default.suki";//has to be json, so this doesn't work. 
                    string defaultSuki = Application.streamingAssetsPath + "/Suki/default.suki";
				SukiSchemaList.Instance.AddFile(defaultSuki);
				schemaFiles = SukiSchemaList.Instance.GetAllSchemaFiles();
                    //sukiParam = ((GenericParameter<string>)ParameterHandler.Instance.AllParameters[0].GetParameter(ParameterStrings.SUKI_PARAM_FILE));
            }
            foreach (string filename in schemaFiles)
            {
                Debug.Log("Loading SUKI JSON file: " + filename);
				string json; 

				string url;
				url = filename;
                url = url.Replace('\\', '/');
                //#if !UNITY_ANDROID || UNITY_EDITOR
                if (Application.platform != RuntimePlatform.Android)
                {
                    Debug.Log("Not Android Platform: adding file:// to url.");
                    url = "file://" + url;
                }
				//#endif
				string sukitext;
				Debug.Log("suki json url="+url);
				WWW localFile = GetWWW(url);
				json = localFile.text;
				Debug.Log("sukifile="+json);


				//json = File.ReadAllText(filename);
                SukiSchemaInfo newSchemaInfo = SukiSchemaInfo.Deserialize(json);
                SukiSchema newSchema = SukiSchema.CreateSchema(newSchemaInfo);
                schemas.Add(newSchema);
            }
        }

        public bool GetTrigger(string name)
        {
            return data.GetTrigger(name);
        }

        public bool GetSignal(string name)
        {
            return data.GetSignal(name);
        }

        public float GetRange(string name)
        {
            return data.GetRange(name);
        }

        public Vector2 GetLocation2D(string name)
        {
            return data.GetLocation2D(name);
        }

        public Vector3 GetLocation3D(string name)
        {
            return data.GetLocation3D(name);
        }

        public bool TriggerExists(string name)
        {
            return data.TriggerExists(name);
        }

        public bool SignalExists(string name)
        {
            return data.SignalExists(name);
        }

        public bool RangeExists(string name)
        {
            return data.RangeExists(name);
        }

        public bool Location2DExists(string name)
        {
            return data.Location2DExists(name);
        }

        public bool Location3DExists(string name)
        {
            return data.Location3DExists(name);
        }
    }
}
