using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

public class ShipEditorTools : MonoBehaviour
{
    public Transform Blocks;

    public void RemoveDuplicates()
    {
        var removedDuplicates = 0;
        var groups = GetAllBlocks().GroupBy(p => p.transform.position);

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

        Debug.Log($"Removed duplicates: {removedDuplicates}");
    }

    public void BeautifyNames()
    {
        var blockId = 0;
        foreach (Transform child in Blocks.transform)
        {
            child.gameObject.name = $"Block {blockId++}";
        }

        Debug.Log($"Beautified {blockId} blocks");
    }

    public void CreateMirror()
    {
        var copiedBlocks = 0;
        var blocksToCopy = Blocks.childCount;

        foreach (Transform child in Blocks.transform)
        {
            Instantiate(child.gameObject, Vector3.Scale(child.position, new Vector3(1, 1, -1)), Quaternion.identity, Blocks);

            if (copiedBlocks++ >= blocksToCopy)
            {
                break;
            }
        }

        Debug.Log($"Created new {copiedBlocks} blocks");
    }

    public void RemoveMirror()
    {
        var children = GetAllBlocks();
        var removedBlocks = 0;
        var blocksToRemove = children.Where(p => p.transform.position.z < 0).ToList();

        foreach (var block in blocksToRemove)
        {
            DestroyImmediate(block);
            removedBlocks++;
        }

        Debug.Log($"Removed {removedBlocks} blocks");
    }

    private List<GameObject> GetAllBlocks()
    {
        var children = new List<GameObject>();
        foreach (Transform child in Blocks)
        {
            children.Add(child.gameObject);
        }

        return children;
    }
}
