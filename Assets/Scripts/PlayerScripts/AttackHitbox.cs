using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 0.3f;
    [SerializeField] private float damageInterval = 0.1f;
    
    private ParticleSystem particles;
    private EdgeCollider2D col;
    private List<GameObject> damagedEnemies = new List<GameObject>();
    
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        col = GetComponent<EdgeCollider2D>();
        
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Destruir después del tiempo de vida
        Destroy(gameObject, lifetime);
        
        // Reproducir partículas
        if (particles != null)
        {
            particles.Play();
        }
        
        // Iniciar daño periódico
        StartCoroutine(DamageOverTime());
    }
    
    IEnumerator DamageOverTime()
    {
        float timer = 0;
        while (timer < lifetime)
        {
            DamageEnemies();
            timer += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }
    }
    
    void DamageEnemies()
    {
        if (col == null) return;
        
        // Detectar enemigos dentro del collider
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        
        Collider2D[] hitEnemies = new Collider2D[10];
        int numEnemies = col.OverlapCollider(filter, hitEnemies);
        
        for (int i = 0; i < numEnemies; i++)
        {
            Collider2D enemy = hitEnemies[i];
            
            if (!damagedEnemies.Contains(enemy.gameObject))
            {
                damagedEnemies.Add(enemy.gameObject);
                ApplyDamage(enemy.gameObject);
            }
        }
    }
    
    void ApplyDamage(GameObject enemy)
    {
        Mabirro enemyScript = enemy.GetComponent<Mabirro>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage);
            Debug.Log($"Ataque golpeó a {enemy.name} - Daño: {damage}");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (damagedEnemies.Contains(other.gameObject)) return;
        
        Mabirro enemy = other.GetComponent<Mabirro>();
        if (enemy != null)
        {
            damagedEnemies.Add(other.gameObject);
            enemy.TakeDamage(damage);
        }
    }
    
}