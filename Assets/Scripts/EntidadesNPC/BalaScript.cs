using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalaScript : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float velocidad = 8f;
    
    
    private Rigidbody2D rb;
    private Vector2 direccion;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        direccion = new Vector2(transform.rotation.x, transform.rotation.y);
        // Aplica velocidad
        if (rb != null)
        {
            rb.velocity = direccion * velocidad;
        }
        
        // Destruye la bala después de un tiempo
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Rotar la bala según su dirección de movimiento
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Si impacta con el jugador
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            Impacto();
        }
        // Si impacta con una pared, muro o suelo
        else if (other.CompareTag("Wall") || other.CompareTag("Suelo") || other.CompareTag("Obstacle"))
        {
            Impacto();
        }
    }
    
    void Impacto()
    {
        // Destruye la bala
        Destroy(gameObject);
    }
    
    // Método para triangular la dirección desde el enemigo
    public void SetDireccion(Vector2 nuevaDireccion)
    {
        direccion = nuevaDireccion.normalized;
        
        // Aplica velocidad si el Rigidbody ya existe
        if (rb != null)
        {
            rb.velocity = direccion * velocidad;
        }
    }
    
    // Método para establecer el daño
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    // Método para establecer la velocidad
    public void SetVelocidad(float newVelocidad)
    {
        velocidad = newVelocidad;
        if (rb != null)
        {
            rb.velocity = direccion * velocidad;
        }
    }
}