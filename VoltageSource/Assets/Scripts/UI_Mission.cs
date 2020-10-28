using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Mission : MonoBehaviour
{
    [SerializeField] private textWriter textWriter;
    public Text messageText;
    private string uiText = "MISSION:  DESTROY YOUR ENEMY BEFORE YOUR TERRITORY GOES MISSING...";

    private void Start()
    {
        textWriter.AddWriter(messageText, uiText, 0.07f);
    }
}
