using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfController : MonsterController
{
    public float knockbackAngleDegrees { get; private set; } = 60.0f;
    public float knockbackCosAngle { get; private set; } = Mathf.Cos(3.14159265f * 60 / 180);
    public float knockbackRange { get; private set; } = 1.1f;
    public float knockbackDuration { get; private set; } = 0.25f;
    public float knockbackSpeed { get; private set; } = 7.5f;
    public float dashDuration { get; private set; } = 0.5f;
    public float dashSpeedModifier { get; private set; } = 1.5f;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 3.75f;
        maxHealth = 100;
        primaryCooldown = new Timer(4.5f, true);
        secondaryCooldown = new Timer(4, true);
        base.Start();
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();
    }

    protected override void OnPrimaryPressed()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        List<GameObject> lanterns = new List<GameObject>();

        for (int i = 0; i < players.Length; ++i)
        {
            if (!(players[i].GetComponent<MonsterController>()))
            {
                if(players[i].GetComponent<WatchmanController>())
                {
                    lanterns.Add(players[i].GetComponent<WatchmanController>().lantern);
                }
                Vector2 toPlayer = (Vector2)players[i].transform.position - GetPosition();
                if (InRange(toPlayer, knockbackRange, knockbackCosAngle))
                    players[i].GetComponent<PlayerController>().ApplyDash(toPlayer.normalized, knockbackDuration, knockbackSpeed);
            }
        }

        for (int i = 0; i < projectiles.Length; ++i)
        {
            Vector2 toProjectile = (Vector2)projectiles[i].transform.position - GetPosition();
            if (toProjectile.sqrMagnitude < knockbackRange)
            {
                toProjectile.Normalize();
                if (Vector2.Dot(toProjectile, direction) > knockbackCosAngle)
                {
                    projectiles[i].GetComponent<ProjectileController>().direction = toProjectile;
                }
            }
        }

        for (int i = 0; i < lanterns.Count; ++i)
        {
            Vector2 toLantern = (Vector2)lanterns[i].transform.position - GetPosition();
            if (toLantern.sqrMagnitude < knockbackRange)
            {
                toLantern.Normalize();
                if (Vector2.Dot(toLantern, direction) > knockbackCosAngle)
                {
                    lanterns[i].GetComponent<LanternController>().Throw(toLantern, knockbackSpeed);
                }
            }
        }

        ActivateHitSprite(knockbackRange, knockbackAngleDegrees);

        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        secondaryCooldown.Reset();
        //Apply the current speed instead of baseSpeed so the player will dash slower if they're slowed down
        ApplyDash(direction, dashDuration, speed * dashSpeedModifier);
    }

    public override float GetDashDistance()
    {
        return speed * dashSpeedModifier * dashDuration;
    }
}
