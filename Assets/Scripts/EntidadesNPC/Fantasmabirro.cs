using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FantasmaBirro : MonoBehaviour
{
    [Header("Configuración de Estados")]
    [SerializeField] private float tiempoTransicion = 0.5f;
    [SerializeField] private float distanciaAgacharse = 3f;
    [SerializeField] private float distanciaNormal = 6f;
    
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeedNormal = 2f;
    [SerializeField] private float moveSpeedAgachado = 0.5f;
    
    [Header("Configuración de Salud")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float knockbackForce = 5f;
    
    [Header("Configuración de Disparo")]
    [SerializeField] private GameObject balaPrefab;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float tiempoEntreDisparos = 2f;
    [SerializeField] private float rangoDisparo = 8f;
    [SerializeField] private float velocidadBala = 8f;
    [SerializeField] private int damageToPlayer = 1;
    
    [Header("Efectos Visuales")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private float flashDuration = 0.1f;
    
    [Header("Referencias")]
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerScript;
    
    [Header("Variables de Estado")]
    private EstadoEnemigo estadoActual = EstadoEnemigo.Normal;
    private bool isInvulnerable = false;
    private bool enTransicion = false;
    private Vector2 direccionMovimiento;
    private int currentHealth;
    private bool isDead = false;
    private bool puedeDisparar = true;
    private Color originalColor;
    
    private enum EstadoEnemigo
    {
        Crouching,
        Spawn,
        Normal,
        Crouch
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        if (animator == null)
        {
            Debug.LogError("FantasmaBirro: No se encontró Animator en los hijos");
            return;
        }
        
        // Busca al jugador y recoge el script para aplicar el daño a posterior
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerMovement>();
        }
        
        // Crear punto de disparo si no existe
        if (puntoDisparo == null)
        {
            GameObject punto = new GameObject("PuntoDisparo");
            punto.transform.SetParent(transform);
            punto.transform.localPosition = new Vector3(0.5f, 0, 0);
            puntoDisparo = punto.transform;
        }
        
        
        // Estado inicial
        currentHealth = maxHealth;
        estadoActual = EstadoEnemigo.Normal;
        isInvulnerable = false;
        
        // Configurar parámetros iniciales del Animator
        animator.SetBool("Agachado", false);
        animator.SetBool("Andando", true);
        
        CambiarDireccionAleatoria();
    }
    
    void Update()
    {
        if (player == null || animator == null || isDead) return;
        
        // Calcular distancia al jugador
        float distanciaAlJugador = Vector2.Distance(transform.position, player.position);
        
        // Manejar cambios de estado basados en distancia
        if (!enTransicion)
        {
            if (distanciaAlJugador < distanciaAgacharse && estadoActual == EstadoEnemigo.Normal)
            {
                StartCoroutine(Agacharse());
            }
            else if (distanciaAlJugador > distanciaNormal && estadoActual == EstadoEnemigo.Crouching)
            {
                StartCoroutine(Levantarse());
            }
        }
        
        // Manejar disparo (solo cuando está normal y no en transición)
        if (!enTransicion && !isDead && estadoActual == EstadoEnemigo.Normal)
        {
            if (distanciaAlJugador <= rangoDisparo && puedeDisparar)
            {
                StartCoroutine(Disparar());
            }
        }
        
        // Manejar el movimiento según el estado actual
        if (!enTransicion && !isDead)
        {
            switch (estadoActual)
            {
                case EstadoEnemigo.Crouching:
                    MoverAgachado();
                    rb.velocity = direccionMovimiento * moveSpeedAgachado;
                    break;
                    
                case EstadoEnemigo.Normal:
                    MoverNormal();
                    rb.velocity = direccionMovimiento * moveSpeedNormal;
                    break;
                    
                default:
                    rb.velocity = Vector2.zero;
                    break;
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        ActualizarAnimacion();
    }
    
    IEnumerator Disparar()
    {
        
        puedeDisparar = false;
        
        // Activa trigger de disparo en el Animator
        animator.SetTrigger("Disparo");
        
        // Esperar un momento para que la animación comience
        yield return new WaitForSeconds(0.1f);
        puntoDisparo.transform.position = this.transform.position;
        // Instancia bala si existe el prefab
        if (balaPrefab != null && puntoDisparo != null && player != null)
        {
            // Calcula dirección hacia el jugador
            Vector2 direccion = (player.position - puntoDisparo.position).normalized;
            float posBala = 1;
            if (player.position.x < puntoDisparo.position.x)
            {
                posBala = -1;
            }
            GameObject bala = Instantiate(balaPrefab, new Vector2(puntoDisparo.position.x + 5 * posBala, puntoDisparo.position.y), Quaternion.identity);
           /* 
            // Orientar la bala hacia la dirección
            float angle = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            bala.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Aplicar velocidad a la bala (CORREGIDO, no recibía velocidad al instanciarse)
            Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();
            if (rbBala != null)
            {
                rbBala.velocity = direccion * velocidadBala;  // ← Cambiado de linearVelocity a velocity
            }
            */
            // Configurar el daño de la bala
            BalaScript balaScript = bala.GetComponent<BalaScript>();
            if (balaScript != null)
            {
                balaScript.SetDamage(damageToPlayer);
            }
            
        }
        
        // Espera el tiempo entre disparos
        yield return new WaitForSeconds(tiempoEntreDisparos);
        puedeDisparar = true;
    }
    
    IEnumerator Agacharse()
    {
        if (enTransicion || isDead) yield break;
        
        enTransicion = true;
        estadoActual = EstadoEnemigo.Crouch;
        
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Crouch");
        animator.SetBool("Andando", false);

        
        yield return new WaitForSeconds(tiempoTransicion);
        
        estadoActual = EstadoEnemigo.Crouching;
        isInvulnerable = true;
        enTransicion = false;
        
        animator.SetBool("Agachado", true);
        
    }
    
    IEnumerator Levantarse()
    {
        if (enTransicion || isDead) yield break;
        
        enTransicion = true;
        estadoActual = EstadoEnemigo.Spawn;
        
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Spawn");
        animator.SetBool("Agachado", false);
        
        
        yield return new WaitForSeconds(tiempoTransicion);
        
        estadoActual = EstadoEnemigo.Normal;
        isInvulnerable = false;
        enTransicion = false;
        
        animator.SetBool("Andando", true);

    }
    
    void MoverAgachado()
    {
        if (Random.Range(0f, 1f) < 0.02f)
        {
            CambiarDireccionAleatoria();
        }
    }
    
    void MoverNormal()
    {
        if (Random.Range(0f, 1f) < 0.01f)
        {
            CambiarDireccionAleatoria();
        }
    }
    
    void CambiarDireccionAleatoria()
    {
        int direccion = Random.Range(0, 8);
        switch (direccion)
        {
            case 0: direccionMovimiento = Vector2.up; break;
            case 1: direccionMovimiento = Vector2.down; break;
            case 2: direccionMovimiento = Vector2.left; break;
            case 3: direccionMovimiento = Vector2.right; break;
            case 4: direccionMovimiento = new Vector2(1, 1).normalized; break;
            case 5: direccionMovimiento = new Vector2(1, -1).normalized; break;
            case 6: direccionMovimiento = new Vector2(-1, 1).normalized; break;
            case 7: direccionMovimiento = new Vector2(-1, -1).normalized; break;
        }
        
        animator.SetFloat("Horizontal", direccionMovimiento.x);
        animator.SetFloat("Vertical", direccionMovimiento.y);
    }
    
    void ActualizarAnimacion()
    {
        if (animator == null || isDead) return;
        
        if (enTransicion) return;
        
        switch (estadoActual)
        {
            case EstadoEnemigo.Crouching:
                animator.SetBool("Agachado", true);
                animator.SetBool("Andando", false);
                break;
                
            case EstadoEnemigo.Normal:
                animator.SetBool("Agachado", false);
                animator.SetBool("Andando", true);
                break;
        }
    }
    
    // ========== MÉTODOS DE DAÑO Y VIDA ==========
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        if (isInvulnerable)
        {
            StartCoroutine(FeedbackInvulnerable());
            return;
        }
        
        if (estadoActual == EstadoEnemigo.Normal)
        {
            currentHealth -= damage;
            
            StartCoroutine(FlashRed());
            AplicarKnockback();
            
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                CambiarDireccionAleatoria();
                
                if (animator != null)
                {
                    animator.SetTrigger("Hurt");
                }
            }
        }
    }
    
    void AplicarKnockback()
    {
        if (player == null) return;
        
        Vector2 knockbackDirection = (transform.position - player.position).normalized;
        rb.velocity = knockbackDirection * knockbackForce;
        
        StartCoroutine(ResetKnockback());
    }
    
    IEnumerator ResetKnockback()
    {
        float originalSpeed = moveSpeedNormal;
        moveSpeedNormal = 0;
        moveSpeedAgachado = 0;
        yield return new WaitForSeconds(0.15f);
        moveSpeedNormal = originalSpeed;
        moveSpeedAgachado = originalSpeed / 4f;
    }
    
    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }
    
    IEnumerator FeedbackInvulnerable()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.gray;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
        }
    }
    
    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("FantasmaBirro ha sido derrotado");
        Destroy(gameObject, 0.5f);
    }
    
    // ========== MÉTODOS PÚBLICOS ==========
    
    public void ForzarSalirAgachado()
    {
        if (isDead) return;
        
        if (estadoActual == EstadoEnemigo.Crouching && !enTransicion)
        {
            StopAllCoroutines();
            enTransicion = false;
            StartCoroutine(Levantarse());
        }
    }
    
    public void ForzarAgacharse()
    {
        if (isDead) return;
        
        if (estadoActual == EstadoEnemigo.Normal && !enTransicion)
        {
            StopAllCoroutines();
            enTransicion = false;
            StartCoroutine(Agacharse());
        }
    }
    
    public bool IsVulnerable()
    {
        return !isInvulnerable && estadoActual == EstadoEnemigo.Normal && !isDead;
    }
    
    public bool IsCrouching()
    {
        return estadoActual == EstadoEnemigo.Crouching;
    }
    
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
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaAgacharse);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaNormal);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDisparo);
        
        if (puntoDisparo != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoDisparo.position, 0.2f);
        }
    }
}