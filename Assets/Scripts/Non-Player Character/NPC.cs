using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;

public class NPC : MonoBehaviour
{

    // Start is called before the first frame update
    private GameObject player = null;
    public float maxDetectionDistance = 1;
    public interactionGlyph interactionGlyph;
    private bool canInteract = false;
    [SerializeField] private TextAsset CSV;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if(player == null)
        {
            Debug.Log("NPC unable to find reference to player.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null) return;

        if (Vector3.Distance(player.transform.position, this.transform.position) < maxDetectionDistance)
        {
            //Debug.Log("Player has entered area.");
            interactionGlyph.setActive(true);
            canInteract = true;
        }
        else
        {
            //Debug.Log("The player has exited the area.");
            interactionGlyph.setActive(false);
            canInteract = false;
        }

        if(canInteract && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Dialogue started! Now parsing \"{CSV.ToString()}\"");
            DialogueSystem.Instance.startDialogue(CSV.ToString());
        }
    }

    void setCanInteract(bool newState){ canInteract = newState; }
    
    
}
