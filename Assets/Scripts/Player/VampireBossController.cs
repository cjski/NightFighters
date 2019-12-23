using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VampireBossController : BossController
{
    private static GameObject slowProjectilePrefab;

    public float biteCosAngle { get; private set; } = Mathf.Cos(3.14159265f * 75 / 180);
    public float biteRange { get; private set; } = 1.0f;
    public float biteStunDuration { get; private set; } = 1.3f;
    public int biteHealAmount { get; private set; } = 10;
    public int biteDamage { get; private set; } = 15;
    public float slowProjectileSpeed { get; private set; } = 6.0f;
    public float slowProjectileExitSpeed { get; private set; } = 15.0f;
    public float slowProjectileReturnSpeed { get; private set; } = 10.0f;
    public float slowProjectileDuration { get; private set; } = 10f;
    public float slowStallTime { get; private set; } = 0.75f;
    public float slowExitTime { get; private set; } = 0.075f;
    public Quaternion[] slowProjectileRotations { get; private set; } = 
    {
        Quaternion.Euler(0, 0, 135),
        Quaternion.Euler(0, 0, 90),
        Quaternion.Euler(0, 0, 45),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, -45),
        Quaternion.Euler(0, 0, -90),
        Quaternion.Euler(0, 0, -135),
        Quaternion.Euler(0, 0, -180),
    };
    public float slowAmount { get; private set; } = 1.1f;
    public float slowDuration { get; private set; } = 2;
    public float dashDuration { get; private set; } = 0.3f;
    public float dashSpeedModifier { get; private set; } = 1.7f;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 3.5f;
        maxHealth = 200;
        primaryCooldown = new Timer(3, true);
        secondaryCooldown = new Timer(6, true);

        slowProjectilePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/BatProjectilePrefab.prefab", typeof(GameObject));
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

        for (int i = 0; i < players.Length; ++i)
        {
            if (!(players[i].GetComponent<MonsterController>()))
            {
                Vector2 toPlayer = (Vector2)players[i].transform.position - GetPosition();
                if (InRange(toPlayer, biteRange, biteCosAngle))
                {
                    players[i].GetComponent<PlayerController>().ApplyStun(biteStunDuration);
                    players[i].GetComponent<PlayerController>().Damage(biteDamage);
                    Heal(biteHealAmount);
                }
            }
        }
        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        for(int i = 0; i < slowProjectileRotations.Length; ++i)
        {
            GameObject attack = Instantiate(slowProjectilePrefab, GetPosition(), transform.rotation);
            attack.GetComponent<BatProjectileController>().Init(slowProjectileRotations[i] * -direction, slowProjectileExitSpeed, slowProjectileReturnSpeed, slowStallTime, slowExitTime, slowProjectileDuration, gameObject, slowAmount, slowDuration);
        }
        ApplyDash(direction, dashDuration, speed * dashSpeedModifier);
        secondaryCooldown.Reset();
    }

    public override float GetDashDistance()
    {
        return speed * dashSpeedModifier * dashDuration;
    }
}
