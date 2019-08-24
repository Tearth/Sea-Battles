using System;
using UnityEngine;

public class CameraEntity : MonoBehaviour
{
    [Header("Target settings")]
    public Transform Destination;
    public Transform Target;

    [Header("General settings")]
    public float MoveSpeed;
    public float RotationSpeed;
    public float MoveLerpSpeed;
    public float RotationLerpSpeed;
    public float ZoomSpeed;

    [Header("Speed modifiers")]
    public float ExtraAcceleration;
    public float ExtraReduction;

    [Header("Constraint settings")]
    public float MinHeight;
    public float MaxHeight;
    public bool DragLock;

    private bool _mouseDrag;

    void Update()
    {
        var speedMultiplier = GetSpeedMultiplier();

        UpdateDragModeStatus();
        MoveCameraIfNecessary(speedMultiplier);
        RotateCameraIfNecessary();
        ApplyConstraint();
        LerpCamera();
    }

    private void UpdateDragModeStatus()
    {
        var mousePositionDelta = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

        // Check if mouse has been moved after click, then activate drag mode if possible
        if (Input.GetMouseButton(1))
        {
            if (!_mouseDrag && mousePositionDelta.sqrMagnitude > 0)
            {
                _mouseDrag = true;
            }
        }

        // Mouse is not in drag mode if left button has been released
        if (Input.GetMouseButtonUp(1))
        {
            _mouseDrag = false;
        }
    }

    private float GetSpeedMultiplier()
    {
        return Input.GetKey(KeyCode.LeftShift) ? ExtraAcceleration : Input.GetKey(KeyCode.LeftControl) ? ExtraReduction : 1;
    }

    private void MoveCameraIfNecessary(float speedMultiplier)
    {
        if (Input.GetKey(KeyCode.W))
        {
            Target.position += Vector3.Scale(Destination.forward, new Vector3(1, 0, 1)) * MoveSpeed * speedMultiplier * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Target.position -= Vector3.Scale(Destination.forward, new Vector3(1, 0, 1)) * MoveSpeed * speedMultiplier * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Target.position -= Vector3.Scale(Destination.right, new Vector3(1, 0, 1)) * MoveSpeed * speedMultiplier * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Target.position += Vector3.Scale(Destination.right, new Vector3(1, 0, 1)) * MoveSpeed * speedMultiplier * Time.deltaTime;
        }

        if (Math.Abs(Input.mouseScrollDelta.y) > 0.001f)
        {
            var deltaSign = (int)Mathf.Sign(Input.mouseScrollDelta.y);
            if (deltaSign == 1 && Destination.position.y > MinHeight || deltaSign == -1 && Destination.position.y < MaxHeight)
            {
                Destination.Translate(deltaSign * new Vector3(0, 0, ZoomSpeed) * Time.deltaTime, Space.Self);
            }
        }
    }

    private void RotateCameraIfNecessary()
    {
        if (_mouseDrag && !DragLock)
        {
            Destination.transform.RotateAround(Target.position, new Vector3(0, 1, 0), Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime);

            if (Input.GetAxis("Mouse Y") < 0 || Destination.position.y > MinHeight)
            {
                Destination.transform.RotateAround(Target.position, Destination.right, -Input.GetAxis("Mouse Y") * RotationSpeed * Time.deltaTime);
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void ApplyConstraint()
    {
        if (Destination.position.y < MinHeight)
        {
            Destination.position = new Vector3(Destination.position.x, MinHeight, Destination.position.z);
        }
    }

    private void LerpCamera()
    {
        Destination.LookAt(Target);
        transform.position = Vector3.Lerp(transform.position, Destination.position, MoveLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Destination.rotation, RotationLerpSpeed * Time.deltaTime);
    }
}
