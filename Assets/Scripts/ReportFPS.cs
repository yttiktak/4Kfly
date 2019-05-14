using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReportFPS : MonoBehaviour
{
    // Start is called before the first frame update
    Text theText;
    float delta;
   // float tPrior,tNow;
    void Start()
    {
        theText = gameObject.GetComponent<Text>();
        delta = 0.4f;
     //   tPrior = System.DateTime.Now.Ticks;
    }

    // Update is called once per frame
    void Update()
    {
     //   tNow = System.DateTime.Now.Ticks;
       delta =  Time.deltaTime * 0.2f + delta * 0.8f;
        theText.text = (1.0f / delta).ToString();
       // tPrior = tNow;
    }
}
