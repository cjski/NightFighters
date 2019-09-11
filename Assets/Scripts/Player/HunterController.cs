using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HunterController : HumanController
{
    private static GameObject arrowPrefab;

    // Start is called before the first frame update
    protected new void Start()
    {
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
        attack.GetComponent<DamageProjectileController>().Init(direction, 0.2f, 2, gameObject, 25);
        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        secondaryCooldown.Reset();
        //Apply the current speed instead of baseSpeed so the player will dash slower if they're slowed down
        ApplyDash(direction, 0.5f, speed * 2);
    }
}
