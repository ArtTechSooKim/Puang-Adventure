# 🏆 Boss Defeat Flow Guide

## 📋 최종 보스 처치 후 흐름

### ⚠️ 중요 변경 사항
**Stage7 → Stage8 진행 시점**: 보스 처치 시 ❌ → **NPC_Puangi와 대화 시** ✅

---

## 🎮 올바른 Quest Flow

### Stage7: FinalBoss

#### 1. 보스 처치 (BossScene)
```
플레이어가 거대 버섯 보스 처치
  ↓
BossDefeatHandler.OnBossDefeated():
  - ✅ 승리 메시지 표시: "이제 푸앙이에게 이걸 가져다 주자..!"
  - ✅ Village로 자동 복귀
  - ❌ Stage 진행하지 않음 (Stage7 유지)
  ↓
VillageScene 도착 (Stage7 상태)
```

#### 2. NPC_Puangi와 대화 (VillageScene)
```
플레이어가 NPC_Puangi에게 접근
  ↓
E키로 대화 시작
  ↓
NPCController.StartInteraction():
  - NPC_Puangi의 Stage7 대화 재생
  - 대화 종료 후
  ↓
AdvanceStage() 호출 (advanceStageOnComplete = true)
  ↓
✅ Stage7 → Stage8_Ending
  ↓
EndingScene으로 이동 (또는 엔딩 트리거)
```

---

## 🔧 BossDefeatHandler.cs 수정

