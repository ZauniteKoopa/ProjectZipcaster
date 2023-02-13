using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZipcasterAnimatorController : PlatformerScaleAnimator
{
    // Main function to update animator variables
    //  Pre: none
    //  Post: updates animator variables based on platformer package state
    protected override void updateAnimatorVariables(PlatformerPackage p) {
        ZipcasterPlatformerPackage zPlatformer = p as ZipcasterPlatformerPackage;
        Debug.Assert(zPlatformer != null);

        if (animator != null) {
            base.updateAnimatorVariables(p);
            animator.SetBool("HookFiring", zPlatformer.isHooking);
            animator.SetBool("ZipDashing", zPlatformer.isDashing);
        }
    }


    // Main function to update sprite transform
    //  Pre: none
    //  Post: Transform variables are updated based on variables from the platformer package
    protected override void updateSpriteTransform(PlatformerPackage p) {
        Debug.Assert(p != null);

        base.updateSpriteTransform(p);

        ZipcasterPlatformerPackage zPlatformer = p as ZipcasterPlatformerPackage;
        Debug.Assert(zPlatformer != null);

        // If dashing, set orientation of the player to the dash direction
        if (zPlatformer.isDashing) {
            Vector2 curDashDir = zPlatformer.dashDirection;

            // In case of dashing left, flip the dashDir vector so that it considers flipX (the sprite won't go upside down)
            if (curDashDir.x < 0f) {
                curDashDir *= -1f;
            }

            transform.right = curDashDir;
        } else {
            transform.right = Vector2.right;
        }

    }


    // Main function to check if whether or not the sprite will face left
    //  Pre: platformer != null
    //  Post: returns a boolean to check if platformer is facing left
    protected override bool isFacingLeft(PlatformerPackage p) {
        Debug.Assert(p != null);

        ZipcasterPlatformerPackage zPlatformer = p as ZipcasterPlatformerPackage;
        Debug.Assert(zPlatformer != null);

        bool playerZipping = zPlatformer.isHooking || zPlatformer.isDashing;

        // If you're moving or grabbing a wall or in the process of zipping, use the base
        if (Mathf.Abs(p.getHorizontalAxis) > 0.01f || p.isGrabbingWall()) {
            return base.isFacingLeft(p);
        
        // Else, do it based on 
        } else {

            return Vector3.Project(zPlatformer.currentMouseDir, Vector3.left).normalized == Vector3.left;
        }
    }

}
