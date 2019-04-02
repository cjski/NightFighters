using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfController : MonsterController
{
    private float knockbackCosAngle = Mathf.Cos(3.14159265f * 60 / 180);
    private float knockbackRange = 2;

    // Start is called before the first frame update
    new protected void Start()
    {
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
                if(InRange(toPlayer, knockbackRange, knockbackCosAngle)) players[i].GetComponent<PlayerController>().ApplyDash(toPlayer.normalized, 1f, 0.15f);
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
                    lanterns[i].GetComponent<LanternController>().direction = toLantern;
                }
            }
        }

        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        secondaryCooldown.Reset();
        //Apply the current speed instead of baseSpeed so the player will dash slower if they're slowed down
        ApplyDash(direction, 0.5f, speed * 2);
    }
}
