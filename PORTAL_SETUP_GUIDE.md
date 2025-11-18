  # 🚪 Portal 설정 가이드 (최종)

Quest 시스템 업데이트에 따른 Portal 설정 가이드입니다.

---

## 🗺️ **Portal 구조 설계**

### 게임 진행 흐름
```
Village (시작)
  ↓ [Stage1 필요]
Forest (슬라임2 + 박쥐2)
  ↓ Village 복귀 → 무기 1차 강화
  ↓ [Stage2 필요]
Cave (박쥐5 + 해골5)
  ↓ Forest 복귀 → Village 복귀 → 무기 2차 강화
  ↓ [Stage6 필요]
Boss (거대 버섯)
```

### Portal 목록

| From Scene | To Scene | Required Stage | 차단 메시지 |
|-----------|----------|---------------|----------|
| **Village** → Forest | 03_ForestScene | `Stage1_ForestHunt` (1) | "칼자루를 먼저 찾아야 해!" |
| **Forest** → Village | 02_VillageScene | 없음 | - |
| **Forest** → Cave | 04_CaveScene | `Stage2_WeaponUpgrade1` (2) | "무기 강화를 잊지 않았어?" |
| **Forest** → Boss | 07_BossScene | `Stage6_WeaponUpgrade2` (6) | "최종 무기가 필요해!" |
| **Cave** → Forest | 03_ForestScene | 없음 | - |

---

## 🛠️ **Unity Editor 작업 순서**

### ⚠️ **작업 전 준비**

1. **기존 Portal 제거**
   - `02_VillageScene`에 있는 `Portal_ToCave` GameObject 삭제
   - 기존 Scene들의 `TutorialScene`으로 가는 Portal들은 그대로 두고 수정만 함

---

## 1️⃣ **VillageScene (02_VillageScene)**

### ✅ Portal 1개만 필요: Village → Forest

#### GameObject 생성
```
Name: Portal_ToForest
Transform:
  Position: (X: -3.0, Y: 2.0, Z: 0)  // 적절한 위치로 조정
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)
```

#### PortalTrigger 컴포넌트 설정
```
Portal Settings:
  ├─ Target Scene Name: "03_ForestScene"
  ├─ Spawn Position: (0, 0.7, 0)
  └─ Use Custom Spawn Position: ☐

Quest Stage Requirements:
  ├─ Requires Quest Stage: ☑
  ├─ Required Stage: Stage1_ForestHunt
  └─ Blocked Message: "칼자루를 먼저 찾아야 해!"

Debug:
  └─ Show Debug Messages: ☑
```

#### Box Collider 2D
```
Is Trigger: ☑ (필수!)
Size: (1.0, 0.2)
Offset: (0, 0)
```

---

## 2️⃣ **ForestScene (03_ForestScene)**

### Portal 3개 필요

#### Portal 1: Forest → Village (복귀용)

**기존 Portal 수정** (TutorialScene → VillageScene으로 변경)

```
Name: Portal_ToVillage
Target Scene Name: "02_VillageScene"
Requires Quest Stage: ☐ (체크 해제)
```

#### Portal 2: Forest → Cave

**새로 생성**

```
Name: Portal_ToCave

Transform:
  Position: (X: 5.0, Y: 2.0, Z: 0)  // 숲 오른쪽

Portal Settings:
  ├─ Target Scene Name: "04_CaveScene"
  ├─ Spawn Position: (0, 0.7, 0)
  └─ Use Custom Spawn Position: ☐

Quest Stage Requirements:
  ├─ Requires Quest Stage: ☑
  ├─ Required Stage: Stage2_WeaponUpgrade1
  └─ Blocked Message: "무기 강화를 잊지 않았어?"

Box Collider 2D:
  ├─ Is Trigger: ☑
  ├─ Size: (1.0, 0.2)
  └─ Offset: (0, 0)
```

#### Portal 3: Forest → Boss

**새로 생성**

```
Name: Portal_ToBoss

Transform:
  Position: (X: -5.0, Y: 2.0, Z: 0)  // 숲 왼쪽

Portal Settings:
  ├─ Target Scene Name: "07_BossScene"
  ├─ Spawn Position: (0, 0.7, 0)
  └─ Use Custom Spawn Position: ☐

Quest Stage Requirements:
  ├─ Requires Quest Stage: ☑
  ├─ Required Stage: Stage6_WeaponUpgrade2
  └─ Blocked Message: "최종 무기가 필요해!"

Box Collider 2D:
  ├─ Is Trigger: ☑
  ├─ Size: (1.0, 0.2)
  └─ Offset: (0, 0)
```

---

## 3️⃣ **CaveScene (04_CaveScene)**

### Portal 1개: Cave → Forest (복귀용)

**기존 Portal 수정**

```
Name: Portal_ToForest (또는 Portal_BackToForest)

Target Scene Name: "03_ForestScene"
Requires Quest Stage: ☐ (체크 해제)
Spawn Position: (0, 0.7, 0)
```

---

## 4️⃣ **BossScene (07_BossScene)**

### 참고 사항

- 보스 처치 후 Village로 **자동 복귀**되므로 Portal 불필요
- 스크립트에서 Scene 전환 처리

---

## 📋 **작업 체크리스트**

