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
    public ClassInformation[] classes = { null, null }; //0 Human, 1 Monster
    public GameObject character = null;
    public enum CharacterState { HumanAlive, HumanDead, MonsterAlive, MonsterDead }

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
        new PlayerInformation(KeyCode.Mouse0, KeyCode.Mouse1)};
    Map stageMap;
    int rows = 4, cols = 6;// 4, 6, 3
    float unitSize = 3.5f;
    Vector2 offset = new Vector2(-4,-2);

    GameObject[,] AIControllers = new GameObject[4, 2];
    static GameObject monsterAIControllerPrefab, humanAIControllerPrefab;

    ClassInformation hunter, watchman, werewolf, vampire;
    List<List<ClassInformation>> classes = new List<List<ClassInformation>>();
    int[,] classSelectionIndex = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
    bool[] ready = { false, false, false, false };
    int[] typeOfClassIndex = { 0, 0, 0, 0 };

    GameObject startButton;
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
        classes.Add(new List<ClassInformation> { hunter, watchman });
        classes.Add(new List<ClassInformation> { werewolf, vampire });

        stageMap = new Map(cols, rows, unitSize, offset);

        characterInfoPanelPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/CharacterInfoPanelPrefab.prefab", typeof(GameObject));

        monsterAIControllerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/MonsterAIController.prefab", typeof(GameObject));
        humanAIControllerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HumanAIController.prefab", typeof(GameObject));

        for( int i=0; i< 4; ++i )
        {
            AIControllers[i, 0] = Instantiate(humanAIControllerPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
            AIControllers[i, 1] = Instantiate(monsterAIControllerPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
        }

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
        startButton = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/StartButtonPrefab.prefab", typeof(GameObject)), new Vector3(10, -1.7f, 0), Quaternion.identity);
        startButton.SetActive(false);

        characterInfoPanels[0] = Instantiate(characterInfoPanelPrefab, new Vector3(0, 8.6f, 0), Quaternion.identity);
        characterInfoPanels[1] = Instantiate(characterInfoPanelPrefab, new Vector3(6.2f, 8.6f, 0), Quaternion.identity);
        characterInfoPanels[2] = Instantiate(characterInfoPanelPrefab, new Vector3(12.4f, 8.6f, 0), Quaternion.identity);
        characterInfoPanels[3] = Instantiate(characterInfoPanelPrefab, new Vector3(18.6f, 8.6f, 0), Quaternion.identity);

        for (int i = 0; i < characterInfoPanels.Length; ++i)
        {
            for (int j = 0; j < 2; ++j) 
            {
                playerInfo[i].classes[j] = classes[j][classSelectionIndex[j, i]];
                characterInfoPanels[i].transform.Find("Canvas" + j).GetComponentInChildren<Text>().text =
                    "Name: " + playerInfo[i].classes[j].name +
                    "\nPassive: " + playerInfo[i].classes[j].passiveDescription +
                    "\nPrimary: " + playerInfo[i].classes[j].primaryDescription +
                    "\nSecondary: " + playerInfo[i].classes[j].secondaryDescription;
            }
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
                        BoxCollider2D readyBox = characterInfoPanels[i].transform.Find("Ready").GetComponent<BoxCollider2D>();
                        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        mPos = new Vector3(mPos.x, mPos.y, readyBox.transform.position.z);
                        for (int j = 0; j < 2; ++j)
                        {
                            BoxCollider2D leftArrow = characterInfoPanels[i].transform.Find("LeftArrow"+j).GetComponent<BoxCollider2D>();
                            BoxCollider2D rightArrow = characterInfoPanels[i].transform.Find("RightArrow"+j).GetComponent<BoxCollider2D>();
                            if (leftArrow.bounds.Contains(mPos))
                            {
                                classSelectionIndex[j, i] = (classSelectionIndex[j, i] + classes[j].Count - 1) % classes[j].Count;
                                playerInfo[i].classes[j] = classes[j][classSelectionIndex[j, i]];
                                characterInfoPanels[i].transform.Find("Canvas" + j).GetComponentInChildren<Text>().text =
                                    "Name: " + playerInfo[i].classes[j].name +
                                    "\nPassive: " + playerInfo[i].classes[j].passiveDescription +
                                    "\nPrimary: " + playerInfo[i].classes[j].primaryDescription +
                                    "\nSecondary: " + playerInfo[i].classes[j].secondaryDescription;
                            }
                            else if (rightArrow.bounds.Contains(mPos))
                            {
                                classSelectionIndex[j, i] = (classSelectionIndex[j, i] + 1) % classes[j].Count;
                                playerInfo[i].classes[j] = classes[j][classSelectionIndex[j, i]];
                                characterInfoPanels[i].transform.Find("Canvas" + j).GetComponentInChildren<Text>().text =
                                    "Name: " + playerInfo[i].classes[j].name +
                                    "\nPassive: " + playerInfo[i].classes[j].passiveDescription +
                                    "\nPrimary: " + playerInfo[i].classes[j].primaryDescription +
                                    "\nSecondary: " + playerInfo[i].classes[j].secondaryDescription;
                            }
                        }
                        if (readyBox.bounds.Contains(mPos))
                        {
                            characterInfoPanels[i].transform.Find("Background").GetComponent<SpriteRenderer>().color = Color.gray;
                            ready[i] = true;
                        }
                    }
                }
                else
                {
                    if (Input.GetKeyDown(playerInfo[i].l))
                    {
                        classSelectionIndex[typeOfClassIndex[i], i] = (classSelectionIndex[typeOfClassIndex[i], i] + classes[typeOfClassIndex[i]].Count - 1) % classes[typeOfClassIndex[i]].Count;
                        playerInfo[i].classes[typeOfClassIndex[i]] = classes[typeOfClassIndex[i]][classSelectionIndex[typeOfClassIndex[i], i]];
                        characterInfoPanels[i].transform.Find("Canvas" + typeOfClassIndex[i]).GetComponentInChildren<Text>().text =
                            "Name: " + playerInfo[i].classes[typeOfClassIndex[i]].name +
                            "\nPassive: " + playerInfo[i].classes[typeOfClassIndex[i]].passiveDescription +
                            "\nPrimary: " + playerInfo[i].classes[typeOfClassIndex[i]].primaryDescription +
                            "\nSecondary: " + playerInfo[i].classes[typeOfClassIndex[i]].secondaryDescription;
                    }
                    else if (Input.GetKeyDown(playerInfo[i].r))
                    {
                        classSelectionIndex[typeOfClassIndex[i], i] = (classSelectionIndex[typeOfClassIndex[i], i] + 1) % classes[typeOfClassIndex[i]].Count;
                        playerInfo[i].classes[typeOfClassIndex[i]] = classes[typeOfClassIndex[i]][classSelectionIndex[typeOfClassIndex[i], i]];
                        characterInfoPanels[i].transform.Find("Canvas" + typeOfClassIndex[i]).GetComponentInChildren<Text>().text =
                            "Name: " + playerInfo[i].classes[typeOfClassIndex[i]].name +
                            "\nPassive: " + playerInfo[i].classes[typeOfClassIndex[i]].passiveDescription +
                            "\nPrimary: " + playerInfo[i].classes[typeOfClassIndex[i]].primaryDescription +
                            "\nSecondary: " + playerInfo[i].classes[typeOfClassIndex[i]].secondaryDescription;
                    }
                    else if (Input.GetKeyDown(playerInfo[i].u))
                    {
                        typeOfClassIndex[i] = 0;
                    }
                    else if (Input.GetKeyDown(playerInfo[i].d))
                    {
                        typeOfClassIndex[i] = 1;
                    }
                    else if (Input.GetKeyDown(playerInfo[i].a))
                    {
                        characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
                        ready[i] = true;
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(playerInfo[i].b))
                {
                    ready[i] = false;
                    characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.white;
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
            startButton.SetActive(true);

            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mPos = new Vector3(mPos.x, mPos.y, startButton.transform.position.z);

                if (startButton.GetComponent<BoxCollider2D>().bounds.Contains(mPos))
                {
                    for (int i = 0; i < characterInfoPanels.Length; ++i)
                    {
                        Destroy(characterInfoPanels[i]);
                        Destroy(startButton);
                    }
                    ReGen();
                    state = State.game;
                }
            }
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    private void Restart()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i=0;i<players.Length;++i)
        {
            Destroy(players[i]);
        }

        playerInfo[0].character = Instantiate(playerInfo[0].classes[0].prefab, new Vector2(1, 1) + offset, Quaternion.identity);
        //playerInfo[0].character.GetComponent<PlayerController>().MapControls(KeyCode.Mouse0, KeyCode.Mouse1);
        playerInfo[1].character = Instantiate(playerInfo[1].classes[1].prefab, new Vector2(cols*unitSize - 1, 1) + offset, Quaternion.identity);
        //playerInfo[1].character.GetComponent<PlayerController>().MapControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.K, KeyCode.L);
        playerInfo[2].character = Instantiate(playerInfo[2].classes[1].prefab, new Vector2(1, rows*unitSize -1) + offset, Quaternion.identity);
        //playerInfo[2].character.GetComponent<PlayerController>().MapControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Z, KeyCode.X);
        playerInfo[3].character = Instantiate(playerInfo[3].classes[1].prefab, new Vector2(cols*unitSize - 1, rows*unitSize - 1) + offset, Quaternion.identity);

        AIControllers[0,0].GetComponent<HumanAI>().Init(stageMap, playerInfo[0].character);
        AIControllers[1,1].GetComponent<MonsterAI>().Init(stageMap, playerInfo[1].character);
        AIControllers[2,1].GetComponent<MonsterAI>().Init(stageMap, playerInfo[2].character);
        AIControllers[3,1].GetComponent<MonsterAI>().Init(stageMap, playerInfo[3].character);

        GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");
        for(int i=0;i<lights.Length;++i)
        {
            lights[i].GetComponent<LightController>().TurnOff();
        }
    }

    private void ReGen()
    {
        stageMap.Generate();
        Restart();
    }
}
