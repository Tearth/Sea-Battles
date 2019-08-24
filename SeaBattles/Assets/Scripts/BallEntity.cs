using UnityEngine;

public class BallEntity : MonoBehaviour
{
    [Header("General settings")]
    public int MaxDepth;

    void Update()
    {
        if (transform.position.y < MaxDepth)
        {
            Destroy(gameObject);
        }
    }
}
