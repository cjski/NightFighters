using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfController : MonsterController
{
    private float knockbackCosAngle = Mathf.Cos(3.14159265f * 60 / 180);
    private float knockbackRange = 2.5f;
    private float knockbackDuration = 1.0f;
    private float knockbackSpeed = 0.15f;
    private float dashDuration = 0.5f;
    private float dashSpeedModifier = 1.5f;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 0.1f;
        maxHealth = 100;
        primaryCooldown = new Timer(3);
        secondaryCooldown = new Timer(4);
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
                Vector2 toPlayer = players[i].transform.position - gameObject.transform.position;
                if(InRange(toPlayer, knockbackRange, knockbackCosAngle))
                    players[i].GetComponent<PlayerController>().ApplyDash(toPlayer.normalized, knockbackDuration, knockbackSpeed);
            }
        }

        for (int i = 0; i < projectiles.Length; ++i)
        {
            Vector2 toProjectile = projectiles[i].transform.position - gameObject.transform.position;
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
            Vector2 toLantern = lanterns[i].transform.position - gameObject.transform.position;
            if (toLantern.sqrMagnitude < knockbackRange)
            {
                toLantern.Normalize();
                if (Vector2.Dot(toLantern, direction) > knockbackCosAngle)
                {
                    lanterns[i].GetComponent<LanternController>().Throw(toLantern, knockbackSpeed);
                }
            }
        }

        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        secondaryCooldown.Reset();
        //Apply the current speed instead of baseSpeed so the player will dash slower if they're slowed down
        ApplyDash(direction, dashDuration, speed * dashSpeedModifier);
    }
}
