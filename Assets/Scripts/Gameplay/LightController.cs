﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

    protected Behaviour halo;
    public bool isOn;
    private float turnOnFixedTime = 2;
    private float turnOnBrokenTime = 4;
    Timer turnOnTimer = new Timer(2);
    Timer turnOffTimer = new Timer(2);
    public bool humansIn = false, monstersIn = false, broken = false;
    private float activateRadSqr = 0.3125f;
    public GameObject currentHumanInLight = null;
    public GameObject currentMonsterInLight = null;

    private Animator anim;

    // Use this for initialization
    protected void Start () {
        halo = (Behaviour)GetComponent("Halo");

        if ( halo )
        {
            if ( isOn )
            {
                halo.enabled = true;
            }
            else
            {
                halo.enabled = false;
            }
        }

        GetComponent<SpriteRenderer>().sortingOrder = -1;

        anim = GetComponent<Animator>();
        if ( anim )
        {
            if ( isOn )
            {
                anim.Play( "On" );
            }
            else
            {
                anim.Play( "Off" );
            }
        }
    }
	
	// Update is called once per frame
	protected void Update () {
        FindPlayersIn();
        if ( isOn )
        {
            if (monstersIn)
            {
                turnOffTimer.Update();
            }
        }
        else 
        {
            if (humansIn)
            {
                turnOnTimer.Update();
            }
        }

        if ( turnOnTimer.done )
        {
            halo.enabled = true;
            if (anim) anim.Play("On");
            turnOnTimer.Reset();
            isOn = true;
        }
        else if (turnOffTimer.done)
        {
            if (broken) turnOnTimer.Set(turnOnBrokenTime);
            else turnOnTimer.Set(turnOnFixedTime);
            halo.enabled = false;
            if (anim) anim.Play("Off");
            turnOffTimer.Reset();
            isOn = false;
        }
    }

    private void FindPlayersIn()
    {
        humansIn = false;
        monstersIn = false;
        broken = false;
        currentMonsterInLight = null;
        currentHumanInLight = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float xDist, yDist;

        for (int i = 0; i < players.Length; ++i)
        {
            xDist = players[i].transform.position.x - gameObject.transform.position.x;
            yDist = players[i].transform.position.y - gameObject.transform.position.y;
            if ((xDist * xDist) + (yDist * yDist) < activateRadSqr)
            {
                if (players[i].GetComponent<HumanController>() != null)
                {
                    currentHumanInLight = players[i];
                    humansIn = true;
                }
                else
                {
                    currentMonsterInLight = players[i];
                    monstersIn = true;
                    if (players[i].GetComponent<WerewolfController>() != null)
                    {
                        broken = true;
                    }
                }
            }
        }
    }

    public bool TurningOn()
    {
        return turnOnTimer.time > 0;
    }

    public bool TurningOff()
    {
        return turnOffTimer.time > 0;
    }

    public void TurnOff()
    {
        turnOffTimer.Reset();
        turnOnTimer.Reset();
        isOn = false;
        if ( halo )
        {
            halo.enabled = false;
            
        }
        if ( anim )
        {
            anim.Play("Off");
        }
    }

    public void TurnOn()
    {
        turnOffTimer.Reset();
        turnOnTimer.Reset();
        isOn = true;
        if ( halo )
        {
            halo.enabled = true;
        }
        if ( anim )
        {
            anim.Play("On");
        }
    }

    // This will be overridden in the LanternController class to return true
    public virtual bool IsLantern()
    {
        return false;
    }
}
