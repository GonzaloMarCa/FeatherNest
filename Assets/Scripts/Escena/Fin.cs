using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fin : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool matarAlEntrar = true;
    [SerializeField] private bool destruirAlMatar = true;
    [SerializeField] private float tiempoDestruccion = 0.5f;

    private bool haMatado = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (haMatado) return;
        
        if (other.CompareTag("Player") && matarAlEntrar)
        {
            MatarJugador(other.gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (haMatado) return;
        
        if (collision.gameObject.CompareTag("Player") && matarAlEntrar)
        {
            MatarJugador(collision.gameObject);
        }
    }
    
    void MatarJugador(GameObject jugador)
    {
        haMatado = true;
        
        PlayerMovement playerScript = jugador.GetComponent<PlayerMovement>();

        
        // Mata al jugador
        if (playerScript != null)
        {
            playerScript.Die();
            Debug.Log($"Jugador murió por contacto con {gameObject.name}");
        }
        
        // Destruir este objeto si está configurado
        if (destruirAlMatar)
        {
            Destroy(gameObject, tiempoDestruccion);
        }
    }
    
    // Visualización en el editor
    void OnDrawGizmos()
    {
        // Mostra el trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            if (col is BoxCollider2D)
            {
                BoxCollider2D box = (BoxCollider2D)col;
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (col is CircleCollider2D)
            {
                CircleCollider2D circle = (CircleCollider2D)col;
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
}