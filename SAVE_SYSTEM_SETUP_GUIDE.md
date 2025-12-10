# 💾 저장 시스템 설정 가이드

## 📋 개요
Puang-Adventure에 완전한 8슬롯 저장/불러오기 시스템이 구현되었습니다:
- 씬 이름과 저장 시간을 표시하는 8개의 저장 슬롯
- 게임 내 SavePanel에서 저장/불러오기
- TitleScene에서 불러오기
- 불러올 때 자동 씬 전환
- 완전한 플레이어 상태 복원 (위치, 체력, 스태미나, 인벤토리)

---

## 🎯 구현 상태

### ✅ 완료된 항목
1. **SaveManager.cs** - 8슬롯 시스템을 가진 싱글톤 매니저
2. **SaveDataPanelController.cs** - 8개 슬롯 버튼을 위한 UI 컨트롤러
3. **SavePanelController.cs** - SaveDataPanel과 통합되도록 업데이트
4. **SaveData.cs** - 완전한 저장 데이터 구조
5. **InventorySaveData.cs** - Resources 로딩 기능 추가
6. **PlayerHealth.cs** - `SetHealth(int value)` 메서드 추가
7. **PlayerStamina.cs** - `SetStamina(float value)` 메서드 추가

---

## 🛠️ 설정 방법

### 1단계: SaveManager GameObject 생성

1. 씬에 새로운 빈 GameObject를 생성합니다 (TitleScene 또는 MainScene 등 처음 로드되는 씬)
2. 이름을 `SaveManager`로 지정
3. `SaveManager.cs` 컴포넌트 추가
4. **중요**: 이 오브젝트는 DontDestroyOnLoad를 통해 모든 씬에서 유지됩니다

### 2단계: SaveDataPanel Prefab 생성

**SaveDataPanel을 Prefab으로 만들어서 모든 씬에 배치합니다!**

UI 구조는 다음과 같아야 합니다:

```
Canvas (TitleScene 또는 InitialScene)
└── SaveDataPanel (처음에는 비활성 상태)
    ├── SaveDataPanelController (컴포넌트)
    ├── CloseButton (선택사항)
    └── SlotButton(1)
        ├── CurrentScene (TextMeshPro)
        └── SaveTime (TextMeshPro)
    ├── SlotButton(2)
        ├── CurrentScene (TextMeshPro)
        └── SaveTime (TextMeshPro)
    ├── SlotButton(3)
    ...
    └── SlotButton(8)
        ├── CurrentScene (TextMeshPro)
        └── SaveTime (TextMeshPro)
```

**팁:**
- SaveDataPanel에 **GridLayoutGroup**을 사용하여 버튼을 자동 배치
- 각 SlotButton은 텍스트 정렬을 위해 **Vertical Layout Group**을 가져야 합니다
- SlotButton 이름은 **반드시 정확히**: `SlotButton(1)`, `SlotButton(2)`, ..., `SlotButton(8)`
- 자식 텍스트 이름은 **반드시 정확히**: `CurrentScene`과 `SaveTime`

**중요:** SaveDataPanel을 Prefab으로 저장한 후 모든 씬의 Canvas에 배치하세요!

### 3단계: SaveDataPanel Prefab 생성 및 배치

1. SaveDataPanel GameObject 선택
2. `SaveDataPanelController.cs` 컴포넌트 추가
3. Inspector에서:
   - `Save Data Panel`: 비워두기 (자동으로 자기 자신을 사용)
   - 스크립트가 이름이 올바르면 슬롯 버튼들을 자동으로 찾습니다
   - 선택적으로 `Close Button` 할당
4. SaveDataPanel을 **Prefab으로 저장** (Assets/Prefabs/SaveDataPanel.prefab)
5. **모든 씬의 Canvas에 SaveDataPanel Prefab 배치**
   - TitleScene Canvas
   - 게임 씬들의 HUD_Canvas (또는 Canvas)

