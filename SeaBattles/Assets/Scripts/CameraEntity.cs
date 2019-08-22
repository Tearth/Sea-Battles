using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEntity : MonoBehaviour
{
    public Transform Destination;
    public Transform Target;

    public float MoveSpeed;
    public float RotationSpeed;
    public float MoveLerpSpeed;
    public float RotationLerpSpeed;
    public float ZoomSpeed;

    private Vector3 _lastMousePosition;
    private Vector3 _deltaMousePosition;

    void Start()
    {
        _lastMousePosition = Input.mousePosition;
        _deltaMousePosition = Vector3.zero;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Target.position += Vector3.Scale(Destination.forward, new Vector3(1, 0, 1)) * MoveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Target.position -= Vector3.Scale(Destination.forward, new Vector3(1, 0, 1)) * MoveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Target.position -= Vector3.Scale(Destination.right, new Vector3(1, 0, 1)) * MoveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Target.position += Vector3.Scale(Destination.right, new Vector3(1, 0, 1)) * MoveSpeed * Time.deltaTime;
        }

        if (Math.Abs(Input.mouseScrollDelta.y) > 0.001f)
        {
            Destination.Translate(Mathf.Sign(Input.mouseScrollDelta.y) * new Vector3(0, 0, ZoomSpeed) * Time.deltaTime, Space.Self);
        }

        if (Input.GetMouseButton(1))
        {
            Destination.transform.RotateAround(Target.position, new Vector3(0, 1, 0), Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime);
        }

        Destination.transform.LookAt(Target);
        transform.position = Vector3.Lerp(transform.position, Destination.position, MoveLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Destination.rotation, RotationLerpSpeed * Time.deltaTime);
    }
}
