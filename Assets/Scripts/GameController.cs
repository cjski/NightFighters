using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameController : MonoBehaviour {

    GameObject humanPrefab;
    GameObject monsterPrefab;

    // Use this for initialization
    void Start () {
        humanPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HumanPrefab.prefab", typeof(GameObject));
        monsterPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/MonsterPrefab.prefab", typeof(GameObject));

        Restart();
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
	}

    private void Restart()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i=0;i<players.Length;++i)
        {
            Destroy(players[i]);
        }

        GameObject p1 = Instantiate(humanPrefab, new Vector3(-5, 3, 0), Quaternion.identity);
        p1.GetComponent<HumanController>().MapControls(KeyCode.Mouse0, KeyCode.Mouse1);
        GameObject p2 = Instantiate(monsterPrefab, new Vector3(3, -3, 0), Quaternion.identity);
        p2.GetComponent<MonsterController>().MapControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.K, KeyCode.L);
    }
}
