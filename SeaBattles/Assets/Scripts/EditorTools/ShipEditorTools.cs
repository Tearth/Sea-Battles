using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ShipEditorTools : MonoBehaviour
{
    public List<MirrorableItem> MirrorableItems;
    public float ScaleToApply;
#if UNITY_EDITOR
    public void RemoveDuplicates()
    {
        var groups = GetAllElements().GroupBy(p => p.transform.position);

        foreach (var group in groups)
        {
            var blocks = group.ToList();
            while (blocks.Count > 1)
            {
                DestroyImmediate(blocks[1]);
                blocks.RemoveAt(1);
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
        ReorderBlocks();
        BeautifyNames();
    }

    public void RemoveMirror()
    {
        var children = GetAllElements();
        var blocksToRemove = children.Where(p => p.transform.position.z < 0).ToList();

        foreach (var block in blocksToRemove)
        {
            DestroyImmediate(block);
        }

        RemoveDuplicates();
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

    public void ReorderBlocks()
    {
        var changed = true;
        var reorderedBlocks = 0;

        while (changed)
        {
            var all = new List<GameObject>();
            foreach (Transform child in MirrorableItems.First().Transform)
            {
                all.Add(child.gameObject);
            }

            all = all.OrderBy(p => p.transform.GetSiblingIndex()).ToList();

            changed = false;
            for (var i = 0; i < all.Count - 1; i++)
            {
                var first = all[i];
                var second = all[i + 1];

                if ((first.transform.position.x > second.transform.position.x) ||
                    (Math.Abs(first.transform.position.x - second.transform.position.x) < 0.001f && 
                     first.transform.position.y > second.transform.position.y) ||
                    (Math.Abs(first.transform.position.x - second.transform.position.x) < 0.001f && 
                     Math.Abs(first.transform.position.y - second.transform.position.y) < 0.001f && 
                     first.transform.position.z > second.transform.position.z))
                {
                    var firstIndex = first.transform.GetSiblingIndex();
                    var secondIndex = second.transform.GetSiblingIndex();

                    first.transform.SetSiblingIndex(secondIndex);
                    second.transform.SetSiblingIndex(firstIndex);

                    changed = true;
                    reorderedBlocks++;
                }
            }
        }

        Debug.Log($"Reordered {reorderedBlocks} blocks");
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
#endif
}