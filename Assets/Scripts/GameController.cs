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

public abstract class Controller
{
    public enum ControllerType
    {
        Keyboard,
        Mouse,
        Gamepad
    }

    public KeyCode aKey, bKey;

    public Controller(KeyCode a, KeyCode b)
    {
        aKey = a;
        bKey = b;
    }

    public abstract ControllerType Type();
}

public class KeyboardController : Controller
{
    public KeyCode lKey, rKey, uKey, dKey;

    public KeyboardController(KeyCode a, KeyCode b, KeyCode l, KeyCode r, KeyCode u, KeyCode d) : base(a, b)
    {
        lKey = l;
        rKey = r;
        uKey = u;
        dKey = d;
    }

    public override ControllerType Type()
    {
        return ControllerType.Keyboard;
    }
}

public class MouseController : Controller
{
    public MouseController() : base(KeyCode.Mouse0, KeyCode.Mouse1)
    { }

    public override ControllerType Type()
    {
        return ControllerType.Mouse;
    }
}

public class PlayerInformation
{
    public Controller controller;
    public ClassInformation classInformation;
    public GameObject character;
    public bool isReady;
    public bool isRealPlayer = false;
    public int classSelectionIndex = 0;
    public enum CharacterState { HumanAlive, HumanDead, MonsterAlive, MonsterDead }

    public PlayerInformation(Controller newController)
    {
        controller = newController;
        isReady = false;
    }

    public PlayerInformation()
    {
        controller = new KeyboardController(KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None);
        isReady = true;
    }
}

public class GameController : MonoBehaviour
{
    enum State { game, characterSelect, newHumanSelect }

    State state = State.characterSelect;
    PlayerInformation[] playerInfo = new PlayerInformation[GameConstants.NUM_PLAYERS];
    int nextPlayerToAddIndex = 0;
    Map stageMap;

    GameObject[,] AIControllers = new GameObject[GameConstants.NUM_PLAYERS, GameConstants.NUM_TYPES_OF_CLASSES];
    static GameObject monsterAIControllerPrefab, humanAIControllerPrefab;

    ClassInformation hunter, watchman, werewolf, vampire;
    List<ClassInformation> classes;
    int allowedHumanPlayerIndex = 0;

    // Index of character now choosing their human class after being converted from a monster
    int newHumanIndex = 0;

    GameObject startButton;
    GameObject characterInfoPanelPrefab;
    GameObject[] characterInfoPanels = new GameObject[GameConstants.NUM_PLAYERS];

