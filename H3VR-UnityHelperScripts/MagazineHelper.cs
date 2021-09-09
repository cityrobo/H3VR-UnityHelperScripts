#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using FistVR;

public class MagazineHelper : EditorWindow
{
	//string myString = "Hello World";
	//bool groupEnabled;
	//bool myBool = true;
	//float myFloat = 1.23f;

	//Magazine properties
	public FVRFireArmMagazine Magazine;
	public bool isCurved;
	public Transform magazineRadiusCenter;

	//Cartridge properties
	public GameObject firstCartridge;
	public int numberOfCartridges = 1;

	public bool mirrorX;
	public float cartridgeOffsetY = 0f;
	public float cartridgeOffsetZ = 0f;

	public float cartridgeAngleOffsetX;

	//Follower properties
	public bool generateFollowerPoints;
	public bool useFollowerOffsets = false;
	public bool invertFollowerOffsets = false;

	public GameObject follower;

	public float followerOffsetY = 0f;
	public float followerOffsetZ = 0f;

	//Gizmo properties
	public bool showGizmo = false;
	public float gizmoSize = 0.01f;



	//Private
	GameObject cartridge_root;

	GameObject[] CartridgeObjectList;
	MeshFilter[] CartridgeMeshFilterList;
	MeshRenderer[] CartridgeMeshRendererList;

	float cartridge_currentX;
	float cartridge_currentY;
	float cartridge_currentZ;

	bool ready1 = true;
	bool ready2 = true;

	GameObject follower_root;

	GameObject[] FollowerObjectList;

	float follower_currentX;
	float follower_currentY;
	float follower_currentZ;

	float cartridge_angleX;
	float cartridge_angleY;

	MagazineGizmos gizmo;

