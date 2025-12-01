using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveMapGenerator : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap groundTilemap;
    public TileBase groundTile;

    [Header("Map Size")]
    public int width = 50;
    public int height = 30;

    // Inspector 메뉴에서 호출하기 위한 버튼
    [ContextMenu("Generate Map")]
    public void GenerateGround()
    {
        if (groundTilemap == null || groundTile == null)
        {
            Debug.LogError("Tilemap 또는 Tile이 비어있습니다!");
            return;
        }

        groundTilemap.ClearAllTiles();

        int startX = -width / 2;
        int startY = -height / 2;

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
            }
        }

        Debug.Log("Cave map generated!");
    }
}
