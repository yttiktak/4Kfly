// COPYRIGHT 2018 Roberta Bennett
// repeatingshadow@protonmail.com
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.ComponentModel;


public class PlaybackScreenSRP : MonoBehaviour {

	public BB bb = new BB(); // EGAD, THIS SHOULD BE A SINGLETON. JUST A BUNCH OF UTILITY FUNCTIONS FOR MAKING HEX ARRAYS.
	
	public Camera theMVCamera;							// camera that is to move to each lens cell position and render

	public GameObject viewingPersonsPosition;		// Used if the playback screen is set up to mimic havin a lens array in front of it.
	public Vector3 cameraSetback = Vector3.zero;	// one trick is to set the fly cameras back towards the viewers position.
	public float toeIn = 0.0f;


	private bool recording = false; // hard wired, use with caution
	private bool paused = false;
	public string recordPath = "";
	public bool stopMesh = false;

	public GameObject PlaybackScreen;				// The mosaic fly-image is painted onto this screen.
	public Camera PlaybackScreenCam;				// And observed by this camera, which renders to the final 4K screen.

	public float spacing = 1.0f;					// Lens array paramters. I use PlaybackScreen params to change size, so leave spacing = 1
	public int nWide = 7;
	public int nTall = 7;
	public bool pointyPartUp = false;

	public bool drawMyGizmos = false; // set true to always draw the camera array frustums


	// I maintain a lot of stuff as global. Makes for a few side-effect procs.
	private Vector3[] translations; // array of virtual camera positions, and hex cell positions.
	private Vector3 txurhc,txulhc,txlrhc,txllhc; // four corners of extents of the translations array

	// private int nTot; // total number of viewpoints, eg cells
	static int nMaxSlicesPerArray = 2048;
	static int nMax = 4096;

//	private Vector3 cameraPositionZero; // Commented out to avoid compiler warnings for not using it.
//	private float cameraSetbackDistance = 0.0f;


	// SWISS ARMY KNIFE. These following take messages from my control panels in the game
	// kindof obsolete, there are keyboard controlls below these that are in use for the full screen version.
	public void ChangeScreenCamSize( float news ) {
		PlaybackScreenCam.orthographicSize = news;
	}
	public void ChangeScreenX( float news ) {
		Vector3 was = PlaybackScreen.transform.localPosition;
		was.x = news;
		PlaybackScreen.transform.localPosition = was;
	}
	public void ChangeScreenY( float news ) {
		Vector3 was = PlaybackScreen.transform.localPosition;
		was.y = news;
		PlaybackScreen.transform.localPosition = was;
	}
	public void ChangeScreenA( float news ) {
		Vector3 was = PlaybackScreen.transform.eulerAngles;
		was.y = news;
		PlaybackScreen.transform.eulerAngles = was;
	}
	public void ChangeScreenT( float news ) {
		Vector3 was = PlaybackScreen.transform.eulerAngles;
		was.z = news;
		PlaybackScreen.transform.eulerAngles = was;
	}

	/**
	public void ChangeFlyCamZ( float news ) {
		cameraSetback.z = news;
		//theMVCamera.GetComponent<MultiviewScript>().cameraSetback = cameraSetback;
	}
	public void ChangeFlyCamX( float news ) { // setback is also used to change camera position for different mosaic taking lens positions
		cameraSetback.x = news;
		//theMVCamera.GetComponent<MultiviewScript>().cameraSetback = cameraSetback;
	}
	**/

	public void ChangeToein( float news ) {
		toeIn = news;
	}


	bool newFov = true;
	public void ChangeFGCamFOV (float news)
	{
		theMVCamera.fieldOfView = news;
		newFov = true;
	}
	public void changeShaderK2(float news) 
	{
		MeshRenderer mer = PlaybackScreen.GetComponent<MeshRenderer> ();
		mer.sharedMaterial.SetFloat("_k2",news);
	}
	public void changeShaderCentripital( float news )
	{
		MeshRenderer mer = PlaybackScreen.GetComponent<MeshRenderer> ();
		mer.sharedMaterial.SetFloat("_centripital",news);
	}

	bool calibrate = false;
	GameObject theatre;
	public void toggleCalibrate( ) {
		MeshRenderer mer = PlaybackScreen.GetComponent<MeshRenderer> ();
		calibrate = !calibrate;
		mer.sharedMaterial.SetInt("_calibrate",calibrate?1:0);
		if (theatre != null) {
			theatre.SetActive (!calibrate); // seems to turn off and then not back on!
		} else {
			Debug.Log("theatre was null gag");
		}
	}


	public void RecordButtonClick ()
	{
		recording = !recording;
		if (recording) {
			if (!Directory.Exists (recordPath)) {
				Debug.Log ("directory for recording not there. Trying to create it");
				Directory.CreateDirectory (recordPath);
			}
			Debug.Log ("recording at 24 fps");
			Time.captureFramerate = 24;
		} else {
			Debug.Log ("not recording");
		}
	}

	void LateUpdate() {
		if (Input.GetKeyDown (KeyCode.Question)) {
			Debug.Log ("R record, P pause, N next scene, o/l lens size, q/a fov, arrow keys move lens");
		}
		// R record, P pause, N next scene, o/l lens size, q/a fov, arrow keys move lens
		if (Input.GetKeyDown (KeyCode.R)) {
			RecordButtonClick ();
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			if (paused) {
				Time.timeScale = 1;
			} else {
				Time.timeScale = 0;
			}
			paused = !paused;
		}
		if (Input.GetKeyDown (KeyCode.N)) {
			PersistSceneSelectorCanvas.canvasInstance.LoadNextScene();
		}
	}

