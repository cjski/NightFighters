using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfBossController : BossController
{
    // Start is called before the first frame update
    new protected void Start()
    {
        baseSpeed = 0.08f;
        maxHealth = 200;
        base.Start();
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();
    }

    protected override void OnPrimaryPressed()
    {

    }

    protected override void OnSecondaryPressed()
    {

    }
}
