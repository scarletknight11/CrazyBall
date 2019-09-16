using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FullSerializer;

public class StringListParameter : GameParameter
{
    [fsProperty]
    private List<string> strings;
    [fsIgnore]
    public List<string> Strings
    {
        get { return strings; }
    }
    [fsProperty]
    private string value_ = "";
    [fsIgnore]
    public string Value
    {
        get { return value_; }
        set
        {
            value_ = value;
            Notify(this);
        }
    }

    public StringListParameter()
    {
        strings = new List<string>();
        value_ = "";
    }

    public override void Print()
    {
        Debug.Log(this.Name + " set to: " + value_.ToString());
    }

    public override bool AllowNetworkSync()
    {
        return false;
    }

    public override void AssignValue(float newVal)
    {

    }

    public override float NetworkValue()
    {
        return -1f;
    }

    public StringListParameter(string name, params string[] strings) : base(name)
    {
        this.strings = new List<string>();
        foreach (string s in strings)
        {
            this.strings.Add(s);
        }
        value_ = strings[0];

    }
}
