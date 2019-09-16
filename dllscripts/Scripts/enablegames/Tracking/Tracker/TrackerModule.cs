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
using FullSerializer;

/// <summary>
/// This object gets data from a delegate for logging. Users register functionality to track.
/// Creates tracker messages that get logged in the tracker file.
/// </summary>
public class TrackerModule
{
    public delegate EnableSerializableValue GetTrackerStringValue();

    private GetTrackerStringValue getVal;
    public EnableSerializableValue Value
    {
        get
        {
            EnableSerializableValue jsonString = getVal();
            return jsonString;
        }
    }

    private string key;
    public string Key
    {
        get { return key; }
    }

    public TrackerMessage GetMessage()
    {
        return new TrackerMessage(this.key, Value);
    }

    /// <summary>
    /// Track method is some method/functionality that returns the value to track as a string for writing to a format somewhere.
    /// </summary>
    /// <param name="trackMethod"></param>
    public TrackerModule(string key, GetTrackerStringValue trackMethod)
    {
        this.key = key;
        this.getVal = trackMethod;
    }
}
