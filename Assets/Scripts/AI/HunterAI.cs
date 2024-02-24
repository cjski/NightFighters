using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterAI : HumanAI
{
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (playerController != null && !playerController.IsStunned())
        {
            HunterController hc = (HunterController)playerController;
            canSeeTarget = false;
            targetIsPlayer = false;
            finalDirection = GetDirectionToTargetForMovement();
            if (canSeeTarget && targetIsPlayer && playerController.primaryCooldown.done)
            {
                playerController.AIUsePrimary();
            }
            // Use an else here so the AI doesn't end up using 2 powers in one turn
            else if(playerController.secondaryCooldown.done)
            {
                RaycastHit2D hitTarget = Physics2D.Raycast(playerController.GetPosition(), finalDirection, hc.GetDashDistance(), wallLayerMask);

                // Dash if there is no wall that the AI would crash into
                if ( hitTarget.collider == null )
                {
                    // Don't dash if you would overshoot(light) or crash into the target(player)
                    if (!canSeeTarget || (canSeeTarget && distanceToTargetSquared > Mathf.Pow(hc.GetDashDistance(),2)))
                    {
                        playerController.AIUseSecondary();
                    }
                }
            }

            if (finalDirection.sqrMagnitude > 0.01f) playerController.AIMove(finalDirection);
        }
    }
}
