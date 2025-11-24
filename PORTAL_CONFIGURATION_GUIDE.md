# 🌀 Portal Configuration Guide

## 📋 Portal System Overview

이 가이드는 Puang Adventure의 모든 포탈 설정을 정리한 문서입니다.

포탈 시스템은 **PortalTrigger** 컴포넌트를 사용하여 씬 간 이동을 처리합니다.

---

## 🎯 Portal Configuration List

### Scene 00: TitleScene
- ❌ **포탈 없음** (버튼 클릭으로 자동 이동)

### Scene 01: InitialScene
- ❌ **포탈 없음** (조건 만족 시 자동 이동)

---

### Scene 02: VillageScene

#### Portal_ToForest
```
Target Scene Name: 03_ForestScene
Target Spawn Point Name: Portal_ToVillage
Quest Stage Required: ✅ Stage1_ForestHunt (이상)
```

**설정 방법:**
1. VillageScene에서 `Portal_ToForest` GameObject 선택
2. PortalTrigger 컴포넌트 설정:
   - Target Scene Name: `03_ForestScene`
   - Target Spawn Point Name: `Portal_ToVillage`
   - Requires Quest Stage: ✅ 체크
   - Required Stage: `Stage1_ForestHunt`

---

### Scene 03: ForestScene

#### Portal_ToVillage
```
Target Scene Name: 02_VillageScene
Target Spawn Point Name: Portal_ToForest
Quest Stage Required: ❌ (항상 사용 가능)
```

**설정 방법:**
1. ForestScene에서 `Portal_ToVillage` GameObject 선택
2. PortalTrigger 컴포넌트 설정:
   - Target Scene Name: `02_VillageScene`
   - Target Spawn Point Name: `Portal_ToForest`
   - Requires Quest Stage: ❌ 체크 해제

---

#### Portal_ToCave
```
Target Scene Name: 04_CaveScene
Target Spawn Point Name: Portal_ToForest
Quest Stage Required: ✅ Stage3_CaveExploration (이상)
```

**설정 방법:**
1. ForestScene에서 `Portal_ToCave` GameObject 선택
2. PortalTrigger 컴포넌트 설정:
   - Target Scene Name: `04_CaveScene`
   - Target Spawn Point Name: `Portal_ToForest`
   - Requires Quest Stage: ✅ 체크
   - Required Stage: `Stage3_CaveExploration`

---

#### Portal_ToBoss
```
Target Scene Name: 07_BossScene
Target Spawn Point Name: (비어있음 - PlayerSpawn 태그 사용)
Quest Stage Required: ✅ Stage7_FinalBoss (이상)
```

**설정 방법:**
1. ForestScene에서 `Portal_ToBoss` GameObject 선택
2. PortalTrigger 컴포넌트 설정:
   - Target Scene Name: `07_BossScene`
   - Target Spawn Point Name: `` (비어있음)
   - Requires Quest Stage: ✅ 체크
   - Required Stage: `Stage7_FinalBoss`

---

### Scene 04: CaveScene

#### Portal_ToForest
```
Target Scene Name: 03_ForestScene
Target Spawn Point Name: Portal_ToCave
Quest Stage Required: ❌ (항상 사용 가능)
```

**설정 방법:**
1. CaveScene에서 `Portal_ToForest` GameObject 선택
2. PortalTrigger 컴포넌트 설정:
   - Target Scene Name: `03_ForestScene`
   - Target Spawn Point Name: `Portal_ToCave`
   - Requires Quest Stage: ❌ 체크 해제

---

### Scene 05: PeuangSadScene
- ❌ **포탈 없음** (컷씬 재생 후 자동 이동)

### Scene 06: UnkillableBossScene
- ❌ **포탈 없음** (필패 보스전 후 자동 이동)

### Scene 07: BossScene
- ❌ **포탈 없음** (보스 처치 후 자동 이동)

### Scene 08: EndingScene
- ❌ **포탈 없음** (엔딩 씬)

---

## 🔧 Component Settings

### PortalTrigger Component

