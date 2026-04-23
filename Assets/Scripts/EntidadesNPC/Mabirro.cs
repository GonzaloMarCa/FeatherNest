using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mabirro : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float changeDirectionInterval = 2f;
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask playerLayer;
    bool empuje = false;
    
    [Header("Configuración de Salud")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private GameObject deathEffect;
    
    [Header("Configuración de Ataque")]
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float pushForceOnHit = 3f;
    
    [Header("Referencias")]
    private Rigidbody2D rb;
    private Animator animator;
    public Transform hijoVisual;
    private Transform player;
    
    [Header("Variables de Estado")]
    private Vector2 currentDirection;
    private float directionTimer;
    private int currentHealth;
    private bool isDead = false;
    private bool canAttack = true;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;


    public LayerMask capaEnemigo;
    
    void Start()
    {
        gameObject.layer = 8;
        rb = GetComponent<Rigidbody2D>();
        animator = hijoVisual?.GetComponent<Animator>();
        spriteRenderer = hijoVisual?.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        currentHealth = maxHealth;
        ChangeRandomDirection();
        directionTimer = changeDirectionInterval;
    }
    
    void Update()
    {
        if (isDead) return;
    

        bool playerDetected = IsPlayerDetected();

        if (playerDetected)
        {
            MoveTowardsPlayerWithAvoidance();
        }
        else
        {
            WanderMovementWithAvoidance();
        }
        
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        if (isDead || empuje) return;
        rb.velocity = new Vector2(0,0);
        rb.velocity = currentDirection * moveSpeed;
    }
    
    bool IsPlayerDetected()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, 
                (player.position - transform.position).normalized, 
                detectionRange, 
                playerLayer | wallLayer);
            
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    void MoveTowardsPlayerWithAvoidance()
    {
        if (player == null) return;
        
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, 
            Vector2.Distance(transform.position, player.position), wallLayer);
        
        if (hit.collider != null)
        {
            ChangeRandomDirection();
        }
        else
        {
            currentDirection = directionToPlayer;
        }
        
        directionTimer = changeDirectionInterval;
    }
    
    void WanderMovementWithAvoidance()
    {
        directionTimer -= Time.deltaTime;
        
        if (IsWallInFront())
        {
            ChangeRandomDirection();
            directionTimer = changeDirectionInterval;
        }
        else if (directionTimer <= 0)
        {
            ChangeRandomDirection();
            directionTimer = changeDirectionInterval;
        }
    }
    
    bool IsWallInFront()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, 
            wallCheckDistance, wallLayer);
        return hit.collider != null;
    }
    
    void ChangeRandomDirection()
    {
        float angle = Random.Range(0, 360);
        currentDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), 
                                        Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        if (currentDirection.x < -0.1f)
        {
            animator.SetFloat("Direction", -1);
        }
        else if (currentDirection.x > 0.1f)
        {
            animator.SetFloat("Direction", 1);
        }
        
        // Parámetro opcional para saber si está persiguiendo

    }
    
    // MÉTODO PARA RECIBIR DAÑO (llamado desde el jugador)
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Efecto visual de daño
        StartCoroutine(FlashRed());
        
        // Efecto de knockback
        if(empuje == false && currentHealth > 0)
        {
            ApplyKnockback();   
        }
        
        Debug.Log($"Mabirro recibió {damage} de daño. Salud restante: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Al recibir daño, cambiar dirección aleatoria (reacción de dolor)
            ChangeRandomDirection();
            
            // Activar animación de daño si existe
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }
    
    void ApplyKnockback()
    {
        if (player == null) return;

        empuje = true;

        // Dirección desde el jugador hacia el enemigo
        Vector2 knockbackDirection = (transform.position - player.position).normalized;
        rb.velocity = knockbackDirection * knockbackForce;
        
        // Pequeño retraso para no anular el knockback inmediatamente
        StartCoroutine(ResetKnockback());
    }
    
    IEnumerator ResetKnockback()
    {
        //bug knockback infinito corregido (motivo de celebración)
        //Primero espera para que afecte el knockback
        yield return new WaitForSeconds(0.3f);

        //Después reestablece el empuje y aplica la velocidad de 0 para que reanude la marcha. Por último, desactiva el bool.
        rb.velocity = new Vector2(0,0);
        rb.velocity = currentDirection * moveSpeed;

        //el error era que aqui creaba un nuevo bool en vez de usar el empuje.
        empuje = false;
    }



    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            //Recoge el sprite y le pinta de rojo temporalmente para indicar el golpe
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }
    
    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        
        // Desactivar collider para no interactuar más
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        // Activar animación de muerte, por el momento inútil
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Efecto de muerte
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Mabirro ha sido derrotado");
        
        // Destruir después de un pequeño retraso (para que se vea la animación)
        Destroy(gameObject, 0.5f);
    }
    
    // MÉTODO PARA DAÑAR AL JUGADOR
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        
        // Colisión con muros
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            ChangeRandomDirection();
            
            Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
            rb.velocity = bounceDirection * moveSpeed * 0.5f;
        }
        
        Debug.Log("comprobando si se pega con jugador");
        // Colisión con el jugador
        if (collision.gameObject.CompareTag("Player") && canAttack)
        {
            Debug.Log("Jugador chocado");
            // Dañar al jugador
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            
            if (playerMovement != null)
            {
                // Aquí llamarías a un método TakeDamage en el jugador
                playerMovement.TakeDamage(damageToPlayer);
                // playerMovement.TakeDamage(damageToPlayer);
                Debug.Log($"Mabirro atacó al jugador causando {damageToPlayer} de daño");
                
                // Aplicar cooldown al ataque
                StartCoroutine(AttackCooldown());
                
                // Empujar al jugador
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = pushDirection * pushForceOnHit;
                }
                
                // Rebote del enemigo
                Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
                rb.velocity = bounceDirection * moveSpeed * 1.5f;
            }
        }
    }
    
    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (Application.isPlaying)
        {
            Gizmos.color = IsWallInFront() ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, currentDirection * wallCheckDistance);
        }
    }
    
    // Método público para obtener si está vivo
    public bool IsAlive()
    {
        return !isDead;
    }
    
    // Método público para obtener la posición del enemigo
    public Vector2 GetPosition()
    {
        return transform.position;
    }
}