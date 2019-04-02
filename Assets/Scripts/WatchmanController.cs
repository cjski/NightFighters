using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WatchmanController : HumanController
{
    static GameObject lightPrefab;
    GameObject lantern;
    bool holdingLantern;
    Timer catchTimer = new Timer(.5f);
    float hitRange = 1;
    float hitCosAngle = Mathf.Cos(3.14159265f * 60 / 180);
    float stunTime = 0.5f;
    int damage = 10;

    // Start is called before the first frame update
    new protected void Start()
    {
        lightPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/LanternPrefab.prefab", typeof(GameObject));
        lantern = Instantiate(lightPrefab, transform.position, Quaternion.identity);
        holdingLantern = true;

        base.Start();
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();
        if(holdingLantern) lantern.transform.position = transform.position;
        else
        {
            catchTimer.Update();
            if (catchTimer.done && (lantern.transform.position - transform.position).sqrMagnitude < 0.3)
            {
                holdingLantern = true;
                catchTimer.Reset();
            }
        }
    }

    protected override void OnPrimaryPressed()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; ++i)
        {
            if (!(players[i].GetComponent<HumanController>()))
            {
                Vector2 toPlayer = players[i].transform.position - gameObject.transform.position;
                if (InRange(toPlayer, hitRange, hitCosAngle))
                {
                    players[i].GetComponent<PlayerController>().ApplyStun(stunTime);
                    players[i].GetComponent<PlayerController>().Damage(damage);
                }
            }
        }
        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        if (holdingLantern)
        {
            holdingLantern = false;
            lantern.GetComponent<LanternController>().Throw(direction, 0.325f);
            secondaryCooldown.Reset();
        }
    }
}
