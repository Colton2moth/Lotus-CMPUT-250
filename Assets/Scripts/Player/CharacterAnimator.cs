using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles 2D sprite directional changes and animation states for the player.
 * Inherits frame playback timing and interruption logic from AnimatedEntity.
 * 
 * Manages front, back, and side sprite sets for idle, walk, and jump animations,
 * flipping the sprite on the X-axis for horizontal movement.
 */
public class CharacterAnimator : AnimatedEntity
{
    /**
     * Groups sprite frame lists for front, back, and side views in the Inspector.
     */
    [System.Serializable]
    public class DirectionalAnimations
    {
        public List<Sprite> front;
        public List<Sprite> back;
        public List<Sprite> side;

        /**
         * Returns the sprite list for a given direction index (0 = front, 1 = back, 2 = side).
         */
        public List<Sprite> GetList(int direction)
        {
            if (direction == 1) return back;
            if (direction == 2) return side;
            return front; // default
        }
    }

    public DirectionalAnimations idle;
    public DirectionalAnimations walk;
    public DirectionalAnimations jump;

    private int currentFacing = 0; // 0 is front, 1 is back, 2 is side.

    /**
     * Grabs the SpriteRenderer and initializes the default front-facing idle cycle.
     */
    void Start()
    {
        // finds sprite renderer component on this object or any of its children
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // default is front facing idle
        DefaultAnimationCycle = idle.GetList(0);

        base.AnimationSetup();
    }

    /**
     * Advances the active animation frame using AnimatedEntity's update cycle.
     */
    void Update()
    {
        base.AnimationUpdate();
    }

    /**
     * Determines facing direction based on input vector.
     * Prioritizes vertical facing (front/back) over horizontal, and flips the sprite horizontally for left-facing movement.
     * @param input Raw 2D directional movement vector.
     */
    public void UpdateFacing(Vector2 input)
    {
        //prioritizes Front facing and back facing actions 
        if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
        {
            SpriteRenderer.flipX = false;
            currentFacing = input.y > 0 ? 1 : 0;
        }
        else
        {
            currentFacing = 2;
            SpriteRenderer.flipX = (input.x < 0);
        }
    }

    /**
     * Swaps the current looping animation cycle between idle and walk based on movement state.
     * @param isMoving True to play walk cycle, false to play idle cycle.
     */
    public void SetMoving(bool isMoving)
    {
        DirectionalAnimations set = isMoving ? walk : idle;
        DefaultAnimationCycle = set.GetList(currentFacing);
    }

    /**
     * Triggers the jump animation cycle for the current facing direction as an interrupt.
     */
    public void TriggerJump()
    {
        List<Sprite> jumpCycle = jump.GetList(currentFacing);
        base.Interrupt(jumpCycle);
    }
}