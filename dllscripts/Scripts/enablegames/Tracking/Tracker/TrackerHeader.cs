using UnityEngine;
using System.Collections;
using FullSerializer;

public class TrackerHeading : EnableSerializableValue {
    [fsProperty]
    public string VersionNumber;
    [fsProperty]
    public string ProjectName;

    public TrackerHeading() {
        VersionNumber = "1.0";
        ProjectName = "EnAbleGame";
    }

    public TrackerHeading(string vn, string pn) {
        VersionNumber = vn;
        ProjectName = pn;
    }

    string EnableSerializableValue.Serialize() {
        return JSONSerializer.Serialize(typeof(TrackerHeading), this);
    }

    object EnableSerializableValue.Deserialize(string json) {
        return JSONSerializer.Deserialize(typeof(TrackerHeading), json);
    }
}

// Serialized data... will get used someday, but the warnings are unncessary
#pragma warning disable 414
public class TrackerHeader : MonoBehaviour {
    private string versionNumber;
    private string projectName;
	// Use this for initialization
	void Awake () {
        versionNumber = Application.version;
        projectName = Application.productName;
        
        Tracker.Instance.Message(new TrackerMessage("Header",new TrackerHeading(versionNumber,projectName)));
        Tracker.Instance.Interrupt((int)egEvent.Type.CustomEvent, "GameStart");
        Destroy(this);
	}
}
