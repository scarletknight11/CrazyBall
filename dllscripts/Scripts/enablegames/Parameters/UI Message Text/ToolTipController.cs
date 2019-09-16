using UnityEngine;
using System.Collections;

public class ToolTipController : MonoBehaviour {

	[SerializeField]
	private ToolTip toolTip;

	private static ToolTipController instance;
	public static ToolTipController Instance {
		get { return instance; }
	}

	void Awake () {
		if(instance != null) {
			Destroy(this.gameObject);
			return;
		}
		instance = this;
		this.gameObject.SetActive(false);
		DontDestroyOnLoad(this);

		
	}

	public void ShowTip(string text) {
		if(toolTip != null)
		{
			toolTip.MakeTip(text);
		}
	}

	void ShowToolTipAtObject(GameObject obj) {

	}

	public void HideToolTip()
	{
		if(toolTip != null)
		{
			toolTip.HideTip();
		}
	}
}
