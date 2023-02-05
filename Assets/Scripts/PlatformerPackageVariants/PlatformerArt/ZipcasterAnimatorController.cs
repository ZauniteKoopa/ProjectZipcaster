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
