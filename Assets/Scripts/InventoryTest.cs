using UnityEngine;

/// <summary>
/// Inventory 시스템 테스트 스크립트
/// Scene의 빈 GameObject에 추가하여 테스트
/// </summary>
public class InventoryTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;

    void Start()
    {
        if (runOnStart)
        {
            RunInventoryTest();
        }
    }

    void Update()
    {
        // T키를 눌러서 수동으로 테스트 실행
        if (Input.GetKeyDown(testKey))
        {
            RunInventoryTest();
        }
    }

    /// <summary>
    /// Inventory 통합 테스트 실행
    /// </summary>
    public void RunInventoryTest()
    {
        Debug.Log("=== 📦 Inventory Test Started ===");

        // 1. Inventory 인스턴스 확인
        if (Inventory.instance == null)
        {
            Debug.LogError("❌ Inventory.instance is null! Make sure Inventory GameObject exists in scene.");
            return;
        }

        Debug.Log("✅ Inventory instance found");

        // 2. ItemData 로드 테스트
        TestItemLoading();

        // 3. 무기 추가 테스트
        TestAddWeapons();

        // 4. 소모품 추가 테스트
        TestAddConsumables();

        // 5. 인벤토리 상태 출력
        PrintInventoryStatus();

        Debug.Log("=== ✅ Inventory Test Completed ===");
    }

    /// <summary>
    /// ItemData 로드 테스트
    /// </summary>
    private void TestItemLoading()
    {
        Debug.Log("\n--- Test 1: ItemData Loading ---");

        // Resources/Items 폴더에서 모든 ItemData 로드
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

        if (allItems.Length == 0)
        {
            Debug.LogWarning("⚠ No ItemData found in Resources/Items folder!");
            Debug.LogWarning("💡 Tip: Move your ItemData assets to Assets/Resources/Items/");
            return;
        }

        Debug.Log($"✅ Found {allItems.Length} ItemData(s) in Resources/Items:");

        foreach (var item in allItems)
        {
            string weaponInfo = item.isWeapon ? $" [Weapon Tier {item.weaponTier}]" : "";
            string questInfo = item.isQuestItem ? " [Quest Item]" : "";
            string ultimateInfo = item.hasUltimate ? " [Ultimate]" : "";

            Debug.Log($"  - {item.itemName} (ID: {item.itemID}){weaponInfo}{questInfo}{ultimateInfo}");
        }
    }

    /// <summary>
    /// 무기 추가 테스트
    /// </summary>
    private void TestAddWeapons()
    {
        Debug.Log("\n--- Test 2: Adding Weapons ---");

        // Tier 0 무기 추가
        ItemData weapon0 = Resources.Load<ItemData>("Items/Item_WeaponTier0");
        if (weapon0 != null)
        {
            bool success = Inventory.instance.AddItem(weapon0);
            Debug.Log($"{(success ? "✅" : "❌")} Added {weapon0.itemName} (Tier {weapon0.weaponTier})");

            // 무기 속성 확인
            if (weapon0.isWeapon)
            {
                Debug.Log($"   └─ Weapon Tier: {weapon0.weaponTier}");
                Debug.Log($"   └─ Has Ultimate: {weapon0.hasUltimate}");
                Debug.Log($"   └─ Is Quest Item: {weapon0.isQuestItem}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ Item_WeaponTier0 not found in Resources/Items/");
        }

        // Tier 1 무기 추가
        ItemData weapon1 = Resources.Load<ItemData>("Items/Item_WeaponTier1");
        if (weapon1 != null)
        {
            bool success = Inventory.instance.AddItem(weapon1);
            Debug.Log($"{(success ? "✅" : "❌")} Added {weapon1.itemName} (Tier {weapon1.weaponTier})");
        }

        // Tier 2 무기 추가
        ItemData weapon2 = Resources.Load<ItemData>("Items/Item_WeaponTier2");
        if (weapon2 != null)
        {
            bool success = Inventory.instance.AddItem(weapon2);
            Debug.Log($"{(success ? "✅" : "❌")} Added {weapon2.itemName} (Tier {weapon2.weaponTier}, Ultimate: {weapon2.hasUltimate})");
        }
    }

    /// <summary>
    /// 소모품 추가 테스트
    /// </summary>
    private void TestAddConsumables()
    {
        Debug.Log("\n--- Test 3: Adding Consumables ---");

        // 슬라임 잔해 추가 (여러 개)
        ItemData slimeResidue = Resources.Load<ItemData>("Items/SlimeResidue");
        if (slimeResidue != null)
        {
            bool success = Inventory.instance.AddItem(slimeResidue);
            Debug.Log($"{(success ? "✅" : "❌")} Added {slimeResidue.itemName} (Stackable: {slimeResidue.isStackable})");

            // 하나 더 추가 (스택 테스트)
            if (slimeResidue.isStackable)
            {
                Inventory.instance.AddItem(slimeResidue);
                Debug.Log($"   └─ Added one more (should stack)");
            }
        }
        else
        {
            Debug.LogWarning("⚠ SlimeResidue not found in Resources/Items/");
        }

        // 박쥐 뼈 추가
        ItemData batBone = Resources.Load<ItemData>("Items/BatBone");
        if (batBone != null)
        {
            bool success = Inventory.instance.AddItem(batBone);
            Debug.Log($"{(success ? "✅" : "❌")} Added {batBone.itemName}");
        }
        else
        {
            Debug.LogWarning("⚠ BatBone not found in Resources/Items/");
        }

        // 해골 뼈 추가
        ItemData skeletonBone = Resources.Load<ItemData>("Items/SkeletonBone");
        if (skeletonBone != null)
        {
            bool success = Inventory.instance.AddItem(skeletonBone);
            Debug.Log($"{(success ? "✅" : "❌")} Added {skeletonBone.itemName}");
        }
        else
        {
            Debug.LogWarning("⚠ SkeletonBone not found in Resources/Items/");
        }

        // 보스 고기 추가
        ItemData bossMeat = Resources.Load<ItemData>("Items/BossMeat");
        if (bossMeat != null)
        {
            bool success = Inventory.instance.AddItem(bossMeat);
            Debug.Log($"{(success ? "✅" : "❌")} Added {bossMeat.itemName}");
        }
        else
        {
            Debug.LogWarning("⚠ BossMeat not found in Resources/Items/");
        }
    }

    /// <summary>
    /// 현재 인벤토리 상태 출력
    /// </summary>
    private void PrintInventoryStatus()
    {
        Debug.Log("\n--- Inventory Status ---");

        if (Inventory.instance.items == null)
        {
            Debug.LogWarning("⚠ Inventory items array is null");
            return;
        }

        int itemCount = 0;
        for (int i = 0; i < Inventory.instance.items.Length; i++)
        {
            if (Inventory.instance.items[i] != null)
            {
                itemCount++;
                ItemData item = Inventory.instance.items[i];
                string stackInfo = item.isStackable ? $" x{item.stackCount}" : "";
                Debug.Log($"  Slot {i}: {item.itemName}{stackInfo}");
            }
        }

        Debug.Log($"📊 Total Items: {itemCount} / {Inventory.instance.capacity}");
    }

    /// <summary>
    /// 인벤토리 초기화 (테스트 리셋용)
    /// </summary>
    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        if (Inventory.instance != null && Inventory.instance.items != null)
        {
            for (int i = 0; i < Inventory.instance.items.Length; i++)
            {
                Inventory.instance.items[i] = null;
            }
            Debug.Log("🗑 Inventory cleared");
        }
    }

    /// <summary>
    /// 테스트 재실행
    /// </summary>
    [ContextMenu("Run Test")]
    public void RunTest()
    {
        RunInventoryTest();
    }
}
