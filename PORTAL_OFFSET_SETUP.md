# 🎯 Portal Spawn Offset Quick Setup Guide

## 문제 해결: 포탈 타자마자 다시 포탈 타는 현상

**해결책**: `PortalSpawnPoint` 컴포넌트를 사용하여 로컬 오프셋 적용!

---

## 🚀 빠른 설정 방법

### 1단계: 도착 포탈에 컴포넌트 추가

각 씬의 **도착 포탈 GameObject**를 선택하고:

```
Add Component → PortalSpawnPoint
```

---

### 2단계: 로컬 오프셋 설정

포탈의 **로컬 좌표계**를 기준으로 오프셋을 설정합니다.

#### 🔵 로컬 좌표계 이해하기:
- **Forward (파란 화살표)**: 포탈이 바라보는 방향
- **Up (초록 화살표)**: 포탈의 위쪽
- **Right (빨간 화살표)**: 포탈의 오른쪽

---

## 📋 각 포탈별 권장 오프셋 설정

### VillageScene

#### Portal_ToForest (도착지: ForestScene)
**목표**: ForestScene의 Portal_ToVillage에서 스폰
```
ForestScene → Portal_ToVillage 선택:
├─ Add Component: PortalSpawnPoint
├─ Local Offset: (0, -1, 0) 또는 (0, 0, -1.5)
│   └─ (0, -1, 0) = 포탈 아래 1유닛
│   └─ (0, 0, -1.5) = 포탈 뒤로 1.5유닛
└─ Match Rotation: ✅ 체크 (포탈 방향 바라보기)
```

---

### ForestScene

#### Portal_ToVillage (도착지: VillageScene)
**목표**: VillageScene의 Portal_ToForest에서 스폰
```
VillageScene → Portal_ToForest 선택:
├─ Add Component: PortalSpawnPoint
├─ Local Offset: (0, -1, 0)
│   └─ 포탈 아래 1유닛에 스폰
└─ Match Rotation: ✅ 체크
```

---

#### Portal_ToCave (도착지: CaveScene)
**목표**: CaveScene의 Portal_ToForest에서 스폰
```
CaveScene → Portal_ToForest 선택:
├─ Add Component: PortalSpawnPoint
├─ Local Offset: (0, -1, 0) 또는 (0, 0, -2)
│   └─ 상황에 맞게 조정
└─ Match Rotation: ✅ 체크
```

---

#### Portal_ToBoss (도착지: BossScene)
**목표**: BossScene의 PlayerSpawn에서 스폰
```
BossScene → PlayerSpawn 선택:
├─ Add Component: PortalSpawnPoint (선택사항)
└─ Local Offset: (0, 0, 0)
    └─ 기본 위치 사용 (보스전 시작 위치)
```

---

### CaveScene

#### Portal_ToForest (도착지: ForestScene)
**목표**: ForestScene의 Portal_ToCave에서 스폰
```
ForestScene → Portal_ToCave 선택:
├─ Add Component: PortalSpawnPoint
├─ Local Offset: (0, 0, -1.5)
│   └─ 포탈 뒤로 1.5유닛 (동굴에서 나오는 느낌)
└─ Match Rotation: ✅ 체크
```

---

## 🎨 Visual Testing in Scene View

### Scene View에서 확인하기:

1. **포탈 GameObject 선택**
2. **녹색 구** 확인 → 플레이어 스폰 위치
3. **노란 선** 확인 → 포탈에서 스폰 위치까지의 거리
4. **파란 화살표** 확인 → 플레이어가 바라볼 방향 (Match Rotation 활성화 시)

### 오프셋이 적절한지 확인:
- ✅ 녹색 구가 포탈 트리거 밖에 있는가?
- ✅ 스폰 위치가 포탈에서 충분히 떨어져 있는가?
- ✅ 스폰 위치가 자연스러운 위치인가?

---

## 🔧 오프셋 값 조정 팁

### 일반적인 오프셋 값:

#### 포탈 아래에 스폰:
```csharp
Local Offset: (0, -1, 0)   // 포탈 아래 1유닛
Local Offset: (0, -1.5, 0) // 포탈 아래 1.5유닛
```

#### 포탈 뒤에 스폰:
```csharp
Local Offset: (0, 0, -1)   // 포탈 뒤 1유닛
Local Offset: (0, 0, -2)   // 포탈 뒤 2유닛
```

#### 포탈 앞에 스폰:
```csharp
Local Offset: (0, 0, 1)    // 포탈 앞 1유닛
Local Offset: (0, 0, 2)    // 포탈 앞 2유닛
```

#### 포탈 옆에 스폰:
```csharp
Local Offset: (1, 0, 0)    // 포탈 오른쪽 1유닛
Local Offset: (-1, 0, 0)   // 포탈 왼쪽 1유닛
```

#### 조합 (대각선):
```csharp
Local Offset: (1, -0.5, -1) // 오른쪽 1, 아래 0.5, 뒤로 1
Local Offset: (0, -1, -1.5) // 아래 1, 뒤로 1.5
```

---

## 🎮 In-Game Testing

### 테스트 순서:
1. **ForestScene 진입** (VillageScene의 Portal_ToForest 사용)
2. **스폰 위치 확인**
   - ✅ 포탈 밖에 스폰되었는가?
   - ✅ 즉시 다시 포탈을 타지 않는가?
3. **다시 VillageScene으로 복귀** (Portal_ToVillage 사용)
4. **스폰 위치 확인**
   - ✅ 자연스러운 위치인가?

### 문제가 있으면:
- Scene View에서 오프셋 값 조정
- 테스트 반복

---

## 📊 Summary Table

| Scene | Portal Name | Offset 권장값 | Match Rotation |
|-------|-------------|--------------|----------------|
| VillageScene | Portal_ToForest | `(0, -1, 0)` | ✅ |
| ForestScene | Portal_ToVillage | `(0, -1, 0)` | ✅ |
| ForestScene | Portal_ToCave | `(0, 0, -1.5)` | ✅ |
| CaveScene | Portal_ToForest | `(0, 0, -1.5)` | ✅ |
| BossScene | PlayerSpawn | `(0, 0, 0)` | ❌ |

---

## 🔍 Debugging

### Console 로그 확인:
```
✅ 성공:
📍 PlayerPersistent: Moved to custom spawn point 'Portal_ToVillage' with offset at (10.5, -2.0, 5.3)

❌ 컴포넌트 없음:
📍 PlayerPersistent: Moved to custom spawn point 'Portal_ToVillage' at (10.0, -1.0, 7.0) (no PortalSpawnPoint component)
```

### PortalSpawnPoint가 없으면:
- 기본 포탈 위치에 스폰됨
- 다시 포탈을 탈 수 있음 ⚠️

### PortalSpawnPoint가 있으면:
- 오프셋이 적용된 위치에 스폰됨
- 포탈 밖에 스폰되어 안전함 ✅

---

## ✅ Final Checklist

### 모든 도착 포탈에 대해:
- [ ] PortalSpawnPoint 컴포넌트 추가됨
- [ ] Local Offset 설정 완료
- [ ] Scene View에서 녹색 구 위치 확인
- [ ] In-Game 테스트 완료
- [ ] 포탈 재진입 문제 해결됨

완료! 🎉
