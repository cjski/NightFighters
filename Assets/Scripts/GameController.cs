using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

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

public class PlayerInformation
{
    public KeyCode l, r, u, d, a, b;
    public ClassInformation selectedClass = null;
    public GameObject character = null;

    public PlayerInformation(KeyCode pA, KeyCode pB, KeyCode pL=KeyCode.None, KeyCode pR=KeyCode.None, KeyCode pU=KeyCode.None, KeyCode pD=KeyCode.None)
    {
        a = pA;
        b = pB;
        l = pL;
        r = pR;
        u = pU;
        d = pD;
    }
}

public class GameController : MonoBehaviour {

    enum State { game, charSelect }

    State state = State.charSelect;
    PlayerInformation[] playerInfo = {new PlayerInformation(KeyCode.Mouse0, KeyCode.Mouse1),
        new PlayerInformation(KeyCode.Z, KeyCode.X, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow),
        new PlayerInformation(KeyCode.K, KeyCode.L, KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S),
        new PlayerInformation(KeyCode.O, KeyCode.P)};
    Map stageMap;
    int rows = 4, cols = 6;// 4, 6, 3
    float unitSize = 3;
    GameObject ai1;

    ClassInformation hunter, watchman, werewolf, vampire;
    ClassInformation[] classes;
    int[] classSelectionIndex = { 0, 0, 0, 0 };
    bool[] ready = { false, false, false, true };

    GameObject characterInfoPanelPrefab; 
    GameObject[] characterInfoPanels = new GameObject[4];

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
        classes = new ClassInformation[4]{ hunter, watchman, werewolf, vampire };
        stageMap = new Map(cols, rows, unitSize);
        characterInfoPanelPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/CharacterInfoPanelPrefab.prefab", typeof(GameObject));

        ai1 = GameObject.Find("AIController");