    // Use this for initialization
    void Start()
    {
        hunter = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HunterPrefab.prefab", typeof(GameObject)),
            "Hunter", "None", "Arrow", "Dash", true);
        watchman = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WatchmanPrefab.prefab", typeof(GameObject)),
            "Watchman", "Lantern", "Stun", "Lantern Toss", true);
        werewolf = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WerewolfPrefab.prefab", typeof(GameObject)),
            "Werewolf", "Break Lights", "Knockback", "Dash", false);
        vampire = new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VampirePrefab.prefab", typeof(GameObject)),
            "Vampire", "None", "Bite(Heal)", "Slow Projectile", false);
        classes = new List<ClassInformation> { hunter, watchman, werewolf, vampire };

        stageMap = new Map(GameConstants.MAP_COLUMNS, GameConstants.MAP_ROWS, GameConstants.MAP_TILE_SIZE, GameConstants.MAP_OFFSET);

        characterInfoPanelPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/CharacterInfoPanelPrefab.prefab", typeof(GameObject));

        monsterAIControllerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/MonsterAIController.prefab", typeof(GameObject));
        humanAIControllerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HumanAIController.prefab", typeof(GameObject));

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            AIControllers[i, GameConstants.HUMAN_CLASS_TYPE_INDEX] = Instantiate(humanAIControllerPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
            AIControllers[i, GameConstants.MONSTER_CLASS_TYPE_INDEX] = Instantiate(monsterAIControllerPrefab, new Vector3(-100, -100, -100), Quaternion.identity);

            if (i == 0)
            {
                playerInfo[i] = new PlayerInformation(ControlSchemeHandler.controlSchemes[0].controller);
                ControlSchemeHandler.controlSchemes[0].isInUse = true;
                ++nextPlayerToAddIndex;
            }
            else
            {
                playerInfo[i] = new PlayerInformation();
            }
        }

        StartCharacterSelect();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.game)
        {
            UpdateGame();
        }
        else if (state == State.characterSelect)
        {
            UpdateCharacterSelect();
        }
        else if (state == State.newHumanSelect)
        {
            UpdateNewHumanCharacterSelection();
        }
    }

    private void UpdateGame()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RegeneratePlayers();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Regenerate();
        }

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (!playerInfo[i].character.GetComponent<PlayerController>().isAlive)
            {
                Destroy(playerInfo[i].character);
                newHumanIndex = i;
                StartNewHumanCharacterSelection();
            }
        }
    }

    private void UpdateCharacterSelect()
    {
        // Register the game start input before the ready input so they don't cause 2 events
        CharacterSelectRegisterStartButtonInput();
        // Register the new controller input here so that the
        RegisterNewControllerAdded();

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            CharacterSelectRegisterInput(i, false);
            ResetCharacterInfoPanel(i, false);
        }
    }

    private void StartCharacterSelect()
    {
        startButton = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/StartButtonPrefab.prefab", typeof(GameObject)), GameConstants.START_BUTTON_POSITION, Quaternion.identity);
        startButton.SetActive(false);

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (playerInfo[i].isRealPlayer)
            {
                characterInfoPanels[i] = Instantiate(characterInfoPanelPrefab, GameConstants.CHARACTER_INFO_PANEL_POSITIONS[i], Quaternion.identity);
                playerInfo[i].classInformation = classes[playerInfo[i].classSelectionIndex];
            }
        }
    }

    private void EndCharacterSelect()
    {
        for (int i = 0; i < characterInfoPanels.Length; ++i)
        {
            Destroy(characterInfoPanels[i]);
            Destroy(startButton);
        }

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            PlayerInformation currentPlayerInfo = playerInfo[i];
            // Resolve the AI classes before spawning, set them to random ones
            if (!currentPlayerInfo.isRealPlayer)
            {
                // Force an AI to be the human if no real player has chosen it
                if (allowedHumanPlayerIndex == -1)
                {
                    currentPlayerInfo.classSelectionIndex = GetRandomHumanClassIndex();
                    allowedHumanPlayerIndex = i;
                }
                else
                {
                    currentPlayerInfo.classSelectionIndex = GetRandomMonsterClassIndex();
                }

                currentPlayerInfo.classInformation = classes[currentPlayerInfo.classSelectionIndex];
            }
        }

        // Set the state before regenerating so we don't override setting it to new human selection
        state = State.game;
        Regenerate();
    }

    private void CharacterSelectRegisterStartButtonInput()
    {
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
                activateStartKey = playerInfo[firstRealPlayerIndex].controller.aKey;
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
                EndCharacterSelect();
            }
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    private void ResolveAllowedHumanIndex()
    {
        // If no player picks the allowed human index then sets it back to -1 otherwise resolve it if we can
        allowedHumanPlayerIndex = -1;
        int j = 0;
        while (j < GameConstants.NUM_PLAYERS && allowedHumanPlayerIndex == -1)
        {
            if (playerInfo[j].isRealPlayer && playerInfo[j].classInformation.isHumanClass)
            {
                allowedHumanPlayerIndex = j;
            }
            ++j;
        }
    }

    private void CharacterSelectRegisterInput(int playerIndex, bool isNewHumanSelection)
    {
        PlayerInformation currentPlayer = playerInfo[playerIndex];
        if (currentPlayer.isRealPlayer)
        {
            if (!currentPlayer.isReady)
            {
                if (currentPlayer.controller.Type() == Controller.ControllerType.Mouse)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        BoxCollider2D readyBox = characterInfoPanels[playerIndex].transform.Find("Ready").GetComponent<BoxCollider2D>();
                        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        mPos = new Vector3(mPos.x, mPos.y, readyBox.transform.position.z);

                        BoxCollider2D leftArrow = characterInfoPanels[playerIndex].transform.Find("LeftArrow").GetComponent<BoxCollider2D>();
                        BoxCollider2D rightArrow = characterInfoPanels[playerIndex].transform.Find("RightArrow").GetComponent<BoxCollider2D>();

                        bool arrowPressed = false;

                        if (leftArrow.bounds.Contains(mPos))
                        {
                            currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + classes.Count - 1) % classes.Count;
                            arrowPressed = true;
                        }
                        else if (rightArrow.bounds.Contains(mPos))
                        {
                            currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + 1) % classes.Count;
                            arrowPressed = true;
                        }

                        if (arrowPressed)
                        {
                            currentPlayer.classInformation = classes[currentPlayer.classSelectionIndex];

                            if (playerIndex == allowedHumanPlayerIndex && !currentPlayer.classInformation.isHumanClass)
                            {
                                ResolveAllowedHumanIndex();
                            }
                            else if (allowedHumanPlayerIndex == -1 && currentPlayer.classInformation.isHumanClass)
                            {
                                allowedHumanPlayerIndex = playerIndex;
                            }
                        }

                        if (readyBox.bounds.Contains(mPos))
                        {
                            if (
                                (!isNewHumanSelection && (!currentPlayer.classInformation.isHumanClass || playerIndex == allowedHumanPlayerIndex)) ||
                                (isNewHumanSelection && currentPlayer.classInformation.isHumanClass)
                               )
                            {
                                currentPlayer.isReady = true;
                            }
                        }
                    }
                }
                else if(currentPlayer.controller.Type() == Controller.ControllerType.Keyboard)
                {
                    bool changedClass = false;
                    KeyboardController playerController = (KeyboardController)currentPlayer.controller;
                    if (Input.GetKeyDown(playerController.lKey))
                    {
                        currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + classes.Count - 1) % classes.Count;
                        changedClass = true;
                    }
                    else if (Input.GetKeyDown(playerController.rKey))
                    {
                        currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + 1) % classes.Count;
                        changedClass = true;
                    }
                    else if (Input.GetKeyDown(playerController.uKey))
                    {

                    }
                    else if (Input.GetKeyDown(playerController.dKey))
                    {

                    }
                    else if (Input.GetKeyDown(playerController.aKey))
                    {
                        if (
                            (!isNewHumanSelection && (!currentPlayer.classInformation.isHumanClass || playerIndex == allowedHumanPlayerIndex)) ||
                            (isNewHumanSelection && currentPlayer.classInformation.isHumanClass)
                           )
                        {
                            currentPlayer.isReady = true;
                        }
                    }

                    if (changedClass)
                    {
                        currentPlayer.classInformation = classes[currentPlayer.classSelectionIndex];

                        if (playerIndex == allowedHumanPlayerIndex && !currentPlayer.classInformation.isHumanClass)
                        {
                            ResolveAllowedHumanIndex();
                        }
                        else if (allowedHumanPlayerIndex == -1 && currentPlayer.classInformation.isHumanClass)
                        {
                            allowedHumanPlayerIndex = playerIndex;
                        }
                    }
                }
                else if(currentPlayer.controller.Type() == Controller.ControllerType.Gamepad)
                {

                }

                if (Input.GetKeyDown(currentPlayer.controller.bKey) && !isNewHumanSelection)
                {
                    currentPlayer.isRealPlayer = false;
                    currentPlayer.isReady = true;

                    Destroy(characterInfoPanels[playerIndex]);

                    // If the allowed human is removed then search through the rest of the active players to give them the chance to be the human
                    if (playerIndex == allowedHumanPlayerIndex)
                    {
                        ResolveAllowedHumanIndex();
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(currentPlayer.controller.bKey))
                {
                    currentPlayer.isReady = false;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(currentPlayer.controller.aKey))
            {
                currentPlayer.isRealPlayer = true;
                currentPlayer.isReady = false;

                characterInfoPanels[playerIndex] = Instantiate(characterInfoPanelPrefab, GameConstants.CHARACTER_INFO_PANEL_POSITIONS[playerIndex], Quaternion.identity);
                currentPlayer.classInformation = classes[currentPlayer.classSelectionIndex];

                if (allowedHumanPlayerIndex == -1 && currentPlayer.classInformation.isHumanClass)
                {
                    allowedHumanPlayerIndex = playerIndex;
                }
            }
        }
    }

    private void RegisterNewControllerAdded()
    {
        int i = 0;
        while (i < ControlSchemeHandler.controlSchemes.Length && nextPlayerToAddIndex < GameConstants.NUM_PLAYERS)
        {
            if (!ControlSchemeHandler.controlSchemes[i].isInUse && Input.GetKeyDown(ControlSchemeHandler.controlSchemes[i].controller.aKey))
            {
                playerInfo[nextPlayerToAddIndex] = new PlayerInformation(ControlSchemeHandler.controlSchemes[i].controller);
                ControlSchemeHandler.controlSchemes[i].isInUse = true;

                playerInfo[nextPlayerToAddIndex].isRealPlayer = true;

                characterInfoPanels[nextPlayerToAddIndex] = Instantiate(characterInfoPanelPrefab, GameConstants.CHARACTER_INFO_PANEL_POSITIONS[nextPlayerToAddIndex], Quaternion.identity);
                playerInfo[nextPlayerToAddIndex].classInformation = classes[playerInfo[nextPlayerToAddIndex].classSelectionIndex];

                if (allowedHumanPlayerIndex == -1 && playerInfo[nextPlayerToAddIndex].classInformation.isHumanClass)
                {
                    allowedHumanPlayerIndex = nextPlayerToAddIndex;
                }

                ++nextPlayerToAddIndex;
            }
            ++i;
        }
    }

    private void StartNewHumanCharacterSelection()
    {
        if (playerInfo[newHumanIndex].isRealPlayer)
        {
            startButton = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/StartButtonPrefab.prefab", typeof(GameObject)), GameConstants.START_BUTTON_POSITION, Quaternion.identity);
            startButton.SetActive(false);
            stageMap.ClearAll();
            HidePlayers();
            state = State.newHumanSelect;
            playerInfo[newHumanIndex].isReady = false;
            characterInfoPanels[newHumanIndex] = Instantiate(characterInfoPanelPrefab, GameConstants.NEW_HUMAN_CHARACTER_INFO_PANEL_POSITION, Quaternion.identity);
        }
        else
        {
            playerInfo[newHumanIndex].classSelectionIndex = GetRandomHumanClassIndex();
            playerInfo[newHumanIndex].classInformation = classes[playerInfo[newHumanIndex].classSelectionIndex];
            state = State.game;
            Regenerate();
        }
    }

    private void NewHumanCharacterSelectionRegisterStartButtonInput()
    {
        PlayerInformation newHumanPlayer = playerInfo[newHumanIndex];
        if (newHumanPlayer.isReady)
        {
            startButton.SetActive(true);
            KeyCode activateStartKey = newHumanPlayer.controller.aKey;

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
                Destroy(characterInfoPanels[newHumanIndex]);
                Destroy(startButton);

                Regenerate();
                ShowPlayers();
                state = State.game;
            }
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    private void UpdateNewHumanCharacterSelection()
    {
        NewHumanCharacterSelectionRegisterStartButtonInput();
        CharacterSelectRegisterInput(newHumanIndex, true);
        ResetCharacterInfoPanel(newHumanIndex, true);
    }

    private void RegeneratePlayers()
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
            if (!playerInfo[i].isRealPlayer || playerInfo[i].classInformation.isHumanClass)
            {
                anyPlayerHasPickedHuman = true;
            }
        }

        // If we don't have a human then set someone to randomly be it(this case will only occur if we have no AI and 4 human players)
        if (!anyPlayerHasPickedHuman)
        {
            newHumanIndex = Random.Range(0, GameConstants.NUM_PLAYERS);
            StartNewHumanCharacterSelection();
            return;
        }

        // Instantiating all the players, human and AI
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            PlayerInformation currentPlayerInfo = playerInfo[i];

            currentPlayerInfo.character = Instantiate(currentPlayerInfo.classInformation.prefab, GameConstants.PLAYER_SPAWN_POSITIONS[i], Quaternion.identity);

            // Spawn player with the correct control scheme
            if (playerInfo[i].isRealPlayer)
            {
                currentPlayerInfo.character.GetComponent<PlayerController>().MapControls(playerInfo[i].controller);
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

    private void HidePlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; ++i)
        {
            players[i].SetActive(false);
        }
    }

    private void ShowPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; ++i)
        {
            players[i].SetActive(true);
        }
    }

    private void Regenerate()
    {
        stageMap.Generate();
        RegeneratePlayers();
    }

    private void ResetCharacterInfoPanel(int playerIndex, bool isNewHumanSelection)
    {
        PlayerInformation currentPlayerInfo = playerInfo[playerIndex];
        if (currentPlayerInfo.isRealPlayer)
        {
            characterInfoPanels[playerIndex].transform.Find("Canvas").GetComponentInChildren<Text>().text =
                "Player " + (playerIndex + 1) +
                "\nName: " + currentPlayerInfo.classInformation.name +
                "\nPassive: " + currentPlayerInfo.classInformation.passiveDescription +
                "\nPrimary: " + currentPlayerInfo.classInformation.primaryDescription +
                "\nSecondary: " + currentPlayerInfo.classInformation.secondaryDescription;

            SpriteRenderer characterInfoPanelSpriteRenderer = characterInfoPanels[playerIndex].transform.Find("ClassSprite").gameObject.GetComponent<SpriteRenderer>();
            SpriteRenderer classSpriteRenderer = currentPlayerInfo.classInformation.prefab.GetComponent<SpriteRenderer>();

            characterInfoPanelSpriteRenderer.sprite = classSpriteRenderer.sprite;
            characterInfoPanelSpriteRenderer.color = classSpriteRenderer.color;

            if (currentPlayerInfo.isReady)
            {
                characterInfoPanels[playerIndex].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
            }
            else if (
                (!isNewHumanSelection && (currentPlayerInfo.classInformation.isHumanClass && playerIndex != allowedHumanPlayerIndex)) ||
                (isNewHumanSelection && !currentPlayerInfo.classInformation.isHumanClass)
                )
            {
                characterInfoPanels[playerIndex].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                characterInfoPanels[playerIndex].transform.Find("Background").gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    private int GetRandomHumanClassIndex()
    {
        return Random.Range(0, GameConstants.NUM_HUMAN_CLASSES);
    }

    // This random return is based on the fact that human classes are all listed first in the index
    private int GetRandomMonsterClassIndex()
    {
        return GameConstants.NUM_HUMAN_CLASSES + Random.Range(0, GameConstants.NUM_MONSTER_CLASSES);
    }
}