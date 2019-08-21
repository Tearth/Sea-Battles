using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public ShipEntity PlayerShip;

    public Text CrewCountText;
    public Text CannonsCountTest;

    void Start()
    {
        
    }

    void Update()
    {
        CrewCountText.text = PlayerShip.CrewCount.ToString();
        CannonsCountTest.text = PlayerShip.CannonsCount.ToString();
    }
}
