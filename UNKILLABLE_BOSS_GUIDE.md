# 💀 UnkillableBoss Scene Guide

## 📋 개요

**UnkillableBossScene (Stage5)**는 플레이어가 반드시 져야 하는 특수 보스전입니다.

### 특징:
- ✅ 플레이어가 **한 대 맞거나** 일정 시간 후 **강제 사망**
- ✅ 사망 시 **GameOver가 아닌 Village로 자동 복귀**
- ✅ Quest Stage 자동 진행 (Stage5 → Stage6)
- ✅ 플레이어 체력 자동 회복

---

## 🔧 시스템 구조

### 1. PlayerHealth.cs 수정
[PlayerHealth.cs:17](Assets/Scripts/PlayerHealth.cs#L17), [PlayerHealth.cs:93-107](Assets/Scripts/PlayerHealth.cs#L93-L107), [PlayerHealth.cs:120-130](Assets/Scripts/PlayerHealth.cs#L120-L130)

#### 새로운 기능:
```csharp
private bool ignoreDeathProcessing = false;

public void SetIgnoreDeathProcessing(bool ignore)
```

**작동 방식:**
- `ignoreDeathProcessing = true`: `Die()` 호출 시 `GameManager.OnPlayerDeath()` 건너뜀
- `ignoreDeathProcessing = false`: 일반 사망 처리 (GameOver)

---

### 2. UnkillableBossController.cs 개선
[UnkillableBossController.cs](Assets/Scripts/UnkillableBossController.cs)

#### 주요 변경 사항:

##### A. 씬 시작 시 사망 처리 비활성화
```csharp
private void Start()
{
    DisablePlayerDeathProcessing(); // 일반 GameOver 방지
    // ...
}

private void DisablePlayerDeathProcessing()
{
    playerHealth.SetIgnoreDeathProcessing(true);
}
```

##### B. 플레이어 사망 감지
```csharp
private void CheckPlayerHealth()
{
    // 체력이 최초 체력보다 낮으면 → 강제 사망
    if (playerHealth.GetCurrentHealth() < initialPlayerHealth)
    {
        ForcePlayerDeath();
    }
}
```

##### C. Village 복귀 시 처리
```csharp
private IEnumerator ReturnToVillage()
{
    // 1. Quest Stage 진행
    QuestManager.Instance.AdvanceStage(); // Stage5 → Stage6

    // 2. 플레이어 체력 회복
    playerHealth.ResetHealth();

    // 3. 일반 사망 처리 재활성화
    playerHealth.SetIgnoreDeathProcessing(false);

    // 4. Village로 이동
    SceneManager.LoadScene("02_VillageScene");
}
```

---

## 🎮 작동 흐름

### UnkillableBossScene 진입:
```
1. PeuangSadScene 컷씬 완료
   ↓
2. UnkillableBossScene 로드
   ↓
3. UnkillableBossController.Start():
   - PlayerHealth.SetIgnoreDeathProcessing(true) 설정
   - QuestStage: Stage4 → Stage5
   - 자동 사망 타이머 시작 (10초)
   ↓
4. 플레이어 보스와 전투
   ↓
5. 두 가지 시나리오:
   A) 플레이어가 한 대 맞음 → CheckPlayerHealth() 감지
   B) 10초 경과 → AutoDeathTimer() 발동
   ↓
6. ForcePlayerDeath():
   - 사망 메시지 표시
   - Village 복귀 코루틴 시작
   ↓
7. ReturnToVillage():
   - Stage5 → Stage6 진행
   - 플레이어 체력 회복
   - ignoreDeathProcessing = false (일반 사망 처리 재활성화)
   - 02_VillageScene 로드
   ↓
8. VillageScene 도착:
   - 플레이어 체력 Full
   - Stage6: 무기 2차 강화 가능
```

---

## ⚙️ Inspector 설정

### UnkillableBossController Component

#### Boss Settings
- **Boss Game Object**: 무적 보스 GameObject 연결
- **Boss Invincibility HP**: 999999 (사용 안 함, 향후 확장용)

#### Player Death Settings
- **Instant Death On Hit**: ✅ true (한 대 맞으면 즉사)
- **Auto Death Time**: 10 (초) - 자동 사망까지의 시간

#### Transition Settings
- **Return Scene Name**: `02_VillageScene`
- **Death Message Duration**: 3 (초) - 사망 메시지 표시 시간
- **Show Debug Messages**: ✅ true (디버그 로그 출력)

---

## 🔍 디버그 로그

### 정상 작동 시:
```
💀 UnkillableBossController: Scene started!
✅ UnkillableBossController: Disabled normal death processing
📈 Advanced to Stage5_UnkillableBoss
💔 Player took damage! Forcing death...
📈 Advanced to Stage6_WeaponUpgrade2
💚 Player health restored and death processing re-enabled
🌀 Returning to Village: 02_VillageScene
```

### PlayerHealth 로그:
```
⚠ PlayerHealth: Death processing will be ignored
Player died
⚠ PlayerHealth: Death processing ignored (special scene handling)
✅ PlayerHealth: Death processing re-enabled
```

---

## 🐛 Troubleshooting

### 문제 1: 플레이어가 죽으면 GameOver가 됨
**원인**: `ignoreDeathProcessing`이 설정되지 않음

**해결:**
1. UnkillableBossScene에 UnkillableBossController 컴포넌트가 있는지 확인
2. Console에서 `✅ Disabled normal death processing` 로그 확인
3. 없으면 빈 GameObject 생성 → UnkillableBossController 추가

---

### 문제 2: Village로 복귀하지 않음
**원인**: ReturnToVillage 코루틴이 실행되지 않음

**해결:**
1. Console에서 `ForcePlayerDeath` 로그 확인
2. `Death Message Duration` 시간 후 자동 이동하는지 확인
3. Return Scene Name이 `02_VillageScene`인지 확인

---

### 문제 3: Village 복귀 후에도 GameOver가 됨
**원인**: `SetIgnoreDeathProcessing(false)`가 호출되지 않음

**해결:**
1. ReturnToVillage 코루틴에서 `SetIgnoreDeathProcessing(false)` 호출 확인
2. Console에서 `✅ Death processing re-enabled` 로그 확인

---

## ✅ 테스트 체크리스트

### UnkillableBossScene 진입:
- [ ] Stage5로 자동 전환됨
- [ ] `Disabled normal death processing` 로그 출력
- [ ] 보스가 무적 상태

### 플레이어 사망:
- [ ] 한 대 맞으면 즉사
- [ ] 또는 10초 후 자동 사망
- [ ] GameOver 화면이 **나타나지 않음** ⚠️
- [ ] 사망 메시지 표시: "으아... 꿈 속이었지만..."

### Village 복귀:
- [ ] Stage6으로 자동 전환
- [ ] 플레이어 체력 Full
- [ ] VillageScene으로 이동
- [ ] 일반 사망 처리 재활성화됨

### Village에서:
- [ ] Stage6 확인 (NPC_ChungBoong에게 무기 2차 강화 가능)
- [ ] 플레이어 체력 100%
- [ ] 일반 전투에서 사망 시 GameOver 정상 작동

---

## 📊 Quest Flow

```
Stage4: PeuangSadCutscene
   ↓ (컷씬 완료 후 자동 이동)
Stage5: UnkillableBoss ← UnkillableBossScene
   ↓ (필패 후)
Stage6: WeaponUpgrade2 ← VillageScene
   ↓ (NPC_ChungBoong에게 무기 2차 강화)
Stage7: FinalBoss ← BossScene
   ↓ (보스 처치)
Stage8: Ending
```

---

## 🎯 핵심 포인트

### ✅ DO:
- UnkillableBossScene 진입 시 `SetIgnoreDeathProcessing(true)` 호출
- Village 복귀 시 `SetIgnoreDeathProcessing(false)` 호출
- 플레이어 체력 회복
- Quest Stage 자동 진행

### ❌ DON'T:
- GameManager.OnPlayerDeath() 직접 호출 금지 (이 씬에서)
- Time.timeScale = 0 설정 금지 (게임이 멈춤)
- GameOver 패널 활성화 금지

---

## 🔧 Context Menu (디버그)

Unity Editor에서 UnkillableBossController를 선택하고 우클릭:

- **Debug: Return to Village** - 즉시 Village로 이동

---

## 💡 향후 확장 가능성

### 다른 특수 씬에서도 사용 가능:
```csharp
// 특수 씬 진입 시
playerHealth.SetIgnoreDeathProcessing(true);

// 특수 씬 종료 시
playerHealth.SetIgnoreDeathProcessing(false);
```

### 예시:
- 튜토리얼 씬 (사망 시 재시작 대신 체크포인트로 이동)
- 스토리 이벤트 씬 (사망 시 특정 컷씬 재생)
- 챌린지 모드 (사망 시 특별 보상 지급)

---

## 📝 Summary

UnkillableBossScene은 특수한 사망 처리를 위해:
1. **PlayerHealth에 `ignoreDeathProcessing` 플래그 추가**
2. **UnkillableBossController가 씬 진입/종료 시 플래그 제어**
3. **일반 GameOver 대신 Village로 자동 복귀**

이렇게 구현하여 스토리 흐름을 자연스럽게 유지합니다! 🎉
