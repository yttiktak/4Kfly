using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/***
 * SceneSelectorCanvasScript attaches to the scene selector canvas (persistent)
 * and arranges to change scenes on button clicks 
 * 
 * 
 * 
 ***/

public class PersistSceneSelectorCanvas : MonoBehaviour {

	public static PersistSceneSelectorCanvas canvasInstance; // enforce singleton

	public bool paused = false;

	public void MyLoadScene(int level)
	{
		gameObject.SendMessage("CopyInto");// sending to Recordator, a script attached to this game object.
		SceneManager.LoadScene(level);
	}

	public void LoadNextScene()
	{
		int levelsInBuild = SceneManager.sceneCountInBuildSettings;
		Scene nowAtScene = SceneManager.GetActiveScene();
		int thisLevel = nowAtScene.buildIndex;
		int gotolevel = 0;
		if ((thisLevel < 0) || (thisLevel == levelsInBuild - 1)) {
			gotolevel = 0;
		} else {
			gotolevel = thisLevel + 1;
		}
		MyLoadScene(gotolevel);
	}

	public void PauseResume() 
	{
		if (paused) {
			Time.timeScale = 1;
		} else {
			Time.timeScale = 0;
		}
		paused = !paused;
	}

	void Awake() {
		DontDestroyOnLoad (gameObject);
			
		if (canvasInstance == null) {
			canvasInstance = this;
		} else {
			Object.Destroy (gameObject);
			Debug.Log ("destroy canvas");
		}

	}
}
