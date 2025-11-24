using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 특정 Stage에서 인벤토리 아이템 보유를 확인하고, 조건 충족 시 자동으로 Scene 전환합니다.
/// Stage1 (ForestScene): 슬라임 잔해2 + 박쥐 뼈2 → Village
/// Stage3 (CaveScene): 박쥐 뼈5 + 해골5 → PeuangSadScene
/// </summary>
public class StageCompletionTracker : MonoBehaviour
{
    public static StageCompletionTracker Instance { get; private set; }

    [Header("Stage 1 - Forest Requirements")]
    [SerializeField] private ItemData slimeResidueItem; // 슬라임 잔해
    [SerializeField] private ItemData batBoneItem;      // 박쥐 뼈
    [SerializeField] private int stage1_RequiredSlimeResidue = 2;
    [SerializeField] private int stage1_RequiredBatBone = 2;
    [SerializeField] private string stage1_TargetScene = "02_VillageScene";
    [SerializeField] private string stage1_SpawnPointName = "PortalToForest"; // 스폰될 오브젝트 이름

    [Header("Stage 3 - Cave Requirements")]
    [SerializeField] private ItemData skeletonBoneItem; // 해골 뼈
    // batBoneItem은 위에서 정의됨
    [SerializeField] private int stage3_RequiredBatBone = 5;
    [SerializeField] private int stage3_RequiredSkeletonBone = 5;
    [SerializeField] private string stage3_TargetScene = "05_PeuangSadScene";
    [SerializeField] private string stage3_SpawnPointName = ""; // 비어있으면 PlayerSpawn 태그 사용

    [Header("Check Settings")]
    [SerializeField] private float checkInterval = 1f; // 인벤토리 체크 간격 (초)
    [SerializeField] private float transitionDelay = 2f; // 대화 표시 후 전환까지 대기 시간
    [SerializeField] private bool showDebugMessages = true;

    private bool isTransitioning = false;
    private float checkTimer = 0f;

    private void Awake()
    {
        // Singleton pattern (Scene별로 새로 생성됨)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (showDebugMessages)
            Debug.Log($"✅ StageCompletionTracker initialized for Scene: {SceneManager.GetActiveScene().name}");
    }

    private void Start()
    {
        // 현재 Scene과 Stage에 맞는 목표 출력
        if (showDebugMessages)
        {
            QuestStage currentStage = QuestManager.Instance != null ? QuestManager.Instance.GetCurrentStage() : QuestStage.Stage0_VillageTutorial;
            Debug.Log($"🎯 Current Stage: {currentStage}");

            if (currentStage == QuestStage.Stage1_ForestHunt)
            {
                Debug.Log($"📋 Stage1 Goal: Slime Residue x{stage1_RequiredSlimeResidue}, Bat Bone x{stage1_RequiredBatBone}");
            }
            else if (currentStage == QuestStage.Stage3_CaveExploration)
            {
                Debug.Log($"📋 Stage3 Goal: Bat Bone x{stage3_RequiredBatBone}, Skeleton Bone x{stage3_RequiredSkeletonBone}");
            }
        }

        // 필요한 아이템이 설정되지 않았다면 경고
        if (slimeResidueItem == null || batBoneItem == null || skeletonBoneItem == null)
        {
            Debug.LogError("⚠️ StageCompletionTracker: Required ItemData not assigned in Inspector!");
        }
    }

