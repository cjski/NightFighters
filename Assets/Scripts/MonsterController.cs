using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : PlayerController {

	// Use this for initialization
	new void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
        if (InLight()) slowFactor = 2;
        else slowFactor = 1;
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
    }
}
