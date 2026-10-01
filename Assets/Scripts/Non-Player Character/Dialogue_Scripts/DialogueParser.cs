using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DialogueParser : MonoBehaviour
{
    // Start is called before the first frame update
    const int ID = 0;
    const int PORTRAITLEFT = 1;
    const int PORTRAITRIGHT = 2;
    const int TITLE = 3;
    const int SFXID = 4;

    const int CHOICENEXTID = 0;
    const int CHOICETALLY = 1;
    const int CHOICETEXT = 2;
    const int TEXTSTART = 5;

    const string ESCAPECHUNKCREATION = "END";
    public static Dialogue ParseDialogue(string text)
    {
        Dialogue returnDialogue = new Dialogue();

        List<string> rawArray = text.Split("\n").ToList<string>();
        
        bool makingChoices = false;
        //Parsing each dialogue chunk and adding it to the dialogue
        foreach(string str in rawArray)
        {

            //skipping empty lines
            if(str == "") continue;

            //Check if making buttons.
            if(string.Equals(str.ToUpper().Trim(), ESCAPECHUNKCREATION)) {
                makingChoices = true;
                continue;
            }

            if (!makingChoices)
            {
                DialogueChunk newChunk = new DialogueChunk();
                Debug.Log($"Currently parsing {str}");
                //Split data into individual parts
                List<string> newChunkData = str.Split(",").ToList<string>();
                //Assign each part to a new chunk
                newChunk.portraitLeft = newChunkData[PORTRAITLEFT];
                newChunk.portraitRight = newChunkData[PORTRAITRIGHT];
                newChunk.textboxTitle = newChunkData[TITLE];
                newChunk.sfxID = newChunkData[SFXID];

                //parsing text from the end, accounting for included semicolons
                string newChunkText = "";
                for(int i = TEXTSTART; i < newChunkData.Count; i++)
                {
                    //Adding a comma for each subsequent segment of the newChunkData
                    if(i != TEXTSTART && i != newChunkData.Count)newChunkText += ",";
                    newChunkText += newChunkData[i];
                }

                newChunk.text = newChunkText;
                //Setting the returnDialogues ID, could be changed later
                returnDialogue.setID(newChunkData[ID]);

                returnDialogue.addChunk(newChunk);
            }
            else
            {
                DialogueChoice newChoice = new DialogueChoice();
                List<string> newChoiceData = str.Split(",").ToList<string>();
                newChoice.nextDialogueID = newChoiceData[CHOICENEXTID];
                newChoice.buttonText = newChoiceData[CHOICETEXT];
                if(int.TryParse(newChoiceData[CHOICETALLY], out int ret)){
                    newChoice.tallyValue = ret;
                }
                else
                {
                    newChoice.tallyValue = 0;
                }
                Debug.Log($"Added choice with text {newChoiceData[CHOICETEXT]}.");
                returnDialogue.addChoice(newChoice);
            }
            
        }

        

        return returnDialogue;
    }
}
