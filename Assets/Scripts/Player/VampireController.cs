using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VampireController : MonsterController
{
    GameObject slowProjectilePrefab;
    private float biteCosAngle = Mathf.Cos(3.14159265f * 60 / 180);
    private float biteRange = 2;

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 0.09f;
        maxHealth = 75;

        slowProjectilePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/SlowProjectilePrefab.prefab", typeof(GameObject));
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
                if (InRange(toPlayer, biteRange, biteCosAngle))
                {
                    players[i].GetComponent<PlayerController>().ApplyStun(1);
                    Heal(5);
                }
            }
        }
        primaryCooldown.Reset();
    }

    protected override void OnSecondaryPressed()
    {
        GameObject attack = Instantiate(slowProjectilePrefab, transform.position, transform.rotation);
        attack.GetComponent<SlowProjectileController>().Init(direction, 0.12f, 2.5f, gameObject, 0.05f, 10);
        secondaryCooldown.Reset();
    }
}
