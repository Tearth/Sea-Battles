using System.Collections.Generic;
using UnityEngine;

public class CannonEntity : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject BallPrefab;

    [Header("Components")]
    public Rigidbody CannonRigidbody;

    [Header("General settings")]
    public Transform BallPoint;
    public Transform Crew;
    public float InitialSpeed;
    public float MinImpulseToDestroy;
    public int CrewCount;
    public bool Destroyed;

    void OnCollisionEnter(Collision collision)
    {
        if (!Destroyed && collision.impulse.sqrMagnitude >= MinImpulseToDestroy)
        {
            CannonRigidbody.isKinematic = false;
            CannonRigidbody.AddForce(-collision.impulse);

            foreach (Transform crew in Crew)
            {
                crew.GetComponent<CrewEntity>().Kill();
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
