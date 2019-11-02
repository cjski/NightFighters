using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfBossController : BossController
{
    public float knockbackCosAngle { get; private set; } = Mathf.Cos(3.14159265f * 90 / 180);
    public float knockbackRange { get; private set; } = 2;
    public float knockbackDuration { get; private set; } = 1.0f;
    public float knockbackSpeed { get; private set; } = 0.15f;
    public int knockbackDamage { get; private set; } = 40;
    public float knockbackStunDuration { get; private set; } = 1.5f;
    public float dashDuration { get; private set; } = 0.75f;
    public float dashSpeedModifier { get; private set; } = 1.6f;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 0.08f;
        maxHealth = 200;
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
        for (int i = 0; i < players.Length; ++i)
        {
            if (!(players[i].GetComponent<MonsterController>()))
            {
                Vector2 toPlayer = players[i].transform.position - gameObject.transform.position;
                if (InRange(toPlayer, knockbackRange, knockbackCosAngle))
                {
                    PlayerController pc = players[i].GetComponent<PlayerController>();
                    pc.ApplyDashWithStun(toPlayer.normalized, knockbackDuration, knockbackSpeed, knockbackStunDuration);
                    pc.Damage(knockbackDamage);
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
