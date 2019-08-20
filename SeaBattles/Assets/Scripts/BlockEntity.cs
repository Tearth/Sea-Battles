using UnityEngine;

public class BlockEntity : MonoBehaviour
{
    public ShipEntity ShipEntity;

    void OnCollisionEnter(Collision collision)
    {
        var contact = collision.contacts[0];
        var boxCollider = (BoxCollider)contact.thisCollider;
        var colPoint = transform.InverseTransformPoint(contact.point);
        var colliderType = boxCollider.size.x > boxCollider.size.z ? ColliderType.ForwardBack : ColliderType.RightLeft;

        switch (colliderType)
        {
            case ColliderType.ForwardBack:
            {
                var rounded = Mathf.Floor(colPoint.x + 0.5f);
                var clamped = Mathf.Clamp(rounded, 0, boxCollider.size.x - 1);
                colPoint = new Vector3(clamped, 0, 0);

                break;
            }

            case ColliderType.RightLeft:
            {
                var rounded = Mathf.Floor(colPoint.z + 0.5f);
                var clamped = Mathf.Clamp(rounded, 0, boxCollider.size.z - 1);
                colPoint = new Vector3(0, 0, clamped);

                break;
            }
        }
        

        ShipEntity.DeleteVoxel(transform.TransformPoint(colPoint));
        ShipEntity.DeleteCollider(colPoint, (BoxCollider)contact.thisCollider, colliderType);
    }
}