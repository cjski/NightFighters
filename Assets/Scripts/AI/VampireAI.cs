﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireAI : MonsterAI
{
    protected static int wallLayerMask;
    // Start is called before the first frame update
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
            VampireController vc = (VampireController)playerController;
            finalDirection = GetDirectionToTargetForMovement();

            if (vc.primaryCooldown.done && Mathf.Pow(vc.biteRange, 2) > closestHumanDistanceSqr)
            {
                vc.AIMove(directionToAttack);
                vc.AIUsePrimary();
            }
            // Do a dash as a vampire if you can slow the humans down, don't just waste it
            else if (vc.secondaryCooldown.done && Mathf.Pow(vc.GetDashDistance(), 2) > closestHumanDistanceSqr)
            {
                RaycastHit2D hitTarget = Physics2D.Raycast(playerController.transform.position, finalDirection, vc.GetDashDistance(), wallLayerMask);

                // Dash if there is no wall that the AI would crash into
                if (hitTarget.collider == null)
                {
                    // Don't dash if you would overshoot a light you want to turn off
                    if (closestLightDistanceSqr > Mathf.Pow(vc.GetDashDistance(), 2))
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
