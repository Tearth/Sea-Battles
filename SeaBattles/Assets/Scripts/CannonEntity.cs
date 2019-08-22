using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonEntity : MonoBehaviour
{
    public Transform BallPoint;
    public GameObject BallPrefab;
    public Rigidbody CannonRigidbody;
    public float InitialSpeed;
    public float MinImpulseToDestroy;
    public int CrewCount;
    public bool Destroyed;

    public List<CrewEntity> CrewList;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!Destroyed && collision.impulse.sqrMagnitude > MinImpulseToDestroy)
        {
            CannonRigidbody.isKinematic = false;
            CannonRigidbody.AddForce(-collision.impulse);

            foreach (var crew in CrewList)
            {
                crew.Kill();
            }

            Destroyed = true;
        }
    }

    public void Shoot()
    {
        if (!Destroyed)
        {
            var ball = Instantiate(BallPrefab, Vector3.zero, Quaternion.identity);
            ball.transform.position = BallPoint.position;

            var ballRigidbody = ball.GetComponent<Rigidbody>();
            ballRigidbody.velocity = -transform.forward * InitialSpeed;
        }
    }
}
