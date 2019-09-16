using UnityEngine;
using System.Collections;
using FullSerializer;

public class EnableString : EnableSerializableValue {
    [fsProperty]
    public string Value {get; set;}

    public EnableString(string val) {
        Value = val;
    }

    public string Serialize() {
        return JSONSerializer.Serialize(typeof(EnableString), this);
    }

    public object Deserialize(string json) {
        return JSONSerializer.Deserialize(typeof(EnableString), json);
    }
}
