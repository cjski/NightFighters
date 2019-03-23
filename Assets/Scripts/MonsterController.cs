using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : PlayerController {

    private static float lightRadSqr = 1;

    private bool prevInLight = false;

	// Use this for initialization
	new protected void Start () {
        base.Start();
    }

    // Update is called once per frame
    new protected void Update () {
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
                if ((xDist * xDist) + (yDist * yDist) < lightRadSqr) return true;
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
