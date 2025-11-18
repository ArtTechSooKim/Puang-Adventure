using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Initializes DontDestroyOnLoad objects and loads the first game scene.
/// Place this script on a GameObject in the InitialScene.
/// InitialScene should ONLY contain persistent objects (Player, GameManager, Inventory, etc.)
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    [Header("First Scene to Load")]
    [Tooltip("Name of the first gameplay scene to load after initialization")]
    [SerializeField] private string firstSceneName = "02_VillageScene";

    [Header("Auto Load")]
    [Tooltip("Automatically load the first scene on Start")]
    [SerializeField] private bool autoLoadFirstScene = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        if (showDebugLogs)
            Debug.Log($"🚀 SceneInitializer: Initialization started in '{SceneManager.GetActiveScene().name}'");

        // All DontDestroyOnLoad objects in this scene will automatically persist
        // (They should have their own DontDestroyOnLoad logic in Awake())

        if (autoLoadFirstScene)
        {
            LoadFirstScene();
        }
    }

    /// <summary>
    /// Load the first gameplay scene
    /// </summary>
    public void LoadFirstScene()
    {
        if (string.IsNullOrEmpty(firstSceneName))
        {
            Debug.LogError("❌ SceneInitializer: First scene name is not set!");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(firstSceneName))
        {
            if (showDebugLogs)
                Debug.Log($"🎬 SceneInitializer: Loading first scene '{firstSceneName}'");

            SceneManager.LoadScene(firstSceneName);
        }
        else
        {
            Debug.LogError($"❌ SceneInitializer: Scene '{firstSceneName}' not found in Build Settings! Add it via File → Build Settings.");
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Load First Scene")]
    private void DebugLoadFirstScene()
    {
        LoadFirstScene();
    }
#endif
}
