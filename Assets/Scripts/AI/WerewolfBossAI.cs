using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfBossAI : BossAI
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
            WerewolfBossController controller = playerController as WerewolfBossController;
            finalDirection = GetDirectionToTargetForMovement();

            if (controller.primaryCooldown.done && Mathf.Pow(controller.knockbackRange, 2) > distanceToTargetSquared)
            {
                controller.AIMove(finalDirection);
                controller.AIUsePrimary();
                return;
            }
            else if (controller.secondaryCooldown.done)
            {
                RaycastHit2D hitTarget = Physics2D.Raycast(controller.GetPosition(), finalDirection, controller.GetDashDistance(), wallLayerMask);

                // Dash if there is no wall that the AI would crash into
                if (hitTarget.collider == null)
                {
                    // Don't dash if you would overshoot on lights
                    if (!canSeeTarget || (canSeeTarget && !targetIsPlayer && distanceToTargetSquared > Mathf.Pow(controller.GetDashDistance(), 2)))
                    {
                        controller.AIUseSecondary();
                    }
                }
            }

            if (finalDirection.sqrMagnitude > 0.01f)
            {
                controller.AIMove(finalDirection);
            }
        }
    }

    /*
    protected override Vector2 GetDirectionToTargetForMovement()
    {
        Vector2 direction = Vector2.zero;

        canSeeTarget = false;

        selfNode = map.GetNode(playerController.GetPosition());

        distanceToTargetSquared = 9999;
        selfPosition = playerController.GetPosition();

        // Have to get these each time because some players may die and then they leave the array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");

        // Pick a monster to target
        for (int i = 0; i < players.Length; ++i)
        {
            HumanController hc = players[i].GetComponent<HumanController>();
            if (hc != null)
            {
                if (IsNewTargetCloser(hc.gameObject, ref direction, ignoreLightLanternProjectileLayerMask))
                {
                    targetIsPlayer = true;
                }
            }
        }

        for (int i = 0; i < lights.Length; ++i)
        {
            LightController lc = lights[i].GetComponent<LightController>();
            // Don't go for any lights that another human is turning on already (We'll just attack the human instead), don't go for lanterns
            if (lc != null && lc.isOn && !lc.IsLantern() && !lc.humansIn)
            {
                if (IsNewTargetCloser(lc.gameObject, ref direction, ignoreLanternProjectileLayerMask, lightTargetDistanceOffset))
                {
                    targetIsPlayer = false;
                }
            }
        }

        direction.Normalize();
        direction += GetDirectionAwayFromObstacles(direction) * weightDirectionAwayFromObstacles;
        return direction;
    }
    */
}
