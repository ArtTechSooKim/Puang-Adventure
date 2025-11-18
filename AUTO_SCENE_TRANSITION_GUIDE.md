# 🎬 자동 Scene 전환 시스템 설치 가이드

새로 추가된 4개의 스크립트를 사용하여 자동 Scene 전환 기능을 구현하는 가이드입니다.

---

## 📋 **자동 전환이 필요한 Scene**

| Scene | 조건 | 자동 이동 | 스크립트 |
|-------|------|----------|---------|
| **ForestScene** (Stage1) | 인벤토리: 슬라임 잔해2 + 박쥐 뼈2 | → Village | `StageCompletionTracker` |
| **CaveScene** (Stage3) | 인벤토리: 박쥐 뼈5 + 해골5 | → PeuangSadScene | `StageCompletionTracker` |
| **PeuangSadScene** (컷씬) | 컷씬 재생 완료 | → UnkillableBossScene | `CutsceneAutoLoader` |
| **UnkillableBossScene** | 플레이어 사망 (강제) | → Village | `UnkillableBossController` |
| **BossScene** (Stage7) | 거대 버섯 처치 | → Village | `BossDefeatHandler` |

---

## 🛠️ **설치 순서**

### 1️⃣ **ForestScene - 인벤토리 아이템 확인**

#### Step 1: StageCompletionTracker 추가
1. `03_ForestScene` 열기
2. Hierarchy에서 **Create Empty** → 이름: `StageCompletionTracker`
3. **Add Component** → `StageCompletionTracker` 스크립트 추가

#### Step 2: Inspector 설정
```
Stage 1 - Forest Requirements:
  ├─ Slime Residue Item: (SlimeResidue 드래그)
  ├─ Bat Bone Item: (BatBone 드래그)
  ├─ Stage1 Required Slime Residue: 2
  ├─ Stage1 Required Bat Bone: 2
  └─ Stage1 Target Scene: "02_VillageScene"

Stage 3 - Cave Requirements:
  ├─ Skeleton Bone Item: (SkeletonBone 드래그)
  ├─ Stage3 Required Bat Bone: 5
  ├─ Stage3 Required Skeleton Bone: 5
  └─ Stage3 Target Scene: "05_PeuangSadScene"

Check Settings:
  ├─ Check Interval: 1
  ├─ Transition Delay: 2
  └─ Show Debug Messages: ☑
```

#### Step 3: 아이템 Asset 드래그
1. **Project 창**에서 `Assets/Resources/Items/` 폴더 열기
2. 다음 아이템들을 Inspector의 해당 필드에 드래그:
   - `SlimeResidue.asset` → `Slime Residue Item`
   - `BatBone.asset` → `Bat Bone Item`
   - `SkeletonBone.asset` → `Skeleton Bone Item`

#### Step 4: Scene 저장
- **Ctrl + S**

**작동 원리:**
- 몬스터가 죽으면 아이템이 인벤토리에 자동 추가됨
- StageCompletionTracker가 **1초마다** 인벤토리 확인
- 조건 충족 시 자동으로 Village로 복귀

---

### 2️⃣ **CaveScene - 인벤토리 아이템 확인**

#### Step 1: StageCompletionTracker 추가
1. `04_CaveScene` 열기
2. Hierarchy에서 **Create Empty** → 이름: `StageCompletionTracker`
3. **Add Component** → `StageCompletionTracker` 추가

#### Step 2: Inspector 설정
```
(ForestScene과 동일하게 설정)

Stage 3 - Cave Requirements:
  ├─ Bat Bone Item: (BatBone 드래그)
  ├─ Skeleton Bone Item: (SkeletonBone 드래그)
  ├─ Stage3 Required Bat Bone: 5
  ├─ Stage3 Required Skeleton Bone: 5
  └─ Stage3 Target Scene: "05_PeuangSadScene"
```

#### Step 3: 아이템 Asset 드래그
- `BatBone.asset` → `Bat Bone Item`
- `SkeletonBone.asset` → `Skeleton Bone Item`
- `SlimeResidue.asset` → `Slime Residue Item` (Stage1용, 필수는 아님)

#### Step 4: Scene 저장

**작동 원리:**
- 박쥐 5마리 + 해골 5마리 처치 시 인벤토리에 아이템 축적
- 조건 충족 시 PeuangSadScene으로 자동 이동

---

