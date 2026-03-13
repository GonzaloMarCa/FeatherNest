using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    Vector3 main;
    float xMain;
    float yMain;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        main = GameObject.Find("Player").transform.position;

        xMain = main.x;
    
        yMain = main.y;
    
        this.transform.position = new Vector3(xMain, yMain, -15);
    }
}
