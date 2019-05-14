using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/***
 * Recordator attaches to a handy persistent canvas. Its job is to find all sliders in all scenes
 * and record or set their values. 
 * 
 * 
 * 
***/

public class SerializableDictionary<TKey,TValue> : SortedDictionary<TKey,TValue>,ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();
    [SerializeField]
    private List<TValue> values = new List<TValue>();
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach(KeyValuePair<TKey,TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
    public void OnAfterDeserialize()
    {
        this.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }
}

[System.Serializable] public class DictionaryOfStringAndFloat : SerializableDictionary<string, float> { }


public class Recordator : MonoBehaviour {

	public static Recordator settingsRecord; // enforce singleton

    //  public static SortedDictionary<string,float> settingsDictionary;
    public static DictionaryOfStringAndFloat settingsDictionary;

	private static string settingFilePath =  "/settingFor4Kfly.txt";
	private string persistentDataPath;

	Slider[] sliders; 
	int SizeSI;			// Rather than build yet another dictionary to track wich slider has which name, just
	int XSI;			// track each one I want to use vie keyboard. 
	int YSI;
	int ZSI;
	int ASI;
	int TiltSI;
	int ForegroundSI;
	int ToeSI;

	// EXPECTING MESSAGES SENT HERE FROM PersistSceneSelectorCanvasScript
	void CopyInto( ) {
		string sceneName = SceneManager.GetActiveScene ().name;
		string fullName;
		sliders = GameObject.FindObjectsOfType<Slider> ();
		for (int i = 0; i < sliders.Length; i++) {
			Slider slide = sliders [i];
			fullName = sceneName + "-" + slide.name;
			if (settingsDictionary.ContainsKey (fullName)) {
				settingsDictionary.Remove (fullName);
			} 
			settingsDictionary.Add (fullName, slide.value);
		}

	}
		
	public void WriteToFile() {
		Debug.Log ("Write it out");
		CopyInto ();
		string json = JsonUtility.ToJson (settingsDictionary, true);
		System.IO.File.WriteAllText (persistentDataPath + settingFilePath, json);
	}
	public void LoadFromFile() {
		string json = System.IO.File.ReadAllText (persistentDataPath + settingFilePath);
		Debug.Log ("load " + json);
		settingsDictionary = (DictionaryOfStringAndFloat)JsonUtility.FromJson<DictionaryOfStringAndFloat> (json);
		SetSliderValues (SceneManager.GetActiveScene ().name);
	}

	void SetSliderValues(string sceneName ) {
		string fullName;
		sliders = GameObject.FindObjectsOfType<Slider> ();
		for (int i = 0; i < sliders.Length; i++) {
			Slider slide = sliders [i];
			fullName = sceneName + "-" + slide.name;
			if (settingsDictionary.ContainsKey (fullName)) {
				Debug.Log ("Set " + fullName);
				slide.value = settingsDictionary [fullName];
			} else {
				Debug.Log ("Add " + fullName);
				settingsDictionary.Add (fullName, slide.value);
			}
			Debug.Log ("setting index of slider named " + slide.name);
			switch (slide.name) {
			case "Size":
				SizeSI = i;
				break;
			case "X":
				XSI = i;
				break;
			case "Y":
				YSI = i;
				break;
			case "Foreground":
				ForegroundSI = i;
				break;
			case "Z":
				ZSI = i;
				break;
			}


		}
	}

	void OnSceneUnloaded(Scene scene) {
		CopyInto ();
	}
	void OnSceneLoaded( Scene scene, LoadSceneMode mode) {
		SetSliderValues (scene.name);
	}

	void Start () {
		settingsDictionary = new DictionaryOfStringAndFloat();
		if (System.IO.File.Exists (persistentDataPath + settingFilePath)) {
			LoadFromFile ();
		} else {
			Debug.Log ("could not find " + persistentDataPath + settingFilePath);
			WriteToFile (); // CopyInto ();
		}
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}
	

	void Awake() {
		persistentDataPath = Application.persistentDataPath;
		Debug.Log (persistentDataPath);
		if (settingsRecord == null) {
			settingsRecord = this;
			DontDestroyOnLoad (gameObject);
		} else {
			Object.Destroy(gameObject);
		}

	}

	void Update() {
		if (Input.anyKey) {
			if (Input.GetKey (KeyCode.LeftArrow)) {
				sliders [XSI].value -= 0.01f;
				Debug.Log ("X " + sliders [XSI].value + " Y:" + sliders [YSI].value);
			}
			if (Input.GetKey (KeyCode.RightArrow)) {
				sliders [XSI].value += 0.01f;
				Debug.Log ("X " + sliders [XSI].value + " Y:" + sliders [YSI].value);
			}
			if (Input.GetKey (KeyCode.UpArrow)) {
				sliders [YSI].value += 0.01f;
				Debug.Log ("X " + sliders [XSI].value + " Y:" + sliders [YSI].value);
			}
			if (Input.GetKey (KeyCode.DownArrow)) {
				sliders [YSI].value -= 0.01f;
				Debug.Log ("X " + sliders [XSI].value + " Y:" + sliders [YSI].value);
			}
			if (Input.inputString.Length < 1) {
				return;
			}
		string inchar = Input.inputString.Substring(0,1);
		switch (inchar) {
		case "o":
			sliders [SizeSI].value += 0.05f;
			Debug.Log ("orthographic size ="+sliders [SizeSI].value);
			Debug.Log ("slider named " + sliders [SizeSI].name + "changed");
		break;
		case "l":
			sliders [SizeSI].value -= 0.05f;
			Debug.Log ("orthographic size ="+sliders [SizeSI].value);
		break;
		case "O":
			sliders [SizeSI].value += 0.5f;
			Debug.Log ("orthographic size ="+sliders [SizeSI].value);
		break;
		case "L":
			sliders [SizeSI].value -= 0.5f;
			Debug.Log ("orthographic size ="+sliders [SizeSI].value);
		break;

			case "q":
				sliders [ForegroundSI].value += 0.2f;
				Debug.Log("fov " + sliders [ForegroundSI].value);
			break;
			case "a":
				sliders [ForegroundSI].value -= 0.2f;
				Debug.Log("fov " + sliders [ForegroundSI].value);
			break;
			case "Q":
				sliders [ForegroundSI].value += 2.0f;
				Debug.Log("fov " + sliders [ForegroundSI].value);
			break;
			case "A":
				sliders [ForegroundSI].value -= 2.0f;
				Debug.Log("fov " + sliders [ForegroundSI].value);
			break;

			case "z":
				sliders [ZSI].value += 1.0f;
				Debug.Log("Z " + sliders [ZSI].value);
			break;
			case "x":
				sliders [ZSI].value -= 1.0f;
				Debug.Log("Z " + sliders [ZSI].value);
			break;
			case "Z":
				sliders [ZSI].value += 10.0f;
				Debug.Log("Z " + sliders [ZSI].value);
			break;
			case "X":
				sliders [ZSI].value -= 10.0f;
				Debug.Log("Z " + sliders [ZSI].value);
			break;

		}// end case
		}// end if anykey
	}

	void OnDisable() {

		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}

}
