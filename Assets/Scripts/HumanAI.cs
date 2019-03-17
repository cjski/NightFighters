using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAI : MonoBehaviour
{
    static Map map;
    HumanController self;

    void Start()
    {

    }

    public void Init(Map gameMap, GameObject human)
    {
        if (map == null) map = gameMap;
        self = human.GetComponent<HumanController>();
        self.ActivateAI();
    }

    void Update()
    {
        self.AIMove(new Vector2(1,0));
    }

    void FindDestinationNode()
    {

    }
}
