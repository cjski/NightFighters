using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

    private Behaviour halo;
    Timer turnOnTimer, turnOffTimer;
    bool humansIn = false, monstersIn = false;
    private float activateRadSqr = 0.25f;

	// Use this for initialization
	void Start () {
        halo = (Behaviour)GetComponent("Halo");
        halo.enabled = false;

        turnOnTimer = new Timer(2);
        turnOffTimer = new Timer(3);
	}
	
	// Update is called once per frame
	void Update () {
        FindPlayersIn();
        if (halo.enabled && monstersIn) turnOffTimer.Update();
        else if (!halo.enabled && humansIn) turnOnTimer.Update();
        if (turnOnTimer.done)
        {
            halo.enabled = true;
            turnOnTimer.Reset();
        }
        else if (turnOffTimer.done)
        {
            halo.enabled = false;
            turnOffTimer.Reset();
        }
    }

    private void FindPlayersIn()
    {
        humansIn = false;
        monstersIn = false;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float xDist, yDist;
        for (int i = 0; i < players.Length; ++i)
        {
            xDist = players[i].transform.position.x - gameObject.transform.position.x;
            yDist = players[i].transform.position.y - gameObject.transform.position.y;
            if ((xDist * xDist) + (yDist * yDist) < activateRadSqr)
            {
                if (players[i].GetComponent<HumanController>() != null) humansIn = true;
                else monstersIn = true;
            }
        }
    }

    public bool On()
    {
        if(halo != null) return halo.enabled;
        return false;
    }
}
