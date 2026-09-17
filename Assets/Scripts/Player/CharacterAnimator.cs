using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:
// Update animated entity to allow for looping anims until action is done (fluttering, jumping etc.)
// Handles 2D sprite animation playback and directional changes for the player. Inehrits AnimatedEntity.cs
public class CharacterAnimator : AnimatedEntity
{
    // data container to group 4-directional sprite frames together in the Inspector. Serializable just makes it so we can see it in the inspector
    [System.Serializable]
    public class DirectionalAnimations {
        public List<Sprite> front;
        public List<Sprite> back;
        public List<Sprite> side;

        // helper method to get the direction 
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
    public DirectionalAnimations flutter;

    private int currentFacing = 0; // 0 is front, 1 is back, 2 is side.

    void Start()
    {
        // finds sprite renderer component on this object or any of its children
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // default is front facing idle
        DefaultAnimationCycle = idle.GetList(0);

        base.AnimationSetup();
    }
    void Update()
    {
        base.AnimationUpdate();
    }

    // Reads input then changes direction of the sprite
    public void UpdateFacing(Vector2 input) 
    {
        //prioritizes Front facing and back facing actions 
        if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
        {
            currentFacing = input.y > 0 ? 1 : 0;
        }
        else
        {
            currentFacing = 2;
            SpriteRenderer.flipX = (input.x < 0);
        }
    }
    
    // Swaps between idle or walk animation cycles
    public void SetMoving(bool isMoving) {
        DirectionalAnimations set = isMoving ? walk : idle;
        DefaultAnimationCycle = set.GetList(currentFacing);
    }

    // interrupts current animation to play jump aniamtion cycle
    public void TriggerJump() {
        List<Sprite> jumpCycle = jump.GetList(currentFacing);
        base.Interrupt(jumpCycle);
    }

    // Interrupts or sets the flutter animation cycle
    public void TriggerFlutter()
    {
        List<Sprite> flutterCycle = flutter.GetList(currentFacing);
        base.Interrupt(flutterCycle);
    }
}
