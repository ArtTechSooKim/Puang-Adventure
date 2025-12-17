using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 안내 패널
/// 현재 스테이지에 따라 플레이어에게 다음 목표를 안내합니다.
/// </summary>
public class QuestGuiderPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("퀘스트 안내 텍스트")]
    [SerializeField] private TextMeshProUGUI questGuideText;

    [Tooltip("퀘스트 제목 텍스트")]
    [SerializeField] private TextMeshProUGUI questTitleText;

    [Tooltip("숨길 대상 GameObject (패널 자체가 아닌 특정 Prefab)")]
    [SerializeField] private GameObject targetToHide;

    [Header("Settings")]
    [Tooltip("자동 업데이트 주기 (초)")]
    [SerializeField] private float updateInterval = 1f;

    [Header("Auto-Hide Conditions")]
    [Tooltip("칼자루 획득 시 패널 숨김 (Stage0 → Stage1)")]
    [SerializeField] private bool hideOnSwordHandleAcquired = true;

    [Tooltip("슬라임2 + 박쥐2 처치 시 패널 숨김 (Stage1 → Stage2)")]
    [SerializeField] private bool hideOnForestEnemiesKilled = true;

    [Tooltip("박쥐5 + 해골5 처치 시 패널 숨김 (Stage3 → Stage4)")]
    [SerializeField] private bool hideOnCaveEnemiesKilled = true;

    private QuestStage lastCheckedStage = QuestStage.Stage0_VillageTutorial;
    private float updateTimer = 0f;

    // 스테이지별 퀘스트 안내 데이터
    private Dictionary<QuestStage, QuestGuideData> questGuides = new Dictionary<QuestStage, QuestGuideData>()
    {
        {
            QuestStage.Stage0_VillageTutorial,
            new QuestGuideData(
                "프롤로그: 칼자루 획득",
                "• 푸앙이와 대화하세요(E키로대화)\n• 칼자루를 찾으세요\n• 무기를 장착하고 숲을 탐험하세요"
            )
        },
        {
            QuestStage.Stage1_ForestHunt,
            new QuestGuideData(
                "1장: 숲의 시련",
                "• 숲으로 이동하세요 (포털 찾기)\n• 슬라임 2마리를 처치하세요\n• 박쥐 2마리를 처치하세요"
            )
        },
        {
            QuestStage.Stage2_WeaponUpgrade1,
            new QuestGuideData(
                "2장: 무기 강화 I",
                "• 마을의 대장장이를 찾아가세요\n• 숲의 검으로 무기를 강화하세요\n• 더 강한 전투를 준비하세요"
            )
        },
        {
            QuestStage.Stage3_CaveExploration,
            new QuestGuideData(
                "3장: 동굴 탐험",
                "• Space 누르면 대시합니다.\n동굴로 이동하세요 (포털 이용)\n• 박쥐 5마리를 처치하세요\n• 해골 5마리를 처치하세요"
            )
        },
        {
            QuestStage.Stage4_PeuangSadCutscene,
            new QuestGuideData(
                "4장: 퓨앙이의 슬픔",
                ""
            )
        },
        {
            QuestStage.Stage5_UnkillableBoss,
            new QuestGuideData(
                "5장: 거대 버섯의 위협",
                "• 거대 버섯 보스와 조우"
            )
        },
        {
            QuestStage.Stage6_WeaponUpgrade2,
            new QuestGuideData(
                "6장: 무기 강화 II",
                "• 중붕이 NPC를 찾아가세요\n• 중붕이의 검으로 무기를 강화하세요\n• 최종 전투를 준비하세요"
            )
        },
        {
            QuestStage.Stage7_FinalBoss,
            new QuestGuideData(
                "7장: 최종 결전",
                "• R누르면 궁극기 사용합니다.\n숲에 보스 방을 찾아보세요\n• 거대 버섯 보스를 처치하세요\n• 처치후 고기를 획득하세요\n• 푸앙이에게 고기를 주세요"
            )
        },
        {
            QuestStage.Stage8_Ending,
            new QuestGuideData(
                "엔딩",
                ""
            )
        }
    };

    private void Start()
    {
        UpdateQuestGuide();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            CheckForStageChange();
            CheckAutoHideConditions();
        }
    }

    /// <summary>
    /// 자동 숨김 조건 확인
    /// </summary>
    private void CheckAutoHideConditions()
    {
        if (QuestManager.Instance == null) return;

        QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

        // 조건 1: 칼자루 획득 (Stage0 완료 → Stage1로 진입)
        if (hideOnSwordHandleAcquired && currentStage >= QuestStage.Stage1_ForestHunt)
        {
            HidePanel();
            return;
        }

        // 조건 2: 슬라임2 + 박쥐2 처치 (Stage1 완료 → Stage2로 진입)
        if (hideOnForestEnemiesKilled && currentStage >= QuestStage.Stage2_WeaponUpgrade1)
        {
            HidePanel();
            return;
        }

        // 조건 3: 박쥐5 + 해골5 처치 (Stage3 완료 → Stage4로 진입)
        if (hideOnCaveEnemiesKilled && currentStage >= QuestStage.Stage4_PeuangSadCutscene)
        {
            HidePanel();
            return;
        }
    }

    /// <summary>
    /// Prefab 숨기기 (QuestGuiderPanel 자체는 유지)
    /// </summary>
    private void HidePanel()
    {
        if (targetToHide != null)
        {
            targetToHide.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠ QuestGuiderPanel: targetToHide가 설정되지 않았습니다! Inspector에서 숨길 GameObject를 할당하세요.");
        }
    }

    /// <summary>
    /// Prefab 다시 보이기
    /// </summary>
    public void ShowPanel()
    {
        if (targetToHide != null)
        {
            targetToHide.SetActive(true);
        }
    }

    /// <summary>
    /// 스테이지 변경 확인
    /// </summary>
    private void CheckForStageChange()
    {
        if (QuestManager.Instance == null)
            return;

        QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

        if (currentStage != lastCheckedStage)
        {
            lastCheckedStage = currentStage;
            UpdateQuestGuide();
        }
    }

    /// <summary>
    /// 퀘스트 안내 업데이트
    /// </summary>
    private void UpdateQuestGuide()
    {
        if (QuestManager.Instance == null)
            return;

        QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

        if (questGuides.TryGetValue(currentStage, out QuestGuideData guideData))
        {
            if (questTitleText != null)
            {
                questTitleText.text = guideData.title;
            }

            if (questGuideText != null)
            {
                questGuideText.text = guideData.description;
            }
        }
        else
        {
            if (questTitleText != null)
            {
                questTitleText.text = "진행 중";
            }

            if (questGuideText != null)
            {
                questGuideText.text = "다음 목표를 찾아보세요.";
            }
        }
    }

    /// <summary>
    /// 수동으로 퀘스트 가이드 갱신
    /// </summary>
    public void RefreshGuide()
    {
        UpdateQuestGuide();
    }
}

/// <summary>
/// 퀘스트 안내 데이터
/// </summary>
[System.Serializable]
public class QuestGuideData
{
    public string title;
    public string description;

    public QuestGuideData(string title, string description)
    {
        this.title = title;
        this.description = description;
    }
}
