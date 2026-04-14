using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomba : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoExplosion = 3f;
    
    [Header("Configuración de Daño")]
    [SerializeField] private int daño = 2;
    [SerializeField] private float radioExplosion = 3f;
    [SerializeField] private LayerMask capasEnemigo;
    
    [Header("Efectos Visuales")]
    [SerializeField] private GameObject efectoExplosion;
    [SerializeField] private Color colorInicial = Color.white;
    [SerializeField] private Color colorFinal = Color.red;
    [SerializeField] private AnimationCurve intensidadColor; // Curva de cambio de color
    
    [Header("Efecto de Parpadeo")]
    [SerializeField] private bool parpadeoAlFinal = true;
    [SerializeField] private float umbralParpadeo = 1f; // Cuando empieza a parpadear (segundos antes de explotar)
    [SerializeField] private float velocidadParpadeo = 10f;
    
    private SpriteRenderer spriteRenderer;
    private float tiempoActual;
    private bool haExplotado = false;
    
    void Start()
    {
        gameObject.layer = capasEnemigo;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        tiempoActual = tiempoExplosion;
        
        // Iniciar efecto de cambio de color
        StartCoroutine(EfectoVisual());
    }
    
    void Update()
    {
        if (haExplotado) return;
        
        tiempoActual -= Time.deltaTime;
        
        if (tiempoActual <= 0)
        {
            Explotar();
        }
    }
    
    IEnumerator EfectoVisual()
    {
        float tiempoTranscurrido = 0;
        bool parpadeando = false;
        
        while (tiempoTranscurrido < tiempoExplosion && !haExplotado)
        {
            float progreso = tiempoTranscurrido / tiempoExplosion;
            
            // Verificar si debe empezar a parpadear
            float tiempoRestante = tiempoExplosion - tiempoTranscurrido;
            
            if (parpadeoAlFinal && tiempoRestante <= umbralParpadeo && !parpadeando)
            {
                parpadeando = true;
                StartCoroutine(Parpadeo());
            }
            
            // Cambio de color progresivo
            if (!parpadeando && spriteRenderer != null)
            {
                float intensidad = intensidadColor != null ? 
                    intensidadColor.Evaluate(progreso) : progreso;
                Color nuevoColor = Color.Lerp(colorInicial, colorFinal, intensidad);
                spriteRenderer.color = nuevoColor;
            }
            
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator Parpadeo()
    {
        while (!haExplotado && tiempoActual > 0)
        {
            if (spriteRenderer != null)
            {
                // Alternar entre color final y blanco
                float parpadeo = Mathf.PingPong(Time.time * velocidadParpadeo, 1);
                spriteRenderer.color = Color.Lerp(Color.white, colorFinal, parpadeo);
            }
            yield return null;
        }
    }
    
    void Explotar()
    {
        if (haExplotado) return;
        
        haExplotado = true;
        DetectarYDañarEnemigos();
        
        if (efectoExplosion != null)
        {
            Instantiate(efectoExplosion, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
    
    void DetectarYDañarEnemigos()
    {
        Collider2D[] enemigosCerca = Physics2D.OverlapCircleAll(transform.position, radioExplosion, capasEnemigo);
        
        foreach (Collider2D enemigo in enemigosCerca)
        {
            Mabirro enemigoScript = enemigo.GetComponent<Mabirro>();
            if (enemigoScript != null)
            {
                enemigoScript.TakeDamage(daño);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}