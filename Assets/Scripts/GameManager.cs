using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    public HapticImpulsePlayer leftController;
    public AudioSource sfxSource;
    private AudioSource backgroundSource;
    public AudioClip backgroundMusic;

    public GAME_STATE state;

    public InputActionProperty rightGripAction;
    public InputActionProperty rightHoldAction;

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
        backgroundSource = gameObject.AddComponent<AudioSource>();
        backgroundSource.clip = backgroundMusic;
        backgroundSource.loop = true;
        backgroundSource.volume = 0.4f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        state = GAME_STATE.PLAYING;
        mistakeTracker.Reset();
        progressTracker.Reset();
        foodRequestSystem.Initialize();
        ingredientManager.Initialize();
        aiManager.Initialize();
        potManager.Initialize();
        uiManager.Initialize();
        streakSystem.Initialize();
        waterManager.Initialize();
        backgroundSource.Play();
    }


    public void EndGame(bool win)
    {
        if (state != GAME_STATE.PLAYING)
        {
            return;
        }
        StopGameProcess();
        if (win)
        {
            foreach (FoodRequestOwner owner in foodRequestSystem.requestOwners)
            {
                owner.SwitchMaterial(RELATIVE_MAT_STATUS.HAPPY);
            }
            aiManager.SwitchMaterial(COUSIN_MAT_STATUS.WIN);
            sceneManager.WinGame();
        }
        else
        {
            foreach (FoodRequestOwner owner in foodRequestSystem.requestOwners)
            {
                owner.SwitchMaterial(RELATIVE_MAT_STATUS.IMPATIENT);
            }
            aiManager.SwitchMaterial(COUSIN_MAT_STATUS.LOSE);
            sceneManager.EndGame();
        }
        state = win ? GAME_STATE.WON : GAME_STATE.LOST;
    }

    private void StopGameProcess()
    {
        aiManager.StopAllCoroutines();
        foodRequestSystem.StopAllCoroutines();
        ingredientManager.StopAllCoroutines();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GAME_STATE.PLAYING) return;
        float grip1 = rightHoldAction.action.ReadValue<float>();
        float grip2 = rightGripAction.action.ReadValue<float>();
        if (grip1 > 0.5f && grip2 > 0.5f)
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
