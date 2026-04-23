using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 0.5f;
    
    

    [Header("Ataque con Partículas")]
    [SerializeField] private ParticleAttack particleAttack;
    [SerializeField] private float attackDuration = 0.3f; // Reduce a 0.3
    [SerializeField] private float attackCooldown = 0.5f; // Reduce a 0.5


    [Header("Referencias")]
    private Rigidbody2D rb;
    private Animator animator;
    public Transform hijoVisual;

    [Header("Salud del Jugador")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;


    [Header("Bomba")]
    [SerializeField] private GameObject bombPrefab; //Bombardo Gerardo
    [SerializeField] private float bombCooldown = 5f; // Tiempo de cooldown de la bomba
    private float bombCooldownTimer = 0f; // Temporizador para el cooldown
    
    // Variables de estado
    private Vector2 movementInput;
    private Vector2 lastMoveDirection;
    private bool isDashing = false;
    private float dashTime;
    private float dashCooldownTime;
    
    // Variables de ataque
    private bool isAttacking = false;
    private float attackTimer;
    private float attackCooldownTimer;
    private bool canAttack = true;
    
    void Start()
    {

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = hijoVisual.GetComponent<Animator>();
        if(animator == null) 
        {
            Debug.LogError("El hijo no posee un componente Animator");
        }
        
        // Crear attackPoint si no existe
       
        
        // Dirección inicial por defecto
        lastMoveDirection = Vector2.down;
    }
    
    void Update()
    {
        // Manejar el temporizador del ataque
        HandleAttackTimers();
        
        // Solo procesar input si no está en dash ni atacando
        if (!isDashing && !isAttacking)
        {
            // Obtener input de movimiento
            movementInput.x = Input.GetAxisRaw("Horizontal");
            movementInput.y = Input.GetAxisRaw("Vertical");
            
            // Normalizar para movimiento diagonal uniforme
            if (movementInput.magnitude > 1f)
                movementInput = movementInput.normalized;
            
            // Guardar última dirección de movimiento si se está moviendo
            if (movementInput != Vector2.zero)
            {
                lastMoveDirection = movementInput;
            }
            
            // Inicio dash (solo si no está atacando)
            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTime <= 0)
            {
                StartDash();
            }
            
            // Inicio ataque
              if (Input.GetMouseButtonDown(0) && particleAttack != null && canAttack)
            {
                StartAttack();
            }
        }
        
        // Cooldown del dash
        if (dashCooldownTime > 0)
        {
            dashCooldownTime -= Time.deltaTime;
        }

        // Cooldown de la bomba
        if (bombCooldownTimer > 0)
        {
            bombCooldownTimer -= Time.deltaTime;
        }
        
        // Terminar dash
        if (isDashing)
        {
            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                StopDash();
            }
        }

        // Tecla F para poner bomba
        if (Input.GetKeyDown(KeyCode.F)) 
        {
            SetBomb();
        }
        
        // Actualizar animaciones
        if (!isAttacking)
        {
            UpdateAnimations();
        }
     
    }
    
    void FixedUpdate()
    {
        // Solo mover si no está atacando
        if (!isAttacking)
        {
            if (isDashing)
            {
                // Movimiento rápido durante el dash
                rb.velocity = lastMoveDirection * dashSpeed;
            }
            else
            {
                // Movimiento normal
                rb.velocity = movementInput * moveSpeed;
            }
        }
        else
        {
            // Detener movimiento durante el ataque
            rb.velocity = Vector2.zero;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDashing)
        {
            // Si está dash, no recibe daño (invulnerabilidad)
            Debug.Log("¡Dash! Invulnerable al daño");
            return;
        }
        
        currentHealth -= damage;
        Debug.Log($"Jugador recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");
        
        // Efecto visual de daño
        StartCoroutine(FlashRed());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        SpriteRenderer sr = hijoVisual.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            //problema respecto al color arreglado :D:D
            Color originalColor = Color.white;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }
    
    void HandleAttackTimers()
    {
        // Temporizador de duración del ataque
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
        }
        
        // Temporizador de cooldown del ataque
        if (!canAttack)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                canAttack = true;
            }
        }
    }
    
    

    void StartAttack()
    {
        isAttacking = true;
        canAttack = false;
        attackTimer = attackDuration;
        attackCooldownTimer = attackCooldown;
        
        // Iniciar el ataque con partículas
        if (particleAttack != null)
        {
            particleAttack.StartAttack(lastMoveDirection);
        }
        
        UpdateAnimations();
        
        
        Debug.Log("¡Ataque iniciado!");
    }


    void SetBomb()
    {
        // Revisa si está en cooldown
        if (bombCooldownTimer > 0)
        {
            Debug.Log($"Bomba en cooldown. Espera {bombCooldownTimer:F1} segundos");
            return;
        }
        
        // Revisa si el prefab existe
        if (bombPrefab != null)
        {
            Instantiate(bombPrefab, new Vector2(transform.position.x, (float)(transform.position.y - 0.5)), Quaternion.identity);
            bombCooldownTimer = bombCooldown;
            Debug.Log("¡Bomba colocada!");
        }
        else
        {
            Debug.LogError("No se ha asignado el prefab de la bomba en el inspector");
        }
    }
    
    void StartDash()
    {
        // No se puede dashear mientras se ataca
        if (isAttacking) return;
        
        isDashing = true;
        dashTime = dashDuration;
        dashCooldownTime = dashCooldown;
        
        // Poner invencibilidad
        gameObject.layer = 6;
        
        // Efecto visual simple (que aun TAMPOCO existe)
        if (animator != null)
        {
           
        }
        
        Debug.Log("¡Dash iniciado en dirección: " + lastMoveDirection);
    }
    
    void StopDash()
    {
        isDashing = false;
        
        // Quitar invuln
        gameObject.layer = 3;
        
        // Pequeño frenado al terminar el dash (queda bonito)
        rb.velocity = Vector2.zero;
    }
    
    void UpdateAnimations()
    {
        float velocidadActual = movementInput.magnitude;
        
        if (isAttacking)
        {
            // Activar animación de ataque
        
            animator.SetFloat("Horizontal_Idle", 0);
            animator.SetFloat("Vertical_Idle", 0);
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            animator.SetFloat("Ataque", 1);
            animator.SetFloat("AttHorizontal", lastMoveDirection.x);
            animator.SetFloat("AttVertical", lastMoveDirection.y);
        
        }   else if (velocidadActual > 0.1f && !isAttacking)
        {
            animator.SetFloat("Horizontal_Idle", 0);
            animator.SetFloat("Vertical_Idle", 0);
            animator.SetFloat("Velocidad", 1);
            animator.SetFloat("Horizontal", movementInput.x);
            animator.SetFloat("Vertical", movementInput.y);
            animator.SetFloat("Ataque", 0);
        }
            else if(velocidadActual < 0.1f && !isAttacking)
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            animator.SetFloat("Velocidad", 0);
            animator.SetFloat("Horizontal_Idle", lastMoveDirection.x);
            animator.SetFloat("Vertical_Idle", lastMoveDirection.y);
            animator.SetFloat("Ataque", 0);
        }
    }
    // Método público para obtener la dirección del último movimiento/ataque
    public Vector2 GetFacingDirection()
    {
        return lastMoveDirection;
    }
    
    // Ver si está en dash
    public bool IsDashing()
    {
        return isDashing;
    }
    
    // Ver si está atacando
    public bool IsAttacking()
    {
        return isAttacking;
    }
    



    void Die()
    {
        Debug.Log("Jugador ha muerto");
        // Aquí puedes añadir lógica de muerte (reiniciar nivel, mostrar pantalla de game over, etc.)
        Time.timeScale = 0; // Pausar el juego como ejemplo
    }
}