	// Commence building the 3D camera

		
	int MakeTranslations()
	{
		int nT; // becomes nTot
		float spacingSign = 1.0f;
		if (!pointyPartUp)
			spacingSign = -1.0f;
		nT = bb.MakeTranslations (spacingSign * spacing, nWide, nTall, ref translations); 

		Assert.IsTrue (nT < nMax, "only have two 2048 texarrays. N too big");
		Debug.Log("made texture and array for size "+nT);

		// negative spacing flags flatside up cell array.

		MeshFilter meshFilter = PlaybackScreen.GetComponent<MeshFilter> ();

		// Mesh projectionMesh = new Mesh ();
		Mesh projectionMesh = meshFilter.sharedMesh;
		bb.MakeHexMesh (translations, ref projectionMesh);  // takes its flat side info clue from the step between translations

		meshFilter.sharedMesh = projectionMesh;  // doing this on validate / awake is problematic. sharedMesh, mesh, what to do?
		meshFilter.sharedMesh.UploadMeshData(false);


		RenderTexture newCamTex = new RenderTexture(theMVCamera.targetTexture);
		theMVCamera.targetTexture.DiscardContents();
		theMVCamera.targetTexture.Release();

		newCamTex.dimension = TextureDimension.Tex2DArray;
		newCamTex.volumeDepth = (nT>nMaxSlicesPerArray) ? nMaxSlicesPerArray : nT;
		newCamTex.Create();
		theMVCamera.targetTexture = newCamTex;
		PlaybackScreen.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTexA",newCamTex); // complains I leak materials in edit mode if just material vs sharedmaterial

		if (nT>nMaxSlicesPerArray) {
			RenderTexture auxTex = new RenderTexture(newCamTex);
			auxTex.dimension = TextureDimension.Tex2DArray;
			auxTex.volumeDepth = (nT>nMaxSlicesPerArray) ? nMaxSlicesPerArray : nT-nMaxSlicesPerArray;
			auxTex.name = "script made me";
			auxTex.Create();
			theMVCamera.GetComponent<MultiviewScript>().auxCamTex = auxTex;
			PlaybackScreen.GetComponent<Renderer>().sharedMaterial.SetTexture("_AuxTexA",auxTex); // complains I leak materials in edit mode if just material vs sharedmaterial
		}

		theMVCamera.GetComponent<MultiviewScript>().cameraPositions = translations;
		theMVCamera.GetComponent<MultiviewScript>().CameraSetback = cameraSetback;



		// now find four corners, for drawing the gizmo representation of the camera frustums
		// to show the extent of what the lens array is looking at
		Vector3 txmin = translations[0];
		Vector3 txmax = translations[nT-1];
		for (int i = 0; i < nT; i++) {
			txmin = Vector3.Min(txmin,translations[i]);
			txmax = Vector3.Max(txmax,translations[i]);
		}
		txulhc = new Vector3(txmin.x,txmax.y,0);
		txurhc = new Vector3(txmax.x,txmax.y,0);
		txllhc = new Vector3(txmin.x,txmin.y,0);
		txlrhc = new Vector3(txmax.x,txmin.y,0);

		return nT;
	}
		
	void OnDrawGizmos ()
	{
		// draw four corners of virtual camera array to show the field of views
		// DrawFrustum methods seem to be in-correct. Frustrating.
		if  (drawMyGizmos) {
			Gizmos.color = Color.green;
			Gizmos.matrix = theMVCamera.transform.parent.localToWorldMatrix;
		  	Gizmos.DrawLine(txulhc,txurhc);
			Gizmos.DrawLine(txllhc,txlrhc);
			Gizmos.DrawLine(txurhc,txlrhc);
			Gizmos.DrawLine(txulhc,txllhc);
		}
	}



//	void Awake () {}


	void Start ()
	{
	//	OnValidate();

		//cameraSetbackDistance = Vector3.Distance(theMVCamera.transform.parent.position+cameraSetback,cameraPositionZero);

		// MakeTex2DArrayFromCameraTarget (nTot); instead must inform MVcamera(s) of textureArray size and translations

		// cameraPositionZero = theMVCamera.transform.position; // not currently in use. Commented out to avoid warnings.
		//cameraSetbackDistance = Vector3.Distance(theMVCamera.transform.parent.position+cameraSetback,cameraPositionZero);
		// nTot =
		theatre = GameObject.Find ("THEATRE");
		MakeTranslations();
	}

	void OnDestroy() {

	}

	bool dirtyTranslations = false;
	void OnValidate () // what to do if params change, like number of clipzones
	{
		dirtyTranslations = true;
	//	nTot = MakeTranslations();

	}
		
	void Update ()
	{

		if (dirtyTranslations) {
			// Nasty. But the shared mesh insists on sending messages, which can not be done in OnValidate or Awake.
			MakeTranslations(); // side effect: creates the global translations array
			dirtyTranslations = false;
		}

		if ((!stopMesh) || (newFov)) {
			// all done in SRP now. theMVCamera(s) do the work
		}

		if (recording) {
			string name = string.Format("{0}/{1:D04}f.png", recordPath, Time.frameCount);
       		 ScreenCapture.CaptureScreenshot(name);
		}

		if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit ();
		}
		if (Input.GetKey (KeyCode.Space)) {
			if (recording) {
				recording = false;
				//canvasGO.SetActive (true);
			}
		}
		if (Input.GetKey (KeyCode.M)) {
			stopMesh = !stopMesh;
			paused = stopMesh;
			if (paused) {
				Time.timeScale = 1;
			} else {
				Time.timeScale = 0;
			}
		}
		if (Input.GetKey (KeyCode.C)) {
			toggleCalibrate ();
		}

	}// end Update

	void OnApplicationQuit(){

	}
}
