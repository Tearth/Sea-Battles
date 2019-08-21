using UnityEngine;

public class BallEntity : MonoBehaviour
{
    public int MaxDepth;

    void Update()
    {
        if (transform.position.y < MaxDepth)
        {
            Destroy(gameObject);
        }
    }
}
