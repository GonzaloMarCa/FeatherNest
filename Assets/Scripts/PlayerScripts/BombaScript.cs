using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomba : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoExplosion = 3f;
    
    [Header("Configuración de Daño")]
    [SerializeField] private int daño = 3;
    [SerializeField] private float radioExplosion = 3.5f;
    
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
        gameObject.layer = 3;
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
            Explosion();
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
                // Alternar entre rojo y blanco AUPA ATHLETIC
                float parpadeo = Mathf.PingPong(Time.time * velocidadParpadeo, 1);
                spriteRenderer.color = Color.Lerp(Color.white, colorFinal, parpadeo);
            }
            yield return null;
        }
    }
    
    void Explosion()
    {
        if (haExplotado) return;
        
        haExplotado = true;
        ExplosionDamage();
        
        if (efectoExplosion != null)
        {
            Instantiate(efectoExplosion, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
    

    void ExplosionDamage()
    {
        Debug.Log($"=== BOMBA EXPLOTANDO ===");
        Debug.Log($"Posición: {transform.position}");
        Debug.Log($"Radio: {radioExplosion}");
        
        // 1. Detectar TODOS los colliders sin filtro
        Collider2D[] todosColliders = Physics2D.OverlapCircleAll(transform.position, radioExplosion);
        
        // Listar todos los colliders encontrados
        foreach (Collider2D col in todosColliders)
        {
            Debug.Log($"  - {col.name}: Layer={LayerMask.LayerToName(col.gameObject.layer)} ({col.gameObject.layer})");
        }
        
        // 2. Detectar específicamente la capa Enemigos
        Collider2D[] enemigosCerca = Physics2D.OverlapCircleAll(transform.position, radioExplosion);
        
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