using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    public float MinHeight;
    public float MaxHeight;

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
            var deltaSign = (int)Mathf.Sign(Input.mouseScrollDelta.y);
            if(deltaSign == 1 && Destination.position.y > MinHeight || deltaSign == -1 && Destination.position.y < MaxHeight)
            {
                Destination.Translate(deltaSign * new Vector3(0, 0, ZoomSpeed) * Time.deltaTime, Space.Self);
            }
        }

        if (Input.GetMouseButton(1))
        {
            Destination.transform.RotateAround(Target.position, new Vector3(0, 1, 0), Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime);

            if (Input.GetAxis("Mouse Y") < 0 || Destination.position.y > MinHeight)
            {
                Destination.transform.RotateAround(Target.position, Destination.right, -Input.GetAxis("Mouse Y") * RotationSpeed * Time.deltaTime);
            }
        }

        Destination.LookAt(Target);
        transform.position = Vector3.Lerp(transform.position, Destination.position, MoveLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Destination.rotation, RotationLerpSpeed * Time.deltaTime);
    }
}
