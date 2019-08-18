using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallEntity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(0, 0, 600, ForceMode.Acceleration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        var rb = collision.gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }
}
