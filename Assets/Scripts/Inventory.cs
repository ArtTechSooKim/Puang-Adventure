using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main inventory system that persists across scenes.
/// Manages item storage for both hotbar and inventory UI.
/// </summary>
public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ Inventory: Initialized and persisting across scenes");
        }
        else
        {
            Debug.LogWarning("⚠ Inventory: Duplicate instance detected - destroying");
            Destroy(gameObject);
            return;
        }
    }

    [Header("Inventory Settings")]
    public int capacity = 20;
    public ItemData[] items;

    [Header("UI References")]
    public InventoryUI inventoryUI;

    private void Start()
    {
        // Re-find UI references in the current scene if null
        RefreshUIReferences();
        RefreshUI();
    }

    /// <summary>
    /// Find and update UI references in the current scene (Public for external calls)
    /// </summary>
    public void RefreshUIReferences()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindAnyObjectByType<InventoryUI>();
            if (inventoryUI != null)
                Debug.Log("✅ Inventory: Found InventoryUI in current scene");
        }

        // Also refresh UI after finding references
        RefreshUI();
    }

    [Header("World Item Settings")]
    public GameObject itemWorldPrefab; // 드롭 시 생성할 아이템 프리팹
    public float pickupIgnoreDuration = 0.5f; // 드롭 후 다시 주울 수 없는 시간
    public float dropOffsetY = 5f; // 플레이어 머리 위로 드롭

    // 초기화
    public void Initialize(int newCapacity)
    {
        if (items == null || items.Length != newCapacity)
        {
            ItemData[] newItems = new ItemData[newCapacity];
            if (items != null)
            {
                for (int i = 0; i < Mathf.Min(items.Length, newCapacity); i++)
                    newItems[i] = items[i];
            }
            items = newItems;
            capacity = newCapacity;
        }
    }

    // 아이템 추가
    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Inventory.AddItem: item is null.");
            return false;
        }

        // 🔹 먼저 핫바에서 빈 슬롯 찾기
        if (Hotbar.instance != null)
        {
            int hotIndex = Hotbar.instance.GetFirstEmptyIndex();
            if (hotIndex != -1 && items != null && hotIndex < items.Length)
            {
                items[hotIndex] = item;
                Debug.Log($"[Inventory] '{item.itemName}' added to Hotbar slot {hotIndex}");
                RefreshUI();

                // 🎯 칼자루 획득 시 Stage0 → Stage1 자동 진행
                CheckQuestItemAcquisition(item);

                return true;
            }
        }

        // 🔹 인벤토리에서 빈 슬롯 찾기
        for (int i = 0; i < capacity; i++)
        {
            if (items == null) break;
            if (items[i] == null)
            {
                items[i] = item;
                Debug.Log($"[Inventory] '{item.itemName}' added to Inventory slot {i}");
                RefreshUI();

                // 🎯 칼자루 획득 시 Stage0 → Stage1 자동 진행
                CheckQuestItemAcquisition(item);

                return true;
            }
        }

        Debug.Log("Inventory full!");
        return false;
    }

    /// <summary>
    /// 특정 퀘스트 아이템 획득 시 자동으로 스테이지 진행
    /// </summary>
    private void CheckQuestItemAcquisition(ItemData item)
    {
        if (QuestManager.Instance == null) return;

        QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

        // Stage0: 칼자루 획득 시 Stage1로 진행
        if (currentStage == QuestStage.Stage0_VillageTutorial)
        {
            // 칼자루 아이템인지 확인: weaponTier = 0인 무기 (Item_WeaponTier0_World)
            if (item.isWeapon && item.weaponTier == 0)
            {
                QuestManager.Instance.AdvanceStage();
                Debug.Log($"🎯 칼자루 획득! ({item.itemName}) Stage0 → Stage1 자동 진행");
            }
        }
    }

    // 아이템 제거
    public void RemoveItemAt(int index)
    {
        if (items == null || index < 0 || index >= items.Length) return;
        items[index] = null;
        RefreshUI();
    }

    // 슬롯 간 교체 (핫바/인벤토리 포함)
    public void SwapItems(int indexA, int indexB)
    {
        if (items == null) return;
        if (indexA < 0 || indexA >= items.Length || indexB < 0 || indexB >= items.Length) return;
        if (indexA == indexB) return;

        ItemData temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        // 🔹 교체 후 UI 전체를 한 번만 갱신 (핫바 포함)
        RefreshUI();
    }

    // 월드에 드롭
    public void DropItemToWorld(int index)
    {
        if (items == null) return;
        if (index < 0 || index >= items.Length) return;
        ItemData d = items[index];
        if (d == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        Vector3 spawnPos = Vector3.zero;

        if (player != null)
        {
            spawnPos = player.transform.position + (player.transform.up * dropOffsetY);
        }
        else
        {
            Debug.LogWarning("Inventory.DropItemToWorld: Player not found. Spawning at origin.");
        }

        if (itemWorldPrefab != null)
        {
            GameObject go = Instantiate(itemWorldPrefab, spawnPos, Quaternion.identity);

            // Rigidbody 설정
            Rigidbody2D goRb = go.GetComponent<Rigidbody2D>();
            if (goRb != null)
            {
                goRb.gravityScale = 0f;
                goRb.linearVelocity = Vector2.zero;
            }

            // ✅ 정렬 레이어 고정 (항상 Player 위에 표시)
            SpriteRenderer prefabSr = itemWorldPrefab.GetComponent<SpriteRenderer>();
            SpriteRenderer goSr = go.GetComponent<SpriteRenderer>();
            if (goSr != null)
            {
                if (prefabSr != null)
                {
                    goSr.sortingLayerName = prefabSr.sortingLayerName;
                    goSr.sortingOrder = prefabSr.sortingOrder;
                    goSr.enabled = prefabSr.enabled;
                }

                // 강제로 Player 레이어로 덮어쓰기
                goSr.sortingLayerName = "Player";
                goSr.sortingOrder = 5;
            }

            // Item 컴포넌트 설정
            Item worldItem = go.GetComponent<Item>();
            if (worldItem != null)
            {
                worldItem.data = d;
                worldItem.SetPickupDelay(pickupIgnoreDuration);
            }
        }
        else
        {
            Debug.LogWarning("Inventory.DropItemToWorld: itemWorldPrefab not assigned.");
        }

        // 아이템 제거 후 UI 갱신
        items[index] = null;
        RefreshUI();
    }

    // UI 전체 갱신 (핫바 + 인벤토리)
    private void RefreshUI()
    {
        if (inventoryUI != null)
            inventoryUI.UpdateUI();

        if (Hotbar.instance != null)
            Hotbar.instance.UpdateUI();
    }
}
