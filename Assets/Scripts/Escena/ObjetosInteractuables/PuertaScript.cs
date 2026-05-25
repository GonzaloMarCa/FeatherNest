using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuertaScript : MonoBehaviour
{
    [SerializeField] public GameObject puertaAbierta;
    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement jugador = collision.gameObject.GetComponent<PlayerMovement>();
            if(jugador.NumLlaves() > 0)
            {
                Instantiate(puertaAbierta, transform.position, Quaternion.identity);
                Destroy(this.gameObject);
            }
        }
    }
}
