using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MonsterController : PlayerController {

    private static GameObject slowProjectilePrefab;
    private bool prevInLight;

	// Use this for initialization
	new void Start () {
        base.Start();

        prevInLight = false;
        slowProjectilePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/SlowProjectilePrefab.prefab", typeof(GameObject));
    }

    // Update is called once per frame
    new void Update () {
        base.Update();
        if (InLight())
        {
            if (!prevInLight)
            {
                prevInLight = true;
                ModifySpeed(-0.05f);
            }
        }
        else if (prevInLight)
        {
            ModifySpeed(0.05f);
            prevInLight = false;
        }
    }

    private bool InLight()
    {
        GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");
        float xDist, yDist;
        for (int i = 0; i < lights.Length; ++i)
        {
            if (lights[i].GetComponent<LightController>().On())
            {
                //Use squared distance for faster calculation
                xDist = transform.position.x - lights[i].transform.position.x;
                yDist = transform.position.y - lights[i].transform.position.y;
                if ((xDist * xDist) + (yDist * yDist) < 4) return true;
            }
        }
        return false;
    }

    protected override void OnPrimaryPressed()
    {
        
    }

    protected override void OnSecondaryPressed()
    {
        GameObject attack = Instantiate(slowProjectilePrefab, transform.position, transform.rotation);
        attack.GetComponent<SlowProjectileController>().Init(direction, 0.2f, 2.5f, gameObject, 0.05f, 10);
        secondaryCooldown.Reset();
    }
}
