using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectIndicatorEntity : MonoBehaviour
{
    public GameObject ElementPrefab;
    public int ElementsCount;
    public float Radius;
    public float RotateSpeed;

    // Start is called before the first frame update
    void Awake()
    {
        var angleStep = 2 * Mathf.PI / ElementsCount;
        var currentAngle = 0f;

        for (var i = 0; i < ElementsCount; i++)
        {
            var elementPosition = new Vector3(Mathf.Sin(currentAngle), 0, Mathf.Cos(currentAngle)) * Radius;
            var elementRotation = new Vector3(0, currentAngle * 360 / (2 * Mathf.PI));
            currentAngle += angleStep;

            var element = Instantiate(ElementPrefab, elementPosition, Quaternion.identity, transform);
            element.transform.localEulerAngles = elementRotation;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.eulerAngles += new Vector3(0, RotateSpeed, 0);
    }

    public void SetAsPreselect()
    {
        SetTransparency(0.5f);
    }

    public void SetAsSelect()
    {
        SetTransparency(1f);
    }

    public void SetTransparency(float value)
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, value);
        }
    }
}
