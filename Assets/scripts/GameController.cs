using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class GameController : MonoBehaviour
{
   
    // Start is called before the first frame update
    public static GameController instance;
    public GameObject[] basketPrefabs; // 5 basket prefabs
    public List<GameObject> currentBaskets = new List<GameObject>();
    public Transform[] basketPositions;
    public Canvas mainCanvas;
    public GameObject gardenerPrefeb;
    public Transform gardenerPosition;
    public Button resumeBtn;
    public Button restartBtn;
    public GameObject gameOver;
    public GameObject onPause;
    GameObject gardenerGameObj;
    private float gameDuration;
    public Text timertxt;
    public Text scoreTxt;
    public Text messageTxt;

    public bool isGameStarted { get; private set; } = false;
    public bool isGameFinished { get; private set; } = false;
    public bool isGamePaused { get; private set; } = false;
    public bool isSuccess { get; private set; } = false;
    public bool isFailure { get; private set; } = false;

    private float eventDelayTimer = 0f, gameSpeed;
    //private bool runOnce = false;

    // Game score related variables.
    public int nTargets = 0;
    public int nSuccess = 0;
    public int nFailure = 0;
    public enum GameStates
    {
        WAITFORSTART,
        STARTGAME,
        SPAWNFRUIT,
        MOVE,
        FAILURE,
        SUCCESS,
        PAUSE,
        STOP,
        NONE
    }
    private GameStates previosState;
    private GameStates gameState = GameStates.WAITFORSTART;
  
    public void setGameState(GameStates state)
    {
        gameState = state;
    }
    void Start()
    {
     instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
        {
            
            if (gameState == GameStates.WAITFORSTART) isGameStarted = true;
            else if (gameState != GameStates.STOP) isGamePaused = !isGamePaused;

        }
        onPause.gameObject.SetActive(isGamePaused);
        gameOver.gameObject.SetActive(gameState == GameStates.STOP);
        resumeBtn.gameObject.SetActive(isGamePaused);
        restartBtn.gameObject.SetActive(gameState == GameStates.STOP);
        if (isGamePaused && gameState != GameStates.PAUSE) pauseGame();
        else if (!isGamePaused && gameState == GameStates.PAUSE) resumeGame();

        messageTxt.text = (gameState == GameStates.WAITFORSTART)
                        ? "Press ctrl+Y To start Game"
                        : "";

        timertxt.text = $"Timer :{gameDuration.ToString("F0")}s";
        scoreTxt.text = nSuccess.ToString();
        Debug.Log(gameState);
      

    }
    public void FixedUpdate()
    {
        runStateMachine();
    }
    public void runStateMachine()
    {
        if (isGamePlayeing()) gameDuration -= Time.deltaTime;
        bool isTimeUp = gameDuration < 0;
        switch (gameState)
        {
            case GameStates.WAITFORSTART:
                if (isGameStarted) gameState = GameStates.STARTGAME;
                break;
            case GameStates.STARTGAME:
                startGame();
                break;
            case GameStates.SPAWNFRUIT:
                FruitSpawner.instance.spawnFruit();
                nTargets++;
                gameState = GameStates.MOVE;
                break;
            case GameStates.MOVE:
                if (isSuccess) gameState = GameStates.SUCCESS;
                if (isFailure) gameState = GameStates.FAILURE;
                break;

            case GameStates.PAUSE:
                Debug.Log(isGamePaused);
                break;
            case GameStates.SUCCESS:
            case GameStates.FAILURE:
                if (eventDelayTimer <= 0f)
                {
                    eventDelayTimer = 0.05f;
                }
                else
                {
                    eventDelayTimer -= Time.deltaTime;
                    if (eventDelayTimer <= 0f)
                    {
                        // Wait for the gamestate to be logged.
                        isFailure = false;
                        isSuccess = false;
                        gameState = isTimeUp ? GameStates.STOP : GameStates.SPAWNFRUIT;
                        //runOnce = false;
                    }
                }
                break;
            case GameStates.STOP:
                if (!gardener.instance.IsGardenerCollecting)
                {
                    //make the gardener visible and start collect the missed fruits
                    
                    gardener.instance.gardenerprefeb.gameObject.SetActive(true);
                    gardener.instance.gardenerprefeb.transform.SetAsLastSibling();

                    gardener.instance.StartGardenerCollecting();
                }
                endGame();
                break;
        }
    }
    public bool isGamePlayeing()
    {
        return gameState != GameStates.WAITFORSTART &&
                gameState != GameStates.PAUSE&&
                gameState != GameStates.STOP;
    }
   
    public void startGame()
    {
        setupBasketsForTrial();
        nTargets = 0;
        nSuccess = 0;
        nFailure = 0;
        gameDuration = 60f;
       
        gameState = GameStates.SPAWNFRUIT;
        //start trial
    }
    public void endGame()
    {
       
        isGameFinished = true;
        //stop trail

    }
    public void pauseGame()
    {
        previosState = gameState;
        gameState = GameStates.PAUSE;
        Time.timeScale = 0f;
    }
    public void resumeGame()
    {
        gameState = previosState;
        Time.timeScale = 1f;
        isGamePaused = false;
    }
    public void setSuccess()
    {
        isSuccess  = true;
        nSuccess++;
    }
    public void setFailure()
    {
        isFailure = true;
        nFailure++;
    }
  
    public void restart()
    {
        isGameFinished = false;
        isGameStarted = false;
        gameState = GameStates.WAITFORSTART;
        Destroy(gardenerGameObj);
    }

    public void setupBasketsForTrial()
    {
        // Destroy old baskets if any
        foreach (var basket in currentBaskets)
        {
            if (basket != null) Destroy(basket);
        }
        currentBaskets.Clear();

        // Shuffle positions
        List<Transform> shuffledPositions = new List<Transform>(basketPositions);
        for (int i = 0; i < shuffledPositions.Count; i++)
        {
            Transform temp = shuffledPositions[i];
            int randomIndex = Random.Range(i, shuffledPositions.Count);
            shuffledPositions[i] = shuffledPositions[randomIndex];
            shuffledPositions[randomIndex] = temp;
        }

        // Spawn baskets at shuffled positions
        for (int i = 0; i < basketPrefabs.Length; i++)
        {
            GameObject basket = Instantiate(basketPrefabs[i], shuffledPositions[i].position, Quaternion.identity, mainCanvas.transform);
            basket.transform.localScale = Vector3.one; // Maintain proper UI scaling
            currentBaskets.Add(basket);
        }
        gardenerGameObj = Instantiate(gardenerPrefeb, gardenerPosition.position, Quaternion.identity, mainCanvas.transform);
       
       
        FruitSpawner.instance.targetPrefebs = currentBaskets.ToArray();
    }

}
