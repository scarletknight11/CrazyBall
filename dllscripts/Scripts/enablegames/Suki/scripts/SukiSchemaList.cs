using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;

namespace Suki
{

    public class SukiSchemaList : MonoBehaviour
    {
        public static float foo = 5;
        private List<string> schemaFiles;
        public static string currentSukiFile;
        public string CurrentSukiFile()
        {
            return currentSukiFile;
        }

		public static WWW GetWWW(string url)
		{

			WWW www = new WWW (url);

//			WaitForSeconds w;
			while (!www.isDone) {
//				w = new WaitForSeconds (0.1f);
        }
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.Log(www.error);
			}

			return www; 
		}
//        private List<string> schemaFiles;

        private static SukiSchemaList instance;
        internal static SukiSchemaList Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SukiSchemaList();
                    instance.Init();
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
            }

            this.gameObject.name = "_SukiSchemaList(Singleton)";
            DontDestroyChildOnLoad(this.gameObject);

            Init();
        }

        public static void DontDestroyChildOnLoad(GameObject child)
        {
            Transform parentTransform = child.transform;

            // If this object doesn't have a parent then its the root transform.
            while (parentTransform.parent != null)
            {
                // Keep going up the chain.
                parentTransform = parentTransform.parent;
            }
            GameObject.DontDestroyOnLoad(parentTransform.gameObject);
        }

        private void Init()
        {
            schemaFiles = new List<string>();
        }

        public void Reset()
        {
            SukiSchemaList.Instance.Init();
        }

        internal List<string> GetAllSchemaFiles()
        {
            List<string> ret = new List<string>();
            foreach (string file in schemaFiles)
            {
                print("suki add file " + file);
                // support lists of files
                if (file.EndsWith(".suki"))
                {
                    currentSukiFile = Path.GetFileName(file);
                    string pathDir = Path.GetDirectoryName(file);
                    Debug.Log("sukipath=" + pathDir);

                    string url;
                    url = file;
                    if (Application.platform != RuntimePlatform.Android)
                    {
                        //#if !UNITY_ANDROID || UNITY_EDITOR
                        url = "file://" + url;
                        //					#endif
                    }
					string sukitext;
					Debug.Log("suki url="+url);
					url = url.Replace ('\\', '/');
//					Debug.Log("cl1="+url);
//					Debug.Log("cl2="+url.Substring(url.Length - 40));
//					string url2 = Application.streamingAssetsPath + "/Suki/default.suki.json";
//					string url2 = Application.streamingAssetsPath + "/Suki/Arms/LElbowAngle.suki";
//					Debug.Log("cl3="+url2);
//					Debug.Log("cl4="+url2.Substring(url.Length - 40));
//					url = url2;
					WWW localFile = GetWWW(url);
					sukitext = localFile.text;
					Debug.Log("sukitext="+sukitext);

					string[] files;
					files = sukitext.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
					//files = File.ReadAllLines(file);
                    foreach (string f in files)
                    {
						Debug.Log ("json file =" + f);
						Debug.Log ("json file len =" + f.Length);
						if (f.Length != 0) {
							Debug.Log("GASF:7:"+pathDir+"/"+f);
                        ret.Add(Path.Combine(pathDir, f));
						}
                    }
                }
                else
                {
                    ret.Add(file);
                }
            }
            foreach (string str in ret)
            {
                Debug.Log(str);
            }
            return ret;
        }

        public void AddFile(string filename)
        {
            filename = filename.Replace('\\', '/');
            Debug.Log("SukiSchemaList:AddFile=" + filename);
            if (!schemaFiles.Contains(filename))
            {
                schemaFiles.Add(filename);
            }
        }


    }
}
