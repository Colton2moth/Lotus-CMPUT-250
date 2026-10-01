using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    /*  Videos:
     *  https://www.youtube.com/watch?v=v2pRG7n_-cs - Unity - Screen fader
     *  
     *  Forum:
     *  https://discussions.unity.com/t/fade-to-black/657361 - Ignore the code btw, its outdated and OnGUI is apparently notoriously laggy. The theory is good
     *  
     *  Documentation:
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Canvas.html
     */
public class ScreenFadeToBlack : MonoBehaviour
{
    private static ScreenFadeToBlack _instance;
    public static ScreenFadeToBlack Instance { get { return _instance; } }

    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float waitOnBlackDuration = 0.4f;

    private CanvasGroup canvasGroup;
    private PlayerController targetPlayer;
    private float timer = 0f;

    // A state machine to track what part of the fade we are in
    private enum FadeState { Idle, FadingOut, WaitingOnBlack, FadingIn }
    private FadeState currentState = FadeState.Idle;

    public bool IsScreenBlack => currentState == FadeState.WaitingOnBlack;

    // Literally ripped this out of audiocontroller and renamed it
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeAndRespawn(PlayerController player)
    {
        // Prevent starting a new fade if one is already happening
        if (currentState != FadeState.Idle) return;

        targetPlayer = player;
        // prevent the player from moving
        targetPlayer.canMove = false;

        timer = 0f;
        currentState = FadeState.FadingOut;
    }

    private void Update()
    {
        // Do nothing if we arent fading
        if (currentState == FadeState.Idle) return;

        timer += Time.deltaTime;

        if (currentState == FadeState.FadingOut)
        {
            // Increase alpha to 1 (full screen of black)
            canvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            
            // if timer is > than fade duration (so once screen is fully black) then teleport the player
            if (timer >= fadeDuration)
            {
                canvasGroup.alpha = 1f;
                targetPlayer.teleport(targetPlayer.currentCheckpointPosition);

                timer = 0f;
                currentState = FadeState.WaitingOnBlack;
            }
        }
        else if (currentState == FadeState.WaitingOnBlack)
        {
            // Pause in the dark for a moment so the camera gets reset properly
            if (timer >= waitOnBlackDuration)
            {
                timer = 0f;
                currentState = FadeState.FadingIn;
            }
        }
        else if (currentState == FadeState.FadingIn)
        {
            // Decrease alpha back to 0 (clear)
            canvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);

            if (timer >= fadeDuration)
            {
                canvasGroup.alpha = 0f;
                targetPlayer.canMove = true;

                currentState = FadeState.Idle;
            }
        }
    }
}
