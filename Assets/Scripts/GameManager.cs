using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public MistakeTracker mistakeTracker;
    public ProgressTracker progressTracker;
    public FoodRequestSystem foodRequestSystem;
    public IngredientManager ingredientManager;
    public AICousinManager aiManager;
    public UIManager uiManager;
    public PotManager potManager;
    public StreakSystem streakSystem;
    public WinLoseSceneManager sceneManager;
    public WaterManager waterManager;

    public HapticImpulsePlayer rightController;
    public AudioSource sfxSource;

    public GAME_STATE state;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        sfxSource = gameObject.AddComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
        state = GAME_STATE.PLAYING;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        mistakeTracker.Reset();
        progressTracker.Reset();
        foodRequestSystem.Initialize();
        ingredientManager.Initialize();
        aiManager.Initialize();
        potManager.Initialize();
        uiManager.Initialize();
        streakSystem.Initialize();
    }


    public void EndGame(bool win)
    {
        if (state != GAME_STATE.PLAYING)
        {
            return;
        }
        Debug.Log(win ? "You Win!" : "You Lose!");
        state = win ? GAME_STATE.WON : GAME_STATE.LOST;

        if (win)
        {
            sceneManager.WinGame();
        }
        else
        {
            sceneManager.EndGame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
