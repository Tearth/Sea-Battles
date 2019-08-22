using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewEntity : MonoBehaviour
{
    public Rigidbody CrewRigidbody;

    public void OnCollisionEnter(Collision collision)
    {

    }

    public void Kill()
    {
        CrewRigidbody.isKinematic = false;
        transform.parent = null;
    }
}
