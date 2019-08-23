using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public Camera Camera;
    public GameObject SelectPrefab;
    public Transform Selections;

    private GameObject _preSelect;

    // Start is called before the first frame update
    void Start()
    {
        _preSelect = Instantiate(SelectPrefab, Vector3.zero, Quaternion.identity, transform);
        _preSelect.GetComponent<SelectIndicatorEntity>().SetAsPreselect();
        _preSelect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Cursor.visible)
        {
            return; 
        }

        if (Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out var hit, float.MaxValue))
        {
            var selectable = hit.collider.gameObject.GetComponent<ISelectable>();
            if (selectable != null)
            {
                if (!selectable.Selected)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            RemoveAllSelections();
                        }

                        var createdSelection = Instantiate(SelectPrefab, Vector3.zero, Quaternion.identity, Selections);
                        createdSelection.GetComponent<SelectIndicatorEntity>().Target = hit.collider.transform;
                        createdSelection.GetComponent<SelectIndicatorEntity>().ForceUpdatePosition();
                        selectable.Selected = true;

                        HidePreselect();
                    }
                    else
                    {
                        ShowPreselect(hit.collider.transform);
                    }
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    RemoveAllSelections();
                }

                HidePreselect();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
            {
                RemoveAllSelections();
            }

            HidePreselect();
        }
    }

    private void ShowPreselect(Transform target)
    {
        _preSelect.SetActive(true);
        _preSelect.GetComponent<SelectIndicatorEntity>().Target = target;
        _preSelect.GetComponent<SelectIndicatorEntity>().ForceUpdatePosition();
    }

    private void HidePreselect()
    {
        _preSelect.SetActive(false);
    }

    private void RemoveAllSelections()
    {
        foreach (Transform child in Selections)
        {
            var target = child.GetComponent<SelectIndicatorEntity>().Target;
            target.GetComponent<ISelectable>().Selected = false;

            Destroy(child.gameObject);
        }
    }
}
