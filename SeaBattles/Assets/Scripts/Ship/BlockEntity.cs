using UnityEngine;

public class BlockEntity : MonoBehaviour
{
    [Header("General settings")]
    public float MinImpulseToDestroy;

    [HideInInspector]
    public ShipEntity ShipEntity;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.sqrMagnitude < MinImpulseToDestroy)
        {
            return;
        }
        
        var contact = collision.GetContact(0);
        var boxCollider = (BoxCollider)contact.thisCollider;
        var collisionPoint = transform.InverseTransformPoint(contact.point);
        var colliderType = GetColliderType(boxCollider.size);

        var collidedVoxelLocalPosition = CalculateCollidedVoxelPosition(colliderType, collisionPoint, boxCollider.size);
        var collidedVoxelWorldPosition = transform.TransformPoint(collidedVoxelLocalPosition);

        DeleteCollider(collidedVoxelLocalPosition, (BoxCollider)contact.thisCollider, collision, colliderType);

        ShipEntity.DeleteVoxel(collidedVoxelWorldPosition);
        ShipEntity.AddDynamicVoxel(collidedVoxelWorldPosition, collision.relativeVelocity);
    }

    private ColliderType GetColliderType(Vector3 size)
    {
        return size.x > size.z ? ColliderType.ForwardBack : ColliderType.RightLeft;
    }

    private Vector3 CalculateCollidedVoxelPosition(ColliderType colliderType, Vector3 collisionPoint, Vector3 colliderSize)
    {
        switch (colliderType)
        {
            case ColliderType.ForwardBack:
            {
                var rounded = Mathf.Floor(collisionPoint.x + 0.5f);
                var clamped = Mathf.Clamp(rounded, 0, colliderSize.x - 1);
                return new Vector3(clamped, 0, 0);
            }

            case ColliderType.RightLeft:
            {
                var rounded = Mathf.Floor(collisionPoint.z + 0.5f);
                var clamped = Mathf.Clamp(rounded, 0, colliderSize.z - 1);
                return new Vector3(0, 0, clamped);
            }
        }

        return Vector3.zero;
    }

    public void DeleteCollider(Vector3 position, BoxCollider boxCollider, Collision collision, ColliderType type)
    {
        // Distance to the center of collider
        var dist = Vector3.Distance(boxCollider.center, position);
        var originalSize = boxCollider.size;

        // Check if we have long or wide collider
        switch (type)
        {
            // Long collider
            case ColliderType.ForwardBack:
            {
                if (position.x < boxCollider.center.x)
                {
                    // Before:
                    //
                    //    hit    center 
                    //    \/       \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // oooo [                   ]
                    // 

                    boxCollider.size = new Vector3(boxCollider.size.x / 2 - dist - 0.5f, boxCollider.size.y, boxCollider.size.z);
                    boxCollider.center = new Vector3(boxCollider.size.x / 2 - 0.5f, boxCollider.center.y, boxCollider.center.z);
                }
                else
                {
                    //           center    hit
                    //             \/      \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // ooooooooooooo [          ]
                    // 

                    var diff = boxCollider.center.x - dist;

                    boxCollider.size = new Vector3(boxCollider.size.x - diff - 1, boxCollider.size.y, boxCollider.size.z);
                    boxCollider.center = new Vector3(boxCollider.size.x / 2 - 0.5f, boxCollider.center.y, boxCollider.center.z);
                }

                // Create new collider in the blank space (on the right)
                var sizeOfNewCollider = originalSize - new Vector3(boxCollider.size.x + 1, 0, 0);
                if (sizeOfNewCollider.x > 0)
                {
                    var rightGameObject = Instantiate(boxCollider.gameObject, Vector3.zero, Quaternion.identity, transform.parent);
                    var rightCollider = rightGameObject.GetComponent<BoxCollider>();

                    rightCollider.transform.position = boxCollider.transform.TransformPoint(position + new Vector3(1, 0, 0));
                    rightCollider.size = sizeOfNewCollider;
                    rightCollider.center = new Vector3(rightCollider.size.x / 2 - 0.5f, 0, 0);
                }

                // If left collider has length equal to zero, remove it
                if (boxCollider.size.x <= 0)
                {
                    Destroy(boxCollider.gameObject);
                }

                break;
            }

            // Wide collider
            case ColliderType.RightLeft:
            {
                if (position.z < boxCollider.center.z)
                {
                    // Before:
                    //
                    //    hit    center 
                    //    \/       \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // oooo [                   ]
                    // 

                    boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z / 2 - dist - 0.5f);
                    boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y, boxCollider.size.z / 2 - 0.5f);
                }
                else
                {
                    //           center    hit
                    //             \/      \/
                    // oooooooooooooooooooooooooo
                    //
                    // After:
                    //
                    // ooooooooooooo [          ]
                    // 

                    var diff = boxCollider.center.z - dist;

                    boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z - diff - 1);
                    boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y, boxCollider.size.z / 2 - 0.5f);
                }

                // Create new collider in the blank space (on the right)
                var sizeOfNewCollider = originalSize - new Vector3(0, 0, boxCollider.size.z + 1);
                if (sizeOfNewCollider.z > 0)
                {
                    var rightGameObject = Instantiate(boxCollider.gameObject, Vector3.zero, Quaternion.identity, transform.parent);
                    var rightCollider = rightGameObject.GetComponent<BoxCollider>();

                    rightCollider.transform.position = boxCollider.transform.TransformPoint(position + new Vector3(0, 0, 1));
                    rightCollider.size = sizeOfNewCollider;
                    rightCollider.center = new Vector3(0, 0, rightCollider.size.z / 2 - 0.5f);
                }

                // If left collider has length equal to zero, remove it
                if (boxCollider.size.z <= 0)
                {
                    Destroy(boxCollider.gameObject);
                }

                break;
            }
        }
    }
}