using UnityEngine;
using System.Collections;

public class TrackerComponent : MonoBehaviour {
    public string[] all, customEvent, onTrigger, onTriggerEnter, onTriggerStay, onTriggerExit, onCollide, onCollisionEter, onCollisionStay, onCollisionExit, awake, onAble,
       onDisable, onDestoy;

    public void TrackModuleOnEvent(TrackerModule module) {
        if (all != null)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.All, all, module));
        }
        if (customEvent != null)
        {
            //Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.CustomEvent, customEvent, module));
			Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.CustomEvent, customEvent, module));
        }
        if ( onTrigger != null && onTrigger.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnTrigger, onTrigger, module));
        }
        if ( onTriggerEnter != null  && onTriggerEnter.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnTriggerEnter, onTriggerEnter, module));
        }
        if ( onTriggerStay != null && onTriggerStay.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnTriggerStay, onTriggerStay, module));
        }
        if ( onTriggerExit != null && onTriggerExit.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnTriggerExit, onTriggerExit, module));
        }
        if ( onCollide != null && onCollide.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnCollide, onCollide, module));
        }
        if ( onCollisionEter != null && onCollisionEter.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnCollisionEnter, onCollisionEter, module));
        }
        if ( onCollisionStay != null && onCollisionStay.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnCollisionStay, onCollisionStay, module));
        }
        if ( onCollisionExit != null && onCollisionExit.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnCollisionEnter, onCollisionExit, module));
        }
        if ( awake != null && awake.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.Awake, awake, module));
        }
        if ( onAble != null && onAble.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnAble, onAble, module));
        }
        if ( onDisable != null && onDisable.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnDisable, onDisable, module));
        }
        if ( onDestoy != null && onDestoy.Length != 0)
        {
            Tracker.Instance.AddModule(OnEventModule.CreateInstance((int)egEvent.Type.OnDestroy, onDestoy, module));
        }
    }
}
