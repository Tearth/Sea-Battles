using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public ShipEntity PlayerShip;

    public Text CrewCountText;
    public Text CannonsCountTest;

    void Update()
    {
        CrewCountText.text = PlayerShip.CrewCount.ToString();
        CannonsCountTest.text = PlayerShip.CannonsCount.ToString();
    }
}
