using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is an User firendly parameter creator for developers.
/// With this script a developer can set up game parameters with the Unity Inspector without use the code at all.
/// </summary>
public class egDefaultParametersCreator : MonoBehaviour {
    public class BoolParam {
        string Category;
        string ParameterName;
        string ParameterDescription;
        bool Value;
    }

    public struct FloatParam
    {
        string Category;
        string ParameterName;
        string ParameterDescription;
        float Value;
    }

    public struct IntParam
    {
        string Category;
        string ParameterName;
        string ParameterDescription;
        int Value;
    }

    public List<int> sdaaa;
    string Category;
    string Param;
    string Description;

    public List<BoolParam> BoolParameters;
    [SerializeField]
    public FloatParam[] RangeParameters;
    [SerializeField]
    public IntParam[] IntParameters;

}
