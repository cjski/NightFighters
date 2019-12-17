using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterAI : HumanAI
{
    protected static int wallLayerMask;
    protected static float allowableDashDistance = 5.0f;
    protected static float minimumDistanceFromTargetForDashSqr = 25.0f;
    protected override void Start()
    {
        wallLayerMask = LayerMask.GetMask("Wall");
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (playerController != null)
        {
            canSeeTarget = false;
            targetIsPlayer = false;
            finalDirection = GetDirectionToTargetForMovement();
            if (canSeeTarget && targetIsPlayer && playerController.primaryCooldown.done)
            {
                playerController.AIUsePrimary();
            }
            if(playerController.secondaryCooldown.done)
            {
                RaycastHit2D hitTarget = Physics2D.Raycast(playerController.transform.position, finalDirection, allowableDashDistance, wallLayerMask);

                // Dash if there is no wall that the AI would crash into
                if ( hitTarget.collider == null )
                {
                    // Don't dash if you would overshoot(light) or crash into the target(player)
                    if (!canSeeTarget || (canSeeTarget && distanceToTargetSquared > minimumDistanceFromTargetForDashSqr) )
                    {
                        playerController.AIUseSecondary();
                    }
                }
            }

            if (finalDirection.sqrMagnitude > 0.01f) playerController.AIMove(finalDirection);
        }
    }
}
