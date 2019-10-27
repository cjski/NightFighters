using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfBossController : BossController
{
    private float knockbackCosAngle = Mathf.Cos(3.14159265f * 90 / 180);
    private float knockbackRange = 2;
    private float knockbackDuration = 1.0f;
    private float knockbackSpeed = 0.15f;
    private int knockbackDamage = 40;
    private float knockbackStunDuration = 1.5f;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 0.08f;
        maxHealth = 200;
        primaryCooldown = new Timer(4.5f);
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

    }
}
