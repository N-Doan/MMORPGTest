using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIElement : MonoBehaviour
{
    public Item item;
    public Text buttonText;
    public Button button;

    private void Start()
    {
        buttonText = gameObject.GetComponentInChildren<Text>();
    }
    public void setDispText()
    {
        buttonText.text = item.name;
    }
}
