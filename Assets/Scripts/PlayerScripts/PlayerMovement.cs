using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    public static PlayerMovement instance;

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
    private UIhpTrack Vidas;
    [SerializeField] public GameObject marcoVidas;
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

    //Objetos usables
    private int llaves;
    

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {


        Vidas = marcoVidas.GetComponent<UIhpTrack>();
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
        if (!isAttacking && !isDashing)
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
            Debug.Log("¡Dash! Invulnerable al daño");
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // No bajar de 0
        
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
            Instantiate(bombPrefab, new Vector2(transform.position.x, (float)transform.position.y), Quaternion.identity);
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
        
        // Animaciones separadas del UpdateAnimations para no romper animaciones y que no se actualicen a mitad del roll
        if (animator != null)
        {
            Debug.Log("Animando dash");
            animator.SetFloat("Ataque", -1);
            animator.SetFloat("Velocidad", -1);
            animator.SetFloat("Horizontal", lastMoveDirection.x);
            animator.SetFloat("Vertical", lastMoveDirection.y);
        }

        isDashing = true;
        dashTime = dashDuration;
        dashCooldownTime = dashCooldown;
        
        // Poner invencibilidad
        gameObject.layer = 6;
      
        
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
        
        // Not commenting allat
        if(IsDashing())
        {
            return;
        }

            animator.SetFloat("Horizontal", lastMoveDirection.x);
            animator.SetFloat("Vertical", lastMoveDirection.y);

        if (IsAttacking())
        {
            animator.SetFloat("Ataque", 1);
            animator.SetFloat("Velocidad", 1);

        }   
            else if (velocidadActual > 0.1f && !IsAttacking())
        {
            animator.SetFloat("Ataque", -1);
            animator.SetFloat("Velocidad", 1);
            
        }
            else if(velocidadActual < 0.1f && !IsAttacking())
        {
            animator.SetFloat("Velocidad", -1);
            animator.SetFloat("Ataque", 1);
        }

            
    }

    public void Heal(int healAmount)
    {
        // Al ponerlo público y con healAmount dentro se puede llamar desde el objeto que haga la función de curar de forma genérica
        Debug.Log("curando");
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        Vidas.ActualizarVidas(healAmount);
        Debug.Log($"Jugador curado. Salud: {currentHealth}/{maxHealth}");

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Recoge la llave, borra la llave
        if (collision.gameObject.CompareTag("Key"))
        {
            llaves ++;
            collision.gameObject.GetComponent<LlaveScript>().DestroyKey();
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
        StartCoroutine(MorirConDelay());
    }

    IEnumerator MorirConDelay()
    {
        // Esperar 0.2 segundos
        yield return new WaitForSeconds(0.2f);
        
        // Cargar la escena con índice 2
        SceneManager.LoadScene(2);
    }


    // Métodos para el UI (exponer información de cooldown)
    public float GetAttackCooldownProgress()
    {
        if (canAttack) return 1f;
        if (attackCooldownTimer <= 0) return 1f;
        return 1f - (attackCooldownTimer / attackCooldown);
    }

    public float GetAttackCooldownRemaining()
    {
        if (canAttack) return 0f;
        return attackCooldownTimer > 0 ? attackCooldownTimer : 0f;
    }

    public float GetBombCooldownProgress()
    {
        if (bombCooldownTimer <= 0) return 1f;
        return 1f - (bombCooldownTimer / bombCooldown);
    }

    public float GetBombCooldownRemaining()
    {
        return bombCooldownTimer > 0 ? bombCooldownTimer : 0f;
    }

    // Para que el UI pueda verificar si el ataque está disponible
    public bool IsAttackReady()
    {
        return canAttack && !isAttacking;
    }

    // Para la bomba
    public bool IsBombReady()
    {
        return bombCooldownTimer <= 0;
    }    
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int NumLlaves()
    {
        return llaves;
    }

    public void UsarLlaves()
    {
        llaves -= 1;
    }


}