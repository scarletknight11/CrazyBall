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

public class GenericParameter<T> : GameParameter
{
    protected T value_;
    public T Value
    {
        get { return value_; }
        set { value_ = GetServerOverride<T>(value); }
    }

    public GenericParameter(string name, T value)
        : base(name)
    {
        value_ = value;
    }

    public override float NetworkValue()
    {
        return -1f;
    }

    public override bool AllowNetworkSync()
    {
        return false;
    }

    public override void AssignValue(float newVal)
    {

    }

    public override void Print()
    {
        Debug.Log(this.Name + " set to: " + value_.ToString());
    }
}
