using System;
using System.Collections.Generic;

public class AbleReplay {

	private List<DateTime> keys;
	
	private Dictionary<DateTime, List<TrackerMessage>> replayDictionary;
	public Dictionary<DateTime, List<TrackerMessage>> SessionDict {
		get {
			if(replayDictionary == null) {
				generateDictionary();
			}
			return replayDictionary;
		}
	}
	
	private Session sess;
	public float Duration {
		get {
			return (float)sess.Seconds();
		}
	}
	
	private void generateDictionary() {
		replayDictionary = new Dictionary<DateTime, List<TrackerMessage>>();
		keys = sess.ReplayTimeStamps();
		for(int i = 0; i < keys.Count; i++) {
			List<TrackerMessage> messages = sess.Lookup(keys[i]);
			replayDictionary.Add(keys[i],messages);
		}
	}
	
	public AbleReplay(Session session) {
		sess = session;
		generateDictionary();
	}
	
	public DateTime TimeLookup(int index) {
		return keys[index];
	}
	
	public List<TrackerMessage> MessagesLookup(int index) {
		return SessionDict[TimeLookup(index)];
	}

	/// <summary>
	/// Return time on a scale of 0..1 at index.
	/// </summary>
	/// <returns>The time at index.</returns>
	/// <param name="index">Index.</param>
	public float NormalizedTimeAtIndex(int index) {
		DateTime start = keys[1];
		DateTime end = keys[index];
		TimeSpan span = end.Subtract(start);
		float seconds = (float)span.TotalSeconds;
		return seconds / Duration;
	}
}