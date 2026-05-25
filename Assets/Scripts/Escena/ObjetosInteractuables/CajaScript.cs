using UnityEngine;

public class CajaScript : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float masa = 1f;
    [SerializeField] private float fuerzaEmpuje = 10f;
    
    private Rigidbody2D rb;
    private bool siendoEmpujada = false;
    private float tiempoSinEmpuje = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = masa;
            // Esto reduce fricción para que no se deslice mucho
            rb.drag = 5f;
            rb.angularDrag = 5f;
        }
    }
    
    void FixedUpdate()
    {
        if (siendoEmpujada)
        {
            tiempoSinEmpuje = 0f;
        }
        else
        {
            tiempoSinEmpuje += Time.fixedDeltaTime;
            
            // Si lleva más de 0.1 segundos sin ser empujada, frena sola
            if (tiempoSinEmpuje > 0.1f && rb.velocity.magnitude < 0.5f)
            {
                rb.velocity = Vector2.zero;
            }
            else if (tiempoSinEmpuje > 0.2f)
            {
                // Frenado progresivo (queda guapo)
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * 10f);
            }
        }
        
        siendoEmpujada = false;
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            siendoEmpujada = true;
            
            // Obtener dirección del jugador respecto a la caja
            Vector2 direccionEmpuje = (transform.position - collision.transform.position).normalized;
            
            // Aplicar fuerza en la dirección que el jugador empuja
            rb.AddForce(-direccionEmpuje * fuerzaEmpuje, ForceMode2D.Force);
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            siendoEmpujada = false;
        }
    }
}
