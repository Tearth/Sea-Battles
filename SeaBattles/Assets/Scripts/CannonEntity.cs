using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonEntity : MonoBehaviour
{
    public Transform BallPoint;
    public GameObject BallPrefab;
    public float InitialSpeed;
    public int CrewCount;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Shoot()
    {
        var ball = Instantiate(BallPrefab, BallPoint.position, Quaternion.identity);
        var ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.velocity = transform.forward * InitialSpeed;
    }
}