### 3️⃣ **PeuangSadScene - 컷씬 자동 재생**

#### Step 1: CutsceneAutoLoader 추가
1. `05_PeuangSadScene` 열기
2. Hierarchy에서 **Create Empty** → 이름: `CutsceneController`
3. **Add Component** → `CutsceneAutoLoader` 추가

#### Step 2: Inspector 설정
```
Cutscene Settings:
  ├─ Cutscene Dialogues: (Array Size: 2)
  │   ├─ Element 0: "저 거대 버섯 고기가 그렇게 맛있다던데.. 푸앙이는 힘이 없어 사냥도 못한다 퓨앙!"
  │   └─ Element 1: "저게 푸앙이가 원하던 거대 버섯...! 재빨리 해치우자."
  ├─ Next Scene Name: "06_UnkillableBossScene"
  ├─ Dialogue Wait Time: 3
  ├─ Transition Delay: 1
  └─ Show Debug Messages: ☑
```

#### Step 3: Scene 저장

---

### 4️⃣ **UnkillableBossScene - 강제 사망 및 복귀**

#### Step 1: UnkillableBossController 추가
1. `06_UnkillableBossScene` 열기
2. Hierarchy에서 **Create Empty** → 이름: `BossController`
3. **Add Component** → `UnkillableBossController` 추가

#### Step 2: Inspector 설정
```
Boss Settings:
  ├─ Boss Game Object: (Boss GameObject 드래그)
  └─ Boss Invincibility HP: 999999

Player Death Settings:
  ├─ Instant Death On Hit: ☑
  └─ Auto Death Time: 10

Transition Settings:
  ├─ Return Scene Name: "02_VillageScene"
  ├─ Death Message Duration: 3
  └─ Show Debug Messages: ☑
```

#### Step 3: Boss GameObject 설정
1. Boss GameObject에 `Boss` Tag 추가
2. Boss 체력 999999 설정

#### Step 4: Scene 저장

---

### 5️⃣ **BossScene - 보스 처치 후 복귀**

#### Step 1: BossDefeatHandler 추가
1. `07_BossScene` 열기
2. Hierarchy에서 **Create Empty** → 이름: `BossDefeatHandler`
3. **Add Component** → `BossDefeatHandler` 추가

#### Step 2: Inspector 설정
```
Boss Settings:
  ├─ Boss Game Object: (최종 보스 GameObject 드래그)
  └─ Boss Tag: "Boss"

Transition Settings:
  ├─ Return Scene Name: "02_VillageScene"
  ├─ Victory Message Duration: 4
  └─ Show Debug Messages: ☑
```

#### Step 3: Boss GameObject 설정
1. 최종 보스 GameObject에 `Boss` Tag 추가

#### Step 4: Scene 저장

---

## 🧪 **테스트 방법**

### Stage1 테스트 (ForestScene)
```
1. Play Mode 시작
2. QuestManager Stage를 Stage1로 설정
3. ForestScene으로 이동
4. DebugItemGiver (F1) 또는 몬스터 처치로 아이템 획득:
   - 슬라임 잔해 x2
   - 박쥐 뼈 x2
5. 자동으로 대화 팝업 → Village로 복귀 확인
```

### Stage3 테스트 (CaveScene)
```
1. QuestManager Stage를 Stage3로 설정
2. CaveScene으로 이동
3. 아이템 획득:
   - 박쥐 뼈 x5
   - 해골 뼈 x5
4. 자동으로 대화 팝업 → PeuangSadScene으로 이동 확인
```

### 디버그 테스트 (빠른 테스트)
```
1. ForestScene 또는 CaveScene 로드
2. Hierarchy에서 StageCompletionTracker 선택
3. Inspector → Component ⋮ 메뉴
4. "Debug: Give Required Items" 클릭
5. 인벤토리에 필요 아이템 자동 추가
6. 1초 후 자동 Scene 전환 확인
```

---

## 🔧 **디버그 기능**

### StageCompletionTracker
```
Context Menu:
  ├─ Debug: Print Inventory (현재 인벤토리 아이템 출력)
  └─ Debug: Give Required Items (필요 아이템 자동 추가)
```

**사용 방법:**
1. Hierarchy에서 StageCompletionTracker GameObject 선택
2. Inspector에서 Component의 ⋮ 메뉴 클릭
3. Debug 메뉴 선택

