using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HunterController : HumanController
{
    private static GameObject arrowPrefab;

    public float dashSpeedModifier { get; private set; } = 2;
    public float dashDuration { get; private set; } = 0.5f;
    public float arrowSpeed { get; private set; } = 10.0f;
    public float arrowDuration { get; private set; } = 2;
    public int arrowDamage { get; private set; } = 10;

    // Start is called before the first frame update
    protected new void Start()
    {
        baseSpeed = 3.5f;
        maxHealth = 80;
        primaryCooldown = new Timer(1, true);
        secondaryCooldown = new Timer(4, true);

        arrowPrefab = (GameObject)Resources.Load<GameObject>("Prefabs/ArrowPrefab");
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
    }

    protected override void OnPrimaryPressed()
    {
        GameObject attack = Instantiate(arrowPrefab, GetPosition(), transform.rotation);
        attack.GetComponent<DamageProjectileController>().Init(direction, arrowSpeed, arrowDuration, gameObject, arrowDamage);
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
