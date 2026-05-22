using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generador : MonoBehaviour
{
    [Header("Configuración del Generador")]
    [SerializeField] private GameObject mabirroPrefab;
    [SerializeField] private float rangoActivacion = 15f;      // Rango para activar el generador
    [SerializeField] private float radioGeneracion = 5f;       // Radio donde aparecen los enemigos
    [SerializeField] private float intervaloEntreSpawns = 5f;  // Tiempo entre spawns
    [SerializeField] private int maxEnemigosSimultaneos = 3;   // Máximo de enemigos vivos a la vez
    
    [Header("Referencias")]
    private Transform player;
    private PlayerMovement playerScript;
    private bool generadorActivo = false;
    private List<GameObject> enemigosVivos = new List<GameObject>();
    
    void Start()
    {
        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogError("Generador: No se encontró al jugador con tag 'Player'");
        }
        
        // Iniciar la corrutina de control
        StartCoroutine(ControlGenerador());
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Verificar si el jugador está dentro del rango de activación
        float distanciaAlJugador = Vector2.Distance(transform.position, player.position);
        bool jugadorCerca = distanciaAlJugador < rangoActivacion;
        
        // Activar o desactivar el generador según la distancia
        if (jugadorCerca && !generadorActivo)
        {
            ActivarGenerador();
        }
        else if (!jugadorCerca && generadorActivo)
        {
            DesactivarGenerador();
        }
    }
    
    IEnumerator ControlGenerador()
    {
        while (true)
        {
            // Solo generar si el generador está activo
            if (generadorActivo)
            {
                // Limpia a los enemigos destruidos de la lista
                enemigosVivos.RemoveAll(e => e == null);
                
                // Comprueba si se pueden crear enemigos
                if (enemigosVivos.Count < maxEnemigosSimultaneos)
                {
                    GenerarEnemigo();
                }
            }
            
            // Espera el intervalo entre spawns
            yield return new WaitForSeconds(intervaloEntreSpawns);
        }
    }
    //Activar y desactivar son métodos para comprobar el funcionamiento
    void ActivarGenerador()
    {
        generadorActivo = true;
        Debug.Log($"Generador activado en {transform.position}");        
    }
    
    void DesactivarGenerador()
    {
        generadorActivo = false;
        Debug.Log($"Generador desactivado en {transform.position}");
    }
    
    void GenerarEnemigo()
    {
        if (mabirroPrefab == null)
        {
            Debug.LogError("Generador: No se ha asignado el prefab del enemigo");
            return;
        }
        
        // Calcula una posición aleatoria dentro del radio de generación
        Vector2 posicionGeneracion = (Vector2)transform.position + Random.insideUnitCircle * radioGeneracion;
        
        // Instancia un enemigo
        GameObject nuevoEnemigo = Instantiate(mabirroPrefab, posicionGeneracion, Quaternion.identity);
        enemigosVivos.Add(nuevoEnemigo);
        
        Debug.Log($"Enemigo generado en {posicionGeneracion}. Enemigos vivos: {enemigosVivos.Count}/{maxEnemigosSimultaneos}");
        
       
        Mabirro enemigoScript = nuevoEnemigo.GetComponent<Mabirro>();
    }
    
    // Método para limpiar la lista cuando un enemigo muere (y que se creen nuevos enemigos)
    public void RemoverEnemigo(GameObject enemigo)
    {
        if (enemigosVivos.Contains(enemigo))
        {
            enemigosVivos.Remove(enemigo);
        }
    }
    
    // Ver los rangos en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de activación (naranja)
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, rangoActivacion);
        
        // Rango de generación (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioGeneracion);
        
        // Centro del generador (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}