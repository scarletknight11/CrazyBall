using UnityEngine;
using System.Collections.Generic;
using FullSerializer;

[fsObject]
public class EnableJoint {
    [fsProperty]
    public Vector3 position;
    [fsProperty]
    public Quaternion orientation;
}

/// <summary>
/// A class that can be serialized in json
/// Used to track an entire body/joint chain
/// This is device agnostic, so it doesn't matter if this is kinect or some other device
/// </summary>
public class EnableBody : EnableSerializableValue {
    [fsProperty]
    private Dictionary<string, EnableJoint> jointDictionary;
	[fsIgnore]
	public Dictionary<string, EnableJoint> Joints {
		get {
			return jointDictionary;
		}
	}

    public EnableBody() {
        jointDictionary = new Dictionary<string, EnableJoint>();
    }

    public void AddJoint(string name, EnableJoint aJoint) {
        jointDictionary.Add(name, aJoint);
    }

    public void AddJoint(string name, Vector3 position, Quaternion orientation) {
        EnableJoint joint = new EnableJoint();
        joint.position = position;
        joint.orientation = orientation;
        AddJoint(name, joint);
    }

    public string JSONString() {
        string ret = JSONSerializer.Serialize(typeof(EnableBody), this);
        return ret;
    }

    public string Serialize() {
        return JSONSerializer.Serialize(typeof(EnableBody), this);
    }

    public object Deserialize(string json) {
        return JSONSerializer.Deserialize(typeof(EnableBody), json);
    }
}

public class BodyConverter : fsDirectConverter<EnableBody> {
    public override object CreateInstance(fsData data, System.Type storageType) {
        return new EnableBody();
    }

    protected override fsResult DoSerialize(EnableBody model, Dictionary<string, fsData> serialized) {
        throw new System.NotImplementedException();
    }

    protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref EnableBody model) {
        throw new System.NotImplementedException();
    }
}