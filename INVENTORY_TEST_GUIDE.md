# 📦 Inventory 통합 테스트 가이드

## 🎯 목적
생성한 ItemData 애셋들이 Inventory 시스템과 올바르게 통합되는지 테스트합니다.

## 📋 사전 준비

### 1. ItemData 애셋 위치 확인
모든 ItemData 애셋을 **Resources/Items/** 폴더로 이동해야 합니다:

```
Assets/
  └─ Resources/
      └─ Items/
          ├─ Item_WeaponTier0.asset
          ├─ Item_WeaponTier1.asset
          ├─ Item_WeaponTier2.asset
          ├─ Item_SlimeResidue.asset
          ├─ Item_BatBone.asset
          ├─ Item_SkeletonBone.asset
          └─ Item_BossMeat.asset
```

**⚠ 중요:** Resources 폴더가 없으면 Assets 폴더 하위에 생성하세요!

### 2. ItemData 설정 확인
각 ItemData 애셋의 Inspector에서 다음을 확인:

#### 무기 아이템 (Item_WeaponTier0/1/2)
- ✅ **itemID**: "WeaponTier0", "WeaponTier1", "WeaponTier2"
- ✅ **itemName**: "나무 검", "철 검", "전설의 검" 등
- ✅ **isWeapon**: true
- ✅ **weaponTier**: 0, 1, 2
- ✅ **hasUltimate**: Tier 2만 true, 나머지 false
- ✅ **isStackable**: false (무기는 중첩 불가)

#### 소모품 아이템 (SlimeResidue, BatBone, etc.)
- ✅ **itemID**: "SlimeResidue", "BatBone", "SkeletonBone", "BossMeat"
- ✅ **itemName**: "슬라임 잔해", "박쥐 뼈" 등
- ✅ **isWeapon**: false
- ✅ **isStackable**: true (중첩 가능)
- ✅ **maxStackSize**: 99 (권장)
- ✅ **isQuestItem**: 퀘스트용이면 true

## 🧪 테스트 실행 방법

### 방법 1: 자동 실행 (권장)
1. Scene에 **빈 GameObject** 생성 (이름: "InventoryTester")
2. **InventoryTest.cs** 컴포넌트 추가
3. Inspector에서 설정:
   - ✅ **Run On Start**: true (체크)
   - ✅ **Auto Load From Resources**: true (체크)
   - ✅ **Test Key**: T
4. **Play 버튼** 클릭
5. **Console 창**에서 결과 확인

### 방법 2: 수동 실행
1. 위와 동일하게 설정하되 **Run On Start**: false
2. Play 모드 진입
3. **T 키** 누르기
4. Console 창에서 결과 확인

### 방법 3: Context Menu 사용
1. InventoryTest 컴포넌트 Inspector에서 우클릭
2. **"Run Test"** 선택
3. Console 창에서 결과 확인

## 📊 예상 결과

테스트가 성공하면 Console에 다음과 같이 표시됩니다:

```
=== 📦 Inventory Test Started ===
✅ Inventory instance found

--- Test 1: ItemData Loading ---
✅ Found 7 ItemData(s) in Resources/Items:
  - 나무 검 (ID: WeaponTier0) [Weapon Tier 0]
  - 철 검 (ID: WeaponTier1) [Weapon Tier 1]
  - 전설의 검 (ID: WeaponTier2) [Weapon Tier 2] [Ultimate]
  - 슬라임 잔해 (ID: SlimeResidue)
  - 박쥐 뼈 (ID: BatBone)
  - 해골 뼈 (ID: SkeletonBone)
  - 보스 고기 (ID: BossMeat)

--- Test 2: Adding Weapons ---
✅ Added 나무 검 (Tier 0)
   └─ Weapon Tier: 0
   └─ Has Ultimate: False
   └─ Is Quest Item: False
✅ Added 철 검 (Tier 1)
✅ Added 전설의 검 (Tier 2, Ultimate: True)

--- Test 3: Adding Consumables ---
✅ Added 슬라임 잔해 (Stackable: True)
   └─ Added one more (should stack)
✅ Added 박쥐 뼈
✅ Added 해골 뼈
✅ Added 보스 고기

--- Inventory Status ---
  Slot 0: 나무 검
  Slot 1: 철 검
  Slot 2: 전설의 검
  Slot 3: 슬라임 잔해 x2
  Slot 4: 박쥐 뼈
  Slot 5: 해골 뼈
  Slot 6: 보스 고기
📊 Total Items: 7 / 20

=== ✅ Inventory Test Completed ===
```

## 🔍 테스트 항목 체크리스트

### ✅ ItemData 로딩 테스트
- [ ] Resources/Items 폴더에서 모든 ItemData 로드 성공
- [ ] 7개의 아이템이 모두 감지됨
- [ ] 각 아이템의 itemID와 itemName이 올바르게 표시됨

### ✅ 무기 시스템 테스트
- [ ] Tier 0 무기 추가 성공
- [ ] Tier 1 무기 추가 성공
- [ ] Tier 2 무기 추가 성공 (Ultimate 속성 확인)
- [ ] 각 무기의 weaponTier 값이 올바름
- [ ] hasUltimate 값이 올바름 (Tier 2만 true)

### ✅ 소모품 시스템 테스트
- [ ] 슬라임 잔해 추가 성공
- [ ] 슬라임 잔해 중복 추가 시 스택됨 (x2 표시)
- [ ] 박쥐 뼈 추가 성공
- [ ] 해골 뼈 추가 성공
- [ ] 보스 고기 추가 성공

### ✅ 인벤토리 상태 테스트
- [ ] 총 7개 아이템이 인벤토리에 존재
- [ ] 각 슬롯에 올바른 아이템 배치
- [ ] 스택 카운트가 올바르게 표시 (슬라임 잔해 x2)
- [ ] UI에도 아이템이 올바르게 표시됨

## ❌ 문제 해결

### ⚠ "No ItemData found in Resources/Items folder!"
**원인:** ItemData 애셋이 Resources/Items 폴더에 없음

**해결방법:**
1. Assets 폴더에 Resources 폴더 생성
2. Resources 폴더에 Items 폴더 생성
3. 모든 ItemData 애셋을 Resources/Items로 이동

### ⚠ "Inventory.instance is null!"
**원인:** Scene에 Inventory GameObject가 없음

**해결방법:**
1. Hierarchy에서 Inventory GameObject 확인
2. Inventory.cs 컴포넌트가 올바르게 부착되어 있는지 확인
3. Scene에 Canvas_UI Prefab이 있는지 확인

### ⚠ "Item_WeaponTier0 not found in Resources/Items/"
**원인:** 특정 ItemData 파일명이 일치하지 않음

**해결방법:**
1. Resources/Items 폴더에서 파일명 확인
2. InventoryTest.cs의 Resources.Load 경로와 일치시키기
3. 파일명: Item_WeaponTier0.asset, Item_WeaponTier1.asset, Item_WeaponTier2.asset

### ⚠ 아이템이 스택되지 않음
**원인:** ItemData의 isStackable이 false로 설정됨

**해결방법:**
1. 해당 ItemData 애셋 선택
2. Inspector에서 **Is Stackable**: true로 설정
3. **Max Stack Size**: 99 설정

## 🎮 추가 테스트 기능

### 인벤토리 초기화
```
1. InventoryTest 컴포넌트 우클릭
2. "Clear Inventory" 선택
3. Console: "🗑 Inventory cleared" 확인
```

### 테스트 재실행
```
1. Play 모드 중 T 키 누르기
또는
2. InventoryTest 컴포넌트 우클릭 → "Run Test"
```

## 📝 테스트 후 확인사항

1. **UI 확인**
   - Inventory UI를 열어서 아이템이 올바르게 표시되는지 확인
   - 아이템 아이콘이 올바르게 표시되는지 확인
   - 스택 카운트가 UI에 표시되는지 확인

2. **드래그 앤 드롭 확인**
   - 아이템을 드래그하여 다른 슬롯으로 이동 가능한지 확인
   - 스택 가능한 아이템끼리 합쳐지는지 확인

3. **핫바 확인**
   - 무기를 핫바 슬롯으로 드래그하여 장착 가능한지 확인
   - 1~6 숫자 키로 무기 전환이 되는지 확인

## 🚀 다음 단계

테스트가 모두 성공했다면:

1. **World Prefab 생성**
   - 각 ItemData에 대응하는 World Prefab 생성
   - Item.cs 스크립트 추가
   - SpriteRenderer, Collider2D 설정

2. **NPC 테스트**
   - NPCData 애셋 생성
   - NPCController 테스트
   - 아이템 교환 시스템 테스트

3. **퀘스트 시스템 연동**
   - QuestManager와 연동
   - Stage 진행에 따른 아이템 획득/소비 테스트

---

**💡 Tip:** 테스트는 새로운 Scene에서 진행하는 것을 권장합니다. 기존 게임 데이터에 영향을 주지 않습니다!
