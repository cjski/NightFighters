﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchmanAI : HumanAI
{
    protected static int ignoreLightLayerMask;
    protected static float lanternDistanceOffset = -2.0f;

    protected override void Start()
    {
        ignoreLightLayerMask = ~LayerMask.GetMask("IgnoreRaycast", "Light");
        base.Start();
    }

    protected override void Update()
    {
        if (playerController == null) return;

        WatchmanController wc = playerController as WatchmanController;

        if (wc == null) return;

        finalDirection = GetDirectionToTargetForMovement();

        if (canSeeTarget && targetIsPlayer)
        {
            if (distanceToTargetSquared <= wc.hitRange && wc.primaryCooldown.done)
            {
                wc.AIUsePrimary();
            }
            else if( distanceToTargetSquared < wc.lanternInitialSpeed * wc.lanternInitialSpeed * 120 && wc.holdingLantern && wc.secondaryCooldown.done)
            {
                wc.AIUseSecondary();
            }
        }

        if (finalDirection.sqrMagnitude > 0.01f) wc.AIMove(finalDirection);
    }

    protected new Vector2 GetDirectionToTargetForMovement()
    {
        Vector2 direction = base.GetDirectionToTargetForMovement();

        WatchmanController wc = playerController as WatchmanController;

        if (wc != null && wc.lantern != null && !wc.holdingLantern)
        {
            if (IsNewTargetCloser(wc.lantern.gameObject, ref direction, ignoreLightLayerMask, lanternDistanceOffset))
            {
                targetIsPlayer = false;
            }
        }

        return direction;
    }
}