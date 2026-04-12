using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 0.5f;
    
    [Header("Referencias")]
    private Rigidbody2D rb;
    private Animator animator;
    
    // Variables de estado
    private Vector2 movementInput;
    private Vector2 lastMoveDirection;
    private bool isDashing = false;
    private float dashTime;
    private float dashCooldownTime;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Dirección inicial por defecto
        lastMoveDirection = Vector2.down;
    }
    
    void Update()
    {
        // Solo procesar input si no está en dash
        if (!isDashing)
        {
            // Obtener input de movimiento
            movementInput.x = Input.GetAxisRaw("Horizontal");
            movementInput.y = Input.GetAxisRaw("Vertical");
            
            // Normalizar para movimiento diagonal uniforme
            movementInput = movementInput.normalized;
            
            // Guardar última dirección de movimiento si se está moviendo
            if (movementInput != Vector2.zero)
            {
                lastMoveDirection = movementInput;
            }
            
            // Inicio dash
            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTime <= 0)
            {
                StartDash();
            }
        }
        
        // Cooldown del dash
        if (dashCooldownTime > 0)
        {
            dashCooldownTime -= Time.deltaTime;
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
        
        // Actualizar animaciones (falta meter las animaciones)
        //UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        if (isDashing)
        {
            // Movimiento rápido durante el dash
            rb.linearVelocity = lastMoveDirection * dashSpeed;
        }
        else
        {
            // Movimiento normal
            rb.linearVelocity = movementInput * moveSpeed;
        }
    }
    void StartDash()
    {
        isDashing = true;
        dashTime = dashDuration;
        dashCooldownTime = dashCooldown;
        
        // Poner invuln
        // gameObject.layer = LayerMask.NameToLayer("Invulnerable");
        
        // Efecto visual simple (opcional)
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }
        
        Debug.Log("¡Dash iniciado en dirección: " + lastMoveDirection);
    }
    
    void StopDash()
    {
        isDashing = false;
        
        // Quitar invuln
        // gameObject.layer = LayerMask.NameToLayer("Player");
        
        // Pequeño frenado al terminar el dash (opcional)
        rb.linearVelocity = Vector2.zero;
    }
    
    /* Falta meter animaciones
    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Actualizar parámetros de animación
            animator.SetFloat("MoveX", movementInput.x);
            animator.SetFloat("MoveY", movementInput.y);
            animator.SetFloat("LastMoveX", lastMoveDirection.x);
            animator.SetFloat("LastMoveY", lastMoveDirection.y);
            animator.SetBool("IsMoving", movementInput != Vector2.zero);
            animator.SetBool("IsDashing", isDashing);
        }
    }
    */
    
    //Recoger dirección para hacer ataques o dash
    public Vector2 GetFacingDirection()
    {
        return lastMoveDirection;
    }
    
    //Ver si está en dash
    public bool IsDashing()
    {
        return isDashing;
    }
}