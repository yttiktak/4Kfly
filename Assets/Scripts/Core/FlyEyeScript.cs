// COPYRIGHT 2018 Roberta Bennett
// repeatingshadow@protonmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.ComponentModel;

// archive. Used in non srp versions

[ExecuteInEditMode]
public class FlyEyeScript : MonoBehaviour {

	public BB bb = new BB(); // EGAD, THIS SHOULD BE A SINGLETON.

	public Camera thecam;							// camera that is to move to each lens cell position and render
	public RenderTexture tarTemplate;				// I build a tex2Darray based on this texture. Otherwise not used.

	public GameObject viewingPersonsPosition;		// Used if the playback screen is set up to mimic havin a lens array in front of it.
	public Vector3 cameraSetback = Vector3.zero;	// one trick is to set the fly cameras back towards the viewers position.
	public float toeIn = 0.0f;

	public float ZOpt = 39;
	public float ZMag = 14;


	private bool recording = false; // hard wired, do not use

	public GameObject PlaybackScreen;				// The mosaic fly-image is painted onto this screen.
	public Camera PlaybackScreenCam;				// And observed by this camera, which renders to the final 4K screen.

	public float spacing = 1.0f;					// Lens array paramters. I use PlaybackScreen params to change size, so leave spacing = 1
	public int nWide = 7;
	public int nTall = 7;
	public bool pointyPartUp = false;

	public bool drawMyGizmos = false; // set true to always draw the camera array frustums

	public float bloomDistance = 10.0f;				// Asset blooming (big when close, small when far) is not totally set up.

	//	private float screenDPI;
	public bool useReplacementShader = true;
	Shader replacementShader = null;

	// I maintain a lot of stuff as global. Makes for a few side-effect procs.
	private Vector3[] translations; // array of virtual camera positions, and hex cell positions.
	private Vector3 txurhc,txulhc,txlrhc,txllhc; // four corners of extents of the translations array

	private RenderTexture tar;						// Made into a tex2dArray when created.
	private RenderTexture tar1;						// overflow tex2dArray n > 2048
	private CommandBuffer commandsAfter;			// Have camera render a view, then xfer to slice in above array via this commandBuffer

	private int nTot; // total number of viewpoints, eg cells

	// private Vector3 cameraPositionZero;

	private GameObject[] taggedToBloom;
	private Vector3[] bloomScales; 			// track starting scale of each GameObject tagged with 'bloom. Use w to track z too.'
	private float[] bloomSpots;				// and distance to camera at starting point.

	private GameObject canvasGO;

	// SWISS ARMY KNIFE. These following take messages from my control panels in the game
	public void ChangeScreenCamSize( float news ) {
		PlaybackScreenCam.orthographicSize = news;
	}
	public void ChangeScreenCamX( float news ) {
		Vector3 was = PlaybackScreen.transform.localPosition;
		was.x = news;
		PlaybackScreen.transform.localPosition = was;
	}
	public void ChangeScreenCamY( float news ) {
		Vector3 was = PlaybackScreen.transform.localPosition;
		was.y = news;
		PlaybackScreen.transform.localPosition = was;
	}
	public void ChangeScreenCamA( float news ) {
		Vector3 was = PlaybackScreen.transform.eulerAngles;
		was.y = news;
		PlaybackScreen.transform.eulerAngles = was;
	}
	public void ChangeScreenCamT( float news ) {
		Vector3 was = PlaybackScreen.transform.eulerAngles;
		was.z = news;
		PlaybackScreen.transform.eulerAngles = was;
	}

	public void ChangeFlyCamZ( float news ) {
		cameraSetback.z = news;
	}
	public void ChangeToein( float news ) {
		toeIn = news;
	}
	public void ChangeZOpt( float news ) {
		ZOpt = news;
		Shader.SetGlobalFloat("_ZOpt", ZOpt);
	}
	public void ChangeZMag( float news ) {
		ZMag = news;
		Shader.SetGlobalFloat("_ZMag", ZMag);
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
	public void RecordButtonClick ()
	{
		recording = !recording;
		if (recording) {
			canvasGO.SetActive (false);
			Debug.Log ("recording");
			Time.captureFramerate = 12;
		} else {
			Debug.Log ("not recording");
		}
	}

	// Commence building the 3D camera

	// First, build the tex2dArray
	void MakeTex2DArrayFromCameraTarget (int slices)
	{
		bool tic;
		if (tar != null) {
			tic = tar.IsCreated ();
			if (tic) {
				Debug.Log ("tar is already created. Release it");
				tar.Release (); 
			}
		}
		tar = new RenderTexture (tarTemplate); // fails on Ubuntu version. Umm. but not failing on 2017.3
		tar.useMipMap = false;
		tar.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray; 
		if (slices < 2048) {
			tar.volumeDepth = slices; // max is 2048!
		} else {
			tar.volumeDepth = 2048; // max is 2048!
		}
		//	tar.enableRandomWrite = false; // for some depth buffer image map idea. not needed normally
		tar.Create ();

		Shader.SetGlobalTexture ("_my2darray",tar);

		/*** ok, two tars to get past 2040 works. Now make them an array of tars maybe? **/
		tar1 = new RenderTexture (tarTemplate); // fails on Ubuntu version. Umm. but now not failing on 2017.3
		tar1.useMipMap = false;
		tar1.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray; 
		if (slices < 2048) {
			tar1.volumeDepth = 1; // place holder
		} else {
			tar1.volumeDepth = slices-2048; // max is 2048!
		}
		tar1.Create ();
		Shader.SetGlobalTexture ("_my2darray1",tar1);
		/*** another day for the tar array array ***/
	}

	int updateTranslations ()
	{
		int nT; // becomes nTot
		float spacingSign = 1.0f;
		if (!pointyPartUp)
			spacingSign = -1.0f;
		nT = bb.MakeTranslations (spacingSign * spacing, nWide, nTall, ref translations); 
		// negative spacing flags flatside up cell array.
		Mesh projectionMesh = new Mesh (); 
		bb.MakeHexMesh (translations, ref projectionMesh);
		Debug.Log("UV four is folling, item 7 of it");
		Debug.Log(projectionMesh.uv4[7]);
		// takes its flat side info clue from the step between translations
		PlaybackScreen.GetComponent<MeshFilter> ().sharedMesh = projectionMesh;

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
			Gizmos.matrix = thecam.transform.parent.localToWorldMatrix;
			Gizmos.DrawLine(txulhc,txurhc);
			Gizmos.DrawLine(txllhc,txlrhc);
			Gizmos.DrawLine(txurhc,txlrhc);
			Gizmos.DrawLine(txulhc,txllhc);

		}
	}
	void Awake () 
	{

		replacementShader = Shader.Find("Custom/ReplacementShader");
		if (replacementShader == null) {
			Debug.Log ("Rplcmt not found");
			Application.Quit ();
		} else {
			Debug.Log ("replacement shader is named " + replacementShader.name);
		}

	}