#### Portal Settings
- **Target Scene Name**: 이동할 씬 이름 (예: `03_ForestScene`)
- **Target Spawn Point Name**: 목적지 씬에서 스폰될 GameObject 이름
  - 예: `Portal_ToVillage`
  - 비어있으면 `PlayerSpawn` 태그를 가진 오브젝트 사용

#### Quest Stage Requirements
- **Requires Quest Stage**: 특정 퀘스트 단계 요구 여부
- **Required Stage**: 필요한 최소 퀘스트 단계
- **Blocked Message**: 조건 미달 시 표시할 메시지

#### Debug
- **Show Debug Messages**: 디버그 로그 출력 여부

---

### PortalSpawnPoint Component (NEW! ⭐)

목적지 포탈에 이 컴포넌트를 추가하면 **로컬 오프셋**을 적용할 수 있습니다!

#### Spawn Offset
- **Local Offset**: 포탈의 로컬 좌표계 기준 오프셋
  - `Vector3.down * 1f`: 포탈 아래 1유닛에 스폰
  - `Vector3.forward * 2f`: 포탈 앞(파란 화살표 방향) 2유닛에 스폰
  - `Vector3.back * 1.5f`: 포탈 뒤에 스폰
  - `Vector3.left` / `Vector3.right`: 포탈 왼쪽/오른쪽에 스폰
  - 조합 가능: `new Vector3(1, -0.5f, 0)` → 오른쪽 1, 아래 0.5

#### Spawn Direction
- **Match Rotation**: 플레이어가 포탈과 같은 방향을 바라보게 할지 여부

#### Debug
- **Show Debug Gizmos**: Scene View에서 스폰 위치 표시 여부

---

### 🎨 Visual Gizmos (Scene View)

PortalSpawnPoint가 있는 GameObject를 선택하면:
- 🟢 **녹색 구**: 플레이어가 스폰될 정확한 위치
- 🟡 **노란 선**: 포탈에서 스폰 위치까지의 연결선
- 🔵 **파란 화살표**: 플레이어가 바라볼 방향 (Match Rotation 시)

---

## 🎮 How Portal System Works

### 1. 플레이어가 포탈 트리거에 진입
```
Player enters Portal_ToForest
  ↓
Check quest stage requirement (if enabled)
  ↓ (Pass)
Set PlayerPrefs: "TargetSpawnPoint" = "Portal_ToVillage"
  ↓
Load scene: 03_ForestScene
```

### 2. 씬 로드 후 스폰 위치 결정
```
ForestScene loaded
  ↓
PlayerPersistent.OnSceneLoaded()
  ↓
Check PlayerPrefs: "TargetSpawnPoint"
  ↓
Found: "Portal_ToVillage"
  ↓
Find GameObject named "Portal_ToVillage"
  ↓
Move player to that position
  ↓
Delete PlayerPrefs: "TargetSpawnPoint"
```

### 3. 스폰 포인트가 없으면
```
No "TargetSpawnPoint" in PlayerPrefs
  ↓
Find GameObject with tag "PlayerSpawn"
  ↓
Move player to that position
```

---

## 📦 Required GameObject Names

각 씬에 다음 이름의 GameObject가 있어야 합니다:

### 02_VillageScene
- ✅ `Portal_ToForest` (포탈 오브젝트)

### 03_ForestScene
- ✅ `Portal_ToVillage` (스폰 포인트)
- ✅ `Portal_ToCave` (포탈 오브젝트)

### 04_CaveScene
- ✅ `Portal_ToForest` (스폰 포인트)

### 07_BossScene
- ✅ `PlayerSpawn` 태그가 있는 오브젝트

---

## ✅ Setup Checklist

각 포탈을 설정할 때 다음을 확인하세요:

### 출발 포탈 (Portal GameObject)
- [ ] Collider2D 컴포넌트 추가 (BoxCollider2D 또는 CircleCollider2D)
- [ ] Collider의 `Is Trigger` 체크
- [ ] PortalTrigger 컴포넌트 추가
- [ ] Target Scene Name 설정
- [ ] Target Spawn Point Name 설정
- [ ] Quest Stage Requirement 설정 (필요시)

