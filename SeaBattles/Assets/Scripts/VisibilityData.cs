using UnityEngine;

public class VisibilityData
{
    public bool Up { get; set; }
    public BoxCollider UpCollider;

    public bool Down { get; set; }
    public BoxCollider DownCollider;

    public bool Forward { get; set; }
    public BoxCollider ForwardCollider;

    public bool Back { get; set; }
    public BoxCollider BackCollider;

    public bool Right { get; set; }
    public BoxCollider RightCollider;

    public bool Left { get; set; }
    public BoxCollider LeftCollider;
}