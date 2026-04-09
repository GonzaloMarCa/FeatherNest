using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generador : MonoBehaviour
{

    public GameObject mabirroPrefab;
    public GameObject mabirroInstanciado;
    void Start()
    {  
            InvokeRepeating(nameof(Spawn), 2.0f, 5f);
        
    }

    void Spawn()
    {
        GameObject.Instantiate(mabirroPrefab);
    }

}
