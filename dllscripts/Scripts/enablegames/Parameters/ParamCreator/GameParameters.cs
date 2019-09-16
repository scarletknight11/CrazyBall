using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FullSerializer;
using System.IO;
using System;

public enum PlayOrder
{
    Horse, // player places all traps until THEY screw up in which case next player gets to place the trap
    PingPong, // players 1 - X place traps then X - 1 run through the level
    Scheme, // Players continue placing traps until they are satisfied--unlimited level building then run through
    Normal // Player 1 places, runs through level. Then player 2. etc.
}

/// <summary>
/// Contains the parameters used by the game at runtime
/// </summary>
public class GameParameters : EnableSerializableValue
{

    public static string FileExtension = ".json";
    public static string SettingsFolder = Path.Combine(Application.persistentDataPath, "AbleSettings");
    public static string DefaultsFolder = "Default";

    [fsProperty]
    private string name;
    [fsIgnore]
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    [fsProperty]
    private Dictionary<string, List<GameParameter>> categories;
    [fsIgnore]
    public Dictionary<string, List<GameParameter>> Categories
    {
        get { return categories; }
    }

    public delegate void GameParameterArgument(GameParameter parameter);
    public static event GameParameterArgument GameParameterCreated;
    public static event GameParameterArgument GameParameterUpdateCheck;

    [fsProperty]
    private Dictionary<string, GameParameter> parameters;

    [fsIgnore]
    public Dictionary<string, GameParameter> Parameters
    {
        get { return parameters; }
        set { parameters = value; }
    }

    /// <summary>
    /// Returns a COPY of the parameters as a List
    /// </summary>
    [fsIgnore]
    public List<GameParameter> ParameterList
    { // Returns a new list. This is not a modular getter
        get { return new List<GameParameter>(parameters.Values); }
    }

    /// <summary>
    /// Adds a new parameter
    /// </summary>
    public GameParameter AddParameter(GameParameter param)
    {
        parameters.Add(param.Name, param);//TODO: using Add like this is unsafe, investigate need for safer operations or try/catch
        if (GameParameterCreated != null)
        {
            //Debug.Log("Parameter created");
            GameParameterCreated(param);
        }
        return param;
    }

    /// <summary>
    /// Returns a refrence to the value referred to by the given key, throws an error if the key is not found
    /// </summary>
    public GameParameter GetParameter(string name)
    {
        return parameters[name];
    }

    /// <summary>
    /// set "parameters" to a new, empty Dictionary object
    /// </summary>
    public GameParameters()
    {
        //Debug.Log("New Game Parameters Initialized.");
        parameters = new Dictionary<string, GameParameter>();
    }

    public GameParameters(bool loadDefault)
    {
        parameters = new Dictionary<string, GameParameter>();
        if (loadDefault)
        {
            LoadDefaultParameters(this);
        }
    }

    public GameParameters(string name, bool loadDefault)
    : this(loadDefault)
    {
        this.name = name;
    }

    public void AnnounceUpdate(GameParameter param)
    {
        if (GameParameterUpdateCheck != null)
        {
            GameParameterUpdateCheck(param);
        }
    }

    public void RegisterCategory(string categoryName, GameParameter param)
    {
        if (categories == null)
        {
            categories = new Dictionary<string, List<GameParameter>>();
        }
        if (categories.ContainsKey(categoryName) == false)
        {
            categories[categoryName] = new List<GameParameter>();
        }
        categories[categoryName].Add(param);
    }

    private string[][] getSukiFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Suki");

