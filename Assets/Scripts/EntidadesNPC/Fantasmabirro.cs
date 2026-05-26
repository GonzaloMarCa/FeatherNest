using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FantasmaBirro : MonoBehaviour
{
    [Header("Configuración de Estados")]
    [SerializeField] private float tiempoTransicion = 0.5f; // Lo que tarda en cambiar entre estados
    [SerializeField] private float distanciaAgacharse = 10f; // Cuando el jugador pasa de este rango hace el cambio a agacharse
    [SerializeField] private float distanciaNormal = 14f; // Cuando el jugador sale de este rango se vuelve a levantar
    
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeedNormal = 2f; // Se mueve muy lento cuando está de pie
    [SerializeField] private float moveSpeedAgachado = 0f; // No se mueve 
    
    [Header("Referencias")]
    private Rigidbody2D rb; 
    private Animator animator;
    private Transform player;
    private SpriteRenderer spriteRenderer;  // Referencia al SpriteRenderer del hijo
    
    [Header("Variables de Estado")]
    private EstadoEnemigo estadoActual = EstadoEnemigo.Normal;  // Máquina de estados :D:D:D:D:D (Aquí no se puede usar blend tree)
    private bool isInvulnerable = false;
    private bool enTransicion = false;
    private Vector2 direccionMovimiento;
    
    private enum EstadoEnemigo
    {
        Crouching, // Transición levantado -> agachado
        Spawn, // Transición agachado -> levantado
        Normal, // No lo vas a adivinar
        Crouch
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Buscar el Animator en el hijo (donde está el SpriteRenderer)
        animator = GetComponentInChildren<Animator>();
        
        // Buscar el SpriteRenderer en el hijo
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // Por si algo está mal configurado y falla, se avisa
        if (animator == null)
        {
            Debug.LogError("FantasmaBirro: No se encontró Animator en los hijos");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("FantasmaBirro: No se encontró SpriteRenderer en los hijos");
        }
        
        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Configurar estado inicial
        estadoActual = EstadoEnemigo.Normal;
        isInvulnerable = false;
        
        // Elegir dirección inicial aleatoria
        CambiarDireccionAleatoria();
        
        // Actualizar animación inicial
        ActualizarAnimacion();
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Mide distancia al jugador
        float distanciaAlJugador = Vector2.Distance(transform.position, player.position);
        
        // Si está cerca se agacja
        bool deberiaAgacharse = distanciaAlJugador < distanciaAgacharse;
        
        // Manejar cambios de estado basados en distancia
        if (!enTransicion)
        {
            if (deberiaAgacharse && (estadoActual == EstadoEnemigo.Normal))
            {
                StartCoroutine(TransicionEstado(EstadoEnemigo.Crouch, EstadoEnemigo.Crouching));
            }
            else if (!deberiaAgacharse && (estadoActual == EstadoEnemigo.Crouching) && (distanciaAlJugador > distanciaNormal))
            {
                StartCoroutine(TransicionEstado(EstadoEnemigo.Spawn, EstadoEnemigo.Normal));
            }
        }
        
        // Maneja el movimiento según el estado actual
        switch (estadoActual)
        {
            // Máquina de estados para el movimiento
            case EstadoEnemigo.Crouching:
                MoverAgachado();
                break;
                
            case EstadoEnemigo.Normal:
                MoverNormal();
                break;
                
            case EstadoEnemigo.Spawn:
            case EstadoEnemigo.Crouch:
                break;
        }
        
        // Aplicar movimiento
        if (!enTransicion)
        {
            rb.velocity = direccionMovimiento * GetVelocidadActual();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        
        // Actualizar animación
        ActualizarAnimacion();
    }
    

    // LA máquina de estados (sólo para animaciones)
    IEnumerator TransicionEstado(EstadoEnemigo estadoTransicion, EstadoEnemigo estadoDestino)
    {
        enTransicion = true;
        estadoActual = estadoTransicion;
        
        rb.velocity = Vector2.zero;
        
        // Activar animación de transición
        if (animator != null)
        {
            if (estadoTransicion == EstadoEnemigo.Spawn)
            {
                animator.SetTrigger("Spawn");
                Debug.Log("FantasmaBirro: Levantándose");
            }
            else if (estadoTransicion == EstadoEnemigo.Crouch)
            {
                animator.SetTrigger("Crouch");
                Debug.Log("FantasmaBirro: Agachándose");
            }
        }
        
        // Esperar a que termine la animación
        yield return new WaitForSeconds(tiempoTransicion);
        
        // Cambiar al estado destino
        estadoActual = estadoDestino;
        enTransicion = false;
        
        // Configurar el nuevo estado
        if (estadoDestino == EstadoEnemigo.Crouching)
        {
            isInvulnerable = true;
            Debug.Log("FantasmaBirro: Agachado (Invulnerable)");
        }
        else if (estadoDestino == EstadoEnemigo.Normal)
        {
            isInvulnerable = false;
            Debug.Log("FantasmaBirro: Normal (Vulnerable)");
        }
        
        CambiarDireccionAleatoria();
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
        
        // Actualizar animación de dirección en el Animator del hijo
        if (animator != null)
        {
            animator.SetFloat("Horizontal", direccionMovimiento.x);
            animator.SetFloat("Vertical", direccionMovimiento.y);
        }
    }
    
    float GetVelocidadActual()
    {
        switch (estadoActual)
        {
            case EstadoEnemigo.Crouching:
                return moveSpeedAgachado;
            case EstadoEnemigo.Normal:
                return moveSpeedNormal;
            default:
                return 0f;
        }
    }
    
    void ActualizarAnimacion()
    {
        if (animator == null) return;
        
        switch (estadoActual)
        {
            case EstadoEnemigo.Crouching:
                animator.SetBool("IsCrouching", true);
                animator.SetBool("IsWalking", false);
                break;
                
            case EstadoEnemigo.Normal:
                animator.SetBool("IsCrouching", false);
                animator.SetBool("IsWalking", true);
                break;
                
            case EstadoEnemigo.Spawn:
            case EstadoEnemigo.Crouch:
                animator.SetBool("IsWalking", false);
                break;
        }
        
        float velocidad = GetVelocidadActual();
        animator.SetFloat("Speed", velocidad > 0.1f ? 1f : 0f);
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
        {
            Debug.Log("FantasmaBirro está agachado - ¡Invulnerable!");
            StartCoroutine(FeedbackInvulnerable());
            return;
        }
        
        if (estadoActual == EstadoEnemigo.Normal)
        {
            Debug.Log($"FantasmaBirro recibió {damage} de daño");
            // Aquí implementas la lógica de vida
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
    
    public void ForzarSalirAgachado()
    {
        if (estadoActual == EstadoEnemigo.Crouching && !enTransicion)
        {
            StopAllCoroutines();
            enTransicion = false;
            StartCoroutine(TransicionEstado(EstadoEnemigo.Spawn, EstadoEnemigo.Normal));
            Debug.Log("¡Bomba! FantasmaBirro forzado a salir de agachado");
        }
    }
    
    public void ForzarAgacharse()
    {
        if (estadoActual == EstadoEnemigo.Normal && !enTransicion)
        {
            StopAllCoroutines();
            enTransicion = false;
            StartCoroutine(TransicionEstado(EstadoEnemigo.Crouch, EstadoEnemigo.Crouching));
            Debug.Log("¡Bomba! FantasmaBirro forzado a agacharse");
        }
    }
    
    public bool IsVulnerable()
    {
        return !isInvulnerable && estadoActual == EstadoEnemigo.Normal;
    }
    
    public bool IsCrouching()
    {
        return estadoActual == EstadoEnemigo.Crouching;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaAgacharse);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaNormal);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, 
            $"Agacharse: {distanciaAgacharse}\nNormal: {distanciaNormal}");
        #endif
    }
}