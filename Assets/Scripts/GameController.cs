using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClassInformation
{
    public GameObject prefab { get; private set; }
    public string name { get; private set; }
    public string passiveDescription { get; private set; }
    public string primaryDescription { get; private set; }
    public string secondaryDescription { get; private set; }

    public ClassInformation(GameObject classPrefab, string className, string classPassiveDescription, string classPrimaryDescription, string classSecondaryDescription)
    {
        prefab = classPrefab;
        name = className;
        passiveDescription = classPassiveDescription;
        primaryDescription = classPrimaryDescription;
        secondaryDescription = classSecondaryDescription;
    }
}

public class GameController : MonoBehaviour {

    GameObject p1, p2, p3, p4;
    Map stageMap;
    int rows = 4, cols = 6;// 4, 6, 3
    float unitSize = 3;
    GameObject ai1;

    ClassInformation hunter, watchman, werewolf, vampire;

    // Use this for initialization
    void Start () {
        hunter = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HunterPrefab.prefab", typeof(GameObject)),
            "Hunter", "None", "Arrow", "Dash");
        watchman = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WatchmanPrefab.prefab", typeof(GameObject)),
            "Watchman", "Lantern", "Stun", "Lantern Toss");
        werewolf = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WerewolfPrefab.prefab", typeof(GameObject)),
            "Werewolf", "Break Lights", "Knockback", "Dash");
        vampire = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VampirePrefab.prefab", typeof(GameObject)),
            "Vampire", "None", "Bite(Heal)", "Slow Projectile");

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

    private void CharacterSelect()
    {

    }

    private void Restart()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i=0;i<players.Length;++i)
        {
            Destroy(players[i]);
        }

        GameObject p1 = Instantiate(watchman.prefab, new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        p1.GetComponent<PlayerController>().MapControls(KeyCode.Mouse0, KeyCode.Mouse1);
        GameObject p2 = Instantiate(vampire.prefab, new Vector3(cols*unitSize - 0.5f, 0.5f, 0), Quaternion.identity);
        p2.GetComponent<PlayerController>().MapControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.K, KeyCode.L);
        GameObject p3 = Instantiate(werewolf.prefab, new Vector3(0.5f, rows*unitSize -0.5f, 0), Quaternion.identity);
        p3.GetComponent<PlayerController>().MapControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Z, KeyCode.X);
        GameObject p4 = Instantiate(hunter.prefab, new Vector3((cols-0.5f)*unitSize, (rows-0.5f)*unitSize, 0), Quaternion.identity);

        ai1.GetComponent<HumanAI>().Init(stageMap, p4);     
    }

    private void ReGen()
    {
        stageMap.Generate();

        Restart();
    }
}