        string[][] folderAndNames = new string[2][];
        folderAndNames[0] = Directory.GetDirectories(path);
        folderAndNames[1] = Directory.GetFiles(path, "*.suki", SearchOption.AllDirectories);
        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < folderAndNames[j].Length; i++)
            {
                folderAndNames[j][i] = folderAndNames[j][i].Remove(0, path.Length + 1);
            }
        }
        return folderAndNames;
    }
    /*
	IEnumerator getWWW(string url)
	{

		string serializedParameters;
		using (WWW www = new WWW (url)) {
			yield return www;
			serializedParameters = www.text;
		}

	}
*/

    public static WWW GetWWW(string url)
    {
        WWW www = new WWW(url);

        WaitForSeconds w;
        while (!www.isDone)
        {
            w = new WaitForSeconds(0.1f);
        }
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.error);
        }
        w = new WaitForSeconds(2.1f);
        //TextAsset defaultParameters = Resources.Load("DefaultParameters") as TextAsset;
        return www;
    }
    //Debug.Log(defaultParameters);
    // string serializedParameters = defaultParameters.text;
    protected void LoadDefaultParameters(GameParameters gpToSet)
    {

        /* string [][] folderAndNames = getSukiFile();
         Debug.Log(folderAndNames[1]);
         RegisterCategory("Suki", AddParameter(new StringListParameter(ParameterStrings.SUKI_TYPE, new string[] {"Arms", "Balance and Reach", "Simple Balance" })));
         RegisterCategory("Suki", AddParameter(new StringListParameter(ParameterStrings.SUKI_FILELIST, folderAndNames[1])));
         RegisterCategory("Suki", AddParameter(new BoolParameter(ParameterStrings.SUKI_EXTENTS, true)));

         //RegisterCategory("Movement", AddParameter(new RangeParameter(ParameterStrings.MOVEMENT_RANGE, 1f, 100f, 100f)));


         RegisterCategory("Gameplay", AddParameter(new RangeParameter(ParameterStrings.STARTING_LEVEL, 1f, 6f, 1f)));
         RegisterCategory("Gameplay", AddParameter(new BoolParameter(ParameterStrings.LOSE_LEVEL, true)));
         RegisterCategory("Gameplay", AddParameter(new RangeParameter(ParameterStrings.NUMBER_OF_LEVELS, 1f, 30f, 25f)));
         //RegisterCategory("Gameplay", AddParameter(new BoolParameter(ParameterStrings.ONLY_RIGHT_INGREDIENTS, false)));
         RegisterCategory("Gameplay", AddParameter(new BoolParameter(ParameterStrings.AUTOPLAY, true)));

         RegisterCategory("Gameplay", AddParameter(new RangeParameter(ParameterStrings.FALLING_SPEED, 1f, 10f, 3f)));
         JSONSerializer.Serialize(typeof(GameParameters), this, Application.streamingAssetsPath + "\\" + "DefaultParameters.json");*/
        try
        {
            string url;
            url = Application.streamingAssetsPath + "/DefaultParameters.json";
//#if !UNITY_ANDROID || UNITY_EDITOR
            if (Application.platform != RuntimePlatform.Android)
            {
                url = "file://" + url;
            }
//#endif
            string serializedParameters;
            //DebugConsole.Log("defaultparam url="+url);
            WWW localFile = GetWWW(url);
            serializedParameters = localFile.text;
            //			DebugConsole.Log("params="+serializedParameters);

            //serializedParameters = File.ReadAllText(url);
            //            string serializedParameters = File.ReadAllText(Application.streamingAssetsPath + "/DefaultParameters.json");
            if (!string.IsNullOrEmpty(serializedParameters))
            {
                GameParameters gp = (GameParameters)JSONSerializer.Deserialize(typeof(GameParameters), serializedParameters);
                gpToSet.name = gp.name;
                gpToSet.categories = gp.categories;
                gpToSet.parameters = gp.parameters;
            }
            //override defaults with data from database
            /* pjdtemp Dictionary<string,string> apiparams=EnableAPI.Instance.GetAllParameters();
                 foreach(KeyValuePair<string,string> kvp in apiparams) {
                     if(gpToSet.parameters.ContainsKey(kvp.Key)) {
                     GameParameter param = gpToSet.parameters[kvp.Key];
                     //TODO: i hope that there's a better way to do this
                         if(param.GetType()==typeof(BoolParameter)) {
                         //((EnumParameter<bool>)param).SetVal(api.GetParameter(kvp.Key));
                         ((BoolParameter)param).Value = Boolean.Parse(apiparams[kvp.Key]);
                         }else if(param.GetType()==typeof(IntParameter)) {
                         ((IntParameter)param).Value = Int32.Parse(apiparams[kvp.Key]);
                         }else if(param.GetType()==typeof(RangeParameter)) {
                         ((RangeParameter)param).Value = Int32.Parse(apiparams[kvp.Key]);
                         }else {
                         throw new NotImplementedException("Support for " + param.GetType() + " is not complete");
                     }
                 }
              }*/
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            throw e;
        }

        //  Debug.Log(parameters[egParameterStrings.SUKI_PARAM_FILE] + "-------------------------------------------------------");
        //RegisterCategory("Gameplay", AddParameter(new StringListParameter(egParameterStrings.SUKI_TYPE, new string[] { "Arms", "Balance and Reach", "Simple Balance" })));
        //RegisterCategory("Gameplay", AddParameter(new StringListParameter(egParameterStrings.SUKI_FILELIST, folderAndNames[1])));
        // RegisterCategory("Gameplay", AddParameter(new BoolParameter(egParameterStrings.SUKI_EXTENTS, true)));
        //RegisterCategory("Block Spawning", AddParameter(new BoolParameter(egParameterStrings.ADAPTATIVE_GAME_DIFFICULTY, true)));

        //JSONSerializer.Serialize(typeof(GameParameters), this, Application.streamingAssetsPath + "\\" + "DefaultParameters.json");
    }


    public void Print()
    {
        foreach (GameParameter p in parameters.Values)
        {
            p.Print();
        }
    }

    public string Serialize()
    {
        return JSONSerializer.Serialize(typeof(GameParameters), this);
    }

    public object Deserialize(string json)
    {
        object ret = JSONSerializer.Deserialize(typeof(GameParameters), json);
        return ret;
    }
}