using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Narrative;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    private static DialogueSystem _ds;

    private DialogueSystem() { }
    public static DialogueSystem Instance { get { return _ds; }}

    //Setting up singleton
    void Awake()
    {
        if(_ds != null && _ds != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log("Initialized dialogueSystem.");
            _ds = this;
        }
    }
    //Is the dialogue UI visible?
    private bool isActive = false;
    //Are the choice buttons visible?
    private bool buttonsActive = false;
    //Current chunk being displayed
    private int chunkIndex = 0;
    //Current dialogue being wirked through
    private Dialogue currentDialogue = null;
    #region References
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private DialoguePortrait portraitLeft;
    [SerializeField] private DialoguePortrait portraitRight;
    [SerializeField] private GameObject ChoiceButton;
    [SerializeField] private GameObject choiceButtonContainer;
    #endregion
    #region Data Containers
    //portrait ID -> portrait Sprite
    [SerializeField] private List<Sprite> rawSprites = new List<Sprite>();
    [SerializeField] private Dictionary<string, Sprite> portraitIDSprite = new Dictionary<string, Sprite>();
    //Raw dialogues as TextAssets
    [SerializeField] private List<TextAsset> rawDialogues = new List<TextAsset>();
    //dialogueID -> Dialogue
    [SerializeField] private Dictionary<string, Dialogue> idDialogue = new Dictionary<string, Dialogue>();
    #endregion
    
    //Current line in a dialogue that we are at
    
    void Start()
    {
        //Grab box for dialogue from scene by type
        dialogueBox = GameObject.FindAnyObjectByType<DialogueBox>();
        //Grab container for choice buttons from container
        choiceButtonContainer = GameObject.FindGameObjectWithTag("ChoiceButtonContainer");
        //add all dialogues to dictionary
        foreach(TextAsset rawTxt in rawDialogues)
        {
            Dialogue newDia = DialogueParser.ParseDialogue(rawTxt.ToString());
            idDialogue[newDia.getID()] = newDia;
        }

        loadSprites();
    }

    public void startDialogue(string CSV)
    {
        if(currentDialogue != null) return;

        isActive = true;
        dialogueBox.ClearName();
        chunkIndex = 0;
        currentDialogue = DialogueParser.ParseDialogue(CSV);

        portraitLeft.EnterFade(0.5f);
        portraitRight.EnterFade(0.5f);
        portraitLeft.SetSprite(portraitIDSprite[currentDialogue.getPortraitLeftID(chunkIndex)]);
        portraitRight.SetSprite(portraitIDSprite[currentDialogue.getPortraitRightID(chunkIndex)]);
        Debug.Log($"Added dialogue with {currentDialogue.getText(chunkIndex)} as its first dialogue.");
        //Debug.Log($"Added dialogue with {currentDialogue.getText(chunkIndex + 1)} as its second dialogue.");
        
        dialogueBox.SetLine(currentDialogue.getText(chunkIndex));
        dialogueBox.SetName(currentDialogue.getTextboxTitle(chunkIndex));
    }
    //Version with a Dialogue passed instead of a string to parse
    public void startDialogue(Dialogue dia)
    {
        Debug.Log("Running start dialogue...");
        if(currentDialogue != null) return;
        isActive = true;
        dialogueBox.ClearName();
        chunkIndex = -1;
        currentDialogue = dia;
        //Debug.Log($"Added dialogue with {currentDialogue.getText(chunkIndex)} as its first dialogue.");
        //Debug.Log($"Added dialogue with {currentDialogue.getText(chunkIndex + 1)} as its second dialogue.");
        progressDialogue();
    }

    //Creates the choice buttons
    private void startChoiceScreen(Dialogue dlg)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        //Create buttons for each option
        List<DialogueChoice> choices = dlg.choices;
        foreach(DialogueChoice choice in choices)
        {
            Debug.Log($"{choice.buttonText}");
            DialogueButton newChoiceButton = Instantiate(ChoiceButton, choiceButtonContainer.transform).GetComponent<DialogueButton>();
            newChoiceButton.setChoice(choice);
            //Adding listener to each button for unique choice
            newChoiceButton.GetComponent<Button>().onClick.AddListener(() => makeChoice(choice));
        }
    }
    //Called on button press, starts the next dialogue based on the passed in choice
    public void makeChoice(DialogueChoice choice)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log($"Made choice {choice.buttonText}");

        //Clear all old dialogue buttons
        foreach(Transform child in choiceButtonContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Initiate chosen Dialogue
        if (idDialogue.Keys.Contains(choice.nextDialogueID))
        {
            startDialogue(idDialogue[choice.nextDialogueID]);
        }
        else
        {
            Debug.Log($"Attempted to start dialogue with ID {choice.nextDialogueID} when no dialogue has such id.");
        }
        
    }

    void Update()
    {
        updateDialogueBox();
        if (isActive)
        {
            if (Input.GetKeyDown(KeyCode.Space)) //TODO: Confirm if we r using space
            {
                progressDialogue();
            }
        }
    }

    void progressDialogue()
    {
        //Reached the end of the current dialogue
        if (currentDialogue.isEndofDialogue(chunkIndex))
        {

            isActive = false;
            portraitLeft.ExitFade(0.5f);
            portraitRight.ExitFade(0.5f);
            //Start choices if we have any
            if(currentDialogue.hasChoices()) startChoiceScreen(currentDialogue);
            currentDialogue = null;

        }
        else //We aren't at the end of the current dialogue
        {
            //Move to next chunk and update
            chunkIndex++;
            dialogueBox.SetLine(currentDialogue.getText(chunkIndex));
            dialogueBox.SetName(currentDialogue.getTextboxTitle(chunkIndex));
            Debug.Log("Here");
            portraitLeft.SetSprite(portraitIDSprite[currentDialogue.getPortraitLeftID(chunkIndex)]);
            portraitRight.SetSprite(portraitIDSprite[currentDialogue.getPortraitRightID(chunkIndex)]);
            
        }
    }
    void updateDialogueBox()
    {
        if (!dialogueBox.IsOpen && isActive) dialogueBox.OpenTextbox();
        if(dialogueBox.IsOpen && !isActive) dialogueBox.CloseTextbox();
    }

    //Adds all sprites to the dictionary with their keys being their names in files
    void loadSprites()
    {
        foreach(Sprite spr in rawSprites)
        {
            portraitIDSprite[spr.name] = spr;
            Debug.Log($"Added {spr.name} to spriteID.");
        }
        portraitIDSprite["EMPTY"] = null;
    }
}
