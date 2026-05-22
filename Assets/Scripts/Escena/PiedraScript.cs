using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiedraScript : MonoBehaviour
{
    [SerializeField] ParticleSystem humo;
    private float lifetime = 1f;
    // Start is called before the first frame update
    public void explotar()
    {
        Instantiate(humo, transform.position, Quaternion.identity);
        Destroy(gameObject, lifetime/2);
    }
}
