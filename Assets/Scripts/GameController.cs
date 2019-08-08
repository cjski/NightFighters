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

    GameObject[,] AIControllers = new GameObject[GameConstants.NUM_PLAYERS, GameConstants.NUM_TYPES_OF_CLASSES];
    static GameObject monsterAIControllerPrefab, humanAIControllerPrefab;

    ClassInformation hunter, watchman, werewolf, vampire;
    List<List<ClassInformation>> classes = new List<List<ClassInformation>>();
    int[,] classSelectionIndex = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
    bool[] ready = { false, false, false, false };
    int[] typeOfClassIndex = { 0, 0, 0, 0 };

    GameObject startButton;
    GameObject characterInfoPanelPrefab; 
    GameObject[] characterInfoPanels = new GameObject[GameConstants.NUM_PLAYERS];

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

        stageMap = new Map(GameConstants.MAP_COLUMNS, GameConstants.MAP_ROWS, GameConstants.MAP_TILE_SIZE, GameConstants.MAP_OFFSET);

        characterInfoPanelPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/CharacterInfoPanelPrefab.prefab", typeof(GameObject));

        monsterAIControllerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/MonsterAIController.prefab", typeof(GameObject));
        humanAIControllerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HumanAIController.prefab", typeof(GameObject));

        for( int i = 0; i < GameConstants.NUM_PLAYERS; ++i )
        {
            AIControllers[i, GameConstants.HUMAN_CLASS_TYPE_INDEX] = Instantiate(humanAIControllerPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
            AIControllers[i, GameConstants.MONSTER_CLASS_TYPE_INDEX] = Instantiate(monsterAIControllerPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
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

        for( int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            characterInfoPanels[i] = Instantiate(characterInfoPanelPrefab, GameConstants.CHARACTER_INFO_PANEL_POSITIONS[i], Quaternion.identity);
        }

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            for (int j = 0; j < GameConstants.NUM_TYPES_OF_CLASSES; ++j) 
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
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
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
                        for (int j = 0; j < GameConstants.NUM_TYPES_OF_CLASSES; ++j)
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
                        typeOfClassIndex[i] = GameConstants.HUMAN_CLASS_TYPE_INDEX;
                    }
                    else if (Input.GetKeyDown(playerInfo[i].d))
                    {
                        typeOfClassIndex[i] = GameConstants.MONSTER_CLASS_TYPE_INDEX;
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
        for( int i = 0; i < GameConstants.NUM_PLAYERS; ++i )
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

        playerInfo[0].character = Instantiate(playerInfo[0].classes[GameConstants.HUMAN_CLASS_TYPE_INDEX].prefab, new Vector2(1, 1) + GameConstants.MAP_OFFSET, Quaternion.identity);
        //playerInfo[0].character.GetComponent<PlayerController>().MapControls(KeyCode.Mouse0, KeyCode.Mouse1);
        playerInfo[1].character = Instantiate(playerInfo[1].classes[GameConstants.MONSTER_CLASS_TYPE_INDEX].prefab, new Vector2(GameConstants.MAP_COLUMNS * GameConstants.MAP_TILE_SIZE - 1, 1) + GameConstants.MAP_OFFSET, Quaternion.identity);
        //playerInfo[1].character.GetComponent<PlayerController>().MapControls(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.K, KeyCode.L);
        playerInfo[2].character = Instantiate(playerInfo[2].classes[GameConstants.MONSTER_CLASS_TYPE_INDEX].prefab, new Vector2(1, GameConstants.MAP_ROWS * GameConstants.MAP_TILE_SIZE - 1) + GameConstants.MAP_OFFSET, Quaternion.identity);
        //playerInfo[2].character.GetComponent<PlayerController>().MapControls(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Z, KeyCode.X);
        playerInfo[3].character = Instantiate(playerInfo[3].classes[GameConstants.MONSTER_CLASS_TYPE_INDEX].prefab, new Vector2(GameConstants.MAP_COLUMNS * GameConstants.MAP_TILE_SIZE - 1, GameConstants.MAP_ROWS * GameConstants.MAP_TILE_SIZE - 1) + GameConstants.MAP_OFFSET, Quaternion.identity);

        AIControllers[0, GameConstants.HUMAN_CLASS_TYPE_INDEX].GetComponent<HumanAI>().Init(stageMap, playerInfo[0].character);
        AIControllers[1, GameConstants.MONSTER_CLASS_TYPE_INDEX].GetComponent<MonsterAI>().Init(stageMap, playerInfo[1].character);
        AIControllers[2, GameConstants.MONSTER_CLASS_TYPE_INDEX].GetComponent<MonsterAI>().Init(stageMap, playerInfo[2].character);
        AIControllers[3, GameConstants.MONSTER_CLASS_TYPE_INDEX].GetComponent<MonsterAI>().Init(stageMap, playerInfo[3].character);

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
