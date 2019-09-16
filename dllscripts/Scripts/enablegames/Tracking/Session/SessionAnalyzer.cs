using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for the Session class. Specifically methods that analyze read in data or in progress sessions
/// Nothing is cached, everything calculated when called
/// </summary>
public static class SessionAnalyzer {
	private static List<string> excludedReplayEndpoints = new List<string>(new string[]{
		"Session Event",
	});

	public static TimeSpan Span(this Session sess) {
		DateTime earliest, latest;
		List<TrackerMessage> beginMessages = sess.Lookup("Game Message");
		earliest = sess.Messages[1].TimeStamp; // index 0 is somehow garbage...
		for(int i = 0; i < beginMessages.Count; i++) {
			EnableString v = beginMessages[i].Value as EnableString;
			if(v.Value.Equals("Game begin")) {
				earliest = beginMessages[i].TimeStamp;
				break;
			}
		}
		TrackerMessage latestMessage;
		int z = sess.Messages.Count - 1;
		latestMessage = sess.Messages[z];
		while(excludedReplayEndpoints.Contains(latestMessage.Key)) {
			z--;
			latestMessage = sess.Messages[z];
		}
		latest = sess.Messages[z].TimeStamp;
		
		TimeSpan span = latest.Subtract(earliest);
		return span;
	}

	public static float Seconds(this Session sess) {
		TimeSpan span = sess.Span();
		return (float)span.TotalSeconds;
	}

	public static DateTime StartTime(this Session sess) {
		DateTime ret = sess.Messages[0].TimeStamp;
		return ret;
	}

	public static List<TrackerMessage> Lookup(this Session sess, string key) {
		List<TrackerMessage> values = new List<TrackerMessage>();
		for(int i = 0; i < sess.Messages.Count; i++) {
			TrackerMessage m = sess.Messages[i];
			if(m.Key == key) {
				values.Add(m);
			}
		}
		return values;
	}

	public static List<TrackerMessage> Lookup(this Session sess, DateTime timeStamp) {
		List<TrackerMessage> values = new List<TrackerMessage>();
		for(int i = 0; i < sess.Messages.Count; i++) {
			TrackerMessage m = sess.Messages[i];
			if(m.TimeStamp.CompareTo(timeStamp) == 0) {
				values.Add(m);
			}
		}
		return values;
	}

	public static List<DateTime> ReplayTimeStamps(this Session sess) {
		List<DateTime> stamps = new List<DateTime>();
		for(int i = 1; i < sess.Messages.Count; i++) { // Skip garbage first message
			if(excludedReplayEndpoints.Contains(sess.Messages[i].Key)) {
				continue;
			}
			bool unique = true;
			/*
			 * stamps.Contains(timestamp) might be unreliable as equivalence with datetimes doesn't really work...
			 */
			for(int z = 0; z < stamps.Count; z++) {
				if(stamps[z].CompareTo(sess.Messages[i].TimeStamp) == 0) {
					unique = false;
					break;
				}
			}
			if(unique) {
				stamps.Add(sess.Messages[i].TimeStamp);
			}
		}
		return stamps;
	}


	/// <summary>
	/// Look up first instance of message with key in the messages list
	/// </summary>
	/// <returns>The message.</returns>
	/// <param name="messages">Messages.</param>
	public static TrackerMessage LookupMessage(List<TrackerMessage> messages, string key) {
		for(int i = 0; i < messages.Count; i++) {
			if(messages[i].Key.Equals(key)) {
				return messages[i];
			}
		}
		return null;
	}
}