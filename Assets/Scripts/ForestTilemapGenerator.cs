using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Forest Scene Tilemap Generator
/// Automatically generates a forest hub area with themed zones for each portal
/// - Village Portal: Dense trees/foliage
/// - Cave Portal: Cave entrance with rocky terrain
/// - Boss Portal: Mysterious/magical atmosphere
/// </summary>
public class ForestTilemapGenerator : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap collisionTilemap;
    [SerializeField] private Tilemap walkBehindTilemap;
    [SerializeField] private Tilemap decoTilemap;

    [Header("Tile Assets - Ground")]
    [SerializeField] private TileBase[] grassTiles;
    [SerializeField] private TileBase[] dirtPathTiles;
    [SerializeField] private TileBase[] stoneTiles;

    [Header("Tile Assets - Nature")]
    [SerializeField] private TileBase[] treeTiles;
    [SerializeField] private TileBase[] bushTiles;
    [SerializeField] private TileBase[] flowerTiles;
    [SerializeField] private TileBase[] rockTiles;

    [Header("Tile Assets - Special")]
    [SerializeField] private TileBase[] caveEntranceTiles;
    [SerializeField] private TileBase[] mysticalTiles;
    [SerializeField] private TileBase[] darkForestTiles;

    [Header("Portal Locations")]
    [SerializeField] private Vector3Int villagePortalPosition = new Vector3Int(-8, 2, 0);
    [SerializeField] private Vector3Int cavePortalPosition = new Vector3Int(8, -3, 0);
    [SerializeField] private Vector3Int bossPortalPosition = new Vector3Int(0, 6, 0);
    [SerializeField] private Vector3Int playerSpawnPosition = new Vector3Int(0, 0, 0);

    [Header("Generation Settings")]
    [SerializeField] private Vector2Int mapSize = new Vector2Int(20, 14);
    [SerializeField] private Vector2Int mapOrigin = new Vector2Int(-10, -7);
    [SerializeField] [Range(0f, 1f)] private float decorationDensity = 0.3f;

    [Header("Preview")]
    [SerializeField] private bool showDebugGizmos = true;

    // Theme zones radius
    private const int THEME_ZONE_RADIUS = 4;

    #region Generation Methods

    /// <summary>
    /// Main generation method - call this to generate the entire forest map
    /// </summary>
    public void GenerateForestMap()
    {
        Debug.Log("🌲 Starting Forest Map Generation...");

        if (!ValidateTilemaps())
        {
            Debug.LogError("❌ Tilemap references are missing! Please assign all tilemaps in the inspector.");
            return;
        }

        // Clear existing tiles
        ClearAllTilemaps();

        // Generate layers from bottom to top
        GenerateBackgroundLayer();
        GenerateGroundLayer();
        GenerateCollisionLayer();
        GenerateDecorationLayer();
        GenerateThemedZones();

        Debug.Log("✅ Forest Map Generation Complete!");
    }

    /// <summary>
    /// Clear all tilemaps
    /// </summary>
    public void ClearAllTilemaps()
    {
        Debug.Log("🧹 Clearing all tilemaps...");

        if (backgroundTilemap != null) backgroundTilemap.ClearAllTiles();
        if (groundTilemap != null) groundTilemap.ClearAllTiles();
        if (collisionTilemap != null) collisionTilemap.ClearAllTiles();
        if (walkBehindTilemap != null) walkBehindTilemap.ClearAllTiles();
        if (decoTilemap != null) decoTilemap.ClearAllTiles();
    }

    /// <summary>
    /// Validate that all required tilemaps are assigned
    /// </summary>
    private bool ValidateTilemaps()
    {
        return backgroundTilemap != null &&
               groundTilemap != null &&
               collisionTilemap != null &&
               walkBehindTilemap != null &&
               decoTilemap != null;
    }

    #endregion

    #region Layer Generation

    /// <summary>
    /// Generate background layer (dark forest backdrop)
    /// </summary>
    private void GenerateBackgroundLayer()
    {
        if (darkForestTiles == null || darkForestTiles.Length == 0)
        {
            Debug.LogWarning("⚠ No dark forest tiles assigned - skipping background layer");
            return;
        }

        for (int x = mapOrigin.x; x < mapOrigin.x + mapSize.x; x++)
        {
            for (int y = mapOrigin.y; y < mapOrigin.y + mapSize.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = GetRandomTile(darkForestTiles);
                if (tile != null)
                {
                    backgroundTilemap.SetTile(pos, tile);
                }
            }
        }

        Debug.Log("✅ Background layer generated");
    }

    /// <summary>
    /// Generate ground layer (grass and paths)
    /// </summary>
    private void GenerateGroundLayer()
    {
        if (grassTiles == null || grassTiles.Length == 0)
        {
            Debug.LogWarning("⚠ No grass tiles assigned - skipping ground layer");
            return;
        }

        // Base grass layer
        for (int x = mapOrigin.x; x < mapOrigin.x + mapSize.x; x++)
        {
            for (int y = mapOrigin.y; y < mapOrigin.y + mapSize.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Create dirt paths connecting portals
                if (IsOnPath(pos))
                {
                    TileBase tile = GetRandomTile(dirtPathTiles);
                    if (tile != null)
                        groundTilemap.SetTile(pos, tile);
                }
                else
                {
                    // Regular grass
                    TileBase tile = GetRandomTile(grassTiles);
                    if (tile != null)
                        groundTilemap.SetTile(pos, tile);
                }
            }
        }

        Debug.Log("✅ Ground layer generated");
    }

    /// <summary>
    /// Generate collision layer (matches ground layer)
    /// </summary>
    private void GenerateCollisionLayer()
    {
        // For now, copy ground layer to collision layer
        // In a more advanced setup, you'd use specific collision tiles
        for (int x = mapOrigin.x; x < mapOrigin.x + mapSize.x; x++)
        {
            for (int y = mapOrigin.y; y < mapOrigin.y + mapSize.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase groundTile = groundTilemap.GetTile(pos);

                if (groundTile != null)
                {
                    // Use the same tile or a collision-specific tile
                    collisionTilemap.SetTile(pos, groundTile);
                }
            }
        }

        Debug.Log("✅ Collision layer generated");
    }

    /// <summary>
    /// Generate decoration layer (rocks, bushes, flowers)
    /// </summary>
    private void GenerateDecorationLayer()
    {
        if (rockTiles == null || rockTiles.Length == 0)
        {
            Debug.LogWarning("⚠ No decoration tiles assigned - skipping decoration layer");
            return;
        }

        for (int x = mapOrigin.x; x < mapOrigin.x + mapSize.x; x++)
        {
            for (int y = mapOrigin.y; y < mapOrigin.y + mapSize.y; y++)
            {
                // Skip portal locations and player spawn
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (IsNearPortalOrSpawn(pos, 2))
                    continue;

                // Random decoration placement
                if (Random.value < decorationDensity)
                {
                    // Randomly choose decoration type
                    float rand = Random.value;
                    TileBase tile = null;

                    if (rand < 0.4f && rockTiles.Length > 0)
                        tile = GetRandomTile(rockTiles);
                    else if (rand < 0.7f && bushTiles.Length > 0)
                        tile = GetRandomTile(bushTiles);
                    else if (flowerTiles.Length > 0)
                        tile = GetRandomTile(flowerTiles);

                    if (tile != null)
                        decoTilemap.SetTile(pos, tile);
                }
            }
        }

        Debug.Log("✅ Decoration layer generated");
    }

    /// <summary>
    /// Generate themed zones around each portal
    /// </summary>
    private void GenerateThemedZones()
    {
        // Village zone: Dense trees/foliage
        GenerateVillageZone();

        // Cave zone: Rocky terrain with cave entrance
        GenerateCaveZone();

        // Boss zone: Mysterious/mystical atmosphere
        GenerateBossZone();

        Debug.Log("✅ Themed zones generated");
    }

    #endregion

    #region Themed Zone Generation

    /// <summary>
    /// Generate Village-themed zone (dense forest, hidden path)
    /// </summary>
    private void GenerateVillageZone()
    {
        if (treeTiles == null || treeTiles.Length == 0)
        {
            Debug.LogWarning("⚠ No tree tiles assigned - skipping village zone");
            return;
        }

        // Place dense trees around village portal
        for (int radius = 1; radius <= THEME_ZONE_RADIUS; radius++)
        {
            for (int angle = 0; angle < 360; angle += 30)
            {
                float rad = angle * Mathf.Deg2Rad;
                int offsetX = Mathf.RoundToInt(Mathf.Cos(rad) * radius);
                int offsetY = Mathf.RoundToInt(Mathf.Sin(rad) * radius);

                Vector3Int pos = villagePortalPosition + new Vector3Int(offsetX, offsetY, 0);

                // Skip if too close to portal center
                if (Vector3Int.Distance(pos, villagePortalPosition) < 1.5f)
                    continue;

                // Place trees on WalkBehind layer
                if (Random.value < 0.7f)
                {
                    TileBase tree = GetRandomTile(treeTiles);
                    if (tree != null)
                        walkBehindTilemap.SetTile(pos, tree);
                }
            }
        }

        Debug.Log($"✅ Village zone generated at {villagePortalPosition}");
    }

    /// <summary>
    /// Generate Cave-themed zone (rocky, dark, cave entrance visible)
    /// </summary>
    private void GenerateCaveZone()
    {
        if (rockTiles == null || rockTiles.Length == 0)
        {
            Debug.LogWarning("⚠ No rock tiles assigned - skipping cave zone");
            return;
        }

        // Place rocks and cave entrance tiles
        for (int radius = 1; radius <= THEME_ZONE_RADIUS; radius++)
        {
            for (int angle = 0; angle < 360; angle += 30)
            {
                float rad = angle * Mathf.Deg2Rad;
                int offsetX = Mathf.RoundToInt(Mathf.Cos(rad) * radius);
                int offsetY = Mathf.RoundToInt(Mathf.Sin(rad) * radius);

                Vector3Int pos = cavePortalPosition + new Vector3Int(offsetX, offsetY, 0);

                // Skip if too close to portal center
                if (Vector3Int.Distance(pos, cavePortalPosition) < 1.5f)
                    continue;

                // Replace ground with stone tiles
                if (Random.value < 0.6f && stoneTiles != null && stoneTiles.Length > 0)
                {
                    TileBase stone = GetRandomTile(stoneTiles);
                    if (stone != null)
                        groundTilemap.SetTile(pos, stone);
                }

                // Add rocks as decoration
                if (Random.value < 0.5f)
                {
                    TileBase rock = GetRandomTile(rockTiles);
                    if (rock != null)
                        decoTilemap.SetTile(pos, rock);
                }
            }
        }

        // Place cave entrance tiles if available
        if (caveEntranceTiles != null && caveEntranceTiles.Length > 0)
        {
            Vector3Int entrancePos = cavePortalPosition + new Vector3Int(0, 1, 0);
            TileBase entrance = GetRandomTile(caveEntranceTiles);
            if (entrance != null)
                walkBehindTilemap.SetTile(entrancePos, entrance);
        }

        Debug.Log($"✅ Cave zone generated at {cavePortalPosition}");
    }

    /// <summary>
    /// Generate Boss-themed zone (mysterious, mystical atmosphere)
    /// </summary>
    private void GenerateBossZone()
    {
        // Use mystical tiles if available, otherwise use special flowers/decorations
        TileBase[] mysticalDecoTiles = (mysticalTiles != null && mysticalTiles.Length > 0)
            ? mysticalTiles
            : flowerTiles;

        if (mysticalDecoTiles == null || mysticalDecoTiles.Length == 0)
        {
            Debug.LogWarning("⚠ No mystical tiles assigned - skipping boss zone");
            return;
        }

        // Create a circular mystical area
        for (int radius = 1; radius <= THEME_ZONE_RADIUS; radius++)
        {
            for (int angle = 0; angle < 360; angle += 20)
            {
                float rad = angle * Mathf.Deg2Rad;
                int offsetX = Mathf.RoundToInt(Mathf.Cos(rad) * radius);
                int offsetY = Mathf.RoundToInt(Mathf.Sin(rad) * radius);

                Vector3Int pos = bossPortalPosition + new Vector3Int(offsetX, offsetY, 0);

                // Skip if too close to portal center
                if (Vector3Int.Distance(pos, bossPortalPosition) < 1.5f)
                    continue;

                // Add mystical decorations
                if (Random.value < 0.4f)
                {
                    TileBase mystical = GetRandomTile(mysticalDecoTiles);
                    if (mystical != null)
                        decoTilemap.SetTile(pos, mystical);
                }
            }
        }

        Debug.Log($"✅ Boss zone generated at {bossPortalPosition}");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if position is on a path connecting portals
    /// </summary>
    private bool IsOnPath(Vector3Int pos)
    {
        if (dirtPathTiles == null || dirtPathTiles.Length == 0)
            return false;

        // Simple path detection: check distance to lines between spawn and portals
        float pathWidth = 1.5f;

        // Path from spawn to village
        if (DistanceToLineSegment(pos, playerSpawnPosition, villagePortalPosition) < pathWidth)
            return true;

        // Path from spawn to cave
        if (DistanceToLineSegment(pos, playerSpawnPosition, cavePortalPosition) < pathWidth)
            return true;

        // Path from spawn to boss
        if (DistanceToLineSegment(pos, playerSpawnPosition, bossPortalPosition) < pathWidth)
            return true;

        return false;
    }

    /// <summary>
    /// Calculate distance from point to line segment
    /// </summary>
    private float DistanceToLineSegment(Vector3Int point, Vector3Int lineStart, Vector3Int lineEnd)
    {
        Vector2 p = new Vector2(point.x, point.y);
        Vector2 a = new Vector2(lineStart.x, lineStart.y);
        Vector2 b = new Vector2(lineEnd.x, lineEnd.y);

        Vector2 ab = b - a;
        Vector2 ap = p - a;

        float abLengthSq = ab.sqrMagnitude;
        if (abLengthSq == 0)
            return Vector2.Distance(p, a);

        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / abLengthSq);
        Vector2 projection = a + ab * t;

        return Vector2.Distance(p, projection);
    }

    /// <summary>
    /// Check if position is near any portal or spawn point
    /// </summary>
    private bool IsNearPortalOrSpawn(Vector3Int pos, int minDistance)
    {
        if (Vector3Int.Distance(pos, villagePortalPosition) < minDistance)
            return true;
        if (Vector3Int.Distance(pos, cavePortalPosition) < minDistance)
            return true;
        if (Vector3Int.Distance(pos, bossPortalPosition) < minDistance)
            return true;
        if (Vector3Int.Distance(pos, playerSpawnPosition) < minDistance)
            return true;

        return false;
    }

    /// <summary>
    /// Get random tile from array
    /// </summary>
    private TileBase GetRandomTile(TileBase[] tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return null;

        return tiles[Random.Range(0, tiles.Length)];
    }

    #endregion

    #region Gizmos

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;

        // Draw map bounds
        Gizmos.color = Color.white;
        Vector3 center = new Vector3(
            mapOrigin.x + mapSize.x / 2f,
            mapOrigin.y + mapSize.y / 2f,
            0
        );
        Vector3 size = new Vector3(mapSize.x, mapSize.y, 0);
        Gizmos.DrawWireCube(center, size);

        // Draw portal locations
        DrawPortalGizmo(villagePortalPosition, Color.green, "Village");
        DrawPortalGizmo(cavePortalPosition, Color.gray, "Cave");
        DrawPortalGizmo(bossPortalPosition, Color.magenta, "Boss");

        // Draw spawn location
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerSpawnPosition, 0.5f);
        UnityEditor.Handles.Label(playerSpawnPosition + Vector3.up, "Spawn");
    }

    private void DrawPortalGizmo(Vector3Int position, Color color, string label)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(position, 0.5f);

        // Draw theme zone radius
        Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
        Gizmos.DrawWireSphere(position, THEME_ZONE_RADIUS);

        UnityEditor.Handles.Label(position + Vector3.up * 1.5f, label);
    }
#endif

    #endregion
}
