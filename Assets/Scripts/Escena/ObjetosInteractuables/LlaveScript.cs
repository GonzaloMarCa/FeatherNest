using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LlaveScript : MonoBehaviour
{
   // [SerializeField] Transform Posicion;
    public void DestroyKey()
    {
        Destroy(this.gameObject);
    }
    
    
    /* Hover de la llave pendiente
    void Update()
    {
        Posicion.position.y += 0.25;
    }
    */
}
