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
    public bool isBossClass { get; private set; }

    public ClassInformation(GameObject classPrefab, string className, string classPassiveDescription, string classPrimaryDescription, string classSecondaryDescription, bool classIsHumanClass, bool classIsBossClass)
    {
        prefab = classPrefab;
        name = className;
        passiveDescription = classPassiveDescription;
        primaryDescription = classPrimaryDescription;
        secondaryDescription = classSecondaryDescription;
        isHumanClass = classIsHumanClass;
        isBossClass = classIsBossClass;
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
    public abstract bool GetAPressed();
    public abstract bool GetBPressed();
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

    public override bool GetAPressed()
    {
        return Input.GetKeyDown(aKey);
    }

    public override bool GetBPressed()
    {
        return Input.GetKeyDown(bKey);
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

    public override bool GetAPressed()
    {
        return Input.GetKeyDown(aKey);
    }

    public override bool GetBPressed()
    {
        return Input.GetKeyDown(bKey);
    }

    public override ControllerType Type()
    {
        return ControllerType.Mouse;
    }
}

public class GamepadController : Controller
{
    public int joystick { get; private set; }
    public string joystickString { get; private set; }
    private Timer selectionWaitTimer;
    public GamepadController(int joystickNum) : base(KeyCode.None, KeyCode.None)
    {
        joystick = joystickNum;
        joystickString = joystick.ToString();
        selectionWaitTimer = new Timer(GameConstants.GAMEPAD_SELECTION_WAIT_TIME, true);
    }

    public override bool GetAPressed()
    {
        return Input.GetButtonDown("CA_" + joystickString);
    }

    public override bool GetBPressed()
    {
        return Input.GetButtonDown("CB_" + joystickString);
    }

    public Vector2 GetAxis()
    {
        // Return - on the y axis because it is inverted normally
        return new Vector2(Input.GetAxis("CXAxis_" + joystickString), -Input.GetAxis("CYAxis_" + joystickString));
    }

    // Updates the wait timer, prevents the selection from calling 30 times in one frame and looping past the selection way too fast
    public Vector2 GetAxisForSelection()
    {
        if(selectionWaitTimer.done)
        {
            selectionWaitTimer.Reset();
            return GetAxis();
        }
        else
        {
            selectionWaitTimer.Update();
            return Vector2.zero;
        }
    }

    public override ControllerType Type()
    {
        return ControllerType.Gamepad;
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
    enum State { game, bossFight, endGameScreen, characterSelect, newHumanSelect, enterGameMenu }

    State state;
    PlayerInformation[] playerInfo = new PlayerInformation[GameConstants.NUM_PLAYERS];
    int nextPlayerToAddIndex = 0;
    Map stageMap;

    List<GameObject> AIControllerPrefabs;
    GameObject[] AIControllers = new GameObject[GameConstants.NUM_PLAYERS];

    List<ClassInformation> classes;
    List<ClassInformation> bossClasses;
    int allowedHumanPlayerIndex = 0;

    // Index of character now choosing their human class after being converted from a monster
    int newHumanIndex = 0;
    int newBossIndex = 0;
    int numHumans = 1;
    int numMonsters = 3;

    Timer endGameScreenTimer = new Timer(2.5f);

    GameObject enterGameMenuPanel;
    GameObject endGamePanel;
    GameObject startButton;
    GameObject characterInfoPanelPrefab;
    GameObject[] characterInfoPanels = new GameObject[GameConstants.NUM_PLAYERS];

    // Use this for initialization
    void Start()
    {
        classes = new List<ClassInformation> {
            new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HunterPrefab.prefab", typeof(GameObject)),
            "Hunter", "None", "Arrow", "Dash", true, false),
            new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WatchmanPrefab.prefab", typeof(GameObject)),
            "Watchman", "Lantern", "Stun", "Lantern Toss", true, false),
            new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WerewolfPrefab.prefab", typeof(GameObject)),
            "Werewolf", "Break Lights", "Knockback", "Dash", false, false),
            new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VampirePrefab.prefab", typeof(GameObject)),
            "Vampire", "None", "Bite(Heal)", "Slow Projectile", false, false)
        };

        bossClasses = new List<ClassInformation>
        {
           new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WerewolfBossPrefab.prefab", typeof(GameObject)),
           "", "", "", "", false, true),
           new ClassInformation((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VampireBossPrefab.prefab", typeof(GameObject)),
           "", "", "", "", false, true)
        };

        stageMap = new Map(GameConstants.MAP_COLUMNS, GameConstants.MAP_ROWS, GameConstants.MAP_TILE_SIZE, GameConstants.MAP_OFFSET);

        characterInfoPanelPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/CharacterInfoPanelPrefab.prefab", typeof(GameObject));

        AIControllerPrefabs = new List<GameObject>
        {
            (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/HunterAIController.prefab", typeof(GameObject)),
            (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WatchmanAIController.prefab", typeof(GameObject)),
            (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WerewolfAIController.prefab", typeof(GameObject)),
            (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VampireAIController.prefab", typeof(GameObject)),
        };

        StartEnterGameMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.game)
        {
            UpdateGame();
        }
        else if(state == State.bossFight)
        {
            UpdateBossFight();
        }
        else if (state == State.characterSelect)
        {
            UpdateCharacterSelect();
        }
        else if (state == State.newHumanSelect)
        {
            UpdateNewHumanCharacterSelection();
        }
        else if(state == State.enterGameMenu)
        {
            UpdateEnterGameMenu();
        }
        else if(state == State.endGameScreen)
        {
            UpdateEndGameScreen();
        }
    }

    private void ClearAllGameObjects()
    {
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (playerInfo[i] != null)
            {
                Destroy(playerInfo[i].character);
            }
        }
        stageMap.ClearAll();
    }

    private void UpdateEnterGameMenu()
    {
        RegisterNewControllersAdded();
        if(nextPlayerToAddIndex != 0)
        {
            Destroy(enterGameMenuPanel);
            StartCharacterSelect();
        }
    }

    private void StartEnterGameMenu()
    {
        enterGameMenuPanel = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/EnterGamePanelPrefab.prefab", typeof(GameObject)), GameConstants.ENTER_GAME_MENU_PANEL_POSITION, Quaternion.identity);
        state = State.enterGameMenu;
        nextPlayerToAddIndex = 0;
        numHumans = 1;
        numMonsters = 3;

        ClearAllGameObjects();

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            playerInfo[i] = new PlayerInformation();
        }

        for (int i = 0; i < ControlSchemeHandler.controlSchemes.Length; ++i)
        {
            ControlSchemeHandler.controlSchemes[i].isInUse = false;
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
        
        int lastMonsterIndex = 0;
        newHumanIndex = -1;
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (!playerInfo[i].character.GetComponent<PlayerController>().isAlive)
            {
                newHumanIndex = i;
            }

            if(!playerInfo[i].classInformation.isHumanClass && newHumanIndex != i)
            {
                lastMonsterIndex = i;
            }
        }

        if (newHumanIndex != -1)
        {
            --numMonsters;
            ++numHumans;
            if(numMonsters == 1)
            {
                newBossIndex = lastMonsterIndex;
            }
            Destroy(playerInfo[newHumanIndex].character);
            StartNewHumanCharacterSelection();
        }
    }

    private void StartBossFight()
    {
        playerInfo[newBossIndex].classInformation = bossClasses[playerInfo[newBossIndex].classSelectionIndex - GameConstants.NUM_HUMAN_CLASSES];
        state = State.bossFight;
        Regenerate();
    }

    private void UpdateBossFight()
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
            if (playerInfo[i].character != null && !playerInfo[i].character.GetComponent<PlayerController>().isAlive)
            {
                if (playerInfo[i].classInformation.isHumanClass)
                {
                    --numHumans;
                    Destroy(playerInfo[i].character);
                    playerInfo[i].character = null;
                }
                else
                {
                    --numMonsters;
                }
            }
        }

        if (numHumans == 0 || numMonsters == 0)
        {
            StartEndGameScreen();
        }
    }

    private void UpdateCharacterSelect()
    {
        // Register the game start input before the ready input so they don't cause 2 events
        CharacterSelectRegisterStartButtonInput();
        // Register the new controller input here so that the panels get made
        RegisterNewControllersAdded();

        bool anyRealPlayers = false;
        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            if (playerInfo[i].isRealPlayer)
            {
                anyRealPlayers = true;
            }
        }

        // Back out to the menu
        if (!anyRealPlayers)
        {
            if (playerInfo[0].controller.GetBPressed())
            {
                Destroy(startButton);
                StartEnterGameMenu();
            }
        }

        for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
        {
            CharacterSelectRegisterInput(i, false);
            ResetCharacterInfoPanel(i, false);
        }
    }

    private void StartEndGameScreen()
    {
        state = State.endGameScreen;
        ClearAllGameObjects();
        endGamePanel = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/EndGamePanelPrefab.prefab", typeof(GameObject)), GameConstants.END_GAME_MENU_PANEL_POSITION, Quaternion.identity);
        SpriteRenderer[] spriteRenderers = {
            endGamePanel.transform.Find("SpriteLeft"  ).gameObject.GetComponent<SpriteRenderer>(),
            endGamePanel.transform.Find("SpriteMiddle").gameObject.GetComponent<SpriteRenderer>(),
            endGamePanel.transform.Find("SpriteRight" ).gameObject.GetComponent<SpriteRenderer>()
        };

        Text[] texts = endGamePanel.transform.Find("Canvas").GetComponentsInChildren<Text>();
        Text text = texts[0].name == "TextVictory" ? texts[0] : texts[1];

        endGameScreenTimer.Reset();

        // Humans lost
        if (numHumans == 0)
        {
            text.text = "Monsters Win!";
            spriteRenderers[1].sprite = playerInfo[newBossIndex].classInformation.prefab.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            text.text = "Humans Win!";
            int rendererIndex = 0;
            for (int i = 0; i < GameConstants.NUM_PLAYERS; ++i)
            {
                if (playerInfo[i].classInformation.isHumanClass)
                {
                    spriteRenderers[rendererIndex].sprite = playerInfo[i].classInformation.prefab.GetComponent<SpriteRenderer>().sprite;
                    ++rendererIndex;
                }
            }
        }
    }

    private void UpdateEndGameScreen()
    {
        Text[] texts = endGamePanel.transform.Find("Canvas").GetComponentsInChildren<Text>();
        Text text = texts[0].name == "TextEnterButton" ? texts[0] : texts[1];
        text.text = "";

        endGameScreenTimer.Update();
        if(endGameScreenTimer.done)
        {
            text.text = "Press Enter to Continue";
            if (Input.GetKeyDown(GameConstants.DEFAULT_GAME_START_KEY))
            {
                Destroy(endGamePanel);
                StartEnterGameMenu();
            }
        }
    }

    private void RegisterNewControllersAdded()
    {
        int i = 0;
        while (i < ControlSchemeHandler.controlSchemes.Length && nextPlayerToAddIndex < GameConstants.NUM_PLAYERS)
        {
            if (!ControlSchemeHandler.controlSchemes[i].isInUse && ControlSchemeHandler.controlSchemes[i].controller.GetAPressed())
            {
                playerInfo[nextPlayerToAddIndex] = new PlayerInformation(ControlSchemeHandler.controlSchemes[i].controller);
                ControlSchemeHandler.controlSchemes[i].isInUse = true;

                ++nextPlayerToAddIndex;
            }
            ++i;
        }
    }

    private void StartCharacterSelect()
    {
        state = State.characterSelect;
        startButton = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/StartButtonPrefab.prefab", typeof(GameObject)), GameConstants.START_BUTTON_POSITION, Quaternion.identity);
        startButton.SetActive(false);
        allowedHumanPlayerIndex = 0;
        playerInfo[0].isRealPlayer = true;

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

            Controller c;
            // Find the first person in the list who is a real player - they will control the start button
            int firstRealPlayerIndex = 0;
            while (firstRealPlayerIndex < GameConstants.NUM_PLAYERS && !playerInfo[firstRealPlayerIndex].isRealPlayer)
            {
                firstRealPlayerIndex++;
            }
            // If the players are all AI then use the default key to start game otherwise use the first available players key
            if (firstRealPlayerIndex == GameConstants.NUM_PLAYERS)
            {
                c = new KeyboardController(GameConstants.DEFAULT_GAME_START_KEY, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None);
            }
            else
            {
                c = playerInfo[firstRealPlayerIndex].controller;
            }

            bool startButtonActivated = false;
            if (c.Type() == Controller.ControllerType.Mouse)
            {
                if (c.GetAPressed())
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
                if (c.GetAPressed())
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
                    if (currentPlayer.controller.GetAPressed())
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
                else // The A press checks for gamepad and keyboard are handled the same
                {
                    bool changedClass = false;
                    Controller playerController = currentPlayer.controller;
                    if (currentPlayer.controller.Type() == Controller.ControllerType.Keyboard)
                    {
                        KeyboardController keyboardController = (KeyboardController)currentPlayer.controller;
                        if (Input.GetKeyDown(keyboardController.lKey))
                        {
                            currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + classes.Count - 1) % classes.Count;
                            changedClass = true;
                        }
                        else if (Input.GetKeyDown(keyboardController.rKey))
                        {
                            currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + 1) % classes.Count;
                            changedClass = true;
                        }
                        else if (Input.GetKeyDown(keyboardController.uKey))
                        {

                        }
                        else if (Input.GetKeyDown(keyboardController.dKey))
                        {

                        }
                    }
                    else if (currentPlayer.controller.Type() == Controller.ControllerType.Gamepad)
                    {
                        GamepadController gamepadController = (GamepadController)currentPlayer.controller;
                        Vector2 axis = gamepadController.GetAxisForSelection();
                        if (axis.x < -GameConstants.GAMEPAD_SELECTION_SENSITIVITY)
                        {
                            currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + classes.Count - 1) % classes.Count;
                            changedClass = true;
                        }
                        else if (axis.x > GameConstants.GAMEPAD_SELECTION_SENSITIVITY)
                        {
                            currentPlayer.classSelectionIndex = (currentPlayer.classSelectionIndex + 1) % classes.Count;
                            changedClass = true;
                        }
                        else if (axis.y > GameConstants.GAMEPAD_SELECTION_SENSITIVITY)
                        {

                        }
                        else if (axis.y < -GameConstants.GAMEPAD_SELECTION_SENSITIVITY)
                        {

                        }
                    }
                    
                    if (playerController.GetAPressed())
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
                // The B press is handled the same for every controller type
                if (currentPlayer.controller.GetBPressed() && !isNewHumanSelection)
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
                if (currentPlayer.controller.GetBPressed())
                {
                    currentPlayer.isReady = false;
                }
            }
        }
        else
        {
            if (currentPlayer.controller.GetAPressed())
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
            if (numMonsters == 1)
            {
                StartBossFight();
            }
            else
            {
                state = State.game;
                Regenerate();
            }
        }
    }

    private void NewHumanCharacterSelectionRegisterStartButtonInput()
    {
        PlayerInformation newHumanPlayer = playerInfo[newHumanIndex];
        if (newHumanPlayer.isReady)
        {
            startButton.SetActive(true);

            bool startButtonActivated = false;
            if (newHumanPlayer.controller.Type() == Controller.ControllerType.Mouse)
            {
                if (newHumanPlayer.controller.GetAPressed())
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
                if (newHumanPlayer.controller.GetAPressed())
                {
                    startButtonActivated = true;
                }
            }

            if (startButtonActivated)
            {
                Destroy(characterInfoPanels[newHumanIndex]);
                Destroy(startButton);
                ShowPlayers();

                if (numMonsters == 1)
                {
                    StartBossFight();
                }
                else
                {
                    state = State.game;
                    Regenerate();
                }
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
                currentPlayerInfo.character.GetComponent<PlayerController>().InitializePlayer(playerInfo[i].controller, i + 1);
            }
            // Spawn AI with controllers to support them
            else
            {
                if(AIControllers[i] != null) Destroy(AIControllers[i]);
                AIControllers[i] = Instantiate(AIControllerPrefabs[playerInfo[i].classSelectionIndex], new Vector3(-100, -100, -100), Quaternion.identity);
                AIControllers[i].GetComponent<AI>().Init(stageMap, currentPlayerInfo.character, i+1);
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