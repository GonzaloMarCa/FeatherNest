using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalaScript : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float velocidad = 8f;
    
    [Header("Efectos")]
    [SerializeField] private GameObject impactoEffect;
    [SerializeField] private AudioClip impactoAudio;
    
    private Rigidbody2D rb;
    private Vector2 direccion;
    private bool direccionConfigurada = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        // Si por algún motivo no se configuró la dirección, usar derecha por defecto
        if (!direccionConfigurada)
        {
            direccion = Vector2.right;
            direccionConfigurada = true;
        }
        
        // Aplicar velocidad
        if (rb != null)
        {
            rb.velocity = direccion * velocidad;
            rb.gravityScale = 0;
            Debug.Log($"Bala iniciada - Dirección: {direccion}, Velocidad: {velocidad}");
        }
        
        // Destruir la bala después de un tiempo
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Rotar la bala según su dirección de movimiento
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        }
    }
    
    void OnTriggerEnter2D(Collider2D objeto)
    {
        Debug.Log($"Bala impactó con: {objeto.tag}");
        
        // Si impacta con el jugador
        if (objeto.CompareTag("Player"))
        {
            PlayerMovement player = objeto.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Bala golpeó al jugador - Daño: {damage}");
            }
            Impacto();
        }
        // Si impacta con una pared, muro o suelo
        else if (objeto.CompareTag("Muro") || objeto.CompareTag("Suelo") || objeto.CompareTag("Untagged"))
        {
            Impacto();
        }
    }
    
    void Impacto()
    {
        // Efecto visual
        if (impactoEffect != null)
        {
            Instantiate(impactoEffect, transform.position, Quaternion.identity);
        }
        
        // Efecto de sonido
        if (impactoAudio != null)
        {
            AudioSource.PlayClipAtPoint(impactoAudio, transform.position);
        }
        
        // Destruir la bala
        Destroy(gameObject);
    }
    
    // Método para establecer la dirección desde el enemigo
    public void SetDireccion(Vector2 nuevaDireccion)
    {
        direccion = nuevaDireccion.normalized;
        direccionConfigurada = true;
        Debug.Log($"SetDireccion llamado: {direccion}");
        
        // Aplicar velocidad si el Rigidbody ya existe
        if (rb != null)
        {
            rb.velocity = new Vector2(direccion.x, direccion.y) * velocidad;
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
        if (rb != null && direccionConfigurada)
        {
            rb.velocity = direccion * velocidad;
        }
    }
}