### 도착 포탈 (Destination Scene)
- [ ] 목적지 씬에 스폰 포인트 GameObject 생성
- [ ] GameObject 이름이 `Target Spawn Point Name`과 일치하는지 확인
- [ ] **PortalSpawnPoint 컴포넌트 추가 (권장!)** ⭐
- [ ] Local Offset 설정 (예: `Vector3.down` 또는 `Vector3.back`)
- [ ] Match Rotation 설정 (필요시)
- [ ] Scene View에서 녹색 구로 스폰 위치 확인

---

## 🎯 Quick Setup Example

### Example: Portal_ToVillage 설정 (ForestScene)

#### 1. 출발 포탈 설정 (VillageScene의 Portal_ToForest)
```
Inspector → PortalTrigger:
├─ Target Scene Name: "03_ForestScene"
└─ Target Spawn Point Name: "Portal_ToVillage"
```

#### 2. 도착 포탈 설정 (ForestScene의 Portal_ToVillage)
```
1. Portal_ToVillage GameObject 선택
2. Add Component → PortalSpawnPoint
3. Inspector → PortalSpawnPoint:
   ├─ Local Offset: (0, -1, 0)  ← 포탈 아래 1유닛
   └─ Match Rotation: ✅ 체크
```

#### 3. 결과 확인
- Scene View에서 Portal_ToVillage 선택
- 녹색 구가 포탈 아래에 표시됨
- 플레이어가 해당 위치에 스폰되고 포탈 방향을 바라봄 ✅

---

## 🔍 Troubleshooting

### 문제: 플레이어가 잘못된 위치에 스폰됨
**해결:**
1. 목적지 씬에 지정된 이름의 GameObject가 있는지 확인
2. PortalTrigger의 `Target Spawn Point Name` 철자 확인
3. Console에서 `PlayerPersistent` 로그 확인:
   ```
   📍 PlayerPersistent: Moved to custom spawn point 'Portal_ToVillage' at (x, y, z)
   ```

### 문제: 포탈이 작동하지 않음
**해결:**
1. Collider2D의 `Is Trigger` 체크 확인
2. Player GameObject에 `Player` 태그가 있는지 확인
3. Quest Stage 요구사항 확인
4. Console에서 에러 메시지 확인

### 문제: Quest Stage 조건이 작동하지 않음
**해결:**
1. QuestManager.Instance가 씬에 있는지 확인
2. 현재 QuestStage 확인:
   ```csharp
   QuestManager.Instance.GetCurrentStage()
   ```
3. PortalTrigger의 `Required Stage` 설정 확인

---

## 🎨 Visual Indicators in Scene View

PortalTrigger를 선택하면 Scene View에 다음 정보가 표시됩니다:

```
Portal → 03_ForestScene
📍 Spawn at: Portal_ToVillage
🔒 Requires: Stage1_ForestHunt (if enabled)
```

---

## 📝 Summary Table

| Current Scene | Portal Name | Target Scene | Spawn Point | Quest Required |
|--------------|-------------|--------------|-------------|----------------|
| VillageScene | Portal_ToForest | ForestScene | Portal_ToVillage | Stage1_ForestHunt |
| ForestScene | Portal_ToVillage | VillageScene | Portal_ToForest | ❌ None |
| ForestScene | Portal_ToCave | CaveScene | Portal_ToForest | Stage3_CaveExploration |
| ForestScene | Portal_ToBoss | BossScene | PlayerSpawn | Stage7_FinalBoss |
| CaveScene | Portal_ToForest | ForestScene | Portal_ToCave | ❌ None |

---

## 🚀 Quick Setup Commands

### Scene View에서 포탈 선택 후:
1. Inspector에서 `Target Scene Name` 입력
2. Inspector에서 `Target Spawn Point Name` 입력
3. 필요시 `Requires Quest Stage` 체크 및 `Required Stage` 선택

완료! 🎉