### CutsceneAutoLoader
```
Context Menu:
  └─ Debug: Skip Cutscene (컷씬 건너뛰기)
```

### UnkillableBossController
```
Context Menu:
  └─ Debug: Return to Village (즉시 Village 복귀)
```

### BossDefeatHandler
```
Context Menu:
  └─ Debug: Force Boss Defeat (보스 강제 처치)
```

---

## ⚠️ **주의사항**

### 1. 아이템 Asset 드래그 필수
- **반드시** Inspector에서 아이템 Asset을 드래그해야 함
- 경로: `Assets/Resources/Items/`
  - `SlimeResidue.asset`
  - `BatBone.asset`
  - `SkeletonBone.asset`

### 2. 몬스터가 아이템을 드롭해야 함
- 몬스터 처치 시 해당 아이템이 인벤토리에 추가되어야 함
- 아이템 자동 귀속 확인:
  - Slime → SlimeResidue
  - Bat → BatBone
  - Skeleton → SkeletonBone

### 3. Boss Tag 설정 필수
- UnkillableBossScene의 Boss: `Boss` Tag
- BossScene의 최종 보스: `Boss` Tag

### 4. Inventory 시스템 확인
- `Inventory.instance` 접근 가능해야 함
- `InventorySlot` 구조 확인:
  - `itemData` (ItemData)
  - `count` (int)

---

## 📊 **전체 흐름 요약**

```
Stage0 (Village)
  → 칼자루 획득
  ↓
Stage1 (Forest)
  → 슬라임2 + 박쥐2 처치 → 아이템 획득
  → 🎬 자동: 아이템 확인 → Village 복귀
  ↓
Stage2 (Village)
  → 중붕이: 아이템 소비 → 무기 1차 강화
  ↓
Stage3 (Cave)
  → 박쥐5 + 해골5 처치 → 아이템 획득
  → 🎬 자동: 아이템 확인 → PeuangSadScene
  ↓
Stage4 (PeuangSadScene)
  → 🎬 자동: 컷씬 재생 → UnkillableBossScene
  ↓
Stage5 (UnkillableBossScene)
  → 🎬 자동: 플레이어 사망 → Village
  ↓
Stage6 (Village)
  → 중붕이: 아이템 소비 → 무기 2차 강화
  ↓
Stage7 (BossScene)
  → 거대 버섯 처치
  → 🎬 자동: Village 복귀
  ↓
Stage8 (Village)
  → 푸앙이와 대화 → Ending
```

---

## ✅ **설치 체크리스트**

### ForestScene (03_ForestScene)
- [ ] `StageCompletionTracker` GameObject 생성
- [ ] StageCompletionTracker 컴포넌트 추가
- [ ] `SlimeResidue.asset` 드래그
- [ ] `BatBone.asset` 드래그
- [ ] `SkeletonBone.asset` 드래그
- [ ] 필요 개수 설정 (Slime: 2, Bat: 2)
- [ ] Scene 저장

### CaveScene (04_CaveScene)
- [ ] `StageCompletionTracker` GameObject 생성
- [ ] StageCompletionTracker 컴포넌트 추가
- [ ] `BatBone.asset` 드래그
- [ ] `SkeletonBone.asset` 드래그
- [ ] 필요 개수 설정 (Bat: 5, Skeleton: 5)
- [ ] Scene 저장

### PeuangSadScene (05_PeuangSadScene)
- [ ] `CutsceneController` GameObject 생성
- [ ] CutsceneAutoLoader 컴포넌트 추가
- [ ] 대화 내용 입력 (2개)
- [ ] Next Scene: "06_UnkillableBossScene"
- [ ] Scene 저장

### UnkillableBossScene (06_UnkillableBossScene)
- [ ] `BossController` GameObject 생성
- [ ] UnkillableBossController 컴포넌트 추가
- [ ] Boss GameObject 드래그
- [ ] Boss Tag: "Boss" 설정
- [ ] Scene 저장

### BossScene (07_BossScene)
- [ ] `BossDefeatHandler` GameObject 생성
- [ ] BossDefeatHandler 컴포넌트 추가
- [ ] Boss GameObject 드래그
- [ ] Boss Tag: "Boss" 설정
- [ ] Scene 저장

---

수고하셨습니다! 🎉

이제 몬스터 처치 → 아이템 획득 → 자동 Scene 전환이 작동합니다.
