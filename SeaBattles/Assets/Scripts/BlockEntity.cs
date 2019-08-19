using UnityEngine;

public class BlockEntity : MonoBehaviour
{
    public ShipEntity ShipEntity;

    void OnCollisionEnter(Collision collision)
    {
        var colPoint = collision.contacts[0].point;

        if (collision.transform.position.x > transform.position.x)
            colPoint -= new Vector3(0.1f, 0, 0);
        else
            colPoint += new Vector3(0.1f, 0, 0);

        if (collision.transform.position.y > transform.position.y)
            colPoint -= new Vector3(0, 0.1f, 0);
        else
            colPoint += new Vector3(0, 0.1f, 0);

        if (collision.transform.position.z > transform.position.z)
            colPoint -= new Vector3(0, 0, 0.1f);
        else
            colPoint += new Vector3(0, 0, 0.1f);

        ShipEntity.DeleteVoxel(colPoint);
    }
}