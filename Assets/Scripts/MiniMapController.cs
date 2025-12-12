using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 각 씬마다 개별로 만들어지는 MiniMapPanel 컨트롤러
/// MiniMap은 원형으로 보이며, 플레이어가 중앙에 고정되고 맵이 스크롤됨
/// </summary>
public class MiniMapController : MonoBehaviour
{
    [Header("Scene Map Data")]
    [Tooltip("이 미니맵이 표시할 씬의 맵 데이터")]
    [SerializeField] private SceneMapData sceneMapData;

    [Header("UI References")]
    [Tooltip("MiniMap Image 컴포넌트 (원본 맵 이미지)")]
    [SerializeField] private Image miniMapImage;

    [Tooltip("MiniMapMask RectTransform (Mask 컴포넌트가 있는 부모)")]
    [SerializeField] private RectTransform miniMapMask;

    [Tooltip("MiniMapCase Image (유리 케이스, 도넛 모양 테두리)")]
    [SerializeField] private Image miniMapCase;

    [Tooltip("PlayerIsHere Image (중앙 고정 화살표)")]
    [SerializeField] private Image playerIsHere;

    [Tooltip("YouHaveToGoHere Image (퀘스트 목표 마커)")]
    [SerializeField] private Image youHaveToGoHere;

    [Header("MiniMap Settings")]
    [Tooltip("미니맵 반지름 (픽셀) - MiniMapMask 크기의 절반")]
    [SerializeField] private float miniMapRadius = 175f;

    [Tooltip("월드에서 보여줄 반경 (world units)")]
    [SerializeField] private float worldViewRadius = 15f;

    [Header("Player Reference")]
    [Tooltip("플레이어 Transform")]
    [SerializeField] private Transform playerTransform;

    [Header("Quest Marker")]
    [Tooltip("퀘스트 목표 위치 (월드 좌표)")]
    [SerializeField] private Vector3 questTargetWorldPosition;

    [Tooltip("퀘스트 마커 표시 여부")]
    [SerializeField] private bool showQuestMarker = false;

    // Runtime
    private RectTransform miniMapRect;
    private RectTransform playerMarkerRect;
    private RectTransform questMarkerRect;
    private float currentPlayerRotation;

    private void Start()
    {
        // 플레이어 자동 검색 (DontDestroyOnLoad로 유지되는 플레이어)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (miniMapImage != null)
        {
            miniMapRect = miniMapImage.rectTransform;

            // 맵 이미지 스프라이트 설정
            if (sceneMapData != null && sceneMapData.mapSprite != null)
            {
                miniMapImage.sprite = sceneMapData.mapSprite;
            }
        }

        if (playerIsHere != null)
        {
            playerMarkerRect = playerIsHere.rectTransform;
        }

        if (youHaveToGoHere != null)
        {
            questMarkerRect = youHaveToGoHere.rectTransform;
            youHaveToGoHere.gameObject.SetActive(showQuestMarker);
        }

        // MapPanelController에 이 컨트롤러 등록
        RegisterToMapPanel();
    }

    private void Update()
    {
        if (playerTransform == null || sceneMapData == null) return;

        UpdateMiniMap();

        if (showQuestMarker)
        {
            UpdateQuestMarker();
        }
    }

    private void LateUpdate()
    {
        if (playerTransform != null && playerMarkerRect != null)
        {
            UpdatePlayerRotation();
        }
    }

