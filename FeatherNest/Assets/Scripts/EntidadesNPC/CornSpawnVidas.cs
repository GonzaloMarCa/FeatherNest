using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornSpawnVidas : MonoBehaviour
{
    [Header("Configuración de Salud")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int damageToPlayer = 0; // No ataca, pero por si acaso
    [SerializeField] private float knockbackForce = 5f;
    
    [Header("Efectos Visuales")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color damageColor = Color.red;
    
    [Header("Referencias")]
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private PlayerMovement playerScript;
    private Color originalColor;
    
    [Header("Variables de Estado")]
    private int currentHealth;
    private bool isDead = false;
    private bool isInvulnerable = false;
    private float invulnerableTime = 0.5f; // Tiempo de invulnerabilidad tras recibir daño
    
    void Start()
    {
        // Busca componentes
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Busca al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerMovement>();
        }
        
        // Configura la salud
        currentHealth = maxHealth;
        
    }
    
    // ========== MÉTODOS DE DAÑO Y SALUD ==========
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        if (isInvulnerable)
        {
            Debug.Log("Generador está invulnerable - ¡No recibe daño!");
            return;
        }
        
        currentHealth -= damage;
        Debug.Log($"Generador recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");
        
        // Efecto visual de daño
        StartCoroutine(FlashDamage());
        
        
        // Activar invulnerabilidad temporal
        StartCoroutine(InvulnerabilidadTemporal());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    
    IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }
    
    IEnumerator InvulnerabilidadTemporal()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }
    
    void Die()
    {
        isDead = true;
        
        // Desactiva el collider para no interactuar
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Desactiva el generador de enemigos
        Generador generador = GetComponent<Generador>();
        if (generador != null)
        {
            enabled = false; // Desactiva el script del generador
        }
        
        Debug.Log("Generador ha sido destruido");
        
        // Destruir después de un pequeño retraso
        Destroy(gameObject, 0.5f);
    }
    
    // ========== MÉTODOS PARA LA BOMBA ==========
    
    // Forzar salir de invulnerabilidad
    public void ForzarVulnerable()
    {
        isInvulnerable = false;
        StopCoroutine(InvulnerabilidadTemporal());
    }
    
    // Comprueba si está vivo
    public bool IsAlive()
    {
        return !isDead;
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    // ========== REFERENCIA AL ANIMATOR ==========
    private Animator animator;
    
    void Awake()
    {
        // Busca al animator en los hijos
        animator = GetComponent<Animator>();
    }
    
    // ========== COLISIÓN CON EL JUGADOR ==========
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        // Ahora mismo está integrado pero no hace nada porque el daño está en 0. Se puede añadir el daño por contacto.
        if (collision.gameObject.CompareTag("Player") && damageToPlayer > 0)
        {
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageToPlayer);
                Debug.Log($"Generador dañó al jugador causando {damageToPlayer} de daño");
                
                // Empuja al jugador
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = pushDirection * knockbackForce;
                }
            }
        }
    }
    
    // Visualización desde editor
    void OnDrawGizmosSelected()
    {
        // Muestra el rango de generación
        Generador generador = GetComponent<Generador>();
        if (generador != null)
        {
            // Usa los valores del generador si están disponibles
            // Esto es sólo para referencia visual, sólo sale en el editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }
        
        // Muestra el radio de detección del generador
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
