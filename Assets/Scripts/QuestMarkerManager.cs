using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 마커 관리자 v2.0
/// 스테이지별로 MiniMap에 여러 개의 목적지를 동시에 표시합니다.
/// </summary>
public class QuestMarkerManager : MonoBehaviour
{
    [Header("Scene Quest Markers")]
    [Tooltip("현재 씬의 퀘스트 목표 위치들 (스테이지별 최대 4개)")]
    [SerializeField] private List<QuestMarkerPoint> questMarkers = new List<QuestMarkerPoint>();

    [Header("Marker UI")]
    [Tooltip("퀘스트 마커 UI Prefab (YouHaveToGoHere)")]
    [SerializeField] private GameObject questMarkerPrefab;

    [Header("Settings")]
    [Tooltip("자동 업데이트 주기 (초)")]
    [SerializeField] private float updateInterval = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = false;

    private MiniMapController miniMapController;
    private QuestStage lastCheckedStage = QuestStage.Stage0_VillageTutorial;
    private float updateTimer = 0f;

    // 현재 활성화된 마커 UI 인스턴스들
    private List<MarkerInstance> activeMarkerInstances = new List<MarkerInstance>();

    private void Start()
    {
        miniMapController = FindObjectOfType<MiniMapController>();

        if (miniMapController == null)
        {
            Debug.LogWarning("⚠ QuestMarkerManager: MiniMapController를 찾을 수 없습니다!");
            return;
        }

        UpdateQuestMarkers();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            CheckForStageChange();
        }

