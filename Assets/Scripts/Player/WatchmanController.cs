using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WatchmanController : HumanController
{
    static GameObject lightPrefab;
    public float hitRange { get; private set; } = 1;
    public float hitCosAngle { get; private set; } = Mathf.Cos(3.14159265f * 60 / 180);
    public float stunTime { get; private set; } = 0.25f;
    public int damage { get; private set; } = 20;
    public float lanternInitialSpeed { get; private set; } = 16.25f;
    public float lanternDistanceToPickUpSqr { get; private set; } = 0.3f;
    public Timer catchTimer { get; private set; } = new Timer(.5f);
    public Timer returnTimer { get; private set; } = new Timer(2.5f, true);

    public GameObject lantern;
    private GameObject lanternPointer;
    public bool holdingLantern { get; private set; }

    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 3.25f;
        maxHealth = 100;
        primaryCooldown = new Timer(2.5f, true);
        secondaryCooldown = new Timer(5, true);

        lightPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/LanternPrefab.prefab", typeof(GameObject));
        lantern = Instantiate(lightPrefab, transform.position, Quaternion.identity);
        lanternPointer = transform.Find("LanternPointer").gameObject;
        lanternPointer.SetActive(false);
        holdingLantern = true;

        base.Start();
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();
        if (holdingLantern)
        {
            lantern.transform.position = transform.position;
        }
        else if(!returnTimer.done)
        {
            returnTimer.Update();
            if (returnTimer.done)
            {
                holdingLantern = true;
                lantern.transform.position = transform.position;
                catchTimer.Reset();
                lantern.SetActive(true);
            }
        }
        else
        {
            Vector2 toLantern = (lantern.transform.position - transform.position);
            float angle = Mathf.Atan2(toLantern.y, toLantern.x);
            lanternPointer.transform.SetPositionAndRotation(
                new Vector3(gameObject.transform.position.x + 0.5f * Mathf.Cos(angle), gameObject.transform.position.y + 0.5f * Mathf.Sin(angle), -1),
                Quaternion.Euler(0, 0, angle * 180 / Mathf.PI + 90));
            catchTimer.Update();
            if (catchTimer.done && toLantern.sqrMagnitude < lanternDistanceToPickUpSqr)
            {
                holdingLantern = true;
                catchTimer.Reset();
                lanternPointer.SetActive(false);
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
            lantern.GetComponent<LanternController>().Throw(direction, lanternInitialSpeed);
            lanternPointer.SetActive(true);
            secondaryCooldown.Reset();
        }
        else
        {
            lantern.SetActive(false);
            lanternPointer.SetActive(false);
            secondaryCooldown.Reset();
            returnTimer.Reset();
        }
    }

    private void OnDestroy()
    {
        Destroy(lantern);
    }
}
