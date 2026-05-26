using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UIKeyCount : MonoBehaviour
{
    [SerializeField] public GameObject player;
    //[SerializeField] public 

    PlayerMovement scriptPlayer;
    public TextMeshProUGUI nLlaves;
    // Start is called before the first frame update
    void Start()
    {
        
        if(player != null)
        {
            scriptPlayer = player.GetComponent<PlayerMovement>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        nLlaves.text = ""+scriptPlayer.NumLlaves();
    }
}
