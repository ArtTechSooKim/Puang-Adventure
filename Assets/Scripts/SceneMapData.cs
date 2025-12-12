using UnityEngine;

/// <summary>
/// 각 씬의 맵 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "SceneMapData", menuName = "Map/Scene Map Data")]
public class SceneMapData : ScriptableObject
{
    [Header("Scene Info")]
    [Tooltip("씬 이름 (SceneManager.GetActiveScene().name과 일치해야 함)")]
    public string sceneName;

    [Header("Map Images")]
    [Tooltip("맵 전체 이미지 (항공뷰)")]
    public Sprite mapSprite;

    [Header("Calibration Points")]
    [Tooltip("정확한 위치 매핑을 위한 캘리브레이션 포인트들 (최소 2개 필수)")]
    public CalibrationPoint[] calibrationPoints;

    // Runtime cache
    private Vector2? cachedWorldScale;

    /// <summary>
    /// 월드 좌표를 0~1 사이의 Normalized 좌표로 변환
    /// </summary>
    public Vector2 WorldToNormalized(Vector3 worldPosition)
    {
        if (calibrationPoints == null || calibrationPoints.Length < 2)
        {
            Debug.LogError("[SceneMapData] Calibration Points가 최소 2개 필요합니다!");
            return Vector2.zero;
        }

        // 가장 가까운 2개의 캘리브레이션 포인트 찾기
        CalibrationPoint closest1 = calibrationPoints[0];
        CalibrationPoint closest2 = calibrationPoints[1];
        float minDist1 = Vector2.Distance(worldPosition, closest1.worldPosition);
        float minDist2 = Vector2.Distance(worldPosition, closest2.worldPosition);

        if (minDist2 < minDist1)
        {
            var temp = closest1;
            closest1 = closest2;
            closest2 = temp;
            var tempDist = minDist1;
            minDist1 = minDist2;
            minDist2 = tempDist;
        }

        for (int i = 2; i < calibrationPoints.Length; i++)
        {
            float dist = Vector2.Distance(worldPosition, calibrationPoints[i].worldPosition);
            if (dist < minDist1)
            {
                closest2 = closest1;
                minDist2 = minDist1;
                closest1 = calibrationPoints[i];
                minDist1 = dist;
            }
            else if (dist < minDist2)
            {
                closest2 = calibrationPoints[i];
                minDist2 = dist;
            }
        }

        // 자동으로 월드 스케일 계산 (캐시)
        if (!cachedWorldScale.HasValue)
        {
            cachedWorldScale = CalculateWorldScale();
        }

        // 가중 평균으로 보간
        float weight1 = 1f / (minDist1 + 0.01f);
        float weight2 = 1f / (minDist2 + 0.01f);
        float totalWeight = weight1 + weight2;

        Vector2 worldDelta1 = (Vector2)worldPosition - closest1.worldPosition;
        Vector2 worldDelta2 = (Vector2)worldPosition - closest2.worldPosition;

        // Calibration Points 간의 거리로 스케일 자동 계산
        Vector2 result1 = closest1.normalizedPosition + worldDelta1 / cachedWorldScale.Value;
        Vector2 result2 = closest2.normalizedPosition + worldDelta2 / cachedWorldScale.Value;

        return (result1 * weight1 + result2 * weight2) / totalWeight;
    }

    /// <summary>
    /// Calibration Points로부터 월드 스케일 자동 계산
    /// </summary>
    private Vector2 CalculateWorldScale()
    {
        if (calibrationPoints.Length < 2) return Vector2.one;

        // 첫 두 포인트로 스케일 계산
        Vector2 worldDelta = calibrationPoints[1].worldPosition - calibrationPoints[0].worldPosition;
        Vector2 normDelta = calibrationPoints[1].normalizedPosition - calibrationPoints[0].normalizedPosition;

        float scaleX = Mathf.Abs(worldDelta.x / normDelta.x);
        float scaleY = Mathf.Abs(worldDelta.y / normDelta.y);

        return new Vector2(scaleX, scaleY);
    }

    /// <summary>
    /// 월드 반경 계산 (MiniMapController에서 사용)
    /// </summary>
    public Vector2 GetWorldSize()
    {
        if (!cachedWorldScale.HasValue)
        {
            cachedWorldScale = CalculateWorldScale();
        }
        return cachedWorldScale.Value;
    }
}

[System.Serializable]
public class CalibrationPoint
{
    [Tooltip("월드 좌표 (실제 게임 위치)")]
    public Vector2 worldPosition;

    [Tooltip("Normalized 좌표 (0~1, 맵 이미지 상의 위치)")]
    public Vector2 normalizedPosition;
}
