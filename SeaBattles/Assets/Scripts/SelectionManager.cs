using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public Camera Camera;
    public GameObject SelectPrefab;

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
        if (Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out var hit, float.MaxValue))
        {
            var selectable = hit.collider.gameObject.GetComponent<ISelectable>();
            if (selectable != null)
            {
                _preSelect.SetActive(true);
                _preSelect.transform.position = new Vector3(hit.transform.position.x, 2, hit.transform.position.z);
            }
            else
            {
                _preSelect.SetActive(false);
            }
        }
        else
        {
            _preSelect.SetActive(false);
        }
    }
}