[BossDefeatHandler.cs:77-94](Assets/Scripts/BossDefeatHandler.cs#L77-L94)

### Before (잘못된 방식):
```csharp
private void OnBossDefeated()
{
    // Stage 즉시 진행 ❌
    QuestManager.Instance.AdvanceStage(); // Stage7 → Stage8

    // Village로 복귀
    StartCoroutine(ShowVictoryAndReturn());
}
```

### After (올바른 방식):
```csharp
private void OnBossDefeated()
{
    // Stage는 NPC_Puangi와 대화할 때 진행됨 ✅
    // 여기서는 Stage 진행하지 않음!
    Debug.Log("⏸ Stage remains at Stage7 - will advance when talking to NPC_Puangi");

    // Village로 복귀
    StartCoroutine(ShowVictoryAndReturn());
}
```

---

## 📦 NPC_Puangi 설정

### NPC_Puangi.asset (Stage7 대화 설정)

#### Inspector 설정:
```
NPC Dialogue Sets:
└─ Stage7_FinalBoss:
   ├─ Dialogue Lines:
   │  └─ "푸앙이: 정말 거대 버섯을 처치했구나! 대단해! 퓨앙!"
   │  └─ "푸앙이: 이제 마을에 평화가 찾아왔어! 고마워!"
   ├─ Required Items: (비어있음 또는 보스 고기 등)
   ├─ Reward Items: (보상 아이템 등)
   └─ Advance Stage On Complete: ✅ true ← 중요!
```

### 체크 포인트:
- [ ] Stage7_FinalBoss 대화 세트 존재
- [ ] `advanceStageOnComplete = true` 설정됨
- [ ] 대화 내용이 엔딩으로 연결되는 내용

---

## 🎯 스토리 흐름

### 플레이어 경험:
```
1. 보스 처치
   "이제 푸앙이에게 이걸 가져다 주자..!"

2. Village로 자동 이동
   (Stage7 상태 유지)

3. NPC_Puangi 찾아가기
   (퀘스트 마커 표시 권장)

4. NPC_Puangi와 대화
   "정말 거대 버섯을 처치했구나! 대단해!"

5. 대화 종료 후 자동으로 Stage8 진행

6. 엔딩 씬 or 엔딩 이벤트 트리거
```

---

## 🔍 디버그 로그

### 보스 처치 시:
```
🎉 Boss defeated! Returning to Village...
⏸ Stage remains at Stage7 - will advance when talking to NPC_Puangi
🌀 Returning to Village: 02_VillageScene
```

### NPC_Puangi 대화 시:
```
💬 Started dialogue with 푸앙이 (Stage: Stage7_FinalBoss)
📈 Quest advanced by 푸앙이
📈 Quest Advanced: Stage7_FinalBoss → Stage8_Ending
```

---

## ✅ 테스트 체크리스트

### BossScene:
- [ ] 보스 처치 완료
- [ ] 승리 메시지 표시: "이제 푸앙이에게..."
- [ ] **Stage8로 진행되지 않음** (Stage7 유지) ⚠️
- [ ] Village로 자동 이동

### VillageScene:
- [ ] Stage7 상태 확인 (QuestManager)
- [ ] NPC_Puangi에게 접근
- [ ] 퀘스트 마커 표시됨 (PortalSpawnPoint에서 느낌표 마커)
- [ ] E키로 대화 시작

### NPC_Puangi 대화:
- [ ] Stage7 대화 재생됨
- [ ] 대화 종료 후 Stage8로 진행
- [ ] Console에서 `Advanced to Stage8_Ending` 로그 확인

### 엔딩:
- [ ] EndingScene 로드 또는 엔딩 이벤트 실행

---

## 🐛 Troubleshooting

### 문제 1: 보스 처치 시 바로 Stage8이 됨
**원인**: BossDefeatHandler에서 `AdvanceStage()` 호출

**해결**:
1. [BossDefeatHandler.cs:87-90](Assets/Scripts/BossDefeatHandler.cs#L87-L90) 확인
2. `AdvanceStage()` 호출이 **제거**되었는지 확인
3. 로그에서 `⏸ Stage remains at Stage7` 확인

---

### 문제 2: NPC_Puangi와 대화해도 Stage8로 안 넘어감
**원인**: NPC_Puangi.asset에서 `advanceStageOnComplete = false`

**해결**:
1. Project 창에서 `Assets/Data/NPCs/NPC_Puangi.asset` 선택
2. Inspector에서 Stage7 대화 세트 찾기
3. `Advance Stage On Complete` ✅ 체크
4. Ctrl+S로 저장

---

### 문제 3: NPC_Puangi가 대화를 안 함
**원인**: Stage7 대화 세트가 없음

**해결**:
1. NPC_Puangi.asset 확인
2. NPC Dialogue Sets에 `Stage7_FinalBoss` 추가
3. Dialogue Lines 작성
4. `Advance Stage On Complete` ✅ 체크

---

## 📊 Quest Stage 전체 흐름

```
Stage0: VillageTutorial
  ↓
Stage1: ForestHunt
  ↓
Stage2: WeaponUpgrade1
  ↓
Stage3: CaveExploration
  ↓
Stage4: PeuangSadCutscene
  ↓
Stage5: UnkillableBoss
  ↓
Stage6: WeaponUpgrade2
  ↓
Stage7: FinalBoss ← BossScene (보스 처치)
  ↓
[Village 복귀 - Stage7 유지] ⚠️
  ↓
[NPC_Puangi와 대화] ← 여기서 Stage 진행!
  ↓
Stage8: Ending ← EndingScene
```

---

## 💡 왜 이렇게 변경했나?

### 문제점 (Before):
```
보스 처치 → 즉시 Stage8
  ↓
Village 도착 시 이미 Stage8
  ↓
❌ NPC_Puangi가 Stage8 대화만 표시
❌ 보스 처치 축하 대화를 놓침
❌ 스토리 흐름이 어색함
```

### 해결 (After):
```
보스 처치 → Stage7 유지
  ↓
Village 도착 → NPC_Puangi 찾아가기
  ↓
NPC_Puangi와 대화 → 축하 메시지
  ↓
✅ 대화 종료 후 Stage8 진행
✅ 자연스러운 스토리 마무리
```

---

## 🎨 추천 개선 사항

### 1. 퀘스트 마커 표시
Village 복귀 시 NPC_Puangi 위에 느낌표 마커 표시:
- NPCController의 퀘스트 마커 시스템 활용
- Stage7일 때만 마커 활성화

### 2. 대화 내용 개선
```
대화 1: "푸앙이: 돌아왔구나! 거대 버섯을 정말 처치했어?"
대화 2: "푸앙이: 믿을 수 없어! 넌 진정한 영웅이야! 퓨앙!"
대화 3: "푸앙이: 마을에 평화가 찾아왔어. 고마워!"
대화 4: "[엔딩으로 이동...]"
```

### 3. 보상 아이템 추가 (선택사항)
- 최종 보상 아이템
- 업적 해금
- 특별 칭호 등

---

## 📝 Summary

### 핵심 변경:
- ❌ **Before**: 보스 처치 → Stage8
- ✅ **After**: 보스 처치 → Village 복귀 (Stage7) → NPC_Puangi 대화 → Stage8

### 필요한 설정:
1. BossDefeatHandler.cs에서 `AdvanceStage()` 제거 ✅
2. NPC_Puangi.asset에서 Stage7 대화 추가
3. `advanceStageOnComplete = true` 설정

완료! 이제 스토리가 자연스럽게 흐릅니다! 🎉
