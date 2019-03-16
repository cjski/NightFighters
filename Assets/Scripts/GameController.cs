using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameController : MonoBehaviour {

    GameObject humanPrefab;
    GameObject monsterPrefab;
    Map stageMap;
    int rows = 5, cols = 9;
    float unitSize = 2;

    // Use this for initialization
    void Start () {
        humanPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HumanPrefab.prefab", typeof(GameObject));
        monsterPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/MonsterPrefab.prefab", typeof(GameObject));

        stageMap = new Map(cols, rows, unitSize);

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
        GameObject p3 = Instantiate(monsterPrefab, new Vector3(0.5f, rows*unitSize -0.5f, 0), Quaternion.identity);
        p3.GetComponent<MonsterController>().MapControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Z, KeyCode.X);
    }

    private void ReGen()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        for (int i = 0; i < walls.Length; ++i)
        {
            Destroy(walls[i]);
        }
        stageMap.Generate();

        Restart();
    }
}
