using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

    private Behaviour halo;
    Timer turnOnTimer, turnOffTimer;
    private int humansIn, monstersIn;

	// Use this for initialization
	void Start () {
        halo = (Behaviour)GetComponent("Halo");
        halo.enabled = false;

        turnOnTimer = new Timer(2);
        turnOffTimer = new Timer(3);

        humansIn = 0;
        monstersIn = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (halo.enabled && monstersIn > 0) turnOffTimer.Update();
        else if (!halo.enabled && humansIn > 0) turnOnTimer.Update();
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter");
        if (collision.gameObject.tag == "Player")
        {
            if (collision.gameObject.GetComponent<HumanController>()) ++humansIn;
            else ++monstersIn;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Exit");
        if (collision.gameObject.tag == "Player")
        {
            if (collision.gameObject.GetComponent<HumanController>()) --humansIn;
            else --monstersIn;
        }
    }

    public bool On()
    {
        if(halo != null) return halo.enabled;
        return false;
    }
}
