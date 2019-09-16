using UnityEngine;

public class PieChartMeshController2 : MonoBehaviour
{
	PieChartMesh2 mPieChart;
	float[] mData;
	public float delay=0.1f;
	public Material mainMaterial;
	public Material[] materials;
	public int segments;
	private Random.State randomBuffer;

	void Start()
	{
		randomBuffer = Random.state;
		Random.InitState(10);

		materials = new Material[segments];
		for (int i=0; i<segments; i++) {
			materials[i]= new Material(mainMaterial);
			materials[i].color=new Color32((byte)Random.Range(0,256),(byte)Random.Range(0,256),(byte)Random.Range(0,256),(byte)255);
			Debug.LogError(materials[i].color);
		}

		mPieChart = gameObject.AddComponent<PieChartMesh2>() as PieChartMesh2;
		if (mPieChart != null)
		{
			mPieChart.Init(mData, 100, 0, 100, materials,delay);
			mData = GenerateRandomValues(segments);
			mPieChart.Draw(mData);
		}
		Random.state = randomBuffer;
	}

	void Update()
	{
		if (Input.GetKeyDown("a"))
		{
			randomBuffer = Random.state;
			Random.InitState(10);
			mData = GenerateRandomValues(segments);
			mPieChart.Draw(mData);
			Random.state = randomBuffer;
		}
	}

	float[] GenerateRandomValues(int length)
	{
		float[] targets = new float[length];

		for (int i = 0; i < length; i++)
		{
			targets[i] = Random.Range(0f, 100f);
		}
		return targets;
	}
}
	
