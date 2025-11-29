using UnityEngine;
using UnityEngine.Tilemaps;
[System.Serializable]
public class TilePreset
{    public int width;
    public int height;
    public TileBase[] tiles; // width*height 순서대로
}public class NinjaVillageGenerator : MonoBehaviour
{    [Header("Tilemaps")]
    public Tilemap backgroundTM;
    public Tilemap groundTM;
    public Tilemap collisionTM;
    public Tilemap walkBehindTM;
    public Tilemap decoTM;
    [Header("Map Settings")]
    public int width = 80;
    public int height = 80;
    public int seed = 1;
    [Header("Terrain Tiles")]
    public TileBase[] sandTiles;    // 밝은 바닥
    public TileBase[] dirtTiles;    // 진한 바닥
    [Header("House Settings")]
    public TilePreset[] housePresets;
    public int houseCount = 30;
    public int minHouseDistance = 6;
    [Header("Tree Settings")]
    public TilePreset[] treePresets;
    public float treeDensity = 0.02f;
    [Header("Village Area")]
    public float villageRadius = 50f;
    [ContextMenu("Generate Village Map")]
    public void Generate()
    {
        Random.InitState(seed);
        backgroundTM.ClearAllTiles();
        groundTM.ClearAllTiles();
        collisionTM.ClearAllTiles();
        walkBehindTM.ClearAllTiles();
        decoTM.ClearAllTiles();
        Vector2 center = new Vector2(width / 2, height / 2);
        
        // ------------------------------------
        // 1) Terrain 생성
        // ------------------------------------
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                TileBase tile;
                // 마을 중심은 더 밝은 모래
                if (dist < villageRadius)
                    tile = sandTiles[Random.Range(0, sandTiles.Length)];
                else
                    tile = dirtTiles[Random.Range(0, dirtTiles.Length)];
                Vector3Int pos = new Vector3Int(x, y, 0);
                backgroundTM.SetTile(pos, tile);
                groundTM.SetTile(pos, tile);
            }
        }
        // ------------------------------------
        // 2) 집 배치
        // ------------------------------------
        var placed = new System.Collections.Generic.List<Vector3>();
        for (int i = 0; i < houseCount; i++)
        {
            TilePreset preset = housePresets[Random.Range(0, housePresets.Length)];
            Vector3Int origin;
            int safety = 200;
            do
            {
                float angle = Random.Range(0, Mathf.PI * 2);
                float r = Random.Range(0, villageRadius * 0.8f);
                int x = Mathf.RoundToInt(center.x + Mathf.Cos(angle) * r);
                int y = Mathf.RoundToInt(center.y + Mathf.Sin(angle) * r);
                origin = new Vector3Int(x, y, 0);
                safety--;
                if (safety <= 0) break;
            } while (IsTooClose(origin, placed, minHouseDistance));
            placed.Add(origin);
            StampHouse(origin, preset);
        }
        // ------------------------------------
        // 3) 나무 배치 (마을 외곽 중심)
        // ------------------------------------
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < villageRadius * 0.7f) continue; // 마을 영역에는 거의 NO trees
                if (Random.value > treeDensity) continue;
                TilePreset tree = treePresets[Random.Range(0, treePresets.Length)];
                StampTree(new Vector3Int(x, y), tree);
            }
        }
        // ------------------------------------
        // 4) 카메라 중앙 이동 (가장 중요!)
        // ------------------------------------
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
    }
    // ---- Helper ----
    bool IsTooClose(Vector3Int pos, System.Collections.Generic.List<Vector3> list, float minDist)
    {
        foreach (var p in list)
            if (Vector3.Distance(pos, p) < minDist) return true;
        return false;
    }
    void StampHouse(Vector3Int origin, TilePreset preset)
    {
        int i = 0;
        for (int row = 0; row < preset.height; row++)
        {
            for (int col = 0; col < preset.width; col++)
            {
                TileBase tile = preset.tiles[i++];
                if (tile == null) continue;
                Vector3Int pos = origin + new Vector3Int(col, preset.height - 1 - row, 0);
                if (row == 0)
                    walkBehindTM.SetTile(pos, tile);
                else
                    collisionTM.SetTile(pos, tile);
            }
        }
    }
    void StampTree(Vector3Int origin, TilePreset preset)
    {
        int i = 0;
        for (int row = 0; row < preset.height; row++)
        {
            for (int col = 0; col < preset.width; col++)
            {
                TileBase tile = preset.tiles[i++];
                if (tile == null) continue;
                Vector3Int pos = origin + new Vector3Int(col, preset.height - 1 - row, 0);
                if (row == 0)
                    decoTM.SetTile(pos, tile);
                else
                    collisionTM.SetTile(pos, tile);
            }
        }
    }
}