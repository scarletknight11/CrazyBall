using UnityEngine;

public class SukiResetButton : MonoBehaviour {

	public void ResetSUKI() {
		Suki.SukiSchemaList.Instance.Reset();
	}
}
