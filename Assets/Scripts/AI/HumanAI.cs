using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAI : AI
{
    protected float range = 3;

    protected override void Start()
    {
        base.Start();
    }

    public override void Init(Map gameMap, GameObject player, int newPlayerNumber)
    {
        if (map == null)
        {
            map = gameMap;
        }
        playerController = player.GetComponent<HumanController>();
        playerController.ActivateAI(newPlayerNumber);
    }

    protected override void Update()
    {
        
    }

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
            MonsterController mc = players[i].GetComponent<MonsterController>();
            if (mc != null)
            {
                if(IsNewTargetCloser(mc.gameObject, ref direction, ignoreLightLanternProjectileLayerMask))
                {
                    targetIsPlayer = true;
                }
            }
        }

        for (int i = 0; i < lights.Length; ++i)
        {
            LightController lc = lights[i].GetComponent<LightController>();
            // Don't go for any lights that another human is turning on already, don't go for lanterns
            if (lc != null && !lc.isOn && !lc.IsLantern() && (!lc.humansIn || lc.currentHumanInLight == playerController.gameObject))
            {
                if(IsNewTargetCloser(lc.gameObject, ref direction, ignoreLanternProjectileLayerMask, lightTargetDistanceOffset))
                {
                    targetIsPlayer = false;
                }
            }
        }
        
        direction.Normalize();
        direction += GetDirectionAwayFromObstacles(direction) * weightDirectionAwayFromObstacles;
        return direction;
    }
}
