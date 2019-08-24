using UnityEngine;

public class CrewEntity : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody CrewRigidbody;

    [Header("General settings")]
    public float MinImpulseToKill;

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.sqrMagnitude >= MinImpulseToKill)
        {
            Kill();
        }
    }

    public void Kill()
    {
        CrewRigidbody.isKinematic = false;
        transform.parent = null;

        Destroy(gameObject);
    }
}
