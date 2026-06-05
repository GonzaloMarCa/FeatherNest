using UnityEngine;
using UnityEngine.EventSystems;

public class ForceReferences : MonoBehaviour
{
    private void Start()
    {
        // Fuerza referencias a Physics2D
        Physics2D.Raycast(Vector2.zero, Vector2.right);
        
        // Fuerza referencia a EventSystem
        var es = FindObjectOfType<EventSystem>();
        
        // Fuerza referencia a Tilemap (si usas tilemaps)
        var tm = FindObjectOfType<UnityEngine.Tilemaps.Tilemap>();
    }
}