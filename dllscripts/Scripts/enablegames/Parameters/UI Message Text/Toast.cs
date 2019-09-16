using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class Toast : MonoBehaviour {
    public float toastTime = 2f;
    public float fadeTime = .1f;

    public CanvasGroup grp;
    public Text toastText;

    private static Toast instance;
	// Use this for initialization
	void Awake () {
        instance = this;
        grp.alpha = 0f;
    }


    public static void MakeToast(string message)
    {
        if(instance == null)
        {
            throw new System.Exception("No toast!");
        }
        MakeToast(message, instance.toastTime);
    }

    public static void MakeToast(string message, float duration)
    {
        if (instance == null)
        {
            throw new System.Exception("No toast!");
        }
        instance.toastText.text = message;
        instance.grp.DOFade(1f, instance.fadeTime);
        instance.grp.DOFade(0f, instance.fadeTime).SetDelay(instance.toastTime);
    }
}
