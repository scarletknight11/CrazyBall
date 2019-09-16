using UnityEngine;
using System.Collections;
using FullSerializer;

public class ParameterTracker : TrackerComponent {
	// Use this for initialization
	void Awake () {
        // Tracker.Instance.AddTickModule(new TrackerModule("Game Parameters", ParameterTick));	
        TrackModuleOnEvent(new TrackerModule("Game Parameters", ParameterTick));
	}

    EnableSerializableValue ParameterTick() {
		if (ParameterHandler.Instance != null && ParameterHandler.Instance.AllParameters != null)
			return ParameterHandler.Instance.AllParameters [0]; // we use only one phase so it will be always "phase" [0]
		else
			return null;
    }
}
