using UnityEngine;
using System.Collections;
using FullSerializer;

public interface EnableSerializableValue {
    string Serialize();
    object Deserialize(string json);
}
