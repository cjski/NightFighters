using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HunterController : HumanController
{
    private static GameObject arrowPrefab;

    private float dashSpeedModifier = 2;
    private float dashDuration = 0.5f;
    private float arrowSpeed = 0.2f;
    private float arrowDuration = 2;
    private int arrowDamage = 20;

    // Start is called before the first frame update
    protected new void Start()
    {
        baseSpeed = 0.1f;
        maxHealth = 80;
        primaryCooldown = new Timer(1.5f);
        secondaryCooldown = new Timer(4.5f);

        arrowPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/ArrowPrefab.prefab", typeof(GameObject));
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
    }

    protected override void OnPrimaryPressed()
    {
        GameObject attack = Instantiate(arrowPrefab, transform.position, transform.rotation);
        attack.GetComponent<DamageProjectileController>().Init(direction, arrowSpeed, arrowDuration, gameObject, arrowDamage);
        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        secondaryCooldown.Reset();
        //Apply the current speed instead of baseSpeed so the player will dash slower if they're slowed down
        ApplyDash(direction, dashDuration, speed * dashSpeedModifier);
    }
}
