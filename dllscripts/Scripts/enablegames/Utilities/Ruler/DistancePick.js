#pragma strict
/**
* Measures distance from clicked point.
*/ 
 private var measuring = false;
 private var startPoint : Vector3;
 private var dist: float;
 private var distInches: float;
 
 function Update() {
     var hit : RaycastHit;
     if (Input.GetMouseButtonDown(0)) {
         if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), hit)) {
             measuring = true;
             startPoint = hit.point;
         }
     }
     
     if (measuring && Input.GetMouseButton(0)) {
         if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), hit)) {
             dist = Vector3.Distance(startPoint, hit.point);
             distInches = dist / .0254;
         }
     }
 
     if (measuring && Input.GetMouseButtonUp(0)) {
         measuring = false;
         if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), hit)) {
             dist = Vector3.Distance(startPoint, hit.point);
             distInches = dist / .0254;
             //Debug.Log("Final distance = "+dist);
         }
     }
 }
 
 function OnGUI() {
     if (measuring) {
         GUI.Label(Rect(50,50,150,50), "Distance:" + dist.ToString() + " m, " + distInches.ToString("F2")+ " inches");
     }
 }