using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAttack : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    [SerializeField] private GameObject attackPrefab;
    // [SerializeField] private float attackOffset = 1f;
   // [SerializeField] private int damage = 1;
    
    private bool isAttacking = false;
    private Transform playerTransform;
    
    void Start()
    {
        playerTransform = transform.parent;
        if (playerTransform == null)
        {
            playerTransform = transform;
        }
    }
    
    public void StartAttack(Vector2 direction)
    {
        if (isAttacking) return;
        
        isAttacking = true;
        
        // Calcular posición del ataque
        Vector3 attackPosition = playerTransform.position;
        
        // Instanciar el prefab de ataque
        if (attackPrefab != null)
        {
            GameObject newAttack = Instantiate(attackPrefab, attackPosition, Quaternion.identity);
            
            // Rotar según dirección
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            newAttack.transform.rotation = Quaternion.Euler(0, 0, angle + 90);
            
            
        }
        
        // Pequeña pausa antes de permitir otro ataque (para que se vea la animación)
        StartCoroutine(ResetAttack());
        
        Debug.Log($"ParticleAttack iniciado en dirección: {direction}");
    }
    
    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public bool CanAttack()
    {
        return !isAttacking;
    }
}