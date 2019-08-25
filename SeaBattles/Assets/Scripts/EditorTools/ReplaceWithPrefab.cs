#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab : ScriptableWizard
{
    public GameObject NewType;
    public GameObject[] OldObjects;

    [MenuItem("Custom/Replace GameObjects")]
    static void CreateWizard()
    {
        DisplayWizard("Replace GameObjects", typeof(ReplaceWithPrefab), "Replace");
    }

    void OnWizardCreate()
    {
        foreach (var gameObject in OldObjects)
        {
            var newObject = (GameObject)PrefabUtility.InstantiatePrefab(NewType);
            newObject.transform.position = gameObject.transform.position;
            newObject.transform.rotation = gameObject.transform.rotation;
            newObject.transform.parent = gameObject.transform.parent;

            DestroyImmediate(gameObject);
        }
    }
}
#endif