	void Start ()
	{

		nTot = updateTranslations (); // side effect: reates the global translations array
		Debug.Log ("n cells " + nTot);
		MakeTex2DArrayFromCameraTarget (nTot);

		commandsAfter = new CommandBuffer ();
		thecam.AddCommandBuffer (CameraEvent.AfterEverything, commandsAfter);

		thecam.enabled = false;
		// cameraPositionZero = thecam.transform.position;

		taggedToBloom = GameObject.FindGameObjectsWithTag ("Bloom");
		bloomScales = new Vector3[taggedToBloom.Length];
		bloomSpots = new float[taggedToBloom.Length];
		for (int gob = 0; gob < taggedToBloom.Length; gob++) {
			bloomScales[gob] = taggedToBloom[gob].transform.GetChild(0).localScale;
			bloomSpots[gob] = Vector3.Distance(taggedToBloom[gob].transform.position,thecam.transform.position);
		}

		canvasGO = GameObject.Find ("Canvas"); // so I can turn off controlls when recording

	}

	void OnDestroy() {
		if (tar != null) {
			tar.Release ();
			DestroyImmediate (tar);
		}
		if (tar1 != null) {
			tar1.Release ();
			DestroyImmediate (tar1);
		}
	}

	void OnValidate () // what to do if params change, like number of clipzones
	{
		Shader.SetGlobalFloat("_ZOpt", ZOpt);
		Shader.SetGlobalFloat("_ZMag", ZMag);
	}

	void Update ()
	{
		// apply bloom to objects tagged for Bloom
		float dcam;
		for (int gob = 0; gob < taggedToBloom.Length; gob++) {
			dcam = Vector3.Distance (taggedToBloom [gob].transform.position, thecam.transform.position);
			taggedToBloom [gob].transform.GetChild (0).localScale = Vector3.one * (bloomSpots [gob] + bloomDistance) / (dcam + bloomDistance);
		}

		// There are issues with the tar texture not being available at times
		// when I try to re-create live.

		if (commandsAfter == null) { // dont know why sometimes not there
			Debug.Log("in scene " + SceneManager.GetActiveScene().name);
			Debug.Log ("had to reinstate commandsAfter");
			commandsAfter = new CommandBuffer ();
			thecam.RemoveCommandBuffers (CameraEvent.AfterEverything);
			thecam.AddCommandBuffer (CameraEvent.AfterEverything, commandsAfter);
		}
		if ((tar == null) | (!tar.IsCreated ())) {
			Debug.Log("in scene " + SceneManager.GetActiveScene().name);
			Debug.Log ("re creating tar");
			tar.Create ();
			Shader.SetGlobalTexture ("_my2darray", tar);
		}

		Shader.SetGlobalFloat("_ZOpt", ZOpt);
		Shader.SetGlobalFloat("_ZMag", ZMag);


		//	For each position in the lens array go there and take a picture
		// putting it all into tar, the array of viewws from each lenslet position.

		for (int i = 0; i < nTot; i++) {
			// put camera at position
			thecam.transform.localPosition = translations [i] - cameraSetback;
			// Dependent on Eulers being zero to start, and translations in xy, and small angles. 
			thecam.transform.localEulerAngles = new Vector3 (translations [i].y, -translations [i].x, 0f) * toeIn;


			commandsAfter.Clear ();

			if (i < 2048) { 
				commandsAfter.CopyTexture (BuiltinRenderTextureType.CameraTarget, 0, tar, i); 
			} else {
				commandsAfter.CopyTexture (BuiltinRenderTextureType.CameraTarget, 0, tar1, i - 2048);	
			} // WILL FAIL AT I > 4096
				
			if (useReplacementShader) {
				thecam.RenderWithShader (replacementShader, "");
			} else {
				thecam.Render ();
			}

		}

		// restore camera position, just 'cause.
		thecam.transform.localPosition = Vector3.zero;
		thecam.transform.LookAt (2.0f * thecam.transform.position - viewingPersonsPosition.transform.position);

		if (recording) {
			string name = string.Format("{0}/{1:D04}fly.png", "/media/roberta/Seagate1/RecordedFromSlatherpi", Time.frameCount);
			ScreenCapture.CaptureScreenshot(name);
		}

		if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit ();
		}
		if (Input.GetKey (KeyCode.Space)) {
			if (recording) {
				recording = false;
				canvasGO.SetActive (true);
			}
		}

	}// end Update
}
