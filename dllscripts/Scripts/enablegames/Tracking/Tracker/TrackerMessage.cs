using UnityEngine;
using System;
using System.Collections;
using FullSerializer;


public class TrackerMessage {
    [fsProperty]
    private DateTime timeStamp;
    public DateTime TimeStamp {
        get { return timeStamp; }
    }

    [fsProperty]
    private string key;
    public string Key { 
        get { return key; }
        set { key = value; }    
    }

    [fsProperty]
    private EnableSerializableValue value;
    [fsIgnore]
    public EnableSerializableValue Value {
        get { return value; }
        set { this.value = value; }
    }

    public string SerializedValue { 
        get {
            return JSONSerializer.Serialize(typeof(TrackerMessage),this);    
        } 
    }

    public TrackerMessage(string key, EnableSerializableValue value) {
        this.timeStamp = DateTime.Now;
        this.key = key;
        this.value = value;
    }

    public TrackerMessage(string key, string value)
        : this(key, new EnableString(value)) {
    }

	public TrackerMessage() {
		this.timeStamp = DateTime.Now;
		this.key = "AbleData";
		this.value = new EnableString("AbleValue");
	}

    public string MessageString {
        get { return string.Format("{0} - {1} : {2}",timeStamp.ToString("h:mm:ss"),key,value); }
    }

    public override string ToString() {
        return MessageString;
    }

    private static readonly fsSerializer _serializer = new fsSerializer();

    public string JSONString() {
        fsData data;
        _serializer.TrySerialize<TrackerMessage>(this, out data);
        return fsJsonPrinter.CompressedJson(data);
    }

    public static TrackerMessage FromJSON(string json) {
        fsData data = fsJsonParser.Parse(json);
        TrackerMessage ret = null;
        _serializer.TryDeserialize<TrackerMessage>(data, ref ret);
        return ret;
    }
}
