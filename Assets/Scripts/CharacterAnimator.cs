using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : AnimatedEntity
{
    [System.Serializable]
    public class DirectionalAnimations {
        public List<Sprite> front;
        public List<Sprite> back;
        public List<Sprite> side;

        public List<Sprite> GetList(int direction) 
        {
            if (direction == 1) return back;
            if (direction == 2) return side;
            return front;
        }
    }

    public DirectionalAnimations idle;
    public DirectionalAnimations walk;
    public DirectionalAnimations jump;

    private int currentFacing = 0; // 0 is front, 1 is back, 2 is side.
    /*  This class is used for the sprite animations, it inherits from AnimatedEntity.cs and is used in PlayerController.cs (That script sends the information to here)
     */
    void Start()
    {
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        DefaultAnimationCycle = idle.GetList(0);

        base.AnimationSetup();
    }
    void Update()
    {
        base.AnimationUpdate();
    }

    public void UpdateFacing(Vector2 input) 
    {
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

    public void SetMoving(bool isMoving) {
        DirectionalAnimations set = isMoving ? walk : idle;
        DefaultAnimationCycle = set.GetList(currentFacing);
    }

    public void TriggerJump() {
        List<Sprite> jumpCycle = jump.GetList(currentFacing);
        base.Interrupt(jumpCycle);
    }
}
