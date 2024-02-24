using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfBossController : BossController
{
    public float knockbackAngleDegrees { get; private set; } = 180.0f;
    public float knockbackCosAngle { get; private set; } = Mathf.Cos(3.14159265f * 180.0f / 180);
    public float knockbackRange { get; private set; } = 1.3f;
    public float knockbackDuration { get; private set; } = 1.0f;
    public float knockbackSpeed { get; private set; } = 7.5f;
    public int knockbackDamage { get; private set; } = 25;
    public float knockbackStunDuration { get; private set; } = 1.5f;
    public float dashDuration { get; private set; } = 0.75f;
    public float dashSpeedModifier { get; private set; } = 1.6f;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 4.0f;
        maxHealth = 250;
        primaryCooldown = new Timer(4.5f, true);
        secondaryCooldown = new Timer(3.75f, true);

        base.Start();
    }

    // Update is called once per frame
    new protected void Update()
    {
        //base.Update();
    }

    protected override void OnPrimaryPressed()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; ++i)
        {
            if (!(players[i].GetComponent<MonsterController>()))
            {
                Vector2 toPlayer = (Vector2)players[i].transform.position - GetPosition();
                if (InRange(toPlayer, knockbackRange, knockbackCosAngle))
                {
                    PlayerController pc = players[i].GetComponent<PlayerController>();
                    pc.ApplyDashWithStun(toPlayer.normalized, knockbackDuration, knockbackSpeed, knockbackStunDuration);
                    pc.Damage(knockbackDamage);
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
