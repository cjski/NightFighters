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
    public bool isHumanClass { get; private set; }

    public ClassInformation(GameObject classPrefab, string className, string classPassiveDescription, string classPrimaryDescription, string classSecondaryDescription, bool classIsHumanClass)
    {
        prefab = classPrefab;
        name = className;
        passiveDescription = classPassiveDescription;
        primaryDescription = classPrimaryDescription;
        secondaryDescription = classSecondaryDescription;
        isHumanClass = classIsHumanClass;
    }
}

public class PlayerInformation
{
    public KeyCode l, r, u, d, a, b;
    public ClassInformation classInformation; //0 Human, 1 Monster
    public GameObject character;
    public bool isReady = false;
    public bool isRealPlayer = false;
    public int classSelectionIndex = 0;
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
    List<ClassInformation> classes;
    int allowedHumanPlayerIndex = 0;

    GameObject startButton;
    GameObject characterInfoPanelPrefab; 
    GameObject[] characterInfoPanels = new GameObject[GameConstants.NUM_PLAYERS];

    // Use this for initialization
    void Start () {
        hunter = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HunterPrefab.prefab", typeof(GameObject)),
            "Hunter", "None", "Arrow", "Dash", true);
        watchman = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WatchmanPrefab.prefab", typeof(GameObject)),
            "Watchman", "Lantern", "Stun", "Lantern Toss", true);
        werewolf = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WerewolfPrefab.prefab", typeof(GameObject)),
            "Werewolf", "Break Lights", "Knockback", "Dash", false);
        vampire = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VampirePrefab.prefab", typeof(GameObject)),
            "Vampire", "None", "Bite(Heal)", "Slow Projectile", false);
        classes = new List<ClassInformation>{ hunter, watchman, werewolf, vampire };

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
            UpdateCharacterSelect();
        }
    }

    private void UpdateCharacterSelect()
    {
        CharacterSelectRegisterInput();
        ResetCharacterInfoPanels();
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
                characterInfoPanels[i] = Instantiate(characterInfoPanelPrefab, GameConstants.CHARACTER_INFO_PANEL_POSITIONS[i], Quaternion.identity);
                playerInfo[i].classInformation = classes[playerInfo[i].classSelectionIndex];
            }
            else
            {
                playerInfo[i].isReady = true;
            }
        }
    }

    private void CharacterSelectRegisterInput()
    {
        // Register the game start input before the ready input so that they dont happen in the same frame
        bool allReady = true;
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (!playerInfo[i].isReady) allReady = false;
        }

        if (allReady)
        {
            startButton.SetActive(true);
            KeyCode activateStartKey = KeyCode.None;

            // Find the first person in the list who is a real player - they will control the start button
            int firstRealPlayerIndex = 0;
            while (firstRealPlayerIndex < GameConstants.NUM_PLAYERS && !playerInfo[firstRealPlayerIndex].isRealPlayer)
            {
                firstRealPlayerIndex++;
            }
            // If the players are all AI then use the default key to start game otherwise use the first available players key
            if (firstRealPlayerIndex == GameConstants.NUM_PLAYERS)
            {
                activateStartKey = GameConstants.DEFAULT_GAME_START_KEY;
            }
            else
            {
                activateStartKey = playerInfo[firstRealPlayerIndex].a;
            }

            bool startButtonActivated = false;
            if (activateStartKey == KeyCode.Mouse0)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mPos = new Vector3(mPos.x, mPos.y, startButton.transform.position.z);

                    if (startButton.GetComponent<BoxCollider2D>().bounds.Contains(mPos))
                    {
                        startButtonActivated = true;
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(activateStartKey))
                {
                    startButtonActivated = true;
                }
            }

            if (startButtonActivated)
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
        else
        {
            startButton.SetActive(false);
        }

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (playerInfo[i].isRealPlayer)
            {
                if (!playerInfo[i].isReady)
                {
                    if (playerInfo[i].a == KeyCode.Mouse0)
                    {
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            BoxCollider2D readyBox = characterInfoPanels[i].transform.Find("Ready").GetComponent<BoxCollider2D>();
                            Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            mPos = new Vector3(mPos.x, mPos.y, readyBox.transform.position.z);

                            BoxCollider2D leftArrow = characterInfoPanels[i].transform.Find("LeftArrow").GetComponent<BoxCollider2D>();
                            BoxCollider2D rightArrow = characterInfoPanels[i].transform.Find("RightArrow").GetComponent<BoxCollider2D>();

                            bool arrowPressed = false;

                            if (leftArrow.bounds.Contains(mPos))
                            {
                                playerInfo[i].classSelectionIndex = (playerInfo[i].classSelectionIndex + classes.Count - 1) % classes.Count;
                                arrowPressed = true;
                            }
                            else if (rightArrow.bounds.Contains(mPos))
                            {
                                playerInfo[i].classSelectionIndex = (playerInfo[i].classSelectionIndex + 1) % classes.Count;
                                arrowPressed = true; 
                            }

                            if(arrowPressed)
                            {
                                playerInfo[i].classInformation = classes[playerInfo[i].classSelectionIndex];

                                if(i == allowedHumanPlayerIndex && !playerInfo[i].classInformation.isHumanClass)
                                {
                                    allowedHumanPlayerIndex = -1;
                                }
                                else if(allowedHumanPlayerIndex == -1 && playerInfo[i].classInformation.isHumanClass)
                                {
                                    allowedHumanPlayerIndex = i;
                                }
                            }

                            if (readyBox.bounds.Contains(mPos))
                            {
                                if (!playerInfo[i].classInformation.isHumanClass || allowedHumanPlayerIndex == i)
                                {
                                    playerInfo[i].isReady = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool changedClass = false;
                        if (Input.GetKeyDown(playerInfo[i].l))
                        {
                            playerInfo[i].classSelectionIndex = (playerInfo[i].classSelectionIndex + classes.Count - 1) % classes.Count;
                            changedClass = true;
                        }
                        else if (Input.GetKeyDown(playerInfo[i].r))
                        {
                            playerInfo[i].classSelectionIndex = (playerInfo[i].classSelectionIndex + 1) % classes.Count;
                            changedClass = true;
                        }
                        else if (Input.GetKeyDown(playerInfo[i].u))
                        {
                            
                        }
                        else if (Input.GetKeyDown(playerInfo[i].d))
                        {
                            
                        }
                        else if (Input.GetKeyDown(playerInfo[i].a))
                        {
                            if (!playerInfo[i].classInformation.isHumanClass || allowedHumanPlayerIndex == i)
                            {
                                playerInfo[i].isReady = true;
                            }
                        }

                        if(changedClass)
                        {
                            playerInfo[i].classInformation = classes[playerInfo[i].classSelectionIndex];

                            if (i == allowedHumanPlayerIndex && !playerInfo[i].classInformation.isHumanClass)
                            {
                                allowedHumanPlayerIndex = -1;
                            }
                            else if (allowedHumanPlayerIndex == -1 && playerInfo[i].classInformation.isHumanClass)
                            {
                                allowedHumanPlayerIndex = i;
                            }
                        }
                    }

                    if (Input.GetKeyDown(playerInfo[i].b))
                    {
                        playerInfo[i].isRealPlayer = false;
                        playerInfo[i].isReady = true;

                        Destroy(characterInfoPanels[i]);

                        // If the allowed human is removed then search through the rest of the active players to give them the chance to be the human
                        if (i == allowedHumanPlayerIndex)
                        {
                            // If no player picks the allowed human index then sets it back to -1
                            allowedHumanPlayerIndex = -1;
                            int j = 0;
                            while(j < GameConstants.NUM_PLAYERS && allowedHumanPlayerIndex == -1)
                            {
                                if(playerInfo[j].isRealPlayer && playerInfo[j].classInformation.isHumanClass)
                                {
                                    allowedHumanPlayerIndex = j;
                                }
                                ++j;
                            } 
                        }
                    }
                }
                else
                {
                    if (Input.GetKeyDown(playerInfo[i].b))
                    {
                        playerInfo[i].isReady = false;
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(playerInfo[i].a))
                {
                    playerInfo[i].isRealPlayer = true;
                    playerInfo[i].isReady = false;

                    characterInfoPanels[i] = Instantiate(characterInfoPanelPrefab, GameConstants.CHARACTER_INFO_PANEL_POSITIONS[i], Quaternion.identity);
                    playerInfo[i].classInformation = classes[playerInfo[i].classSelectionIndex];

                    if (allowedHumanPlayerIndex == -1 && playerInfo[i].classInformation.isHumanClass)
                    {
                        allowedHumanPlayerIndex = i;
                    }
                }
            }
        }
    }

    private void Restart()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; ++i)
        {
            Destroy(players[i]);
        }

        // Check if we have at least one human character or an AI we can force to be human
        bool anyPlayerHasPickedHuman = false;
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if(!playerInfo[i].isRealPlayer || playerInfo[i].classInformation.isHumanClass)
            {
                anyPlayerHasPickedHuman = true;
            }
        }

        // If we don't have a human then set someone to randomly be it(this case will only occur if we have no AI and 4 human players)
        if(!anyPlayerHasPickedHuman)
        {
            int playerForcedToBeHumanIndex = Random.Range(0, GameConstants.NUM_PLAYERS);
            playerInfo[playerForcedToBeHumanIndex].classSelectionIndex = Random.Range(0, GameConstants.NUM_HUMAN_CLASSES);
            allowedHumanPlayerIndex = playerForcedToBeHumanIndex;
            playerInfo[playerForcedToBeHumanIndex].classInformation = classes[playerInfo[playerForcedToBeHumanIndex].classSelectionIndex];
        }
        
        // Instantiating all the players, human and AI
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            PlayerInformation currentPlayerInfo = playerInfo[i];

            // Resolve the AI classes before spawning, set them to random ones
            if (!currentPlayerInfo.isRealPlayer)
            {
                // Force an AI to be the human if no real player has chosen it
                if (allowedHumanPlayerIndex == -1)
                {
                    currentPlayerInfo.classSelectionIndex = Random.Range(0, GameConstants.NUM_HUMAN_CLASSES);
                    allowedHumanPlayerIndex = i;
                }
                else
                {
                    currentPlayerInfo.classSelectionIndex = GameConstants.NUM_HUMAN_CLASSES + Random.Range(0, GameConstants.NUM_MONSTER_CLASSES);
                }

                currentPlayerInfo.classInformation = classes[currentPlayerInfo.classSelectionIndex];
            }

            currentPlayerInfo.character = Instantiate(currentPlayerInfo.classInformation.prefab, GameConstants.PLAYER_SPAWN_POSITIONS[i], Quaternion.identity);
            
            // Spawn player with the correct control scheme
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
            // Spawn AI with controllers to support them
            else
            {
                // Allow the AI to spawn as a human if all players are monsters
                if (playerInfo[i].classInformation.isHumanClass)
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
        for (int i = 0; i < lights.Length; ++i)
        {
            lights[i].GetComponent<LightController>().TurnOff();
        }
    }

    private void ReGen()
    {
        stageMap.Generate();
        Restart();
    }

    private void ResetCharacterInfoPanels()
    {
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (playerInfo[i].isRealPlayer)
            {
                characterInfoPanels[i].transform.Find("Canvas").GetComponentInChildren<Text>().text =
                    "Name: " + playerInfo[i].classInformation.name +
                    "\nPassive: " + playerInfo[i].classInformation.passiveDescription +
                    "\nPrimary: " + playerInfo[i].classInformation.primaryDescription +
                    "\nSecondary: " + playerInfo[i].classInformation.secondaryDescription;

                SpriteRenderer characterInfoPanelSpriteRenderer = characterInfoPanels[i].transform.Find("ClassSprite").gameObject.GetComponent<SpriteRenderer>();
                SpriteRenderer classSpriteRenderer = playerInfo[i].classInformation.prefab.GetComponent<SpriteRenderer>();

                characterInfoPanelSpriteRenderer.sprite = classSpriteRenderer.sprite;
                characterInfoPanelSpriteRenderer.color = classSpriteRenderer.color;

                if (playerInfo[i].isReady)
                {
                    characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
                }
                else if (playerInfo[i].classInformation.isHumanClass && i != allowedHumanPlayerIndex)
                {
                    characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                }
                else
                {
                    characterInfoPanels[i].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }
}
