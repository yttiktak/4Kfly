// COPYRIGHT 2018 Roberta Bennett
// repeatingshadow@protonmail.com
using System.Collections;
using System.Collections.Generic;
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



public class NewBehaviourScript : MonoBehaviour {

	public BB bb = new BB(); // EGAD, THIS SHOULD BE A SINGLETON. JUST A BUNCH OF UTILITY FUNCTIONS FOR MAKING HEX ARRAYS.
	
	public Camera thecam;							// camera that is to move to each lens cell position and render
	public RenderTexture tarTemplate;				// I build a tex2Darray based on this texture. Otherwise not used.

	public GameObject viewingPersonsPosition;		// Used if the playback screen is set up to mimic havin a lens array in front of it.
	public Vector3 cameraSetback = Vector3.zero;	// one trick is to set the fly cameras back towards the viewers position.
	public float toeIn = 0.0f;

	public float ZOpt = 39;
	public float ZMag = 14;

	// removing this multi-camera idea from the code piece by piece. Much still in place.
	public Camera skyboxCam;						// If these cameras are enabled, the fly camera takes settings from each in turn
	public Camera backgroundCam;					// to render different parts of the scene at different fov values. 
	public Camera midgroundCam;
	public Camera foregroundCam;

	private bool recording = false; // hard wired, use with caution
	private bool paused = false;

	public GameObject PlaybackScreen;				// The mosaic fly-image is painted onto this screen.
	public Camera PlaybackScreenCam;				// And observed by this camera, which renders to the final 4K screen.

	public float spacing = 1.0f;					// Lens array paramters. I use PlaybackScreen params to change size, so leave spacing = 1
	public int nWide = 7;
	public int nTall = 7;
	public bool pointyPartUp = false;

	public bool drawMyGizmos = false; // set true to always draw the camera array frustums

	public float bloomDistance = 10.0f;				// Asset blooming (big when close, small when far) is not totally set up.

	// public string recordPath = "/media/roberta/Seagate1/RecordedFromSlatherpi";

	//	private float screenDPI;
	public bool useReplacementShader = true;
	Shader replacementShader = null;

	public string recordPath = "";

	// I maintain a lot of stuff as global. Makes for a few side-effect procs.
	private Vector3[] translations; // array of virtual camera positions, and hex cell positions.
	private Vector3 txurhc,txulhc,txlrhc,txllhc; // four corners of extents of the translations array

	private RenderTexture tar;						// Made into a tex2dArray when created.
	private RenderTexture tar1;						// overflow tex2dArray n > 2048
	private CommandBuffer commandsAfter;			// Have camera render a view, then xfer to slice in above array via this commandBuffer

	private int nTot; // total number of viewpoints, eg cells
	static int nMax = 4096;

	private Vector3 cameraPositionZero;
	private float cameraSetbackDistance = 0.0f;

	private GameObject[] taggedToBloom;
	private Vector3[] bloomScales; 			// track starting scale of each GameObject tagged with 'bloom. Use w to track z too.'
	private float[] bloomSpots;				// and distance to camera at starting point.

//	private GameObject canvasGO;