    /// <summary>
    /// 플레이어 위치에 따라 미니맵 스크롤
    /// </summary>
    private void UpdateMiniMap()
    {
        if (miniMapRect == null) return;

        // 플레이어 월드 좌표
        Vector3 playerWorldPos = playerTransform.position;

        // 월드 좌표 → Normalized 좌표 (0~1)
        Vector2 normalizedPos = sceneMapData.WorldToNormalized(playerWorldPos);

        // Calibration Points로부터 자동 계산된 월드 스케일 가져오기
        Vector2 worldScale = sceneMapData.GetWorldSize();

        // 월드 반경에 따른 줌 스케일 계산
        float scaleX = (worldViewRadius * 2f) / worldScale.x;
        float scaleY = (worldViewRadius * 2f) / worldScale.y;
        float scale = Mathf.Max(scaleX, scaleY);

        // 미니맵 이미지 크기 조정 (줌)
        float mapSize = miniMapRadius * 2f / scale;
        miniMapRect.sizeDelta = new Vector2(mapSize, mapSize);

        // 미니맵 이미지 위치 조정 (플레이어가 중앙에 오도록)
        Vector2 offset = new Vector2(
            (0.5f - normalizedPos.x) * mapSize,
            (0.5f - normalizedPos.y) * mapSize
        );
        miniMapRect.anchoredPosition = offset;
    }

    /// <summary>
    /// 플레이어 방향에 따라 화살표 회전
    /// </summary>
    private void UpdatePlayerRotation()
    {
        if (playerMarkerRect == null || playerTransform == null) return;

        // Animator에서 MoveX, MoveY 값 가져오기
        Animator animator = playerTransform.GetComponent<Animator>();
        if (animator != null)
        {
            float moveX = animator.GetFloat("MoveX");
            float moveY = animator.GetFloat("MoveY");

            if (moveX != 0 || moveY != 0)
            {
                float angle = Mathf.Atan2(moveY, moveX) * Mathf.Rad2Deg;
                currentPlayerRotation = angle;
            }
        }

        playerMarkerRect.localRotation = Quaternion.Euler(0f, 0f, currentPlayerRotation);
    }

    /// <summary>
    /// 퀘스트 목표 마커 위치 업데이트
    /// </summary>
    private void UpdateQuestMarker()
    {
        if (questMarkerRect == null || playerTransform == null) return;

        Vector3 playerWorldPos = playerTransform.position;

        // 퀘스트 목표까지의 상대 벡터
        Vector3 directionToQuest = questTargetWorldPosition - playerWorldPos;
        float distanceToQuest = directionToQuest.magnitude;

        // 거리가 worldViewRadius를 넘으면 경계에 고정
        if (distanceToQuest > worldViewRadius)
        {
            directionToQuest = directionToQuest.normalized * worldViewRadius;
        }

        // 상대 위치를 미니맵 UI 좌표로 변환
        float pixelPerWorldUnit = miniMapRadius / worldViewRadius;
        Vector2 markerPos = new Vector2(
            directionToQuest.x * pixelPerWorldUnit,
            directionToQuest.y * pixelPerWorldUnit
        );

        // 마커가 반지름 120을 넘지 않도록 클램프
        if (markerPos.magnitude > miniMapRadius)
        {
            markerPos = markerPos.normalized * miniMapRadius;
        }

        questMarkerRect.anchoredPosition = markerPos;
    }

    /// <summary>
    /// MapPanelController에 이 미니맵 컨트롤러 등록
    /// </summary>
    private void RegisterToMapPanel()
    {
        MapPanelController mapPanelController = FindObjectOfType<MapPanelController>();
        if (mapPanelController != null)
        {
            mapPanelController.SetActiveMiniMapController(this);
        }
    }

    #region Public Methods for MapPanelController

    /// <summary>
    /// 플레이어 월드 좌표 반환 (MapPanelController에서 호출)
    /// </summary>
    public Vector3 GetPlayerWorldPosition()
    {
        return playerTransform != null ? playerTransform.position : Vector3.zero;
    }

    /// <summary>
    /// 플레이어 회전 각도 반환 (MapPanelController에서 호출)
    /// </summary>
    public float GetPlayerRotation()
    {
        return currentPlayerRotation;
    }

    /// <summary>
    /// 퀘스트 목표 위치 설정 (외부에서 호출)
    /// </summary>
    public void SetQuestTarget(Vector3 worldPosition, bool show)
    {
        questTargetWorldPosition = worldPosition;
        showQuestMarker = show;

        if (youHaveToGoHere != null)
        {
            youHaveToGoHere.gameObject.SetActive(show);
        }
    }

    #endregion
}
