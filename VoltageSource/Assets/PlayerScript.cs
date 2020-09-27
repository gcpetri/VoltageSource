using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Handle being hit and store health. Best done by get and set funcitons
    [SerializeField] private float health;
    public float Health
    {
        get => health;
        set
        {
            if (health <= 0)
            {
                Debug.Log("Died");
                // If player dies then all function to handle their death
            }
            health = value;
        }
    }
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
