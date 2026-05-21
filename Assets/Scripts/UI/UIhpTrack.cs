using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class UIhpTrack : MonoBehaviour
{
   [Header("Referencias del Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    
    [Header("Configuración de Vidas")]
    [SerializeField] private GameObject plumaPrefab;
    [SerializeField] private Transform livesContainer;
    [SerializeField] private Sprite plumaActiva;
    [SerializeField] private Sprite plumaPerdida; // Opcional: sprite de vida perdida (gris, rota, etc.)
    
    [Header("Efectos Visuales")]
    [SerializeField] private bool animarPerdida = true;
    [SerializeField] private float animacionDuracion = 0.3f;
    
    private List<GameObject> plumasList = new List<GameObject>();
    private int currentLives;
    
    void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }
        
        if (playerMovement != null)
        {
            currentLives = playerMovement.GetMaxHealth();
            CrearPlumas(currentLives);
        }
    }
    
    void Update()
    {
        if (playerMovement != null)
        {
            int nuevasVidas = playerMovement.GetCurrentHealth();
            if (nuevasVidas != currentLives)
            {
                ActualizarVidas(nuevasVidas);
            }
        }
    }
    
    void CrearPlumas(int cantidad)
    {
        // Limpiar plumas existentes
        foreach (Transform child in livesContainer)
        {
            Destroy(child.gameObject);
        }
        plumasList.Clear();
        
        // Crear nuevas plumas
        for (int i = 0; i < cantidad; i++)
        {
            GameObject nuevaPluma = Instantiate(plumaPrefab, livesContainer);
            nuevaPluma.name = $"Pluma_{i}";
            
            // Configurar imagen
            Image img = nuevaPluma.GetComponent<Image>();
            if (img != null && plumaActiva != null)
            {
                img.sprite = plumaActiva;
            }
            
            plumasList.Add(nuevaPluma);
        }
    }
    
    void ActualizarVidas(int nuevasVidas)
    {
        int vidasPerdidas = currentLives - nuevasVidas;
        
        if (vidasPerdidas > 0)
        {
            // Perdió vidas
            for (int i = currentLives - 1; i >= nuevasVidas; i--)
            {
                if (i < plumasList.Count)
                {
                    if (animarPerdida)
                    {
                        StartCoroutine(AnimarPerdidaPluma(plumasList[i]));
                    }
                    else
                    {
                        if (plumaPerdida != null)
                        {
                            plumasList[i].GetComponent<Image>().sprite = plumaPerdida;
                        }
                        else
                        {
                            plumasList[i].SetActive(false);
                        }
                    }
                }
            }
        }
        else if (vidasPerdidas < 0)
        {
            // Ganó vidas (si tu juego permite ganar vidas)
            // Recargar todas las plumas
            CrearPlumas(nuevasVidas);
        }
        
        currentLives = nuevasVidas;
    }
    
    IEnumerator AnimarPerdidaPluma(GameObject pluma)
    {
        Image img = pluma.GetComponent<Image>();
        RectTransform rect = pluma.GetComponent<RectTransform>();
        
        
        // Animación de escala (crece ligeramente)
        float tiempo = 0;
        Vector3 escalaOriginal = rect.localScale;
        pluma.GetComponent<Graphic>().color = new Color(0,0,0,0.7f);
        while (tiempo < animacionDuracion / 2)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / (animacionDuracion / 2);
            rect.localScale = escalaOriginal * (1 + (1 - t) * 0.4f);
            yield return null;
        }
        
        
        //Explicación del código para mí mismo, que esto no iba:
        //Primero: Reorganizar las escalas para que se haga grande al recibir el golpe y encoja hasta el tamaño original posteriormente
        //Segundo: En la línea 125 (pluma.getComponent...) estaba recogiendo el componente "Image" pero con ese saltaba error de objeto vacío,
        //          había que recoger el componente graphic que contiene la parte visual
        //Tercero: Se tiene que introducir el objeto como parámetro del Enumerator para no pintar todas simultáneamente y pintar sólo la correspondiente
        
    
        
        // Animación de contracción
        tiempo = 0;
        while (tiempo < animacionDuracion / 2)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / (animacionDuracion / 2);
            rect.localScale = escalaOriginal;
            yield return null;
        }
        
        //rect.localScale = escalaOriginal;
    }

    // Método público para recargar todas las vidas
    public void ResetLives(int maxLives)
    {
        CrearPlumas(maxLives);
        currentLives = maxLives;
    }
}