### VillageScene (02_VillageScene)
- [ ] 기존 `Portal_ToCave` 삭제 (있다면)
- [ ] `Portal_ToForest` 생성
  - [ ] PortalTrigger 컴포넌트 추가
  - [ ] Target: `03_ForestScene`
  - [ ] Required Stage: `Stage1_ForestHunt`
  - [ ] Box Collider 2D `Is Trigger` 체크
- [ ] Scene 저장 (Ctrl+S)

### ForestScene (03_ForestScene)
- [ ] 기존 Portal을 `Portal_ToVillage`로 수정
  - [ ] Target: `02_VillageScene`
  - [ ] Stage 조건 제거
- [ ] `Portal_ToCave` 생성
  - [ ] Target: `04_CaveScene`
  - [ ] Required Stage: `Stage2_WeaponUpgrade1`
  - [ ] Box Collider 2D 추가
- [ ] `Portal_ToBoss` 생성
  - [ ] Target: `07_BossScene`
  - [ ] Required Stage: `Stage6_WeaponUpgrade2`
  - [ ] Box Collider 2D 추가
- [ ] Scene 저장 (Ctrl+S)

### CaveScene (04_CaveScene)
- [ ] 기존 Portal을 `Portal_ToForest`로 수정
  - [ ] Target: `03_ForestScene`
  - [ ] Stage 조건 제거
- [ ] Scene 저장 (Ctrl+S)

---

## 🧪 **테스트 시나리오**

### Stage 0 (게임 시작)
```
Village:
  ✓ Forest Portal → 차단 ❌ "칼자루를 먼저 찾아야 해!"
```

### Stage 1 (칼자루 획득)
```
Village:
  ✓ Forest Portal → 진입 가능 ✅

Forest:
  ✓ Village Portal → 복귀 가능 ✅
  ✓ Cave Portal → 차단 ❌ "무기 강화를 잊지 않았어?"
  ✓ Boss Portal → 차단 ❌
```

### Stage 2 (무기 1차 강화)
```
Forest:
  ✓ Village Portal → 복귀 가능 ✅
  ✓ Cave Portal → 진입 가능 ✅
  ✓ Boss Portal → 차단 ❌

Cave:
  ✓ Forest Portal → 복귀 가능 ✅
```

### Stage 6 (무기 2차 강화)
```
Forest:
  ✓ Village Portal → 복귀 가능 ✅
  ✓ Cave Portal → 진입 가능 ✅
  ✓ Boss Portal → 진입 가능 ✅
```

---

## 🎨 **Portal 시각화 (선택 사항)**

Portal을 눈에 보이게 만들기:

### 방법 1: Sprite 추가
```
Portal GameObject 선택
→ Add Component → Sprite Renderer
→ Sprite: 포탈 이미지 (빛나는 원 등)
→ Color: 파란색 / 초록색
→ Sorting Layer: Default
→ Order in Layer: 5
```

### 방법 2: 텍스트 표시
```
Portal GameObject 우클릭
→ UI → Text - TextMeshPro
→ Text: "→ 마을" / "→ 숲" / "→ 동굴" / "→ 보스"
→ Font Size: 1.5
→ Alignment: Center
```

### 방법 3: Particle Effect
```
Portal GameObject 우클릭
→ Effects → Particle System
→ 빛나는 입자 효과 설정
```

---

## 🔧 **문제 해결**

### Portal이 작동하지 않을 때

**증상 1: Portal에 들어가도 반응 없음**
```
원인:
  - Box Collider 2D의 Is Trigger 체크 안 됨
  - Player에 Collider2D 없음
  - Player Tag가 "Player"가 아님

해결:
  1. Portal GameObject 선택
  2. Box Collider 2D → Is Trigger ☑ 확인
  3. Player GameObject → Tag: "Player" 확인
```

**증상 2: Scene 로드 실패**
```
원인:
  - Scene 이름 오타
  - Build Settings에 Scene 미등록

해결:
  1. File → Build Settings
  2. Scene이 리스트에 있는지 확인
  3. Target Scene Name 철자 확인
     - "03_ForestScene" (O)
     - "ForestScene" (X)
```

**증상 3: Stage 조건이 무시됨**
```
원인:
  - Requires Quest Stage 체크 안 됨
  - QuestManager가 Scene에 없음

해결:
  1. Portal → Requires Quest Stage ☑ 확인
  2. Hierarchy에서 QuestManager 찾기
  3. Console에서 Debug 메시지 확인
```

---

## 📊 **Portal 구조 요약**

```
         Village
            ↓
         [Stage1]
            ↓
    ┌─── Forest ───┐
    │       ↓       │
    │   [Stage2]   │
    │       ↓       │
    │     Cave ─────┘
    │
    │   [Stage6]
    │       ↓
    └──→  Boss
```

**복귀 경로:**
- Cave → Forest → Village (PlayerSpawn으로 소환)
- Boss → Village (스크립트 자동 처리)

---

## ✅ **완료 후 확인**

1. **모든 Portal 테스트**
   - Play Mode로 게임 실행
   - 각 Stage별 Portal 진입/차단 확인

2. **Console 확인**
   ```
   ✅ Portal access granted - Stage requirement met (Stage1_ForestHunt)
   ❌ Portal access denied - Stage Stage2_WeaponUpgrade1 required
   🌀 PortalTrigger: Player entered portal 'Portal_ToForest'
   ```

3. **PlayerSpawn 확인**
   - Village Scene에 `PlayerSpawn` Tag 있는지 확인
   - 복귀 시 올바른 위치에 소환되는지 확인

---

수고하셨습니다! 🎉
