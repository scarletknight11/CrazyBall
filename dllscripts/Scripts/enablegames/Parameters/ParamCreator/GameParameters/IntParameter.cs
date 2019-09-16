using UnityEngine;
using System.Collections;

public class IntParameter : GameParameter {
    [FullSerializer.fsProperty]
    private int value_;
    [FullSerializer.fsIgnore]
    public int Value {
        get { return value_; }
        set {
            Notify(this);
            value_ = GetServerOverride<int>(value); }
    }

    public IntParameter(string name, int value) 
        : base(name) {
        Value = value;
    }

	public override float NetworkValue () {
		return (float)value_;
	}

	public override bool AllowNetworkSync () {
		return true;
	}

	public override void AssignValue (float newVal) {
		Value = (int) newVal;
	}

    public override void Print () {
        Debug.Log(this.Name + " set to: " + value_.ToString());
    }
}
