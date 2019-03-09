using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HumanController : PlayerController {

    private static GameObject damageProjectilePrefab;

    // Use this for initialization
    new void Start () {
        base.Start();

        dashTime = new Timer(.5f);

        damageProjectilePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/DamageProjectilePrefab.prefab", typeof(GameObject));
    }

    // Update is called once per frame
    new void Update () {
        base.Update();
	}

    protected override void OnPrimaryPressed()
    {
        movementType = MovementType.Dashing;
        primaryCooldown.Reset();
        dashTime.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        GameObject attack = Instantiate(damageProjectilePrefab, transform.position, transform.rotation);
        attack.GetComponent<DamageProjectileController>().Init(direction, 0.3f, 2, gameObject, 10);
        secondaryCooldown.Reset();
    }
}
