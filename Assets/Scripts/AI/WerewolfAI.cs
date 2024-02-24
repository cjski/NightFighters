using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfAI : MonsterAI
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (playerController)
        {
            WerewolfController wc = (WerewolfController)playerController;
            finalDirection = GetDirectionToTargetForMovement();

            if(wc.primaryCooldown.done && Mathf.Pow(wc.knockbackRange, 2) > closestHumanDistanceSqr)
            {
                wc.AIMove(directionToAttack);
                wc.AIUsePrimary();
            }
            else if(wc.secondaryCooldown.done)
            {
                RaycastHit2D hitTarget = Physics2D.Raycast(playerController.GetPosition(), finalDirection, wc.GetDashDistance(), wallLayerMask);

                // Dash if there is no wall that the AI would crash into
                if (hitTarget.collider)
                {
                    // Don't dash if you would overshoot a light you want to turn off or if you have a position you don't want to move from
                    if (closestLightDistanceSqr > Mathf.Pow(wc.GetDashDistance(), 2) && finalDirection.sqrMagnitude > 0.01f)
                    {
                        playerController.AIUseSecondary();
                    }
                }
            }

            if (finalDirection.sqrMagnitude > 0.01f)
            {
                playerController.AIMove(finalDirection);
            }
        }
    }
}
