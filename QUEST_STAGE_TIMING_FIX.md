# 🔧 Quest Stage Timing Fix Guide

## 📋 문제 상황

**증상**: NPC와 상호작용(E키)하는 순간 Quest Stage가 즉시 변경되어, 현재 스테이지 대화가 아닌 다음 스테이지 대화가 표시됨.

### 문제 발생 원인:

```
1. E키 누름
2. StartInteraction() 호출
3. currentStage = GetCurrentStage() 실행
4. ❌ 하지만 같은 프레임 또는 직전에 다른 시스템이 스테이지를 변경함
5. 잘못된 스테이지의 대화가 표시됨
```

---

## 🔍 문제의 근본 원인

### Quest Stage를 변경하는 3가지 시스템:

#### 1. **StageCompletionTracker** (아이템 수집 완료)
[StageCompletionTracker.cs:223](Assets/Scripts/StageCompletionTracker.cs#L223)

**Before (문제):**
```csharp
// 대화 시작
DialogueManager.Instance.StartDialogue(...);

// 즉시 스테이지 변경 ❌
AdvanceQuestStage();

// 2초 대기
yield return new WaitForSeconds(transitionDelay);
```

**After (해결):**
```csharp
// 대화 시작
DialogueManager.Instance.StartDialogue(...);

// 대화가 끝날 때까지 대기 ✅
while (DialogueManager.Instance.IsOpen())
{
    yield return null;
}

// 대화 종료 후 스테이지 변경 ✅
AdvanceQuestStage();
```

---

#### 2. **NPCController** (NPC 대화 완료)
[NPCController.cs:279-299](Assets/Scripts/NPCController.cs#L279-L299)

**원래 구조 (올바름):**
```csharp
// 대화 종료 대기
while (DialogueManager.Instance.IsOpen())
{
    yield return null;
}

// 대화 종료 후 스테이지 변경 ✅
QuestManager.Instance.AdvanceStage();
```

**추가 개선:**
- **Stage Locking**: E키를 누르는 순간 스테이지를 잠금
- 대화 중 다른 곳에서 스테이지가 변경되어도 올바른 대화 표시

[NPCController.cs:235-241](Assets/Scripts/NPCController.cs#L235-L241)
```csharp
// 🔒 대화 시작 전에 스테이지를 잠금
lockedStageForDialogue = QuestManager.Instance.GetCurrentStage();
currentStage = lockedStageForDialogue;

// 이제 대화 도중 스테이지가 변경되어도
// lockedStageForDialogue를 사용하므로 올바른 대화가 표시됨
NPCDialogueSet dialogueSet = npcData.GetDialogueForStage(lockedStageForDialogue);
```

---

#### 3. **QuestManager** (Stage8 → EndingScene)
[QuestManager.cs:100-124](Assets/Scripts/QuestManager.cs#L100-L124)

**Stage8 도달 시 자동 EndingScene 로드:**
```csharp
private void OnStageChanged(QuestStage from, QuestStage to)
{
    if (to == QuestStage.Stage8_Ending)
    {
        StartCoroutine(LoadEndingScene());
    }
}

private IEnumerator LoadEndingScene()
{
    yield return new WaitForSeconds(0.5f);
    SceneManager.LoadScene("08_EndingScene");
}
```

---

## ✅ 해결책 요약

### 1. StageCompletionTracker 수정
- **대화 시작 → 대화 종료 대기 → 스테이지 변경**
- 플레이어가 대화를 끝내기 전에는 스테이지가 변경되지 않음

### 2. NPCController 개선
- **Stage Locking**: E키를 누르는 순간 스테이지를 잠금
- **중복 상호작용 방지**: 대화 중이거나 스테이지 변경 대기 중이면 무시

### 3. QuestManager 자동 전환
- Stage8 도달 시 자동으로 EndingScene 로드

---

## 🎮 전체 Quest Flow (수정 후)

### Stage1 완료 (ForestScene):
```
슬라임 잔해2 + 박쥐 뼈2 획득
  ↓
대화 시작: "칼이 무딘것 같아..."
  ↓
플레이어 대화 진행 (Space/Enter)
  ↓
대화 종료 ✅
  ↓
Stage1 → Stage2 변경 ✅
  ↓
0.5초 후 VillageScene 로드
```

### NPC_ChungBoong과 상호작용 (Stage2):
```
E키 누름
  ↓
🔒 lockedStageForDialogue = Stage2 (잠금!)
  ↓
Stage2 대화 표시 ✅ (올바른 대화!)
  ↓
플레이어 대화 진행
  ↓
대화 종료 ✅
  ↓
advanceStageOnComplete가 true면 Stage3로 변경
```

### Boss 처치 후 (Stage7):
```
보스 처치
  ↓
Village 복귀 (Stage7 유지)
  ↓
NPC_Puangi에게 접근
  ↓
E키 누름
  ↓
🔒 lockedStageForDialogue = Stage7 (잠금!)
  ↓
Stage7 대화 표시: "거대 버섯을 처치했구나!"
  ↓
대화 종료 ✅
  ↓
Stage7 → Stage8 변경
  ↓
QuestManager.OnStageChanged() 감지
  ↓
0.5초 후 EndingScene 자동 로드 ✅
```

---

## 🔒 Stage Locking 시스템

### 작동 원리:

```csharp
// 1. E키를 누르는 순간
private void StartInteraction()
{
    // 현재 스테이지를 잠금
    lockedStageForDialogue = QuestManager.Instance.GetCurrentStage();

    // 이제 이 대화는 잠긴 스테이지를 사용
    NPCDialogueSet dialogueSet = npcData.GetDialogueForStage(lockedStageForDialogue);

    // 대화 표시
    ShowDialogue(dialogueSet.dialogueLines);
}

// 2. 대화 종료 후
private IEnumerator AdvanceStageAfterDialogue()
{
    // 대화가 끝날 때까지 대기
    while (DialogueManager.Instance.IsOpen())
    {
        yield return null;
    }

    // 이제 스테이지 변경
    QuestManager.Instance.AdvanceStage();
}
```

### 왜 안전한가?

```
Timeline:
T0: E키 누름 → lockedStageForDialogue = Stage2 🔒
T1: Stage2 대화 표시
T2: (다른 시스템이 Stage3로 변경해도...)
T3: 여전히 Stage2 대화가 표시됨 (잠겨있음!)
T4: 대화 종료
T5: Stage가 이미 Stage3이면 다시 변경하지 않음
```

---

## 🐛 Troubleshooting

### 문제 1: 여전히 잘못된 대화가 표시됨

**확인 사항:**
1. Console에서 `🔒 Locked stage for dialogue` 로그 확인
2. 해당 로그의 스테이지가 올바른지 확인

**해결:**
- NPCController의 `showDebugMessages = true` 설정
- Console에서 스테이지 변경 흐름 추적

---

### 문제 2: 대화가 중복으로 시작됨

**원인:** 여러 NPC가 동시에 상호작용 시도

**해결:** 이미 구현됨!
```csharp
// 이미 대화 중이면 무시
if (DialogueManager.Instance.IsOpen())
{
    return;
}

// 스테이지 변경 대기 중이면 무시
if (isWaitingForStageAdvance)
{
    return;
}
```

---

### 문제 3: Stage8에 도달했는데 EndingScene으로 안 넘어감

**확인 사항:**
1. QuestManager에 `OnStageChanged()` 메서드 있는지 확인
2. Console에서 `🎬 Stage8 reached!` 로그 확인

**해결:**
- QuestManager.cs 최신 버전 확인
- `08_EndingScene`이 Build Settings에 추가되었는지 확인

---

## 📊 스테이지 변경 타이밍 비교

### Before (문제):
```
Stage1 아이템 획득
  ↓
대화 시작
  ↓ (동시에)
Stage2로 변경 ❌
  ↓
NPC와 상호작용
  ↓
Stage2 대화 표시 (Stage1 대화를 놓침!)
```

### After (해결):
```
Stage1 아이템 획득
  ↓
대화 시작 (Stage1 잠금 🔒)
  ↓
대화 진행
  ↓
대화 종료 ✅
  ↓
Stage2로 변경
  ↓
NPC와 상호작용
  ↓
Stage2 대화 표시 (올바름!)
```

---

## 🎯 핵심 원칙

### 1. **대화 종료 후 스테이지 변경**
모든 시스템이 이 원칙을 따름:
- StageCompletionTracker ✅
- NPCController ✅
- QuestManager (Stage8) ✅

### 2. **Stage Locking**
대화 시작 시 스테이지를 잠가서 대화 도중 변경 방지

### 3. **중복 방지**
대화 중이거나 스테이지 변경 대기 중일 때 새로운 상호작용 무시

---

## 🔍 디버그 로그 예시

### 정상 작동 시:
```
📦 Inventory Check - Slime: 2/2, Bat: 2/2
🎉 Stage1 목표 달성! Village로 복귀합니다.
💬 대화 시작: "칼이 무딘것 같아..."
(플레이어가 대화 진행...)
📈 Quest Stage advanced: Stage1 → Stage2
🌀 Transitioning to Scene: 02_VillageScene

(Village 도착 후 NPC_ChungBoong과 상호작용)
🔒 Locked stage for dialogue: Stage2_WeaponUpgrade1
💬 Started dialogue with 청붕이 (Locked Stage: Stage2_WeaponUpgrade1)
(플레이어가 대화 진행...)
📈 Quest advanced by 청붕이
📈 Quest Advanced: Stage2_WeaponUpgrade1 → Stage3_CaveExploration
```

---

## 📝 Summary

### 변경된 파일:
1. **StageCompletionTracker.cs** - 대화 종료 후 스테이지 변경
2. **NPCController.cs** - Stage Locking + 중복 방지
3. **QuestManager.cs** - Stage8 → EndingScene 자동 전환

### 핵심 개선:
- ✅ 대화 중 스테이지 변경 방지
- ✅ 올바른 스테이지의 대화 표시
- ✅ 중복 상호작용 방지
- ✅ Stage8 자동 엔딩 전환

이제 모든 Quest 진행이 올바른 순서로 작동합니다! 🎉
