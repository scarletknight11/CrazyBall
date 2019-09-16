using UnityEngine;
using System.Collections;

/// <summary>
/// Class that bind a TrackerModule to an event and the tags that represent the egObject that call a certain event
/// </summary>
public class OnEventModule : ScriptableObject /*: MonoBehaviour*/
{
    public int eventType;
    public string[] tags; // Tags represent the name of the Listener
    public TrackerModule module;

	public static OnEventModule CreateInstance(int _eventType, string[] _tags, TrackerModule _module)
	{
		var data=ScriptableObject.CreateInstance<OnEventModule> ();
		data.Init (_eventType, _tags, _module);
		return data;

	}

    public void Init(int _eventType, string[] _tags, TrackerModule _module)
    {
        eventType = _eventType;
        tags = _tags;
        module = _module;
    }
}

/// <summary>
/// Class that contains all the types of events supported by EGLibrary
/// </summary>
public static class egEvent
{

    private static int length;
    public static int Length
    {
        get
        {
            length = Type.GetNames(typeof(Type)).Length;
            return length;
        }
    }
    /// <summary>
    /// List of Events.
    /// </summary>
    public enum Type
    {
        All,
        CustomEvent,
        OnTrigger,
        OnTriggerEnter,
        OnTriggerStay,
        OnTriggerExit,
        OnCollide,
        OnCollisionEnter,
        OnCollisionStay,
        OnCollisionExit,
        Awake,
        OnAble,
        OnDisable,
        OnDestroy
    }

}