**중요:** 모든 씬에 동일한 Prefab을 사용하므로, Prefab을 수정하면 모든 씬에 자동 반영됩니다!

### 4단계: SavePanel 설정 (게임 내)

게임 내 저장 패널 (UI_MasterPanel → SavePanel):

1. `SavePanel` GameObject 찾기
2. `SavePanelController.cs` 컴포넌트 추가/업데이트
3. Inspector에서:
   - "저장하기" 버튼을 `Button Save`에 할당
   - "불러오기" 버튼을 `Button Load`에 할당
   - `SaveDataPanelController` 레퍼런스 할당

### 5단계: TitleScene Load 버튼 설정

TitleScene에서:

1. `LoadButton` GameObject 찾기
2. `TitleSceneLoadButton.cs` 컴포넌트 추가
3. Inspector에서 (선택사항 - 자동으로 찾습니다):
   - `Load Button`: 자동으로 자기 자신의 Button 컴포넌트를 찾음
   - `Save Data Panel Controller`: 자동으로 씬의 SaveDataPanelController를 찾음

**중요:** TitleScene의 Canvas 안에 SaveDataPanel Prefab이 배치되어 있어야 합니다!

### 6단계: Resources/Items 폴더 생성

인벤토리 아이템이 올바르게 로드되려면:

1. 폴더 생성: `Assets/Resources/Items/`
2. 모든 `ItemData` ScriptableObject를 이 폴더로 이동
3. 각 ItemData의 `itemID`가 파일 이름과 일치하는지 확인

**예시:**
```
Assets/Resources/Items/
├── forest_sword.asset (itemID = "forest_sword")
├── chunbung_sword.asset (itemID = "chunbung_sword")
└── health_potion.asset (itemID = "health_potion")
```

---

## 🎮 사용 방법

### 게임 내 저장/불러오기

1. UI_MasterPanel 열기 (Tab 키)
2. SavePanel로 이동
3. "저장하기" 버튼 클릭
   - SaveDataPanel이 **저장 모드**로 열림
   - 슬롯(1-8) 중 하나를 클릭하여 저장
   - UI가 씬 이름과 타임스탬프로 업데이트됨
4. "불러오기" 버튼 클릭
   - SaveDataPanel이 **불러오기 모드**로 열림
   - 데이터가 있는 슬롯을 클릭하여 불러오기
   - 필요한 경우 자동으로 씬 전환

### TitleScene 불러오기

1. TitleScene에서 "Load" 버튼 클릭
2. SaveDataPanel이 **불러오기 모드**로 열림
3. 데이터가 있는 슬롯을 클릭하여 불러오기
4. 저장된 씬으로 완전한 플레이어 상태와 함께 로드

---

## 🔍 저장되는 항목

각 저장 슬롯은 다음을 저장합니다:
- **씬 이름** - 현재 활성 씬
- **저장 시간** - 타임스탬프 (yyyy-MM-dd HH:mm:ss)
- **플레이어 위치** - Transform 위치
- **플레이어 체력** - 현재 HP
- **플레이어 스태미나** - 현재 스태미나 값
- **인벤토리** - 스택 개수를 포함한 모든 아이템

---

## 📁 저장 파일 위치

저장 파일은 다음 위치에 저장됩니다:
```
Application.persistentDataPath/SaveData/save_slot_1.json
Application.persistentDataPath/SaveData/save_slot_2.json
...
Application.persistentDataPath/SaveData/save_slot_8.json
```

