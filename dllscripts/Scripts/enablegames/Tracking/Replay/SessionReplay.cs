using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ReplayState {
	Forward,
	Reverse,
	Pause,
	Stop
}

public struct ReplayData {
	public DateTime time;
	public List<TrackerMessage> messages;
}

public class ReplayEventData {
	private ReplayData from, to;
	public ReplayData From {
		get {
			return from;
		}
	}

	public ReplayData To {
		get {
			return to;
		}
	}

	private float duration;
	public float Duration {
		get {
			return duration;
		}
	}

	public ReplayEventData(ReplayData from, ReplayData to) {
		this.from = from;
		this.to = to;
		TimeSpan span = to.time.Subtract(from.time);
		float ms = span.Milliseconds;
		this.duration = Mathf.Abs(ms * 0.001f); // Convert to seconds
	}
}

public class SessionReplay : MonoBehaviour {
	public delegate void ReplayEvent(ReplayEventData data);
	public static event ReplayEvent OnReplayTransition;

	public SessionReader reader;
	public Slider slider;
	public Text nameField;
	public Text timerField;
	public RectTransform tickMarkParent;

	public Image forwardButton;
	public Sprite pauseSprite, playSprite;

	public GameObject tickPrefab;

	private Session sess;
	private AbleReplay replay;
	// Use this for initialization
	void Start () {
		Setup();
		StartCoroutine(Replay());
	}

	void Setup() {
		sess = reader.GetSession();
		replayLength = sess.Seconds();
		replay = new AbleReplay(sess);
		nameField.text = sess.Name;
		int i = 0;
		foreach(DateTime date in replay.SessionDict.Keys) {
			List<TrackerMessage> messages = replay.SessionDict[date];
			if(SessionAnalyzer.LookupMessage(messages,"Player Skeleton") != null) {
				PlaceTick(replay.NormalizedTimeAtIndex(i));
			}
			i++;
		}
	}

	void PlaceTick(float t) {
		float sliderWidth = tickMarkParent.rect.width;
		GameObject tickObject = Instantiate(tickPrefab) as GameObject;
		tickObject.transform.SetParent(tickMarkParent);
		RectTransform tickTrans = (RectTransform)tickObject.transform;
		Vector2 anchoredP = tickTrans.anchoredPosition;
		anchoredP.y = 0f;
		anchoredP.x = sliderWidth * t;
		tickTrans.anchoredPosition = anchoredP;
	}

	private ReplayState state;

	int timeIndex = 0;
	int maxTimeIndex {
		get {
			return replay.SessionDict.Count - 1;
		}
	}

	private float replayLength;

	private float elapsed;

	IEnumerator Replay () {
		state = ReplayState.Forward;
		elapsed = 0f;
		timeIndex = 0;
		while(state != ReplayState.Stop) {
			if(state == ReplayState.Forward) {
				yield return StartCoroutine(Forward());
			} else if(state == ReplayState.Reverse) {
				yield return StartCoroutine(Reverse());
			} else {
				yield return null; // Pause... don't tick up or down
			}
		}
		elapsed = 0f; // reset elapsed for the ui
	}

	IEnumerator Forward () {
		while(state == ReplayState.Forward) {
			int previousIndex = timeIndex;
			timeIndex++;
			int currentIndex = timeIndex;
			if(currentIndex >= replay.SessionDict.Keys.Count) {
				state = ReplayState.Pause;
				timeIndex = replay.SessionDict.Keys.Count - 1;
			} else {
				ReplayData prev, cur;
				prev.time = replay.TimeLookup(previousIndex);
				prev.messages = replay.MessagesLookup(previousIndex);
				cur.time = replay.TimeLookup(currentIndex);
				cur.messages = replay.MessagesLookup(currentIndex);
				ReplayEventData ed = new ReplayEventData(prev, cur);
				yield return StartCoroutine(ReplayTransition(ed, true));
			}
		}
	}

	IEnumerator ReplayTransition(ReplayEventData ed, bool forward) {
		if(OnReplayTransition != null) {
			OnReplayTransition(ed);
		}
		float t;
		t = Mathf.Abs(ed.Duration);
		while(t > 0f) {
			t -= Time.deltaTime;
			if(forward) {
				elapsed += Time.deltaTime;
			} else {
				elapsed -= Time.deltaTime;
			}
			yield return null;
		}
	}

	IEnumerator Reverse () {
		while(state == ReplayState.Reverse) {
			int previousIndex = timeIndex;
			timeIndex--;
			int currentIndex = timeIndex;
			if(currentIndex < 0) {
				state = ReplayState.Pause;
				timeIndex = 0;
			} else {
				ReplayData prev, cur;
				prev.time = replay.TimeLookup(previousIndex);
				prev.messages = replay.MessagesLookup(previousIndex);
				cur.time = replay.TimeLookup(currentIndex);
				cur.messages = replay.MessagesLookup(currentIndex);
				ReplayEventData ed = new ReplayEventData(prev, cur);
				yield return StartCoroutine(ReplayTransition(ed, false));
			}
		}
		print ("Finished Playing");
	}

	void Update () {
		slider.value = elapsed / replayLength;
		string elapseString = string.Format("{0} / {1}",elapsed.ToString("F2"),replayLength.ToString("F2"));
		timerField.text = elapseString;
	}

	public void PlayButton() {
		if(state == ReplayState.Stop) {
			StartCoroutine(Replay());
		} else if(state != ReplayState.Forward) {
			Unpause(ReplayState.Forward);
		} else {
			Pause();
		}
	}

	void Pause() {
		state = ReplayState.Pause;
		forwardButton.sprite = pauseSprite;
	}

	void Unpause(ReplayState newState) {
		state = newState;
		forwardButton.sprite = playSprite;
	}

	public void StopButton() {
		Unpause (ReplayState.Stop);
	}

	public void ReverseButton() {
		Unpause (ReplayState.Reverse);
	}
}
