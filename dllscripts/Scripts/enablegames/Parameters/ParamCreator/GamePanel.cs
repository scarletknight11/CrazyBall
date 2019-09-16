using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GamePanel : MonoBehaviour {
	public static float TWEEN_TIME = .2f;

	private Vector3 upScale;
	public float panelScale = 1f;


    void OnDisable()
    {
        print("GamePanel:OnDisable");
    }
    void OnEnable () {
        print("GamePanel:OnEnable");
        //Vector3 startV = new Vector3(0.5f, 0.5f, 0.5f);
        this.transform.localScale = Vector3.one;
//        this.transform.DOScale(panelScale,TWEEN_TIME);
	}
   
	public void Hide () {
//        this.transform.DOScale(0f,TWEEN_TIME);
        StartCoroutine(Disable());
	}

    public void HidePanel() {
        this.transform.DOScale(0f, TWEEN_TIME);
        StartCoroutine(Disable());
    }


	IEnumerator Disable () {
        yield return new WaitForSeconds(TWEEN_TIME);
        print("GamePanel:Disble:" + this.gameObject.name);
		this.gameObject.SetActive(false);
	}

	public void Show () {
        print("GamePanel:Enable:"+this.gameObject.name);
        this.gameObject.SetActive(true);
	}
}