	// SWISS ARMY KNIFE. These following take messages from my control panels in the game
	// kindof obsolete, there are keyboard controlls below these that are in use for the full screen version.
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
	public void ChangeSkyCamFOV (float news)
	{
		if (skyboxCam) {
			skyboxCam.fieldOfView = news;
		}
	}
	public void ChangeBGCamFOV (float news)
	{
		if (backgroundCam) {
			backgroundCam.fieldOfView = news;
		}
	}
	public void ChangeMGCamFOV (float news)
	{
		if (midgroundCam) {
			midgroundCam.fieldOfView = news;
		}
	}
	public void ChangeFGCamFOV (float news)
	{
		if ((foregroundCam) && (foregroundCam.enabled)) {
			foregroundCam.fieldOfView = news;
		} else {
			thecam.fieldOfView = news;
		}
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
		//	canvasGO.SetActive (false);
			Debug.Log ("recording");
			Time.captureFramerate = 12;
		} else {
			Debug.Log ("not recording");
		}
	}
		
	void LateUpdate() {
		Vector3 was; // for load/modify/write 
		float wasfov;
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
			int levelsInBuild = SceneManager.sceneCountInBuildSettings;
			Scene nowAtScene = SceneManager.GetActiveScene();
			int thisLevel = nowAtScene.buildIndex;
			int gotolevel = 0;
			if ((thisLevel < 0) || (thisLevel == levelsInBuild - 1)) {
				gotolevel = 0;
			} else {
				gotolevel = thisLevel + 1;
			}
			SceneManager.LoadScene(gotolevel);
		}
		if (Input.anyKey) {
			if (Input.inputString.Length < 1) {
				if (Input.GetKey (KeyCode.LeftArrow)) {
					was = PlaybackScreen.transform.localPosition;
					was.x -= 0.01f;
					PlaybackScreen.transform.localPosition = was;
					Debug.Log ("x y = "+was.x+ " " + was.y);
				}
				if (Input.GetKey (KeyCode.RightArrow)) {
					was = PlaybackScreen.transform.localPosition;
					was.x += 0.01f;
					PlaybackScreen.transform.localPosition = was;
					Debug.Log ("x y = " + was.x + " " + was.y);
				}
				if (Input.GetKey (KeyCode.UpArrow)) {
					was = PlaybackScreen.transform.localPosition;
					was.y += 0.01f;
					PlaybackScreen.transform.localPosition = was;
					Debug.Log ("x y = " + was.x + " " + was.y);
				}
				if (Input.GetKey (KeyCode.DownArrow)) {
					was = PlaybackScreen.transform.localPosition;
					was.y -= 0.01f;
					PlaybackScreen.transform.localPosition = was;
					Debug.Log ("x y = " + was.x + " " + was.y);
				}
				return;
			}

			string inchar = Input.inputString.Substring(0,1);
			switch (inchar) {
			case "o":
				PlaybackScreenCam.orthographicSize += 0.051f;
				Debug.Log ("orthographic size ="+PlaybackScreenCam.orthographicSize);
			break;
			case "l":
				PlaybackScreenCam.orthographicSize -= 0.051f;
				Debug.Log ("orthographic size ="+PlaybackScreenCam.orthographicSize);
			break;
			case "O":
				PlaybackScreenCam.orthographicSize += 0.5f;
				Debug.Log ("orthographic size ="+PlaybackScreenCam.orthographicSize);
			break;
			case "L":
				PlaybackScreenCam.orthographicSize -= 0.5f;
				Debug.Log ("orthographic size ="+PlaybackScreenCam.orthographicSize);
			break;

			case "q":
				wasfov = thecam.fieldOfView;
				wasfov += 0.51f;
				thecam.fieldOfView = wasfov;
				Debug.Log ("fov = " + wasfov);
			break;
			case "a":
				wasfov = thecam.fieldOfView;
				wasfov -= 0.51f;
				thecam.fieldOfView = wasfov;	
				Debug.Log ("fov = " + wasfov);		
			break;
			case "Q":
				wasfov = thecam.fieldOfView;
				wasfov += 2.51f;
				thecam.fieldOfView = wasfov;
				Debug.Log ("fov = " + wasfov);
			break;
			case "A":
				wasfov = thecam.fieldOfView;
				wasfov -= 2.51f;
				thecam.fieldOfView = wasfov;
				Debug.Log ("fov = " + wasfov);;
			break;

			}// end case

		}//end if input
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
		tar = new RenderTexture (tarTemplate); 
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
		tar1 = new RenderTexture (tarTemplate); 
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

		Assert.IsTrue (nT < nMax, "only have two 2048 texarrays. N too big");

		// negative spacing flags flatside up cell array.
		Mesh projectionMesh = new Mesh (); 
		bb.MakeHexMesh (translations, ref projectionMesh); 
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

	// GAHHHHHHHHH. OBSOLETE. NO DOCS. MUST LEARN NEW PATH TO NETWORKING. GAHHHHHHHH
	public int hostId;
	void SetupNetwork() {
		string ipaddrs = "*";
		NetworkTransport.Init();
		// get the last (or only) IP address
		IPAddress[] addrs = Dns.GetHostAddresses (Dns.GetHostName ());
		foreach(IPAddress myip in addrs) {
			if (myip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				Debug.Log (myip.ToString ());
				ipaddrs = myip.ToString ();
			}
		}
		ConnectionConfig config = new ConnectionConfig ();
		int channelId = config.AddChannel (QosType.Reliable);
		HostTopology topology = new HostTopology (config, 10);
		hostId = NetworkTransport.AddHost (topology, 8761, ipaddrs);
		// need to close port when done. Where? How? Tune in next episode to find out..
	}

	void Awake () 
	{			

		cameraSetbackDistance = Vector3.Distance(thecam.transform.parent.position+cameraSetback,cameraPositionZero);

		// I check for this shader even if I have the 'use replacement shader' unchecked. Could not log if, but do.
		replacementShader = Shader.Find("Custom/ReplacementShader");
		if (replacementShader == null) {
			Debug.Log ("Rplcmt not found");
			Application.Quit ();
		} else {
			Debug.Log ("using replacement shader named " + replacementShader.name);
		}

	}

	void Start ()
	{

		nTot = updateTranslations (); // side effect: creates the global translations array
		Debug.Log ("n cells " + nTot);
		MakeTex2DArrayFromCameraTarget (nTot);

		commandsAfter = new CommandBuffer ();
		thecam.AddCommandBuffer (CameraEvent.AfterEverything, commandsAfter);

		thecam.enabled = false;
		cameraPositionZero = thecam.transform.position;
		cameraSetbackDistance = Vector3.Distance(thecam.transform.parent.position+cameraSetback,cameraPositionZero);

		// bloom z collapse not implemented yet. Code lies in wait..
		taggedToBloom = GameObject.FindGameObjectsWithTag ("Bloom");
		bloomScales = new Vector3[taggedToBloom.Length];
		bloomSpots = new float[taggedToBloom.Length];
		for (int gob = 0; gob < taggedToBloom.Length; gob++) {
			bloomScales[gob] = taggedToBloom[gob].transform.GetChild(0).localScale;
			bloomSpots[gob] = Vector3.Distance(taggedToBloom[gob].transform.position,thecam.transform.position);
		}

	//	canvasGO = GameObject.Find ("Canvas"); // so I can turn off controlls when recording

		// SetupNetwork();
	}

	// IS THIS SENSE?? DOESNT IT DESTROY TEXTURE ARRAY (TAR) WHEN I CHANGE SCENES?? seems to work ok tho..
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
		Shader.SetGlobalFloat("_ZOpt", ZOpt); // replacement shader, if used, makes use of these.
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

		for (int i = 0; i < nTot; i ++) {

			thecam.transform.localPosition = translations [i] - cameraSetback;			// put camera at position
			thecam.transform.localEulerAngles = new Vector3 (translations [i].y, -translations [i].x, 0f) * toeIn;		// Dependent on Eulers being zero to start, and translations in xy, and small angles. 

			commandsAfter.Clear ();

			// WILL FAIL AT I > 4096. Assertion is in updateTranslations
			if (i < 2048) {
				commandsAfter.CopyTexture (BuiltinRenderTextureType.CameraTarget, 0, tar, i); 
			} else {
				commandsAfter.CopyTexture (BuiltinRenderTextureType.CameraTarget, 0, tar1, i - 2048);	
			}

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
			string name = string.Format("{0}/{1:D04}fly.png", recordPath, Time.frameCount);
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

	}// end Update

	void OnApplicationQuit(){

	}
}