        // 활성 마커들의 위치 업데이트
        UpdateMarkerPositions();
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
            UpdateQuestMarkers();
        }
    }

    /// <summary>
    /// 현재 스테이지에 맞는 퀘스트 마커들 표시
    /// </summary>
    private void UpdateQuestMarkers()
    {
        if (QuestManager.Instance == null || miniMapController == null)
            return;

        QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

        // 기존 마커들 제거
        ClearAllMarkers();

        // 현재 스테이지에 해당하는 마커들 찾기
        List<QuestMarkerPoint> activeMarkers = questMarkers.FindAll(m => m.questStage == currentStage);

        if (showDebugMessages)
            Debug.Log($"🎯 QuestMarkerManager: Stage {currentStage} - {activeMarkers.Count}개 마커 활성화");

        // 각 마커에 대해 UI 생성
        foreach (var marker in activeMarkers)
        {
            if (marker.markerTransform != null)
            {
                CreateMarkerUI(marker);
            }
        }
    }

    /// <summary>
    /// 마커 UI 생성
    /// </summary>
    private void CreateMarkerUI(QuestMarkerPoint markerData)
    {
        if (questMarkerPrefab == null)
        {
            Debug.LogWarning("⚠ QuestMarkerManager: questMarkerPrefab이 설정되지 않았습니다!");
            return;
        }

        // MiniMapController의 Mask 찾기
        Transform miniMapMask = miniMapController.transform.Find("MiniMapMask");
        if (miniMapMask == null)
        {
            Debug.LogWarning("⚠ QuestMarkerManager: MiniMapMask를 찾을 수 없습니다!");
            return;
        }

        // 마커 UI 생성
        GameObject markerObj = Instantiate(questMarkerPrefab, miniMapMask);
        RectTransform markerRect = markerObj.GetComponent<RectTransform>();

        if (markerRect != null)
        {
            MarkerInstance instance = new MarkerInstance
            {
                markerData = markerData,
                markerObject = markerObj,
                markerRect = markerRect
            };

            activeMarkerInstances.Add(instance);
            markerObj.SetActive(true);

            if (showDebugMessages)
                Debug.Log($"✅ 마커 생성: {markerData.markerName}");
        }
    }

    /// <summary>
    /// 모든 마커 제거
    /// </summary>
    private void ClearAllMarkers()
    {
        foreach (var instance in activeMarkerInstances)
        {
            if (instance.markerObject != null)
            {
                Destroy(instance.markerObject);
            }
        }
        activeMarkerInstances.Clear();

        if (showDebugMessages)
            Debug.Log("🗑 모든 마커 제거됨");
    }

    /// <summary>
    /// 활성 마커들의 위치 업데이트
    /// </summary>
    private void UpdateMarkerPositions()
    {
        if (miniMapController == null) return;

        Transform playerTransform = GetPlayerTransform();
        if (playerTransform == null) return;

        Vector3 playerWorldPos = playerTransform.position;

        foreach (var instance in activeMarkerInstances)
        {
            if (instance.markerRect != null && instance.markerData.markerTransform != null)
            {
                UpdateSingleMarker(instance, playerWorldPos);
            }
        }
    }

    /// <summary>
    /// 개별 마커 위치 업데이트
    /// 원 밖의 마커는 무조건 도넛 가장자리에 표시
    /// </summary>
    private void UpdateSingleMarker(MarkerInstance marker, Vector3 playerWorldPos)
    {
        if (miniMapController == null) return;

        Vector3 targetWorldPos = marker.markerData.markerTransform.position;
        Vector3 directionToQuest = targetWorldPos - playerWorldPos;

        // MiniMapController에서 설정값 가져오기
        float miniMapRadius = miniMapController.GetMiniMapRadius();
        float worldViewRadius = miniMapController.GetWorldViewRadius();

        // 상대 위치를 미니맵 UI 좌표로 변환
        float pixelPerWorldUnit = miniMapRadius / worldViewRadius;
        Vector2 markerPos = new Vector2(
            directionToQuest.x * pixelPerWorldUnit,
            directionToQuest.y * pixelPerWorldUnit
        );

        // 마커 위치가 원 안에 있는지 확인 (픽셀 거리 기준)
        float markerPixelDistance = markerPos.magnitude;
        bool isOutsideCircle = markerPixelDistance > miniMapRadius;

        // 원 밖에 있으면 무조건 도넛 가장자리(원의 경계)에 고정
        if (isOutsideCircle)
        {
            markerPos = markerPos.normalized * miniMapRadius;
        }

        marker.markerRect.anchoredPosition = markerPos;

        // 마커가 원 밖에 있을 때 방향 표시를 위해 회전
        if (isOutsideCircle)
        {
            float angle = Mathf.Atan2(directionToQuest.y, directionToQuest.x) * Mathf.Rad2Deg;
            marker.markerRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // 원 안에 있으면 회전 없음
            marker.markerRect.localRotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// 플레이어 Transform 가져오기
    /// </summary>
    private Transform GetPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    /// <summary>
    /// 특정 위치에 퀘스트 마커 수동 추가 (런타임)
    /// </summary>
    public void AddMarkerAtPosition(string markerName, Vector3 worldPosition, QuestStage stage)
    {
        // 임시 GameObject 생성
        GameObject tempMarker = new GameObject($"TempMarker_{markerName}");
        tempMarker.transform.position = worldPosition;

        QuestMarkerPoint newMarker = new QuestMarkerPoint
        {
            markerName = markerName,
            markerTransform = tempMarker.transform,
            questStage = stage
        };

        questMarkers.Add(newMarker);
        UpdateQuestMarkers();
    }

    /// <summary>
    /// 퀘스트 마커 숨기기
    /// </summary>
    public void HideAllMarkers()
    {
        ClearAllMarkers();
    }

    /// <summary>
    /// 현재 활성화된 마커 개수
    /// </summary>
    public int GetActiveMarkerCount()
    {
        return activeMarkerInstances.Count;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 씬에 있는 NPC, Portal, Item 등을 자동으로 퀘스트 마커로 추가
    /// </summary>
    [ContextMenu("Auto-Find Quest Markers")]
    private void AutoFindQuestMarkers()
    {
        questMarkers.Clear();

        // NPC 찾기
        NPCController[] npcs = FindObjectsOfType<NPCController>();
        foreach (var npc in npcs)
        {
            QuestMarkerPoint marker = new QuestMarkerPoint
            {
                markerName = "NPC: " + npc.name,
                markerTransform = npc.transform,
                questStage = QuestStage.Stage0_VillageTutorial
            };
            questMarkers.Add(marker);
        }

        // Portal 찾기
        PortalTrigger[] portals = FindObjectsOfType<PortalTrigger>();
        foreach (var portal in portals)
        {
            QuestMarkerPoint marker = new QuestMarkerPoint
            {
                markerName = "Portal: " + portal.name,
                markerTransform = portal.transform,
                questStage = QuestStage.Stage1_ForestHunt
            };
            questMarkers.Add(marker);
        }

        Debug.Log($"✅ {questMarkers.Count}개의 마커를 자동으로 찾았습니다.");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void OnDestroy()
    {
        ClearAllMarkers();
    }
}

/// <summary>
/// 퀘스트 마커 포인트 데이터
/// </summary>
[System.Serializable]
public class QuestMarkerPoint
{
    [Tooltip("마커 이름 (설명용)")]
    public string markerName;

    [Tooltip("마커 위치 (Transform) - 씬의 GameObject를 드래그")]
    public Transform markerTransform;

    [Tooltip("이 마커가 활성화될 퀘스트 스테이지")]
    public QuestStage questStage;
}

/// <summary>
/// 런타임 마커 인스턴스
/// </summary>
[System.Serializable]
public class MarkerInstance
{
    public QuestMarkerPoint markerData;
    public GameObject markerObject;
    public RectTransform markerRect;
}
