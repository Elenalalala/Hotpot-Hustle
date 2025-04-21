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

    public HapticImpulsePlayer rightController;
    public AudioSource sfxSource;

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
    }

    public void EndGame(bool win)
    {
        Debug.Log(win ? "You Win!" : "You Lose!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
