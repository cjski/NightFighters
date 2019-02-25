using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HumanController : PlayerController {

    public static GameObject projectilePrefab;

    // Use this for initialization
    new void Start () {
        base.Start();

        dashTime = new Timer(.5f);

        projectilePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/ProjectilePrefab.prefab", typeof(GameObject));
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
        GameObject attack = Instantiate(projectilePrefab, transform.position, transform.rotation);
        attack.GetComponent<ProjectileController>().Init(direction, 0.3f, 2, gameObject);
        secondaryCooldown.Reset();
    }
}
