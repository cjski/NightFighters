using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameController : MonoBehaviour {

    GameObject humanPrefab;
    GameObject monsterPrefab;
    Map stageMap;
    int rows = 4, cols = 6;// 4, 6, 3
    float unitSize = 3;
    GameObject ai1;

    // Use this for initialization
    void Start () {
        humanPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HumanPrefab.prefab", typeof(GameObject));
        monsterPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/MonsterPrefab.prefab", typeof(GameObject));

        stageMap = new Map(cols, rows, unitSize);

        ai1 = GameObject.Find("AIController");

        Restart();
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ReGen();
        }
    }

    private void Restart()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i=0;i<players.Length;++i)
        {
            Destroy(players[i]);
        }

        GameObject p1 = Instantiate(humanPrefab, new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        p1.GetComponent<HumanController>().MapControls(KeyCode.Mouse0, KeyCode.Mouse1);
        GameObject p2 = Instantiate(monsterPrefab, new Vector3(cols*unitSize - 0.5f, 0.5f, 0), Quaternion.identity);
        p2.GetComponent<MonsterController>().MapControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.K, KeyCode.L);
        //GameObject p3 = Instantiate(monsterPrefab, new Vector3(0.5f, rows*unitSize -0.5f, 0), Quaternion.identity);
        //p3.GetComponent<MonsterController>().MapControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Z, KeyCode.X);
        GameObject p4 = Instantiate(humanPrefab, new Vector3(2, 2, 0), Quaternion.identity);

        ai1.GetComponent<HumanAI>().Init(stageMap, p4);     
    }

    private void ReGen()
    {
        stageMap.Generate();

        Restart();
    }
}
