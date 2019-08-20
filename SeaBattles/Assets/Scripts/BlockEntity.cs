using UnityEngine;

public class BlockEntity : MonoBehaviour
{
    public ShipEntity ShipEntity;

    void OnCollisionEnter(Collision collision)
    {
        var contact = collision.contacts[0];
        var boxCollider = (BoxCollider)contact.thisCollider;
        var colPoint = transform.InverseTransformPoint(contact.point);

        var roundedX = Mathf.Floor(colPoint.x + 0.5f);
        var clampedX = Mathf.Clamp(roundedX, 0, boxCollider.size.x - 1);
        colPoint = new Vector3(clampedX, 0, 0);

        ShipEntity.DeleteVoxel(transform.TransformPoint(colPoint));
        ShipEntity.DeleteCollider(colPoint, (BoxCollider)contact.thisCollider);
    }
}