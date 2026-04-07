using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generador : MonoBehaviour
{

    public GameObject mabirroPrefab;
    public GameObject mabirroInstanciado;
    void Start()
    {
        //mabirroInstanciado = GameObject.Instantiate (mabirroPrefab) as GameObject;
        for(int i = 0; i<100; i++)
        {
            GameObject.Instantiate(mabirroPrefab);
        }
    }

}