        StartCharacterSelect();
    }
	
	// Update is called once per frame
	void Update () {
        if (state == State.game)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                ReGen();
            }
        }
        else if(state == State.charSelect)
        {
            CharacterSelect();
        }
    }

    private void StartCharacterSelect()
    {
        characterInfoPanels[0] = Instantiate(characterInfoPanelPrefab, new Vector3(0, 8.6f, 0), Quaternion.identity);
        characterInfoPanels[1] = Instantiate(characterInfoPanelPrefab, new Vector3(6.2f, 8.6f, 0), Quaternion.identity);
        characterInfoPanels[2] = Instantiate(characterInfoPanelPrefab, new Vector3(12.4f, 8.6f, 0), Quaternion.identity);
        characterInfoPanels[3] = Instantiate(characterInfoPanelPrefab, new Vector3(18.6f, 8.6f, 0), Quaternion.identity);

        for (int i = 0; i < characterInfoPanels.Length; ++i)
        {
            playerInfo[i].selectedClass = classes[classSelectionIndex[i]];
            characterInfoPanels[i].GetComponentInChildren<Text>().text =
                "Name: " + playerInfo[i].selectedClass.name +
                "\nPassive: " + playerInfo[i].selectedClass.passiveDescription +
                "\nPrimary: " + playerInfo[i].selectedClass.primaryDescription +
                "\nSecondary: " + playerInfo[i].selectedClass.secondaryDescription;
        }

    }

    private void CharacterSelect()
    {
        for (int i = 0; i < playerInfo.Length; ++i)
        {
            if (!ready[i])
            {
                if (playerInfo[i].l == KeyCode.None)
                {
                    if(Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        BoxCollider2D leftArrow = characterInfoPanels[i].transform.Find("LeftArrow").GetComponent<BoxCollider2D>();
                        BoxCollider2D rightArrow = characterInfoPanels[i].transform.Find("RightArrow").GetComponent<BoxCollider2D>();
                        BoxCollider2D readyBox = characterInfoPanels[i].transform.Find("Ready").GetComponent<BoxCollider2D>();
                        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        mPos = new Vector3(mPos.x, mPos.y, leftArrow.transform.position.z);
                        if (leftArrow.bounds.Contains(mPos))
                        {
                            classSelectionIndex[i] = (classSelectionIndex[i] + classes.Length - 1) % classes.Length;
                            playerInfo[i].selectedClass = classes[classSelectionIndex[i]];
                            characterInfoPanels[i].GetComponentInChildren<Text>().text =
                                "Name: " + playerInfo[i].selectedClass.name +
                                "\nPassive: " + playerInfo[i].selectedClass.passiveDescription +
                                "\nPrimary: " + playerInfo[i].selectedClass.primaryDescription +
                                "\nSecondary: " + playerInfo[i].selectedClass.secondaryDescription;
                        }
                        else if (rightArrow.bounds.Contains(mPos))
                        {
                            classSelectionIndex[i] = (classSelectionIndex[i] + 1) % classes.Length;
                            playerInfo[i].selectedClass = classes[classSelectionIndex[i]];
                            characterInfoPanels[i].GetComponentInChildren<Text>().text =
                                "Name: " + playerInfo[i].selectedClass.name +
                                "\nPassive: " + playerInfo[i].selectedClass.passiveDescription +
                                "\nPrimary: " + playerInfo[i].selectedClass.primaryDescription +
                                "\nSecondary: " + playerInfo[i].selectedClass.secondaryDescription;
                        }
                        else if (readyBox.bounds.Contains(mPos))
                        {
                            characterInfoPanels[i].GetComponentInChildren<Text>().text += "\nREADY";
                            ready[i] = true;
                        }
                    }
                }
                else
                {
                    if (Input.GetKeyDown(playerInfo[i].l))
                    {
                        classSelectionIndex[i] = (classSelectionIndex[i] + classes.Length - 1) % classes.Length;
                        playerInfo[i].selectedClass = classes[classSelectionIndex[i]];
                        characterInfoPanels[i].GetComponentInChildren<Text>().text =
                            "Name: " + playerInfo[i].selectedClass.name +
                            "\nPassive: " + playerInfo[i].selectedClass.passiveDescription +
                            "\nPrimary: " + playerInfo[i].selectedClass.primaryDescription +
                            "\nSecondary: " + playerInfo[i].selectedClass.secondaryDescription;
                    }
                    else if (Input.GetKeyDown(playerInfo[i].r))
                    {
                        classSelectionIndex[i] = (classSelectionIndex[i] + 1) % classes.Length;
                        playerInfo[i].selectedClass = classes[classSelectionIndex[i]];
                        characterInfoPanels[i].GetComponentInChildren<Text>().text =
                            "Name: " + playerInfo[i].selectedClass.name +
                            "\nPassive: " + playerInfo[i].selectedClass.passiveDescription +
                            "\nPrimary: " + playerInfo[i].selectedClass.primaryDescription +
                            "\nSecondary: " + playerInfo[i].selectedClass.secondaryDescription;
                    }
                    else if (Input.GetKeyDown(playerInfo[i].a))
                    {
                        characterInfoPanels[i].GetComponentInChildren<Text>().text += "\nREADY";
                        ready[i] = true;
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(playerInfo[i].b))
                {
                    ready[i] = false;
                    characterInfoPanels[i].GetComponentInChildren<Text>().text =
                        "Name: " + playerInfo[i].selectedClass.name +
                        "\nPassive: " + playerInfo[i].selectedClass.passiveDescription +
                        "\nPrimary: " + playerInfo[i].selectedClass.primaryDescription +
                        "\nSecondary: " + playerInfo[i].selectedClass.secondaryDescription;
                }
            }
        }

        bool allReady = true;
        for(int i=0; i < ready.Length; ++i)
        {
            if (!ready[i]) allReady = false;
        }

        if (allReady)
        {
            for (int i = 0; i < characterInfoPanels.Length; ++i)
            {
                Destroy(characterInfoPanels[i]);
            }
            ReGen();
            state = State.game;
        }
    }

    private void Restart()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i=0;i<players.Length;++i)
        {
            Destroy(players[i]);
        }

        playerInfo[0].character = Instantiate(playerInfo[0].selectedClass.prefab, new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        playerInfo[0].character.GetComponent<PlayerController>().MapControls(KeyCode.Mouse0, KeyCode.Mouse1);
        playerInfo[1].character = Instantiate(playerInfo[1].selectedClass.prefab, new Vector3(cols*unitSize - 0.5f, 0.5f, 0), Quaternion.identity);
        playerInfo[1].character.GetComponent<PlayerController>().MapControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.K, KeyCode.L);
        playerInfo[2].character = Instantiate(playerInfo[2].selectedClass.prefab, new Vector3(0.5f, rows*unitSize -0.5f, 0), Quaternion.identity);
        playerInfo[2].character.GetComponent<PlayerController>().MapControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Z, KeyCode.X);
        playerInfo[3].character = Instantiate(playerInfo[3].selectedClass.prefab, new Vector3((cols-0.5f)*unitSize, (rows-0.5f)*unitSize, 0), Quaternion.identity);

        ai1.GetComponent<HumanAI>().Init(stageMap, playerInfo[3].character);     
    }

    private void ReGen()
    {
        stageMap.Generate();

        Restart();
    }
}
