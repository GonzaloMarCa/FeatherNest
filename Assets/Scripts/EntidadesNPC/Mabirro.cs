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
    
    [Header("Referencias")]
    private Rigidbody2D rb;
    private Animator animator;
    public Transform hijoVisual;
    private Transform player;
    
    [Header("Variables de Estado")]
    private Vector2 currentDirection;
    private float directionTimer;

    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = hijoVisual?.GetComponent<Animator>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        ChangeRandomDirection();
        directionTimer = changeDirectionInterval;
        
    
    }
    
    void Update()
    {
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
        rb.velocity = currentDirection * moveSpeed;
        Debug.Log(currentDirection.x);
    }
    
    bool IsPlayerDetected()
    {
        if (player == null) return false;
        
        // Detección por distancia
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // También puede detectar por raycast para ver si hay línea de visión
        if (distanceToPlayer <= detectionRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, 
                (player.position - transform.position).normalized, 
                detectionRange, 
                playerLayer | wallLayer);
            
            // Si el raycast impacta al jugador, hay línea de visión
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
        
        // Dirección hacia el jugador
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        // Verificar si hay un obstáculo en el camino
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, 
            Vector2.Distance(transform.position, player.position), wallLayer);
        
        if (hit.collider != null)
        {
            // Hay obstáculo, cambiar dirección aleatoria
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
        
        // Verificar si hay pared delante
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
        // Dirección aleatoria en 8 direcciones
        float angle = Random.Range(0, 360);
        currentDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), 
                                        Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        
    
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        if (currentDirection.x < 0.5f)
        {
            animator.SetFloat("Direction", -1);
        }
        else
        {
            animator.SetFloat("Direction", 1);
        }
        
        
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            ChangeRandomDirection();
            
            // Pequeño retroceso para no quedar pegado
            Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
            rb.velocity = bounceDirection * moveSpeed * 0.5f;
        }
        
        // Si colisiona con el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Dañar al jugador (cuando hagamos vida y tal)
            Debug.Log("Enemigo tocó al jugador");
            
            // Rebote al golpear al jugador
            Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
            rb.velocity = bounceDirection * moveSpeed * 1.5f;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar rango de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Visualizar raycast de pared
        if (Application.isPlaying)
        {
            Gizmos.color = IsWallInFront() ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, currentDirection * wallCheckDistance);
        }
    }
}