	// Add menu item named "Magazine Helper" to the Window menu
	[MenuItem("Window/Magazine Helper")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(MagazineHelper));
	}

	void OnGUI()
	{
		GUILayout.Label("Cartridge Settings", EditorStyles.boldLabel);
		EditorGUIUtility.labelWidth = 300f;
		//myString = EditorGUILayout.TextField ("Text Field", myString);

		//groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
		//myBool = EditorGUILayout.Toggle ("Toggle", myBool);
		//myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3f, 3f);
		//EditorGUILayout.EndToggleGroup ();

		Magazine = (FVRFireArmMagazine)EditorGUILayout.ObjectField("Magazine", Magazine, typeof(FVRFireArmMagazine), true);
		if (Magazine == null)
		{
			EditorGUILayout.HelpBox("Please add Magazine!", MessageType.Error);
			ready1 = false;
		}
		
		isCurved = EditorGUILayout.Toggle("is curved", isCurved);
		if (isCurved) {
			magazineRadiusCenter = (Transform)EditorGUILayout.ObjectField("Magazine Radius Center", magazineRadiusCenter, typeof(Transform),true);
			if (magazineRadiusCenter == null)
			{
				EditorGUILayout.HelpBox("Please add Magazine Radius Center!", MessageType.Error);
				ready1 = false;
			}
			else
			{
				cartridgeAngleOffsetX = EditorGUILayout.Slider("Cartridge Angle Offset X", cartridgeAngleOffsetX, -10f, 10f);
				showGizmo = EditorGUILayout.Toggle("Show Curved Mag Gizmo", showGizmo);
				gizmoSize = EditorGUILayout.FloatField("Gizmo Size", gizmoSize);
			}
		}
		else showGizmo = false;

		ShowGizmo(showGizmo);
		if (showGizmo) gizmo.gizmoSize = gizmoSize;

		firstCartridge = (GameObject) EditorGUILayout.ObjectField ("First Cartridge",firstCartridge, typeof(GameObject), true);
		if (firstCartridge == null) 
		{
			EditorGUILayout.HelpBox ("Please add Reference Cartridge!", MessageType.Error);
			ready1 = false;
		}
		
		numberOfCartridges = EditorGUILayout.IntField ("Number of Cartridges", numberOfCartridges);
		if (numberOfCartridges <= 0)
			numberOfCartridges = 1;

		mirrorX = EditorGUILayout.Toggle ("Is double stacked Magazine (mirror X axis)", mirrorX);
		if (!isCurved)
		{
			cartridgeOffsetY = EditorGUILayout.Slider("Cartridge Offset Y", cartridgeOffsetY, -0.1f, 0.1f);
			cartridgeOffsetZ = EditorGUILayout.Slider("Cartridge Offset Z", cartridgeOffsetZ, -0.1f, 0.1f);
		}

		generateFollowerPoints = EditorGUILayout.BeginToggleGroup ("Generate Follower Points", generateFollowerPoints);
		{
			follower = (GameObject)EditorGUILayout.ObjectField("Last Round Follower", follower, typeof(GameObject), true);
			if (follower == null && generateFollowerPoints)
			{
				EditorGUILayout.HelpBox("Please add Follower!", MessageType.Error);
				ready2 = false;
			}


			followerOffsetY = cartridgeOffsetY;
			followerOffsetZ = cartridgeOffsetZ;

		}
		EditorGUILayout.EndToggleGroup ();

		if (ready1 && !generateFollowerPoints) {
			if (GUILayout.Button ("Add Cartridges", GUILayout.ExpandWidth (true)))
				AddCartridges ();
			if (GUILayout.Button ("Clear Cartridges", GUILayout.ExpandWidth (true)))
				ClearCartridges (true);			
		} else if (ready1 && ready2) 
		{
			if (GUILayout.Button ("Add Cartridges and FollowerPoints", GUILayout.ExpandWidth (true))) 
			{	
				AddCartridges ();
				AddFollowerPoints ();
			}
			if (GUILayout.Button ("Clear Cartridges and FollowerPoints", GUILayout.ExpandWidth (true))) 
			{
				ClearCartridges (true);
				ClearFollowerPoints (true);
			}
			if (GUILayout.Button ("Remove FollowerPoint Visuals", GUILayout.ExpandWidth (true))) 
			{
				RemoveFollowerPointVisuals ();
			}
		}
		ready1 = true;
		ready2 = true;
	}



	private void AddCartridges()
	{
		ClearCartridges ();
		CartridgeObjectList = new GameObject[numberOfCartridges];
		CartridgeMeshFilterList = new MeshFilter[numberOfCartridges];
		CartridgeMeshRendererList = new MeshRenderer[numberOfCartridges];

		CartridgeObjectList [0] = firstCartridge;
		CartridgeMeshFilterList [0] = firstCartridge.GetComponent<MeshFilter> ();
		CartridgeMeshRendererList [0] = firstCartridge.GetComponent<MeshRenderer> ();


		cartridge_currentX = firstCartridge.transform.position.x;
		cartridge_currentY = firstCartridge.transform.position.y;
		cartridge_currentZ = firstCartridge.transform.position.z;

		if (cartridge_root == null) {
			cartridge_root = new GameObject ();
			cartridge_root.name = "Cartridge Root";
			cartridge_root.transform.parent = firstCartridge.transform.parent;
			cartridge_root.transform.localPosition = new Vector3 (0, 0, 0);
			cartridge_root.transform.localEulerAngles = new Vector3 (0, 0, 0);
			cartridge_root.transform.localScale = new Vector3 (1, 1, 1);
		}

		for (int i = 2; i <= numberOfCartridges; i++) {

			if (isCurved)
            {
				Vector3 curvedPos = CalculateNextCartridgePos(cartridgeAngleOffsetX*(i-1));

				cartridge_currentY = curvedPos.y;
				cartridge_currentZ = curvedPos.z;
			}
			else
            {
				cartridge_currentY += cartridgeOffsetY;
				cartridge_currentZ += cartridgeOffsetZ;
			}

			Quaternion rot = firstCartridge.transform.localRotation;
			Vector3 euler;
			if (isCurved) 
				euler = new Vector3(firstCartridge.transform.localRotation.eulerAngles.x - cartridgeAngleOffsetX * (i - 1), firstCartridge.transform.localRotation.eulerAngles.y, firstCartridge.transform.localRotation.eulerAngles.z);
			else 
				euler = new Vector3(firstCartridge.transform.localRotation.eulerAngles.x, firstCartridge.transform.localRotation.eulerAngles.y, firstCartridge.transform.localRotation.eulerAngles.z);
			rot.eulerAngles = euler;
			Vector3 pos = new Vector3 (cartridge_currentX, cartridge_currentY, cartridge_currentZ);
			GameObject nextCartridge = GameObject.Instantiate (firstCartridge, pos,rot,cartridge_root.transform);

			nextCartridge.name = firstCartridge.name + " (" + i.ToString() + ")";

			if (mirrorX && i % 2 == 0)
				nextCartridge.transform.localPosition = new Vector3(-nextCartridge.transform.localPosition.x, nextCartridge.transform.localPosition.y, nextCartridge.transform.localPosition.z);

			CartridgeObjectList[i-1] = nextCartridge;
			CartridgeMeshFilterList[i-1] = nextCartridge.GetComponent<MeshFilter> ();
			CartridgeMeshRendererList[i-1] = nextCartridge.GetComponent<MeshRenderer> ();
		}

		Magazine.DisplayBullets = CartridgeObjectList;
		Magazine.DisplayMeshFilters = CartridgeMeshFilterList;
		Magazine.DisplayRenderers = CartridgeMeshRendererList;
	}

	private void ClearCartridges(bool all = false)
	{
		if (cartridge_root != null) 
		{
			int children = cartridge_root.transform.childCount;
			for (int i = children -1 ; i >= 0; i--) {
				GameObject.DestroyImmediate (cartridge_root.transform.GetChild (i).gameObject);
			}
			if (all)
				DestroyImmediate (cartridge_root);
		}
	}

	private void AddFollowerPoints()
	{
		ClearFollowerPoints ();
		FollowerObjectList = new GameObject[numberOfCartridges];

		FollowerObjectList [0] = follower;


		follower_currentX = follower.transform.position.x;
		follower_currentY = follower.transform.position.y;
		follower_currentZ = follower.transform.position.z;

		if (follower_root == null) {
			follower_root = new GameObject ();
			follower_root.name = "Follower Root";
			follower_root.transform.parent = follower.transform.parent;
			follower_root.transform.localPosition = new Vector3 (0, 0, 0);
			follower_root.transform.localEulerAngles = new Vector3 (0, 0, 0);
			follower_root.transform.localScale = new Vector3 (1, 1, 1);
		}

		for (int i = 2; i <= numberOfCartridges; i++) {

			if (isCurved)
			{
				Vector3 curvedPos = CalculateNextCartridgePos(cartridgeAngleOffsetX * (i - 1));

				follower_currentY = curvedPos.y;
				follower_currentZ = curvedPos.z;
			}
			else
			{
				follower_currentY += followerOffsetY;
				follower_currentZ += followerOffsetZ;
			}


			Vector3 pos = new Vector3 (follower_currentX, follower_currentY, follower_currentZ);
			Quaternion rot = follower.transform.localRotation;
			Vector3 euler;
			if (isCurved) 
				euler = new Vector3(follower.transform.localRotation.eulerAngles.x - cartridgeAngleOffsetX * (i - 1), follower.transform.localRotation.eulerAngles.y, follower.transform.localRotation.eulerAngles.z);
			else 
				euler = new Vector3(follower.transform.localRotation.eulerAngles.x, follower.transform.localRotation.eulerAngles.y, follower.transform.localRotation.eulerAngles.z);
			rot.eulerAngles = euler;

			GameObject nextFollowerPoint = GameObject.Instantiate (follower,pos,rot,follower_root.transform);

			nextFollowerPoint.name = follower.name + " (" + i.ToString() + ")";

			FollowerObjectList[i-1] = nextFollowerPoint;
		}
	}

	private void ClearFollowerPoints(bool all = false)
	{
		if (follower_root != null) 
		{
			int children = follower_root.transform.childCount;
			for (int i = children -1 ; i >= 0; i--) {
				GameObject.DestroyImmediate (follower_root.transform.GetChild (i).gameObject);
			}
			if (all)
				DestroyImmediate (follower_root);
		}
	}
	private void RemoveFollowerPointVisuals()
	{
		if (follower_root == null)
			return;
		foreach (var f in FollowerObjectList) {
			if (f != follower) 
			{
				MeshRenderer renderer = f.GetComponent<MeshRenderer> ();
				if (renderer == null) 
					renderer = f.GetComponentInChildren<MeshRenderer>();
				DestroyImmediate (renderer);
				MeshFilter filter = f.GetComponent<MeshFilter> ();
				if (filter == null) 
					filter = f.GetComponentInChildren<MeshFilter>();
				DestroyImmediate(filter);
			}

		}
	}

	private void ShowGizmo(bool on)
    {
        switch (on)
        {
			case false:
				if (gizmo != null) DestroyImmediate(gizmo);
				break;
			case true:
				if (gizmo == null) gizmo = magazineRadiusCenter.gameObject.AddComponent<MagazineGizmos>();
				break;

			default:
                break;
        }
    }


	private Vector3 CalculateNextCartridgePos(float deltaA)
    {
		deltaA *= Mathf.Deg2Rad;
		Vector3 pos = new Vector3();

		Vector3 delta = firstCartridge.transform.position - magazineRadiusCenter.position;
		float radius = Mathf.Sqrt(Mathf.Pow(delta.y, 2) + Mathf.Pow(delta.z, 2));
		float angle = Mathf.Atan2(delta.y, delta.z);

		//ClearConsole();

		//Debug.Log("Radius: " + radius);
		//Debug.Log("Angle: " + Mathf.Rad2Deg*angle);

		float y = Mathf.Sin(angle + deltaA) * radius + magazineRadiusCenter.transform.position.y;
		float z = Mathf.Cos(angle + deltaA) * radius + magazineRadiusCenter.transform.position.z;



		pos = new Vector3(firstCartridge.transform.position.x, y, z);

        //Debug.Log("Before:");

        //Debug.Log(pos.y);
		//Debug.Log(pos.z);

		//GameObject test1 = GameObject.Instantiate(firstCartridge, pos, new Quaternion(), cartridge_root.transform);

		return pos;
    }

    void ClearConsole()
    {
		var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
		var type = assembly.GetType("UnityEditorInternal.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}


	void OnDestroy()
    {
		DestroyImmediate(gizmo);
    }
}
#endif