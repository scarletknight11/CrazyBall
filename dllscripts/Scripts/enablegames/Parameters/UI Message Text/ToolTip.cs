using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class ToolTip : MonoBehaviour {
	[SerializeField]
	private string text;
	public string Text {
		get { return text; }
		set { text = value; }
	}

	public CanvasGroup grp;
	public Text tipText;
	public bool autoFade = false;


	private static float delay = 0.3f;

	void Awake () {



		if(this.GetComponent<Text>() != null)
		{
			tipText = this.GetComponent<Text>();
		}

		if (this.GetComponent<CanvasGroup>() != null)
		{
			grp.alpha = 0f;
			grp = this.GetComponent<CanvasGroup>();
		}
	}


	/*
	void Enter() {
	}

	IEnumerator DelayAndShowTip() {
		yield return new WaitForSeconds(delay);
	}

	void Exit () {
	}
}

	*/


	public float tipTime = 2f;
	public float fadeTime = .1f;

	


	
	// Use this for initialization
	

	public void MakeTip(string message)
	{
		MakeTip(message,tipTime, autoFade);
	}

	public void MakeTip(string message, float duration, bool hideAfterTime)
	{
		tipText.text = message;
		grp.DOFade(1f, fadeTime);
		if (hideAfterTime)
		{
			HideTip(duration);
		}
	}


	public void HideTip()
	{
		grp.DOFade(0f, fadeTime);
	}

	
	public void HideTip(float duration)
	{
		grp.DOFade(0f, fadeTime).SetDelay(duration);
	} 
}

