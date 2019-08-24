using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

public class ShipEditorTools : MonoBehaviour
{
    public List<MirrorableItem> MirrorableItems;
    public float ScaleToApply;

    public void RemoveDuplicates()
    {
        var removedDuplicates = 0;
        var groups = GetAllElements().GroupBy(p => p.transform.position);

        foreach (var group in groups)
        {
            var blocks = group.ToList();
            while (blocks.Count > 1)
            {
                DestroyImmediate(blocks[1]);
                blocks.RemoveAt(1);

                removedDuplicates++;
            }
        }
    }

    public void BeautifyNames()
    {
        foreach (var mirrorableItem in MirrorableItems)
        {
            var id = 0;
            foreach (Transform child in mirrorableItem.Transform)
            {
                child.gameObject.name = $"{mirrorableItem.Name} {id++}";
            }
        }
    }

    public void CreateMirror()
    {
        foreach (var mirrorableItem in MirrorableItems)
        {
            var copiedBlocks = 0;
            var blocksToCopy = mirrorableItem.Transform.childCount;

            foreach (Transform child in mirrorableItem.Transform)
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

                var createdGameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                createdGameObject.transform.position = Vector3.Scale(child.position, new Vector3(1, 1, -1));
                createdGameObject.transform.parent = mirrorableItem.Transform;

                if (copiedBlocks++ >= blocksToCopy)
                {
                    break;
                }
            }
        }

        RemoveDuplicates();
        BeautifyNames();
    }

    public void RemoveMirror()
    {
        var children = GetAllElements();
        var removedBlocks = 0;
        var blocksToRemove = children.Where(p => p.transform.position.z < 0).ToList();

        foreach (var block in blocksToRemove)
        {
            DestroyImmediate(block);
            removedBlocks++;
        }

        RemoveDuplicates();
        BeautifyNames();
    }

    public void ApplyScale()
    {
        foreach (var mirrorableItem in MirrorableItems)
        {
            var firstBlockScale = mirrorableItem.Transform.GetChild(0).transform.localScale;
            var scaleVector = new Vector3(ScaleToApply / firstBlockScale.x, ScaleToApply / firstBlockScale.y, ScaleToApply / firstBlockScale.z);

            foreach (Transform child in mirrorableItem.Transform)
            {
                child.localScale = Vector3.Scale(child.localScale, scaleVector);
                child.localPosition = Vector3.Scale(child.localPosition, scaleVector);
            }
        }
    }

    private List<GameObject> GetAllElements()
    {
        var children = new List<GameObject>();
        foreach (var mirrorableItem in MirrorableItems)
        {
            foreach (Transform child in mirrorableItem.Transform)
            {
                children.Add(child.gameObject);
            }
        }

        return children;
    }
}
