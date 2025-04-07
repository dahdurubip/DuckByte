using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Unity.Collections;
using Unity.VisualScripting;
public class NewMonoBehaviourScript : MonoBehaviour
{
    public NavMeshAgent NM;
    public Vector3 Point;
    public Transform wayPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        NM.SetDestination(Point);
    }
}

