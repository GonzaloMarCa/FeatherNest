using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayBomba : MonoBehaviour
{
    float lifetime = 1f;
    ParticleSystem particles;

    void Start()
    {
        particles = GetComponent<ParticleSystem>();
   
        Destroy(gameObject, lifetime);
    }

}
