using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    

    
    private List<DialogueChunk> chunks = new List<DialogueChunk>();
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    private string ID;

    public void addChunk(DialogueChunk newChunk)
    {
        chunks.Add(newChunk);
    }
    public void addChoice(DialogueChoice newChoice)
    {
        choices.Add(newChoice);
    }

    public bool isEndofDialogue(int index)
    {
        Debug.Log($"Comparing {index} >= {chunks.Count} - 1 and found {index >= chunks.Count - 1}");
        return index >= (chunks.Count - 1);
    }

    public bool hasChoices()
    {
        return choices.Count > 0;
    }

    public void setID(string newID){ ID = newID; }
    //Returns the ID of the dialogue
    public string getID(){ return ID; }

    //Returns text of the given index
    public string getText(int index){ return chunks[index].text; }
    
    //Returns the left portrait of the given dialogue chunk index
    public string getPortraitLeftID(int index){ return chunks[index].portraitLeft; }
    //Returns the right portrait of the given dialogue chunk index
    public string getPortraitRightID(int index){ return chunks[index].portraitRight; }
    
    public string getSoundID(int index){ return chunks[index].sfxID; }

    public string getTextboxTitle(int index){ return chunks[index].textboxTitle; }
}