    private void Update()
    {
        if (isTransitioning)
            return;

        // 일정 간격으로 인벤토리 체크
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckInventoryCompletion();
        }
    }

    /// <summary>
    /// 인벤토리에 필요한 아이템이 있는지 확인
    /// </summary>
    private void CheckInventoryCompletion()
    {
        if (Inventory.instance == null)
        {
            if (showDebugMessages)
                Debug.LogWarning("⚠️ StageCompletionTracker: Inventory.instance is null!");
            return;
        }

        if (QuestManager.Instance == null)
        {
            if (showDebugMessages)
                Debug.LogWarning("⚠️ StageCompletionTracker: QuestManager.Instance is null!");
            return;
        }

        QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

        bool isCompleted = false;
        string targetScene = "";
        string targetSpawnPoint = "";

        // Stage1: ForestScene - 슬라임 잔해2 + 박쥐 뼈2
        if (currentStage == QuestStage.Stage1_ForestHunt)
        {
            if (slimeResidueItem == null || batBoneItem == null)
            {
                if (showDebugMessages)
                    Debug.LogError("❌ StageCompletionTracker: slimeResidueItem or batBoneItem is not assigned in Inspector!");
                return;
            }

            int slimeCount = CountItemInInventory(slimeResidueItem);
            int batCount = CountItemInInventory(batBoneItem);

            if (showDebugMessages && (slimeCount > 0 || batCount > 0))
            {
                Debug.Log($"📦 Inventory Check - Slime: {slimeCount}/{stage1_RequiredSlimeResidue}, Bat: {batCount}/{stage1_RequiredBatBone}");
            }

            if (slimeCount >= stage1_RequiredSlimeResidue && batCount >= stage1_RequiredBatBone)
            {
                isCompleted = true;
                targetScene = stage1_TargetScene;
                targetSpawnPoint = stage1_SpawnPointName;

                if (showDebugMessages)
                    Debug.Log("🎉 Stage1 목표 달성! Village로 복귀합니다.");
            }
        }
        // Stage3: CaveScene - 박쥐 뼈5 + 해골5
        else if (currentStage == QuestStage.Stage3_CaveExploration)
        {
            if (batBoneItem == null || skeletonBoneItem == null)
            {
                if (showDebugMessages)
                    Debug.LogError("❌ StageCompletionTracker: batBoneItem or skeletonBoneItem is not assigned in Inspector!");
                return;
            }

            int batCount = CountItemInInventory(batBoneItem);
            int skeletonCount = CountItemInInventory(skeletonBoneItem);

            if (showDebugMessages && (batCount > 0 || skeletonCount > 0))
            {
                Debug.Log($"📦 Inventory Check - Bat: {batCount}/{stage3_RequiredBatBone}, Skeleton: {skeletonCount}/{stage3_RequiredSkeletonBone}");
            }

            if (batCount >= stage3_RequiredBatBone && skeletonCount >= stage3_RequiredSkeletonBone)
            {
                isCompleted = true;
                targetScene = stage3_TargetScene;
                targetSpawnPoint = stage3_SpawnPointName;

                if (showDebugMessages)
                    Debug.Log("🎉 Stage3 목표 달성! PeuangSadScene으로 이동합니다.");
            }
        }

        if (isCompleted)
        {
            StartCoroutine(TransitionToNextScene(targetScene, targetSpawnPoint));
        }
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템 개수 세기
    /// </summary>
    private int CountItemInInventory(ItemData targetItem)
    {
        if (targetItem == null || Inventory.instance == null)
            return 0;

        int count = 0;

        // Hotbar + Inventory 모두 확인 (items는 ItemData 배열)
        foreach (ItemData item in Inventory.instance.items)
        {
            if (item != null && item.itemID == targetItem.itemID)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 다음 Scene으로 전환
    /// </summary>
    private IEnumerator TransitionToNextScene(string targetScene, string spawnPointName = "")
    {
        isTransitioning = true;

        // 대화 표시
        if (DialogueManager.Instance != null)
        {
            string message = GetCompletionMessage();
            DialogueManager.Instance.StartDialogue(new System.Collections.Generic.List<string> { message });
        }

        // 대화가 끝날 때까지 대기
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            yield return null;
        }

        // 대화가 끝난 후 Quest Stage 자동 전환
        AdvanceQuestStage();

        // 짧은 딜레이 (스테이지 변경 후)
        yield return new WaitForSeconds(0.5f);

        // 스폰 포인트가 지정되었으면 PlayerPersistent에 저장
        if (!string.IsNullOrEmpty(spawnPointName))
        {
            // Scene 로드 후 특정 오브젝트 위치로 이동하도록 표시
            PlayerPrefs.SetString("TargetSpawnPoint", spawnPointName);

            if (showDebugMessages)
                Debug.Log($"🎯 Target spawn point set: {spawnPointName}");
        }
        else
        {
            // 스폰 포인트 미지정 시 PlayerSpawn 태그 사용
            PlayerPrefs.DeleteKey("TargetSpawnPoint");
        }

        // Scene 전환
        if (showDebugMessages)
            Debug.Log($"🌀 Transitioning to Scene: {targetScene}");

        SceneManager.LoadScene(targetScene);
    }

    /// <summary>
    /// Quest Stage를 다음 단계로 진행
    /// </summary>
    private void AdvanceQuestStage()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("⚠️ QuestManager not found - cannot advance stage");
            return;
        }

        QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

        // Stage1 완료 → Stage2로 전환
        if (currentStage == QuestStage.Stage1_ForestHunt)
        {
            QuestManager.Instance.SetStage(QuestStage.Stage2_WeaponUpgrade1);
            if (showDebugMessages)
                Debug.Log("📈 Quest Stage advanced: Stage1 → Stage2");
        }
        // Stage3 완료 → Stage4로 전환
        else if (currentStage == QuestStage.Stage3_CaveExploration)
        {
            QuestManager.Instance.SetStage(QuestStage.Stage4_PeuangSadCutscene);
            if (showDebugMessages)
                Debug.Log("📈 Quest Stage advanced: Stage3 → Stage4");
        }
    }

    /// <summary>
    /// 완료 메시지 가져오기
    /// </summary>
    private string GetCompletionMessage()
    {
        QuestStage currentStage = QuestManager.Instance != null ? QuestManager.Instance.GetCurrentStage() : QuestStage.Stage0_VillageTutorial;

        if (currentStage == QuestStage.Stage1_ForestHunt)
        {
            return "\"누가 쓰다 버린 칼이라 그런가.. 많이 무딘것 같아. 마을에 가서 무기를 강화시키자.\"";
        }
        else if (currentStage == QuestStage.Stage3_CaveExploration)
        {
            return "\"머리가 어지러워.. 사냥을 너무 많이 했나... 잠깐 쉬도록 하자..\"";
        }

        return "목표를 달성했습니다!";
    }

    /// <summary>
    /// 디버그: 현재 인벤토리 아이템 출력
    /// </summary>
    [ContextMenu("Debug: Print Inventory")]
    private void DebugPrintInventory()
    {
        if (Inventory.instance == null)
        {
            Debug.LogWarning("⚠️ Inventory not found!");
            return;
        }

        Debug.Log($"=== Inventory Contents ===");
        Debug.Log($"Slime Residue: {CountItemInInventory(slimeResidueItem)}");
        Debug.Log($"Bat Bone: {CountItemInInventory(batBoneItem)}");
        Debug.Log($"Skeleton Bone: {CountItemInInventory(skeletonBoneItem)}");
    }

    /// <summary>
    /// 디버그: 목표 강제 달성 (아이템 추가)
    /// </summary>
    [ContextMenu("Debug: Give Required Items")]
    private void DebugGiveRequiredItems()
    {
        if (Inventory.instance == null)
        {
            Debug.LogWarning("⚠️ Inventory not found!");
            return;
        }

        QuestStage currentStage = QuestManager.Instance != null ? QuestManager.Instance.GetCurrentStage() : QuestStage.Stage0_VillageTutorial;

        if (currentStage == QuestStage.Stage1_ForestHunt)
        {
            for (int i = 0; i < stage1_RequiredSlimeResidue; i++)
                Inventory.instance.AddItem(slimeResidueItem);
            for (int i = 0; i < stage1_RequiredBatBone; i++)
                Inventory.instance.AddItem(batBoneItem);
            Debug.Log("🔧 Stage1 아이템 추가 완료!");
        }
        else if (currentStage == QuestStage.Stage3_CaveExploration)
        {
            for (int i = 0; i < stage3_RequiredBatBone; i++)
                Inventory.instance.AddItem(batBoneItem);
            for (int i = 0; i < stage3_RequiredSkeletonBone; i++)
                Inventory.instance.AddItem(skeletonBoneItem);
            Debug.Log("🔧 Stage3 아이템 추가 완료!");
        }
    }
}
