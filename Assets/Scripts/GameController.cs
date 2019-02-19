using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameController : MonoBehaviour {

    GameObject playerPrefab;

    // Use this for initialization
    void Start () {
        playerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/PlayerPrefab.prefab", typeof(GameObject));

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

        GameObject p1 = Instantiate(playerPrefab, new Vector3(-5, 3, 0), Quaternion.identity);
        p1.GetComponent<PlayerController>().MapControls(KeyCode.Mouse0, KeyCode.Mouse1);
        GameObject p2 = Instantiate(playerPrefab, new Vector3(3, -3, 0), Quaternion.identity);
        p2.GetComponent<PlayerController>().MapControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.K, KeyCode.L);
    }
}
