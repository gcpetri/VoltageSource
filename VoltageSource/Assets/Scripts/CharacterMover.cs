using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 


[RequireComponent(typeof(NavMeshAgent))]
public class CharacterMover : MonoBehaviour
{
    private NavMeshAgent _agent;
    [SerializeField] private float speed = 1f;
    
    // Start is called before the first frame update
    void Start()
    {
        _agent = this.GetComponent<NavMeshAgent>(); // Ran at runtime, one time assign
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 relativeMovement = new Vector3(Input.GetAxis("Horizontal"), 0f , Input.GetAxis("Vertical"));
        _agent.Move(relativeMovement * (Time.deltaTime * speed));
    }
}
