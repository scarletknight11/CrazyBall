using UnityEngine;
using System;
using System.Collections;
using FullSerializer;
using enableGame;

public abstract class GameParameter : Subject
{
    /*do NOT use a dummy variable to hold the real name, it doesnt (trivially?) work
    for example, the following does not work:

    public string nameholder;
    [fsProperty]
    private string name {
        get {return (alias!=null)?alias:nameholder; }
        set {nameholder=value; }
    }
    */


    // the thing I will be searching for to check if the whole theme or just part of it is being changed.
    // FIX: don't forget to update the bottom two buttons if you change the whole theme
    [fsProperty]
    private string name;
    public string Name
    {
        get { return name; }
        set { name = value; }
    }
    [fsProperty]
    private string alias;
    public string Alias
    {
        get { return alias; }
        set { alias = value; }
    }


    [fsProperty]
    private string description;
    public string Description
    {
        get { return description; }
        set { description = value; }
    }

    public GameParameter()
    {
        name = "";
        alias = "";
    }

    // this is theoretically where the name of the parameter is updated. called from stringlistparameter creator
    public GameParameter(string parameterName)
    {
        Debug.Log("test");//Environment.StackTrace);
        name = parameterName;
    }

    public void ForceUpdate()
    {

    }

    protected T GetServerOverride<T>(T current)
    {
        try
        {
            string p = EnableAPI.Instance.GetParameter(this.name.ToLower());
            if (!string.IsNullOrEmpty(p))
            {
                Debug.Log("Overriding " + this.name + " : " + p);
                return (T)Convert.ChangeType(p, typeof(T));
            }
        }
        catch (Exception) { }
        return current;
    }

    public abstract bool AllowNetworkSync();

    public abstract void AssignValue(float newVal);

    public abstract float NetworkValue();

    public abstract void Print();
}
