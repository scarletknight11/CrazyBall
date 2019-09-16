using UnityEngine;
using System.Collections;
using FullSerializer;
using System.Collections.Generic;

public class BoolParameter : GameParameter {
    [fsProperty]
    private bool value_;
    [fsIgnore]
    public bool Value {
        get { return value_; }
        set {
            Notify(this);
            value_ = GetServerOverride<bool>(value); }
    }

	[fsProperty]
	public List<string> gameObjectsToActivateName;

	public BoolParameter(string name, bool value) 
        : base(name) {
        Value = value;
	}

	/*//alex//
	public BoolParameter(string name, bool value, List<string> gameObjectsToActivateName) : base(name)
	{
		Value = value;
		this.gameObjectsToActivateName = gameObjectsToActivateName;
	}
	*///alex//

	public override void AssignValue (float newVal) {
		if(newVal == 1f) {
			Value = true;
		} else {
			Value = false;
		}
	}

    public BoolParameter()
    {
        value_ = true;
    }

	public override bool AllowNetworkSync () {
		return true;
	}

	public override float NetworkValue () {
		return value_ ? 1f : 0f;
	}

    public override void Print() {
        Debug.Log(this.Name + " set to: " + value_.ToString());
    }
}
