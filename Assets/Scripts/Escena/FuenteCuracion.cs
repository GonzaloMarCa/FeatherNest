using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuenteCuracion : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float radioCuracion = 10f;
    [SerializeField] private float intervaloCuracion = 0.5f;
    [SerializeField] private int cantidadCuracion = 1;
    
    private Transform player;
    private PlayerMovement playerScript;
    private float tiempoUltimaCuracion = 0f;
    
    void Start()
    {
        //Si intento llamar el método desde el playerObj tal cual sin pasarlo a playerScript salta error
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerMovement>();
        }
    }
    
    void Update()
    {
        if (player == null || playerScript == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Verifica si está dentro del rango y si ha pasado suficiente tiempo
        if (distanceToPlayer < radioCuracion && Time.time >= tiempoUltimaCuracion)
        {
            Curar();
            tiempoUltimaCuracion = Time.time + intervaloCuracion;
        }
    }

    //No te vas a creer lo que esto hace
    void Curar()
    {
        int vidaActual = playerScript.GetCurrentHealth();
        int vidaMaxima = playerScript.GetMaxHealth();
        
        if (vidaActual < vidaMaxima)
        {
            playerScript.Heal(cantidadCuracion);
            Debug.Log($"Fuente de curación: Jugador curado +{cantidadCuracion}");
        }
    }
    //Ver el rango de curación de la fuente
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioCuracion);
    }
}