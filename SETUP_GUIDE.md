# Phase 1 시스템 설정 및 테스트 가이드

> QuestManager, ItemData, NPCData, NPCController, PortalTrigger 설정 및 테스트 방법

---

## 📋 목차
1. [초기 설정](#1-초기-설정)
2. [QuestManager 설정](#2-questmanager-설정)
3. [ItemData 설정](#3-itemdata-설정)
4. [NPCData 설정](#4-npcdata-설정)
5. [NPCController 설정](#5-npccontroller-설정)
6. [PortalTrigger 설정](#6-portaltrigger-설정)
7. [통합 테스트](#7-통합-테스트)

---

## 1. 초기 설정

### 1.1 필수 GameObject 생성

#### Scene에 QuestManager 추가
```
1. Hierarchy 우클릭 → Create Empty
2. 이름을 "QuestManager"로 변경
3. QuestManager.cs 컴포넌트 추가
4. Inspector에서 설정:
   - Current Stage: Stage0_Tutorial (기본값)
   - Show Debug Messages: ✅ (테스트 중 권장)
```

**중요**: QuestManager는 DontDestroyOnLoad가 적용되어 있어 Scene 전환 시에도 유지됩니다.

#### Inventory 확인
```
- Scene에 Inventory GameObject가 이미 있는지 확인
  (Hierarchy에서 "Inventory"라는 이름의 GameObject 찾기)
- 없다면 Create Empty로 새로 생성 후 Inventory.cs 추가
- DontDestroyOnLoad는 스크립트에 이미 적용되어 있음
```

**참고**: 각 Manager는 독립적인 GameObject로 존재합니다:
- GameManager (GameManager.cs)
- Inventory (Inventory.cs) - 아이템 관리
- DialogueManager (DialogueManager.cs)
- QuestManager (QuestManager.cs)
- UIReferenceManager (UIReferenceManager.cs)

#### DialogueManager 확인 및 분리
```
⚠️ 중요: DialogueManager는 GameManager와 분리되어야 합니다!

현재 상태 확인:
1. GameManager GameObject 선택
2. Inspector에서 DialogueManager.cs가 붙어있는지 확인

분리 방법 (필요 시):
1. GameManager에서 DialogueManager 컴포넌트 제거 (Remove Component)
2. Hierarchy 우클릭 → Create Empty
3. 이름을 "DialogueManager"로 변경
4. DialogueManager.cs 컴포넌트 추가
5. UI 참조는 자동으로 재연결됨 (RefreshUIReferences)

UI 구조 요구사항:
- Canvas/DialoguePanel (GameObject)
- Canvas/DialoguePanel/DialogueText (TextMeshProUGUI)

DontDestroyOnLoad가 적용되어 Scene 전환 시에도 유지됩니다.
```

---

## 2. QuestManager 설정

### 2.1 기본 설정
```
Inspector:
├─ Current Stage: Stage0_Tutorial
└─ Show Debug Messages: ✅
```

### 2.2 테스트 방법

#### Test 1: 기본 동작 확인
```csharp
1. Unity 에디터 Play 모드 실행
2. Console 확인: "✅ QuestManager initialized at Stage: Stage0_Tutorial"
3. Hierarchy에서 QuestManager 선택
4. Inspector에서 Current Stage 값 확인
```

#### Test 2: Stage 진행 테스트 (Context Menu 사용)
```
1. QuestManager GameObject 선택
2. Inspector에서 우클릭 메뉴 열기
3. "Debug: Advance Stage" 선택
4. Console 확인: "📈 Quest Advanced: Stage0_Tutorial → Stage1_FirstQuest"
5. Inspector에서 Current Stage가 변경되었는지 확인
6. 여러 번 반복하여 Stage2, Stage3... 진행
```

#### Test 3: Stage 리셋 테스트
```
1. QuestManager 우클릭 → "Debug: Reset Quest"
2. Console 확인: "🔄 Quest Reset to Stage0_Tutorial"
3. Current Stage가 Stage0_Tutorial로 돌아갔는지 확인
```

#### Test 4: Stage 조건 확인 (스크립트 테스트)
```csharp
// 테스트용 스크립트 작성 (Test.cs)
void Update()
{
    if (Input.GetKeyDown(KeyCode.T))
    {
        bool reached = QuestManager.Instance.IsStageReached(QuestStage.Stage2_WeaponUpgrade);
        Debug.Log($"Stage2 도달 여부: {reached}");
    }
}
```

**예상 결과**:
- Stage0~1일 때: `false`
- Stage2 이상일 때: `true`

---

## 3. ItemData 설정

### 3.1 새로운 필드 추가 확인

기존 ItemData ScriptableObject들이 자동으로 새 필드를 갖게 됩니다:
```
[Header("Weapon Properties")]
├─ Is Weapon: false (기본값)
├─ Weapon Tier: 0
└─ Has Ultimate: false

[Header("Quest Properties")]
└─ Is Quest Item: false
```

### 3.2 무기 아이템 설정 예시

#### 무기 아이템 생성 (낡은 칼자루)
```
1. Project 창에서 우클릭
2. Create → Item → Basic Item
3. 이름: "Item_WeaponTier0"
4. Inspector 설정:

[Basic Info]
├─ Item ID: "weapon_tier0"
├─ Item Name: "낡은 칼자루"
└─ Sprite: (무기 스프라이트)

[Stacking]
├─ Is Stackable: ❌ (무기는 스택 불가)
└─ Max Stack Size: 1

[Weapon Properties]
├─ Is Weapon: ✅
├─ Weapon Tier: 0
└─ Has Ultimate: ❌

[Quest Properties]
└─ Is Quest Item: ✅
```

#### 1티어 무기 (숲의 검)
```
Item ID: "weapon_tier1"
Item Name: "숲의 검"
Is Weapon: ✅
Weapon Tier: 1
Has Ultimate: ❌
Is Quest Item: ✅
```

#### 2티어 무기 (중붕이의 검)
```
Item ID: "weapon_tier2"
Item Name: "중붕이의 검"
Is Weapon: ✅
Weapon Tier: 2
Has Ultimate: ✅
Is Quest Item: ✅
```

### 3.3 퀘스트 소모 아이템 설정

#### 슬라임 잔해
```
[Basic Info]
├─ Item ID: "slime_residue"
├─ Item Name: "슬라임 잔해"
└─ Sprite: (슬라임 아이콘)

[Stacking]
├─ Is Stackable: ✅
└─ Max Stack Size: 99

[Weapon Properties]
└─ Is Weapon: ❌

[Quest Properties]
└─ Is Quest Item: ✅
```

#### 박쥐 뼈
```
Item ID: "bat_bone"
Item Name: "박쥐 뼈"
Is Stackable: ✅
Is Quest Item: ✅
```

### 3.4 테스트 방법

#### Test 1: Inspector 확인
```
1. 생성한 ItemData 선택
2. Inspector에서 새로운 필드들이 보이는지 확인
3. 각 필드 값을 변경해보고 저장 확인
```

#### Test 2: InventoryManager 통합 테스트
```csharp
// Test.cs
void Start()
{
    // ItemData 로드
    ItemData weapon = Resources.Load<ItemData>("Items/Item_WeaponTier0");

    // 인벤토리에 추가
    bool success = InventoryManager.Instance.AddItem(weapon, 1);
    Debug.Log($"아이템 추가: {success}");

    // 무기 속성 확인
    if (weapon.isWeapon)
    {
        Debug.Log($"무기 등급: {weapon.weaponTier}");
        Debug.Log($"궁극기 보유: {weapon.hasUltimate}");
    }
}
```

**예상 결과**:
```
📦 InventoryManager: Added 1x 낡은 칼자루 to slot 0
아이템 추가: True
무기 등급: 0
궁극기 보유: False
```

---

## 4. NPCData 설정

### 4.1 NPCData ScriptableObject 생성

#### 푸앙이 NPC 생성
```
1. Project 창 우클릭
2. Create → NPC → NPC Data
3. 이름: "NPC_Puangi"
4. Inspector 설정:

[NPC Info]
├─ NPC Name: "푸앙이"
└─ NPC Sprite: (푸앙이 스프라이트)

[Quest Interactions]
├─ Gives Quest Items: ✅ (고기 보상)
└─ Requires Quest Items: ❌
```

#### 4.2 Dialogue Sets 설정 (푸앙이 예시)

**Stage0 대화 (게임 시작)**
```
Dialogue Set [0]:
├─ Quest Stage: Stage0_Tutorial
├─ Dialogue Lines:
│   "앗, 모험가님! 도와주실 수 있나요?"
│   "숲 속 몬스터들이 너무 많아져서..."
│   "고기를 구할 수가 없어요!"
│   "몬스터를 처치하고 고기를 가져다주시면 보상을 드릴게요!"
├─ Required Items: (없음)
├─ Reward Items: (없음)
├─ Advance Stage On Complete: ✅
└─ Insufficient Items Message: ""
```

**Stage1 대화 (무기 찾기)**
```
Dialogue Set [1]:
├─ Quest Stage: Stage1_FirstQuest
├─ Dialogue Lines:
│   "마을 수풀에 낡은 무기가 있다는 소문을 들었어요."
│   "그걸 찾아보시는 게 어떨까요?"
├─ Required Items: (없음)
├─ Reward Items: (없음)
├─ Advance Stage On Complete: ❌
└─ Insufficient Items Message: ""
```

**Stage8 대화 (보스 처치 후)**
```
Dialogue Set [2]:
├─ Quest Stage: Stage4_BossDefeated
├─ Dialogue Lines:
│   "와! 보스를 처치하셨군요!"
│   "정말 대단하세요! 이제 고기를 구할 수 있겠어요!"
│   "감사의 표시로 이걸 드릴게요!"
├─ Required Items: (없음)
├─ Reward Items:
│   - Item_BossMeat (보스 고기)
├─ Advance Stage On Complete: ✅ (Stage9로)
└─ Insufficient Items Message: ""
```

#### 4.3 중붕이 NPC 생성

```
[NPC Info]
├─ NPC Name: "중붕이"
└─ NPC Sprite: (중붕이 스프라이트)

[Quest Interactions]
├─ Gives Quest Items: ✅ (무기 지급)
└─ Requires Quest Items: ✅ (재료 필요)
```

**Stage3 대화 (첫 강화)**
```
Dialogue Set [0]:
├─ Quest Stage: Stage3_BossPreparation
├─ Dialogue Lines:
│   "오, 던전을 클리어했군요!"
│   "슬라임 잔해 1개와 박쥐 뼈 1개를 주시면"
│   "더 강한 무기로 강화해드릴게요!"
├─ Required Items:
│   - Item_SlimeResidue (슬라임 잔해)
│   - Item_BatBone (박쥐 뼈)
├─ Reward Items:
│   - Item_WeaponTier1 (숲의 검)
├─ Advance Stage On Complete: ✅
└─ Insufficient Items Message: "재료가 부족합니다. 슬라임 잔해와 박쥐 뼈를 가져오세요!"
```

**Stage6 대화 (최종 강화)**
```
Dialogue Set [1]:
├─ Quest Stage: Stage5_FinalQuest
├─ Dialogue Lines:
│   "보스를 만났다고요? 무서웠겠어요..."
│   "박쥐 뼈 2개와 해골 뼈 1개를 주시면"
│   "최종 무기를 만들어드리죠!"
├─ Required Items:
│   - Item_BatBone x2
│   - Item_SkeletonBone x1
├─ Reward Items:
│   - Item_WeaponTier2 (중붕이의 검)
├─ Advance Stage On Complete: ✅
└─ Insufficient Items Message: "재료가 부족합니다. 박쥐 뼈 2개와 해골 뼈 1개가 필요해요!"
```

### 4.4 테스트 방법

#### Test 1: NPCData 검증
```
1. NPCData 선택
2. Inspector에서 Dialogue Sets 펼치기
3. 각 Stage별 대화가 올바르게 설정되었는지 확인
4. Required Items와 Reward Items 배열 확인
```

#### Test 2: 스크립트로 검증
```csharp
// Test.cs
void Start()
{
    NPCData puangi = Resources.Load<NPCData>("NPC/NPC_Puangi");

    // Stage0 대화 가져오기
    NPCDialogueSet dialogue = puangi.GetDialogueForStage(QuestStage.Stage0_Tutorial);

    Debug.Log($"대화 라인 수: {dialogue.dialogueLines.Count}");
    Debug.Log($"첫 대화: {dialogue.dialogueLines[0]}");
    Debug.Log($"Stage 진행 여부: {dialogue.advanceStageOnComplete}");
}
```

**예상 결과**:
```
대화 라인 수: 4
첫 대화: 앗, 모험가님! 도와주실 수 있나요?
Stage 진행 여부: True
```

---

## 5. NPCController 설정

### 5.1 Scene에 NPC 배치

#### 푸앙이 GameObject 생성
```
1. Hierarchy 우클릭 → 2D Object → Sprite
2. 이름: "NPC_Puangi"
3. Sprite Renderer 설정:
   - Sprite: 푸앙이 스프라이트
   - Sorting Layer: Default
   - Order in Layer: 1

4. NPCController.cs 추가
5. Inspector 설정:

[NPC Configuration]
└─ NPC Data: NPC_Puangi (드래그 앤 드롭)

[Interaction Settings]
├─ Auto Open On Enter: ❌ (E키로 상호작용)
└─ Interaction Key: E

[Visual Feedback]
└─ Interaction Prompt: (없음 또는 UI 프롬프트)

[Debug]
└─ Show Debug Messages: ✅
```

#### BoxCollider2D 설정
```
- NPCController가 자동으로 BoxCollider2D를 추가함
- 수동 조정:
  - Is Trigger: ✅
  - Size: (2, 2) - 플레이어가 가까이 올 범위
  - Offset: (0, 0)
```

### 5.2 중붕이 NPC 설정

```
동일한 방식으로 생성:
├─ GameObject 이름: "NPC_Joongboongi"
├─ Sprite: 중붕이 스프라이트
├─ NPCController 추가
└─ NPC Data: NPC_Joongboongi
```

### 5.3 테스트 방법

#### Test 1: 상호작용 범위 확인
```
1. Play 모드 실행
2. Player를 NPC 근처로 이동
3. Console 확인: "💬 Player entered 푸앙이's interaction range"
4. Player를 멀리 이동
5. Console 확인: "💬 Player left 푸앙이's interaction range"
```

#### Test 2: 대화 시작 테스트
```
1. QuestManager가 Stage0_Tutorial인지 확인
2. Player를 푸앙이 근처로 이동
3. E키 누르기
4. DialogueManager에서 대화가 시작되는지 확인
5. Console 확인: "💬 Started dialogue with 푸앙이 (Stage: Stage0_Tutorial)"
```

#### Test 3: Stage별 대화 분기 테스트
```
1. QuestManager 우클릭 → "Debug: Advance Stage"
2. Stage를 Stage1_FirstQuest로 변경
3. 푸앙이와 다시 대화 (E키)
4. Stage1 대화가 나오는지 확인
5. Stage를 계속 변경하면서 각 Stage별 대화 확인
```

#### Test 4: 아이템 요구 조건 테스트 (중붕이)
```
1. QuestManager를 Stage3_BossPreparation으로 설정
2. 인벤토리에 아이템 없이 중붕이와 대화
3. "재료가 부족합니다..." 메시지 확인

4. Inventory에 아이템 추가 (Inspector 또는 게임 내 획득):
   - 슬라임 잔해 1개
   - 박쥐 뼈 1개

5. 중붕이와 다시 대화
6. Console 확인:
   - "🗑 Consumed item: 슬라임 잔해"
   - "🗑 Consumed item: 박쥐 뼈"
   - "🎁 Gave player: 숲의 검"
   - "📈 Quest advanced by 중붕이"
```

#### Test 5: Context Menu 디버깅
```
1. NPCController GameObject 선택
2. Inspector 우클릭
3. "Debug: Test Interaction" 선택
   → Player 범위 체크 없이 즉시 대화 시작
4. "Debug: Print Current Stage Dialogue" 선택
   → Console에 현재 Stage 대화 정보 출력
```

**예상 출력**:
```
=== 푸앙이 at Stage Stage0_Tutorial ===
Dialogue Lines: 4
Required Items: None
Reward Items: None
Advances Stage: True
```

---

## 6. PortalTrigger 설정

### 6.1 Portal GameObject 생성

#### Stage1 던전 입구 Portal
```
1. Hierarchy 우클릭 → Create Empty
2. 이름: "Portal_Stage1"
3. PortalTrigger.cs 추가
4. Inspector 설정:

[Portal Settings]
├─ Target Scene Name: "Stage1Scene"
├─ Spawn Position: (0, 0, 0)
└─ Use Custom Spawn Position: ❌ (PlayerSpawn 태그 사용)

[Quest Stage Requirements]
├─ Requires Quest Stage: ✅
├─ Required Stage: Stage2_WeaponUpgrade
└─ Blocked Message: "무기를 먼저 획득하세요! 마을 수풀을 확인해보세요."

[Debug]
└─ Show Debug Messages: ✅
```

#### BoxCollider2D 설정
```
- PortalTrigger가 자동으로 Trigger로 설정
- Size: (1.5, 1.5)
- Offset: (0, 0)
```

#### Stage2 Portal
```
Portal_Stage2:
├─ Target Scene Name: "Stage2Scene"
├─ Requires Quest Stage: ✅
├─ Required Stage: Stage4_BossDefeated (1차 강화 완료)
└─ Blocked Message: "아직 준비가 안 되었어요. 중붕이에게 무기를 강화받으세요!"
```

#### Stage3 Portal (보스)
```
Portal_Stage3:
├─ Target Scene Name: "Stage3Scene"
├─ Requires Quest Stage: ✅
├─ Required Stage: Stage5_FinalQuest (Stage2 클리어)
└─ Blocked Message: "아직 이 던전에 들어갈 수 없습니다."
```

### 6.2 Visual 설정 (선택사항)

Portal에 스프라이트 추가:
```
1. Portal GameObject에 Sprite Renderer 추가
2. Sprite: 포탈 이펙트 스프라이트
3. Color: 파란색 계열
4. Inspector에서 Portal Visual 필드에 Sprite Renderer 드래그
```

### 6.3 테스트 방법

#### Test 1: Portal 차단 테스트
```
1. QuestManager를 Stage0_Tutorial로 설정
2. Player를 Portal_Stage1로 이동
3. Portal 진입 시도
4. 예상 결과:
   - Scene 전환 없음
   - DialogueManager에서 차단 메시지 표시
   - Console: "❌ Portal access denied - Stage Stage2_WeaponUpgrade required"
   - Console: "🚫 Portal blocked: 무기를 먼저 획득하세요!..."
```

#### Test 2: Portal 통과 테스트
```
1. QuestManager를 Stage2_WeaponUpgrade로 설정
2. Player를 Portal_Stage1로 이동
3. Portal 진입
4. 예상 결과:
   - Console: "✅ Portal access granted - Stage requirement met"
   - Console: "🌀 PortalTrigger: Player entered portal 'Portal_Stage1' → Loading scene 'Stage1Scene'"
   - Stage1Scene으로 전환 (Scene이 Build Settings에 있어야 함)
```

#### Test 3: Scene Editor Gizmo 확인
```
1. Scene View에서 Portal GameObject 선택
2. Scene View에 표시되는 것들:
   - 파란색 반투명 박스 (Portal 범위)
   - 위쪽 화살표 (Portal 위치)
   - 씬 이름 라벨
   - 🔒 아이콘 + Required Stage (requiresQuestStage = true일 때)

3. Portal 선택 해제 후 다시 선택
4. 노란색으로 표시되는 추가 정보 확인
```

#### Test 4: 여러 Stage 시뮬레이션
```
[시나리오]: 게임 진행 순서대로 Portal 테스트

Stage0 → Portal 진입 시도:
- Portal_Stage1: ❌ 차단
- Portal_Stage2: ❌ 차단
- Portal_Stage3: ❌ 차단

Stage2 (무기 획득) → Portal 진입:
- Portal_Stage1: ✅ 통과
- Portal_Stage2: ❌ 차단
- Portal_Stage3: ❌ 차단

Stage4 (1차 강화) → Portal 진입:
- Portal_Stage1: ✅ 통과
- Portal_Stage2: ✅ 통과
- Portal_Stage3: ❌ 차단

Stage5 (Stage2 클리어) → Portal 진입:
- Portal_Stage1: ✅ 통과
- Portal_Stage2: ✅ 통과
- Portal_Stage3: ✅ 통과
```

---

## 7. 통합 테스트

### 7.1 전체 퀘스트 플로우 테스트

#### 시나리오 1: 게임 시작 → 첫 던전 클리어

```
[Stage0] 게임 시작
1. QuestManager: Stage0_Tutorial
2. 푸앙이와 대화 (E키)
   → Stage1_FirstQuest로 진행
   → "마을 수풀에 무기가 있다는 소문..."

[Stage1] 무기 찾기
3. 마을 수풀 Trigger 진입 (별도 구현 필요)
   → Item_WeaponTier0 획득
   → Stage2_WeaponUpgrade로 진행

[Stage2] 던전 입장
4. Portal_Stage1 진입 시도
   → ✅ 통과
   → Stage1Scene 로드

5. 던전에서 적 처치 (미니 슬라임, 박쥐)
   → 슬라임 잔해, 박쥐 뼈 드롭
   → 모든 적 처치 시 Stage3_BossPreparation로 진행

6. VillageScene으로 복귀

[Stage3] 무기 강화
7. 중붕이와 대화
   → 재료 소모 (슬라임 잔해, 박쥐 뼈)
   → 숲의 검 획득
   → Stage4_BossDefeated로 진행
```

#### 시나리오 2: 보스 첫 만남 → 패배 → 재도전

```
[Stage4] Stage2 던전
1. Portal_Stage2 진입
2. 적 처치 (미니 슬라임, 박쥐, 해골)
   → Stage5_FinalQuest로 진행
   → Stage3Scene 자동 진입

[Stage5] 보스 꿈 (패배)
3. CutScene2 재생
4. 보스 무적 상태
5. 보스 접촉 → 플레이어 사망
   → Stage6_GameComplete로 진행
   → VillageScene 복귀

[Stage6] 최종 무기 강화
6. 중붕이와 대화
   → 재료 소모 (박쥐 뼈 2개, 해골 뼈 1개)
   → 중붕이의 검 획득 (궁극기 있음)
   → Stage7로 진행

[Stage7] 보스 재도전
7. Portal_Stage3 진입
8. CutScene3 재생
9. 보스 처치
   → Stage8로 진행
   → VillageScene 복귀

[Stage8] 퀘스트 완료
10. 푸앙이와 대화
    → 보스 고기 획득
    → Stage9로 진행
```

### 7.2 디버깅 체크리스트

#### QuestManager
```
✅ Singleton이 정상 작동하는가?
✅ DontDestroyOnLoad가 적용되어 Scene 전환 시에도 유지되는가?
✅ AdvanceStage()가 정상적으로 Stage를 증가시키는가?
✅ IsStageReached()가 올바른 bool 값을 반환하는가?
```

#### ItemData
```
✅ 새로운 필드들이 Inspector에 표시되는가?
✅ CreateRuntimeCopy()가 새 필드들도 복사하는가?
✅ CopyFrom()이 새 필드들도 복사하는가?
✅ isWeapon과 isQuestItem이 올바르게 동작하는가?
```

#### NPCData
```
✅ ScriptableObject 생성이 정상적으로 되는가?
✅ Dialogue Sets이 Stage별로 올바르게 설정되는가?
✅ GetDialogueForStage()가 올바른 대화를 반환하는가?
✅ Required Items와 Reward Items 배열이 정상 작동하는가?
```

#### NPCController
```
✅ Player 진입/퇴출 감지가 정상 작동하는가?
✅ E키 입력이 정상적으로 감지되는가?
✅ Stage별 대화 분기가 올바르게 동작하는가?
✅ CheckRequiredItems()가 올바르게 인벤토리를 확인하는가?
✅ ConsumeItems()가 아이템을 정상적으로 소모하는가?
✅ GiveRewardItems()가 아이템을 정상적으로 지급하는가?
✅ advanceStageOnComplete가 올바르게 동작하는가?
```

#### PortalTrigger
```
✅ Quest Stage 요구 조건이 올바르게 확인되는가?
✅ Stage 미달 시 차단 메시지가 표시되는가?
✅ Stage 도달 시 Scene 전환이 정상 작동하는가?
✅ DialogueManager와 통합이 정상 작동하는가?
✅ Scene Editor Gizmo가 올바르게 표시되는가?
```

### 7.3 자주 발생하는 문제 및 해결

#### 문제 1: "QuestManager.Instance is null"
```
원인: QuestManager GameObject가 Scene에 없음
해결: Hierarchy에 QuestManager GameObject 생성 및 컴포넌트 추가
```

#### 문제 2: "DialogueManager.Instance is null"
```
원인: DialogueManager GameObject가 Scene에 없음
해결: DialogueManager GameObject 확인 및 Instance 패턴 적용 확인
```

#### 문제 3: NPCController가 대화를 시작하지 않음
```
원인 1: NPCData가 할당되지 않음
해결: Inspector에서 NPC Data 필드 확인

원인 2: Player 태그가 없음
해결: Player GameObject에 "Player" 태그 추가

원인 3: Collider2D가 Trigger가 아님
해결: Inspector에서 Is Trigger ✅ 확인
```

#### 문제 4: Portal이 작동하지 않음
```
원인 1: Scene이 Build Settings에 없음
해결: File → Build Settings → Add Open Scenes

원인 2: Quest Stage가 부족함
해결: QuestManager에서 Current Stage 확인

원인 3: Player 태그가 없음
해결: Player GameObject에 "Player" 태그 추가
```

#### 문제 5: 아이템 소모가 작동하지 않음
```
원인: Inventory.instance가 null
해결: Inventory GameObject 확인 및 DontDestroyOnLoad 적용 확인
```

---

## 8. 다음 단계

Phase 1 완료 후:

1. **Scene 제작**:
   - VillageScene, Stage1Scene, Stage2Scene, Stage3Scene 구현
   - PlayerSpawn 태그 설정

2. **적 AI 구현**:
   - 미니 슬라임, 박쥐, 해골 AI
   - 드롭 아이템 시스템

3. **컷신 시스템**:
   - Timeline 기반 컷신
   - 카메라 연출

4. **보스전 로직**:
   - 보스 무적 상태 (Stage5)
   - 보스 전투 패턴 (Stage7)

5. **엔딩 Scene**:
   - 크레딧 스크롤
   - 파티 일러스트

---

## 📝 요약

Phase 1에서 구현한 시스템:
- ✅ **QuestManager**: 퀘스트 진행 관리
- ✅ **ItemData**: 무기/퀘스트 아이템 확장
- ✅ **NPCData**: Stage별 대화 및 아이템 교환
- ✅ **NPCController**: NPC 상호작용 로직
- ✅ **PortalTrigger**: Quest Stage 기반 Scene 전환

모든 시스템이 서로 통합되어 있어 **바로 게임 플레이 가능**합니다!

다음 작업: Scene 제작 및 적 AI 구현
