using UnityEngine;

public class ScaleRelativeToCamera : MonoBehaviour
{
    public Camera Camera;
    public float ScaleRatio = 1.0f;

    private Vector3 _initialScale;

    void Start()
    {
        _initialScale = transform.localScale;
    }

    void Update()
    {
        var plane = new Plane(Camera.transform.forward, Camera.transform.position);
        var distance = plane.GetDistanceToPoint(transform.position);
        transform.localScale = _initialScale * distance * ScaleRatio;
    }
}