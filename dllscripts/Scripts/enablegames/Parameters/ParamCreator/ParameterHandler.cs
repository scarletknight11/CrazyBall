#region copyright
/*
* Copyright (C) EnAble Games LLC - All Rights Reserved
* Unauthorized copying of these files, via any medium is strictly prohibited
* Proprietary and confidential
* fullserializer by jacobdufault is provided under the MIT license.
*/
#endregion

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// There could be multiple game parameters. This handles accessing them all
public class ParameterHandler : MonoBehaviour
{
    public BoolParameter[] boolParameters;
    public RangeParameter[] rangeParameters;
    public StringListParameter[] stringListParameters;
    public bool loadDefault = true;

    public List<GameParameter> AllDefaultParameters
    {
        get
        {
            List<GameParameter> ret = new List<GameParameter>();
            ret.AddRange(boolParameters);
            ret.AddRange(stringListParameters);
            ret.AddRange(rangeParameters);
            return ret;
        }
    }

    private Dictionary<string, GameParameters> parameters;
    public Dictionary<string, GameParameters> Parameters
    {
        get
        {
            if (parameters == null)
            {
                AddParameters(egParameterStrings.WARMUP_NAME);
                AddParameters(egParameterStrings.CONDITION_NAME);
                AddParameters(egParameterStrings.COOLDOWN_NAME);
            }
            return parameters;
        }
    }
    private static ParameterHandler instance;
    public static ParameterHandler Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = Resources.Load<GameObject>("_ParameterHandler");
				if (obj != null) {
					GameObject inst = Instantiate (obj);
					instance = inst.GetComponent<ParameterHandler> ();
				}
            }
            return instance;
        }
    }

    public List<GameParameters> AllParameters
    {
        get
        {
            List<GameParameters> ret = new List<GameParameters>();
            foreach (GameParameters v in this.Parameters.Values)
            {
                ret.Add(v);
            }
            return ret;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            return;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    public void ForceUpdate(GameParameter parameter)
    {
        foreach (GameParameters gameParams in parameters.Values)
        {
            if (gameParams.ParameterList.Contains(parameter))
            {
                gameParams.AnnounceUpdate(parameter);
            }
        }
    }

    public void ResetParameters()
    {
        instance.parameters = null;
    }

    public void AddParameters(string name)
    {
        if (parameters == null)
        {
            parameters = new Dictionary<string, GameParameters>();
        }
        if (parameters.ContainsKey(name))
        {
            return;
        }
        parameters.Add(name, new GameParameters(name, loadDefault));
    }

    public GameParameters GetParameters(string name)
    {
        if (parameters == null)
        {
            parameters = new Dictionary<string, GameParameters>();
        }
        if (parameters.ContainsKey(name) == false)
        {
            AddParameters(name);
        }
        return parameters[name];
    }
}
