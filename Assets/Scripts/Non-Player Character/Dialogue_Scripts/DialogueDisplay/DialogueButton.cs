using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class DialogueButton : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI text;
    private DialogueChoice choice;

    public void setChoice(DialogueChoice newChoice)
    {
        choice = newChoice;
        text.text = choice.buttonText;
    }

    
}
