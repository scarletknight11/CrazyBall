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
using FullSerializer;
using System.Collections.Generic;
using System;

[System.Serializable]
public class StringParameter : GameParameter
{
    [fsProperty, SerializeField]
    private string value_ = "";
    [fsIgnore]
    public string Value
    {
        get { return value_; }
        set { value_ = GetServerOverride<string>(value); }
    }

    public string placeHolder;

    [fsProperty]
    public List<string> gameObjectsToactivateName;

    public StringParameter(string name, string _placeholder)
        : base(name)
    {
        value_ = "";
        placeHolder = _placeholder;
    }
  

    public override bool AllowNetworkSync()
    {
        return true;
    }

 

    public override void Print()
    {
        Debug.Log(this.Name + " set to: " + value_.ToString());
    }

    public override void AssignValue(float newVal)
    {
        throw new NotImplementedException();
    }

    public override float NetworkValue()
    {
        throw new NotImplementedException();
    }
}