**Windows:**
`C:\Users\<사용자이름>\AppData\LocalLow\<회사명>\<게임명>\SaveData\`

**Mac:**
`~/Library/Application Support/<회사명>/<게임명>/SaveData/`

---

## 🐛 디버깅

### 디버그 메뉴 옵션

**SaveManager:**
- 컴포넌트 우클릭 → Debug: Show Save Directory
- 컴포넌트 우클릭 → Debug: List All Saves
- 컴포넌트 우클릭 → Debug: Delete All Saves

**SaveDataPanelController:**
- 컴포넌트 우클릭 → Debug: Refresh Slots UI
- 컴포넌트 우클릭 → Debug: Open Save Mode
- 컴포넌트 우클릭 → Debug: Open Load Mode

### 일반적인 문제

**문제:** "SaveManager.Instance is null"
- **해결:** SaveManager GameObject가 씬에 존재하는지 확인

**문제:** "SlotButton(X) not found"
- **해결:** 버튼 이름이 정확히 일치하는지 확인: `SlotButton(1)`, `SlotButton(2)`, 등

**문제:** "Item not found in Resources/Items/X"
- **해결:** ItemData ScriptableObject를 `Assets/Resources/Items/` 폴더로 이동

**문제:** 인벤토리가 로드되지 않음
- **해결:** ItemData.itemID가 Resources/Items의 파일 이름과 일치하는지 확인

**문제:** 체력/스태미나가 복원되지 않음
- **해결:** `SetHealth()`와 `SetStamina()` 메서드가 추가되었는지 확인

---

## 🔧 커스터마이징

### 슬롯 개수 변경

`SaveManager.cs`에서:
```csharp
[SerializeField] private int maxSlots = 8; // 원하는 숫자로 변경
```

### 퀘스트 진행도 추가

`SaveData.cs`에 추가:
```csharp
public int questStage;
```

`SaveManager.CollectSaveData()`에 추가:
```csharp
if (QuestManager.Instance != null)
{
    data.questStage = (int)QuestManager.Instance.currentStage;
}
```

`SaveManager.ApplySaveData()`에 추가:
```csharp
if (QuestManager.Instance != null)
{
    QuestManager.Instance.currentStage = (QuestStage)data.questStage;
}
```

---

## ✅ 테스트 체크리스트

- [ ] SaveManager가 씬에 존재하고 씬 간에 유지됨
- [ ] SaveDataPanel UI에 SlotButton(1-8)이라는 이름의 버튼 8개가 있음
- [ ] 각 슬롯 버튼에 CurrentScene과 SaveTime TextMeshPro 컴포넌트가 있음
- [ ] SaveDataPanelController가 슬롯 버튼을 자동으로 찾음
- [ ] SavePanelController가 SaveDataPanelController에 연결됨
- [ ] "저장하기" 클릭 시 SaveDataPanel이 저장 모드로 열림
- [ ] "불러오기" 클릭 시 SaveDataPanel이 불러오기 모드로 열림
- [ ] 슬롯에 저장하면 UI가 씬 이름과 타임스탬프로 업데이트됨
- [ ] 슬롯에서 불러오면 플레이어 위치, 체력, 스태미나가 복원됨
- [ ] 다른 씬에서 불러오면 올바르게 씬 전환됨
- [ ] 인벤토리 아이템이 올바른 스택 개수로 복원됨
- [ ] TitleScene Load 버튼이 SaveDataPanel을 열음
- [ ] Resources/Items 폴더에 모든 ItemData 에셋이 포함됨

---

## 📞 지원

문제가 발생하면:
1. Unity Console에서 Debug 로그 확인 (🟢 녹색 = 성공, ⚠️ 노란색 = 경고, ❌ 빨간색 = 오류)
2. Debug 메뉴 옵션을 사용하여 저장 데이터 검사
3. 모든 설정 단계가 완료되었는지 확인

---

## 🎉 완료!

이제 게임에 8슬롯 완전한 저장/불러오기 시스템이 있습니다!

테스트 방법:
1. 게임을 플레이하고, 이동하고, 아이템 수집
2. SavePanel을 열고 Slot 1에 저장
3. TitleScene으로 종료
4. Load를 클릭하고 Slot 1 선택
5. 모든 것이 올바르게 복원되는지 확인
