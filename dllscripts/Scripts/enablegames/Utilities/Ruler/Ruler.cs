using UnityEngine;
using System.Collections;

//based on : public class ConnectPointsWithCubeMesh : MonoBehaviour 
//http://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity
public class Ruler : MonoBehaviour
{
	public bool generateProjections = false;
	// Material used for the connecting lines
	public Material lineMat;
	public float texScalePerMeter = 3.28084f;  //3.28084 feet per meter, 39.3701 inches/m, 100 cm/m

	public float radius = 0.05f;

	// Connect all of the `points` to the `mainPoint`
	public GameObject mainPoint;
	public GameObject[] points;

	// Fill in this with the default Unity Cube mesh
	// We will account for the cube pivot/origin being in the middle.
	public Mesh cubeMesh;
	private Material [] lineMatCopies;  //for multiple tilings

	GameObject[] rulerGameObjects;
	GameObject[] textGameObjects;

	//public Material textMat;
	//public Font textFont;
	public Vector3 textScale;
	public string unitName;

	// Initialize ruler and ruler text (distance) objects
	void Start() 
	{
		if (generateProjections) {
			this.rulerGameObjects = new GameObject[4];  //one to track, and 3 projections (X,Y,Z)
			this.textGameObjects = new GameObject[4];  //one to track, and 3 projections (X,Y,Z)
		} else {
			this.rulerGameObjects = new GameObject[points.Length];
			this.textGameObjects = new GameObject[points.Length];
		}
		this.lineMatCopies = new Material[rulerGameObjects.Length];

		for(int i = 0; i < rulerGameObjects.Length; i++) {
			// Make a gameobject that we will put the ring on
			// And then put it as a child on the gameobject that has this Command and Control script
			this.rulerGameObjects[i] = new GameObject();
			this.rulerGameObjects[i].name = "Ruler #" + i;
			this.rulerGameObjects[i].transform.parent = this.gameObject.transform;

			this.textGameObjects[i] = new GameObject();
			this.textGameObjects[i].name = "Ruler text#" + i;
			this.textGameObjects[i].transform.parent = this.gameObject.transform;

			// We make a offset gameobject to counteract the default cubemesh pivot/origin being in the middle
			GameObject rulerOffsetCubeMeshObject = new GameObject();
			rulerOffsetCubeMeshObject.transform.parent = this.rulerGameObjects[i].transform;

			// Offset the cube so that the pivot/origin is at the bottom in relation to the outer ring     gameobject.
			rulerOffsetCubeMeshObject.transform.localPosition = new Vector3(0f, 1f, 0f);
			// Set the radius
			if (i > 0 && generateProjections == true) {
				rulerOffsetCubeMeshObject.transform.localScale = new Vector3 (radius * 0.125f, 1f, radius * 0.5f);
			} else {
				rulerOffsetCubeMeshObject.transform.localScale = new Vector3 (radius * 0.25f, 1f, radius);
			}
			// Create the the Mesh and renderer to show the connecting ring
			MeshFilter rulerMesh = rulerOffsetCubeMeshObject.AddComponent<MeshFilter>();
			rulerMesh.mesh = this.cubeMesh;

			MeshRenderer rulerRenderer = rulerOffsetCubeMeshObject.AddComponent<MeshRenderer>();
			rulerRenderer.material = new Material(lineMat);
			Color rulerColor = lineMat.color;
			if (i > 0 && generateProjections == true) {
				if (i== 1)
					rulerColor = Color.red;
				if (i== 2)
					rulerColor = Color.green;
				if (i== 3)
					rulerColor = Color.blue;
			}
			lineMatCopies [i] = rulerRenderer.material;
			rulerRenderer.material.SetColor("_Color", rulerColor);

			Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            GameObject textObject = new GameObject();
			TextMesh textMesh = textObject.AddComponent<TextMesh>();
			textMesh.font = ArialFont;//textFont;
			textMesh.color = rulerColor;
			textMesh.text = "Ruler #" + i;
			textObject.transform.parent = this.textGameObjects [i].transform;
		}
	}

	// Update is called once per frame
	void Update() 
	{
		Vector3 endPosition;
		for(int i = 0; i < rulerGameObjects.Length; i++) {
			if (i > 0 && generateProjections==true) {
				{
					endPosition = mainPoint.transform.position;
					endPosition [i - 1] = this.points [0].transform.position[i -1];  //zero out one of the axis to project
				}
			} else {
				endPosition = this.points [i].transform.position;
			}
			// Move the ring to the point
			//this.rulerGameObjects[i].transform.position = endPosition;

			this.rulerGameObjects[i].transform.position = 0.5f * (endPosition + this.mainPoint.transform.position);
			//slightly offset so text doesn't overlap
			this.textGameObjects [i].transform.position = (0.5f + i*0.04f) * (endPosition + this.mainPoint.transform.position);

			var delta = endPosition - this.mainPoint.transform.position;
			this.rulerGameObjects[i].transform.position += delta;

			// Match the ruler texture scale and ruler geometry scale to the distance
			float cubeDistance = Vector3.Distance(endPosition, this.mainPoint.transform.position);
			this.rulerGameObjects[i].transform.localScale = new Vector3(this.rulerGameObjects[i].transform.localScale.x, cubeDistance, this.rulerGameObjects[i].transform.localScale.z);
			Vector3 textLocalScale = textScale;
			textLocalScale.x = -textLocalScale.x;
			this.textGameObjects [i].transform.localScale = textLocalScale;
			string texName = "_MainTex";
			Vector2 texScale = new Vector2 (1f, cubeDistance*texScalePerMeter);
			lineMatCopies[i].SetTextureScale(texName,texScale);
			TextMesh textMesh = this.textGameObjects [i].transform.GetChild(0).GetComponent<TextMesh>();
			float cubeDistanceInches = cubeDistance * 39.3701f;
			textMesh.text = cubeDistance.ToString ("F2") + "m (" + cubeDistanceInches.ToString("F2") + "in)";

			// Make the cube look at the main point.
			// Since the cube is pointing up(y) and the forward is z, we need to offset by 90 degrees.
			this.rulerGameObjects[i].transform.LookAt(this.mainPoint.transform, Vector3.up);
			this.rulerGameObjects[i].transform.rotation *= Quaternion.Euler(90, 0, 0);
		}
	}


	/* Screenspace GUI text not used
	void OnGUI(){
		return;
		for (int i = 0; i < rulerGameObjects.Length; i++) {
			//Vector3 position = this.points [i].transform.position;
			//Vector3 position = this.transform.position;
			//Vector3 position = mainPoint.transform.position;
			Vector3 position = this.rulerGameObjects[i].transform.position;
			Vector3 screenPos = Camera.main.WorldToScreenPoint (position);
			float sx, sy;
			sx = screenPos.x;
			sy = Screen.height - screenPos.y;
			Rect r = new Rect (sx, sy, sx + 50.0f, sy + 50.0f);
			GUILayout.BeginArea (r);

			GUILayout.BeginVertical ();
			showSearch = GUILayout.Toggle (showSearch, "Ruler value");
			GUILayout.EndVertical ();

			//GUILayout.Label ("Selected Part: " + output);
			GUILayout.EndArea ();
		}
	}
	*/
}
