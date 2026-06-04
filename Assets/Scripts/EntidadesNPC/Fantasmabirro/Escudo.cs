using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escudo : MonoBehaviour
{
    [Header("Configuración de Animación")]
    [SerializeField] private float duracionAparicion = 0.3f;
    [SerializeField] private float duracionVisible = 2f;
    [SerializeField] private float duracionDesaparicion = 0.3f;
    
    [Header("Escala")]
    [SerializeField] private Vector3 escalaInicial = Vector3.zero;
    [SerializeField] private Vector3 escalaFinal = Vector3.one;
    
    private SpriteRenderer spriteRenderer;
    private bool activo = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        transform.localScale = escalaInicial;
    }
    
    public void MostrarEscudo()
    {
        if (activo) return;
        
        activo = true;
        StartCoroutine(AnimacionEscudo());
    }
    
    IEnumerator AnimacionEscudo()
    {
        // Activar sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // ANIMACIÓN DE APARICIÓN (crecer)
        float tiempo = 0;
        while (tiempo < duracionAparicion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionAparicion;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
            yield return null;
        }
        
        transform.localScale = escalaFinal;
        
        // MANTENER VISIBLE
        yield return new WaitForSeconds(duracionVisible);
        
        // ANIMACIÓN DE DESAPARICIÓN (encoger)
        tiempo = 0;
        while (tiempo < duracionDesaparicion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionDesaparicion;
            transform.localScale = Vector3.Lerp(escalaFinal, escalaInicial, t);
            yield return null;
        }
        
        transform.localScale = escalaInicial;
        
        // Desactivar sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        activo = false;
        
        // Destruir el escudo después de la animación (opcional, si es desechable)
        Destroy(gameObject);
    }
}
