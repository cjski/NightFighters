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
    public bool isReady = false;
    public bool isRealPlayer = false;
    public int typeOfClass = 0;
    public int[] classSelectionIndexes = new int[2];
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
        new PlayerInformation(KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad1, KeyCode.Keypad3, KeyCode.Keypad5, KeyCode.Keypad2)};
    Map stageMap;

    GameObject[,] AIControllers = new GameObject[GameConstants.NUM_PLAYERS, GameConstants.NUM_TYPES_OF_CLASSES];
    static GameObject monsterAIControllerPrefab, humanAIControllerPrefab;

    ClassInformation hunter, watchman, werewolf, vampire;
    List<List<ClassInformation>> classes = new List<List<ClassInformation>>();

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

        playerInfo[0].isRealPlayer = true;

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
            if (playerInfo[i].isRealPlayer)
            {
                playerInfo[i].isReady = false;
            }
            else
            {
                playerInfo[i].isReady = true;
            }
            characterInfoPanels[i] = Instantiate(characterInfoPanelPrefab, GameConstants.CHARACTER_INFO_PANEL_POSITIONS[i], Quaternion.identity);
        }

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            for (int j = 0; j < GameConstants.NUM_TYPES_OF_CLASSES; ++j)
            {
                playerInfo[i].classes[j] = classes[j][playerInfo[i].classSelectionIndexes[j]];
                ResetCharacterInfoPanel(i, j);
            }
            if(!playerInfo[i].isRealPlayer)
            {
                characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }

    }

    private void CharacterSelect()
    {
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (playerInfo[i].isRealPlayer)
            {
                if (!playerInfo[i].isReady)
                {
                    if (playerInfo[i].l == KeyCode.None)
                    {
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            BoxCollider2D readyBox = characterInfoPanels[i].transform.Find("Ready").GetComponent<BoxCollider2D>();
                            Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            mPos = new Vector3(mPos.x, mPos.y, readyBox.transform.position.z);
                            for (int j = 0; j < GameConstants.NUM_TYPES_OF_CLASSES; ++j)
                            {
                                BoxCollider2D leftArrow = characterInfoPanels[i].transform.Find("LeftArrow" + j).GetComponent<BoxCollider2D>();
                                BoxCollider2D rightArrow = characterInfoPanels[i].transform.Find("RightArrow" + j).GetComponent<BoxCollider2D>();

                                bool arrowPressed = false;

                                if (leftArrow.bounds.Contains(mPos))
                                {
                                    playerInfo[i].classSelectionIndexes[j] = (playerInfo[i].classSelectionIndexes[j] + classes[j].Count - 1) % classes[j].Count;
                                    arrowPressed = true;
                                }
                                else if (rightArrow.bounds.Contains(mPos))
                                {
                                    playerInfo[i].classSelectionIndexes[j] = (playerInfo[i].classSelectionIndexes[j] + 1) % classes[j].Count;
                                    arrowPressed = true; 
                                }

                                if(arrowPressed)
                                {
                                    playerInfo[i].classes[j] = classes[j][playerInfo[i].classSelectionIndexes[j]];
                                    ResetCharacterInfoPanel(i, j);
                                }
                            }
                            if (readyBox.bounds.Contains(mPos))
                            {
                                characterInfoPanels[i].transform.Find("Background").GetComponent<SpriteRenderer>().color = Color.gray;
                                playerInfo[i].isReady = true;
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(playerInfo[i].l))
                        {
                            playerInfo[i].classSelectionIndexes[playerInfo[i].typeOfClass] = (playerInfo[i].classSelectionIndexes[playerInfo[i].typeOfClass] + classes[playerInfo[i].typeOfClass].Count - 1) % classes[playerInfo[i].typeOfClass].Count;
                            playerInfo[i].classes[playerInfo[i].typeOfClass] = classes[playerInfo[i].typeOfClass][playerInfo[i].classSelectionIndexes[playerInfo[i].typeOfClass]];
                            ResetCharacterInfoPanel(i, playerInfo[i].typeOfClass);
                        }
                        else if (Input.GetKeyDown(playerInfo[i].r))
                        {
                            playerInfo[i].classSelectionIndexes[playerInfo[i].typeOfClass] = (playerInfo[i].classSelectionIndexes[playerInfo[i].typeOfClass] + 1) % classes[playerInfo[i].typeOfClass].Count;
                            playerInfo[i].classes[playerInfo[i].typeOfClass] = classes[playerInfo[i].typeOfClass][playerInfo[i].classSelectionIndexes[playerInfo[i].typeOfClass]];
                            ResetCharacterInfoPanel(i, playerInfo[i].typeOfClass);
                        }
                        else if (Input.GetKeyDown(playerInfo[i].u))
                        {
                            playerInfo[i].typeOfClass = GameConstants.HUMAN_CLASS_TYPE_INDEX;
                        }
                        else if (Input.GetKeyDown(playerInfo[i].d))
                        {
                            playerInfo[i].typeOfClass = GameConstants.MONSTER_CLASS_TYPE_INDEX;
                        }
                        else if (Input.GetKeyDown(playerInfo[i].a))
                        {
                            characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
                            playerInfo[i].isReady = true;
                        }
                    }

                    if (Input.GetKeyDown(playerInfo[i].b))
                    {
                        playerInfo[i].isRealPlayer = false;
                        playerInfo[i].isReady = true;
                        characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.black;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(playerInfo[i].b))
                    {
                        playerInfo[i].isReady = false;
                        characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(playerInfo[i].a))
                {
                    playerInfo[i].isRealPlayer = true;
                    playerInfo[i].isReady = false;
                    characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }

        bool allReady = true;
        for( int i = 0; i < GameConstants.NUM_PLAYERS; ++i )
        {
            if (!playerInfo[i].isReady) allReady = false;
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

        for( int i=0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            PlayerInformation currentPlayerInfo = playerInfo[i];

            currentPlayerInfo.character = Instantiate(currentPlayerInfo.classes[playerInfo[i].typeOfClass].prefab, GameConstants.PLAYER_SPAWN_POSITIONS[i], Quaternion.identity);

            if (playerInfo[i].isRealPlayer)
            {
                if (currentPlayerInfo.l == KeyCode.None)
                {
                    currentPlayerInfo.character.GetComponent<PlayerController>().MapControls(
                        currentPlayerInfo.a,
                        currentPlayerInfo.b
                    );
                }
                else
                {
                    currentPlayerInfo.character.GetComponent<PlayerController>().MapControls(
                        currentPlayerInfo.l,
                        currentPlayerInfo.r,
                        currentPlayerInfo.u,
                        currentPlayerInfo.d,
                        currentPlayerInfo.a,
                        currentPlayerInfo.b
                    );
                }
            }
            else
            {
                if (playerInfo[i].typeOfClass == GameConstants.HUMAN_CLASS_TYPE_INDEX)
                {
                    AIControllers[i, GameConstants.HUMAN_CLASS_TYPE_INDEX].GetComponent<HumanAI>().Init(stageMap, currentPlayerInfo.character);
                }
                else
                {
                    AIControllers[i, GameConstants.MONSTER_CLASS_TYPE_INDEX].GetComponent<MonsterAI>().Init(stageMap, currentPlayerInfo.character);
                }
            }
        }

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

    private void ResetCharacterInfoPanel( int playerIndex, int classIndex )
    {
        characterInfoPanels[playerIndex].transform.Find("Canvas" + classIndex).GetComponentInChildren<Text>().text =
            "Name: " + playerInfo[playerIndex].classes[classIndex].name +
            "\nPassive: " + playerInfo[playerIndex].classes[classIndex].passiveDescription +
            "\nPrimary: " + playerInfo[playerIndex].classes[classIndex].primaryDescription +
            "\nSecondary: " + playerInfo[playerIndex].classes[classIndex].secondaryDescription;
    }
}
