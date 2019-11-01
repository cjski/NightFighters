using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VampireBossController : BossController
{
    private static GameObject slowProjectilePrefab;

    public float biteCosAngle { get; private set; } = Mathf.Cos(3.14159265f * 60 / 180);
    public float biteRange { get; private set; } = 2;
    public float biteStunDuration { get; private set; } = 1;
    public int biteHealAmount { get; private set; } = 5;
    public int biteDamage { get; private set; } = 10;
    public float slowProjectileSpeed { get; private set; } = 0.12f;
    public float slowProjectileDuration { get; private set; } = 2.5f;
    public Quaternion[] slowProjectileRotations { get; private set; } = 
    {
        Quaternion.Euler(0, 0, 15),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, -15)
    };
    public float slowAmount { get; private set; } = 0.05f;
    public float slowDuration { get; private set; } = 10;
    public float dashDuration { get; private set; } = 0.3f;
    public float dashSpeedModifier { get; private set; } = 1.7f;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 0.09f;
        maxHealth = 300;
        primaryCooldown = new Timer(3);
        secondaryCooldown = new Timer(6);

        slowProjectilePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/SlowProjectilePrefab.prefab", typeof(GameObject));
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
                Vector2 toPlayer = players[i].transform.position - gameObject.transform.position;
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
            GameObject attack = Instantiate(slowProjectilePrefab, transform.position, transform.rotation);
            //Quaternions have to be multiplied first
            attack.GetComponent<SlowProjectileController>().Init(slowProjectileRotations[i] * direction, slowProjectileSpeed, slowProjectileDuration, gameObject, slowAmount, slowDuration);
        }
        ApplyDash(-direction, dashDuration, speed * dashSpeedModifier);
        secondaryCooldown.Reset();
    }
}
