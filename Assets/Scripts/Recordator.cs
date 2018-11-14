using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

	private static string settingFilePath =  "/settingForFly4k.txt";
	private string persistentDataPath;

	// EXPECTING MESSAGES SENT HERE FROM SceneSelectorCanvasScript
	void CopyInto( ) {
		string sceneName = SceneManager.GetActiveScene ().name;
		string fullName;
		Slider[] sliders = GameObject.FindObjectsOfType<Slider> ();
		foreach (Slider slide in sliders) {
			fullName = sceneName + "-" + slide.name;
			if (settingsDictionary.ContainsKey(fullName)) {
				settingsDictionary.Remove(fullName);
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
		Slider[] sliders = GameObject.FindObjectsOfType<Slider> ();
		foreach (Slider slide in sliders) {
			fullName = sceneName + "-" + slide.name;
			if (settingsDictionary.ContainsKey (fullName)) {
				Debug.Log ("Set " + fullName);
				slide.value = settingsDictionary [fullName];
			} else {
				Debug.Log ("Add " + fullName);
				settingsDictionary.Add (fullName, slide.value);
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

	}
	void OnDisable() {

		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}

}
