using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI_MasterPanel의 MapPanel을 제어하는 컨트롤러
/// Tab 키를 누르면 현재 씬의 맵과 플레이어 위치를 표시
/// </summary>
public class MapPanelController : MonoBehaviour
{
    [Header("Scene Map Data")]
    [Tooltip("모든 씬의 맵 데이터 목록")]
    [SerializeField] private List<SceneMapData> sceneMapDataList;

    [Header("UI References")]
    [Tooltip("MapPanel 안의 Map Image 컴포넌트")]
    [SerializeField] private Image mapImage;

    [Tooltip("MapPanel의 RectTransform (크기 제한용)")]
    [SerializeField] private RectTransform mapPanelRect;

    [Header("Player Marker")]
    [Tooltip("플레이어 위치 표시 Prefab")]
    [SerializeField] private GameObject playerMarkerPrefab;

    [Header("Map Size Constraints")]
    [Tooltip("맵 이미지 최대 가로 크기")]
    [SerializeField] private float maxWidth = 1300f;

    [Tooltip("맵 이미지 최대 세로 크기")]
    [SerializeField] private float maxHeight = 500f;

    [Header("MiniMap Reference")]
    [Tooltip("미니맵 컨트롤러 (플레이어 위치 정보 가져오기용)")]
    [SerializeField] private MiniMapController activeMiniMapController;

    // Runtime
    private SceneMapData currentSceneData;
    private GameObject playerMarkerInstance;
    private RectTransform playerMarkerRect;

    private void Start()
    {
        // 시작 시 플레이어 마커 생성 준비 (아직 활성화하지 않음)
        if (playerMarkerPrefab != null && mapImage != null)
        {
            playerMarkerInstance = Instantiate(playerMarkerPrefab, mapImage.transform.parent);
            playerMarkerRect = playerMarkerInstance.GetComponent<RectTransform>();
            playerMarkerInstance.SetActive(false); // 맵이 열릴 때까지 숨김
        }
    }

    private void OnEnable()
    {
        // MapPanel이 활성화될 때마다 맵 업데이트
        OnMapPanelOpened();
    }

    private void OnDisable()
    {
        // MapPanel이 비활성화될 때
        OnMapPanelClosed();
    }

    /// <summary>
    /// MapPanel이 열릴 때 호출 (Tab 키를 눌렀을 때)
    /// </summary>
    public void OnMapPanelOpened()
    {
        // activeMiniMapController가 없으면 자동으로 찾기
        if (activeMiniMapController == null)
        {
            activeMiniMapController = FindObjectOfType<MiniMapController>();
            if (activeMiniMapController != null)
            {
                Debug.Log($"[MapPanelController] MiniMapController 자동 검색 성공: {activeMiniMapController.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[MapPanelController] MiniMapController를 찾을 수 없습니다!");
            }
        }

        LoadCurrentSceneMap();
        UpdatePlayerMarker();
    }

    /// <summary>
    /// 현재 씬의 맵 데이터를 로드하고 표시
    /// </summary>
    private void LoadCurrentSceneMap()
    {
        // 먼저 MiniMapController에서 SceneMapData 가져오기 시도
        if (activeMiniMapController != null)
        {
            currentSceneData = activeMiniMapController.GetSceneMapData();
            if (currentSceneData != null)
            {
                Debug.Log($"[MapPanelController] MiniMapController에서 SceneMapData 로드 성공: {currentSceneData.sceneName}");
            }
        }

        // MiniMapController에서 못 가져왔으면 sceneMapDataList에서 검색
        if (currentSceneData == null && sceneMapDataList != null && sceneMapDataList.Count > 0)
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            currentSceneData = sceneMapDataList.Find(data => data.sceneName == currentSceneName);

            if (currentSceneData != null)
            {
                Debug.Log($"[MapPanelController] sceneMapDataList에서 SceneMapData 로드 성공: {currentSceneData.sceneName}");
            }
        }

        if (currentSceneData == null)
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.LogWarning($"[MapPanelController] SceneMapData not found for scene: {currentSceneName}");
            return;
        }

        if (mapImage == null)
        {
            Debug.LogError("[MapPanelController] mapImage is not assigned!");
            return;
        }

        // 맵 이미지 설정
        mapImage.sprite = currentSceneData.mapSprite;

        // 비율 맞춰서 크기 조정
        if (currentSceneData.mapSprite != null)
        {
            ResizeMapImage(currentSceneData.mapSprite);
        }
    }

    /// <summary>
    /// 맵 이미지를 maxWidth x maxHeight 안에서 비율을 유지하며 최대한 크게 조정
    /// </summary>
    private void ResizeMapImage(Sprite sprite)
    {
        if (mapImage.rectTransform == null) return;

        float spriteWidth = sprite.rect.width;
        float spriteHeight = sprite.rect.height;
        float spriteAspect = spriteWidth / spriteHeight;

        float targetWidth, targetHeight;

        // 가로 기준으로 맞추기
        if (spriteWidth / maxWidth > spriteHeight / maxHeight)
        {
            targetWidth = maxWidth;
            targetHeight = maxWidth / spriteAspect;
        }
        // 세로 기준으로 맞추기
        else
        {
            targetHeight = maxHeight;
            targetWidth = maxHeight * spriteAspect;
        }

        mapImage.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

        Debug.Log($"[MapPanelController] Map resized to {targetWidth} x {targetHeight} (Original: {spriteWidth} x {spriteHeight})");
    }

    /// <summary>
    /// 미니맵에서 플레이어 위치와 방향 정보를 가져와서 맵 패널에 표시
    /// </summary>
    private void UpdatePlayerMarker()
    {
        if (currentSceneData == null || playerMarkerInstance == null || playerMarkerRect == null)
            return;

        if (activeMiniMapController == null)
        {
            Debug.LogWarning("[MapPanelController] activeMiniMapController is not assigned!");
            return;
        }

        // 미니맵에서 플레이어 월드 좌표 가져오기
        Vector3 playerWorldPos = activeMiniMapController.GetPlayerWorldPosition();

        // 월드 좌표 → Normalized 좌표 (0~1)
        Vector2 normalizedPos = currentSceneData.WorldToNormalized(playerWorldPos);

        // Normalized 좌표 → 맵 이미지 UI 좌표
        Vector2 mapSize = mapImage.rectTransform.sizeDelta;
        Vector2 localPos = new Vector2(
            (normalizedPos.x - 0.5f) * mapSize.x,
            (normalizedPos.y - 0.5f) * mapSize.y
        );

        playerMarkerRect.anchoredPosition = localPos;

        // 플레이어 방향 (회전) 가져오기
        float playerRotation = activeMiniMapController.GetPlayerRotation();
        playerMarkerRect.rotation = Quaternion.Euler(0f, 0f, playerRotation);

        // 마커 활성화
        playerMarkerInstance.SetActive(true);

        Debug.Log($"[MapPanelController] Player marker updated at {localPos} (World: {playerWorldPos}, Normalized: {normalizedPos})");
    }

    /// <summary>
    /// MapPanel이 닫힐 때 호출
    /// </summary>
    public void OnMapPanelClosed()
    {
        if (playerMarkerInstance != null)
        {
            playerMarkerInstance.SetActive(false);
        }
    }

    /// <summary>
    /// 외부에서 활성 미니맵 컨트롤러를 설정
    /// </summary>
    public void SetActiveMiniMapController(MiniMapController controller)
    {
        activeMiniMapController = controller;
    }
}
