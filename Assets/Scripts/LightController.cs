﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

    protected Behaviour halo;
    private float turnOnFixedTime = 2;
    private float turnOnBrokenTime = 4;
    Timer turnOnTimer = new Timer(2);
    Timer turnOffTimer = new Timer(2);
    Timer brokenTimer = new Timer(2);
    bool humansIn = false, monstersIn = false, broken = false;
    private float activateRadSqr = 0.25f;

	// Use this for initialization
	protected void Start () {
        halo = (Behaviour)GetComponent("Halo");
        halo.enabled = false;
	}
	
	// Update is called once per frame
	protected void Update () {
        FindPlayersIn();
        if (halo.enabled && monstersIn) turnOffTimer.Update();
        else if (!halo.enabled && humansIn)
        {
            turnOnTimer.Update();
        }
        if (turnOnTimer.done)
        {
            halo.enabled = true;
            turnOnTimer.Reset();
        }
        else if (turnOffTimer.done)
        {
            if (broken) turnOnTimer.Set(turnOnBrokenTime);
            else turnOnTimer.Set(turnOnFixedTime);
            halo.enabled = false;
            turnOffTimer.Reset();
        }
    }

    private void FindPlayersIn()
    {
        humansIn = false;
        monstersIn = false;
        broken = false;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float xDist, yDist;
        for (int i = 0; i < players.Length; ++i)
        {
            xDist = players[i].transform.position.x - gameObject.transform.position.x;
            yDist = players[i].transform.position.y - gameObject.transform.position.y;
            if ((xDist * xDist) + (yDist * yDist) < activateRadSqr)
            {
                if (players[i].GetComponent<HumanController>() != null) humansIn = true;
                else
                {
                    monstersIn = true;
                    if (players[i].GetComponent<WerewolfController>() != null) broken = true;
                }
            }
        }
    }

    public bool On()
    {
        if(halo != null) return halo.enabled;
        return false;
    }
}
