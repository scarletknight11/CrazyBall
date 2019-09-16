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
using System.Collections.Generic;
using FullSerializer;

/// <summary>
/// MonoBehaviour that records the game data. Subscribe game behavior to the tracker by defining a message delegate. See TrackerModule.
/// </summary>
public class Tracker : MonoBehaviour
{
    /// <summary>
    /// How long between each tracker 'tick'. 100 ticks in a second would have .01 seconds between updates
    /// </summary>
    [SerializeField]
    private float SecondsBetweenUpdates = 0.3f;

    /// <summary>
    /// Singleton. A prefab can be made if a person wishes otherwise the package creates the gameobject for the user and the gameobject can track with singleton calls.
    /// </summary>
    private static Tracker instance;
    public static Tracker Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject();
                Tracker i = go.AddComponent<Tracker>() as Tracker;
                //TrackerHeader th = go.AddComponent < TrackerHeader >() as TrackerHeader;
                go.name = "_AbleTracker(Singleton)";
                instance = i;
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        tickModules = new List<TrackerModule>();
        _StartingTime = System.DateTime.Now;
    }

    private bool track = false;
    private List<TrackerModule> tickModules; // These modules update every SecondsBetweenUpdate


    private List<OnEventModule>[] eventModules;
    /// <summary>
    /// Tracking routine. Developer defines when to start and stop with Begin and End tracking
    /// The routine simply waits for SecondsBetweenUpdates and logs each module into the tracker.
    /// </summary>
    /// <returns></returns>
    /*IEnumerator Tracking() {
        Session session = SessionCreator.Instance.CurrentSession;
        while (track) {
            yield return new WaitForSeconds(SecondsBetweenUpdates);
            foreach (TrackerModule module in this.tickModules) {
				try{
                	session.AddMessage(module.GetMessage());
				} catch {
					session.AddMessage(new TrackerMessage(module.Key, "Error in tracking module."));
				}
            }
        }
    }*/

    // _StartingTime is used for count milliseconds between a tick and another
    private System.DateTime _StartingTime;

    /// <summary>
    /// Instead of the Tracking IEnumerator now the tick are calculated in FixedUpdate to avoid the multi instantiation of co routines
    /// _StartingTime read the System DateTime to count the milliseconds between ticks.
    /// </summary>
    void FixedUpdate()
    {
        //print((System.DateTime.Now - _StartingTime).TotalMilliseconds);
		float mil = SecondsBetweenUpdates * 1000;
        if (track && ((System.DateTime.Now - _StartingTime).TotalMilliseconds >= mil))
        {
            _StartingTime = System.DateTime.Now;
            foreach (TrackerModule module in this.tickModules)
            {
                try
                {
                    SessionCreator.Instance.CurrentSession.AddMessage(module.GetMessage());
                }
                catch
                {
                    SessionCreator.Instance.CurrentSession.AddMessage(new TrackerMessage(module.Key, "Error in tracking module."));
                }
            }
        }
    }

    /// <summary>
    /// Add a new OnEventModule to track
    /// </summary>
    /// <param name="module">A module that will be track when certains objects triggers certains events</param>
    public void AddModule(OnEventModule module)
    {
        if (eventModules == null)
        {
            eventModules = new List<OnEventModule>[egEvent.Length];
            for (int i = 0; i < egEvent.Length; i++)
            {
                eventModules[i] = new List<OnEventModule>();
            }
        }
        eventModules[module.eventType].Add(module);
    }

    /// <summary>
    /// Add a new Module to be tracked every tick
    /// </summary>
    /// <param name="module"></param>
    public void AddModule(TrackerModule module)
    {
        tickModules.Add(module);
    }

    /// <summary>
    /// Add a new tracker module that is evaluated every tic the tracker runs.
    /// </summary>
    /// <param name="module"></param>
    public void AddTickModule(TrackerModule module)
    {
        tickModules.Add(module);
    }

    /// <summary>
    /// Start the tracker
    /// </summary>
    public void BeginTracking()
    {
        track = true;
        //  StartCoroutine(Tracking());
    }

    /// <summary>
    /// Stop the tracker
    /// </summary>
    public void StopTracking()
    {
        //  if (track == true) {
        track = false;
        SessionCreator.Instance.CurrentSession.Save();
        // }
    }

    /// <summary>
    /// Default game message.
    /// </summary>
    /// <param name="messageText">String to display next to the default message key</param>
    public void Message(string messageText)
    {
        SessionCreator.Instance.CurrentSession.AddMessage(new TrackerMessage("Game Message", messageText));
    }

    /// <summary>
    /// Make a game message with a specified tracker message object
    /// </summary>
    /// <param name="message">Used to specify a specific key and message</param>
    public void Message(TrackerMessage message)
    {
        SessionCreator.Instance.CurrentSession.AddMessage(message);
    }

    /// <summary>
    /// Make a game message with a specific key and text
    /// </summary>
    /// <param name="key">Key displayed in the tracker data</param>
    /// <param name="messageText">Text displayed in message</param>
    public void Message(string key, string messageText)
    {
        Message(new TrackerMessage(key, messageText));
    }

    /// <summary>
    /// Gets current timestamp, for use in formatting.
    /// </summary>
    /// <returns></returns>
    public static string CurrentTimeStamp()
    {
        return System.DateTime.Now.ToString("h:mm:ss");
    }

    /// <summary>
    /// Force the tracker to stop tracking when the application exits
    /// </summary>
    public void OnApplicationQuit()
    {
        if (SessionCreator.Instance.CurrentSession != null)
        {
            // Interrupt((int)egEvent.Type.CustomEvent, "EndGame");
            Tracker.Instance.StopTracking();
        }
    }

    /*void onDestroy()
    {
        if (SessionCreator.Instance.CurrentSession != null)
        {
            //Interrupt((int)egEvent.Type.CustomEvent, "EndGame");
            Tracker.Instance.StopTracking();
        }
    }*/

    /// <summary>
    /// Methods that track all the modules that are supposed to be tracked by a certain event and objects
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="tag"></param>
    public void Interrupt(int eventType, string[] tags)
    {
        if (eventModules == null)
        {
            return;
        }
        if (eventType != (int)egEvent.Type.All)
        {
            Interrupt((int)egEvent.Type.All, tags);
        }
        /*
            Track all the modules that require to be tracked on every type of event by a certain object
        */
        foreach (OnEventModule eventModule in eventModules[eventType])
        {
            foreach (string moduleTag in eventModule.tags)
            {
                foreach (string objectTag in tags)
                {
                    if (moduleTag == objectTag)
                    {
                        SessionCreator.Instance.CurrentSession.AddMessage(eventModule.module.GetMessage());
                    }
                }
            }
        }
    }

    public void Interrupt(int eventType, string tag)
    {
        if (eventModules == null)
        {
            return;
        }
        if (eventType != (int)egEvent.Type.All)
        {
            Interrupt((int)egEvent.Type.All, tag);
        }
        /*
            Track all the modules that require to be tracked on every type of event by a certain object
        */
        foreach (OnEventModule eventModule in eventModules[eventType])
        {
            foreach (string moduleTag in eventModule.tags)
            {
                if (moduleTag == tag)
                {
					TrackerMessage msg = eventModule.module.GetMessage ();
					//Debug.Log ("Tracker message=" + msg);
                    SessionCreator.Instance.CurrentSession.AddMessage(msg);
                }
            }
        }
    }
}
