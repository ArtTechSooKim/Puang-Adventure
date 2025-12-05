# 튜토리얼 씬 설정 가이드

TutorialScene을 완성하기 위한 Unity 에디터 설정 가이드입니다.

## 1. 씬 구조 확인

### TutorialScene Hierarchy 구조
```
TutorialScene
├── Grid (Tilemap - 간단한 사각형 맵)
├── PlayerSpawn (Transform at 0,0,0)
├── Player (DontDestroyOnLoad에서 온 플레이어)
├── Tutorial_Canvas
│   ├── TutorialPanel
│   │   └── TutorialText (TextMeshProUGUI)
│   ├── AnyStatePanel
│   │   └── AnyStateText (TextMeshProUGUI)
│   └── SkipButton (Button)
├── HUD_Canvas (플레이어와 함께 온 UI)
│   ├── HPBar (Slider)
│   ├── STBar (Slider)
│   └── ...
├── Main Camera (또는 Cinemachine Virtual Camera)
└── TutorialManager (빈 GameObject)
```

## 2. Tutorial_Canvas 설정

### 2.1 Tutorial_Canvas
- **Canvas 설정**
  - Render Mode: `Screen Space - Overlay`
  - Canvas Scaler: `Scale With Screen Size`
  - Reference Resolution: `1920 x 1080`
  - Sort Order: `10` (HUD_Canvas보다 위에 표시)

### 2.2 TutorialPanel
- **RectTransform**
  - Anchor: Top Center
  - Position: `(0, -100, 0)` (중앙 상단)
  - Size: `(800, 200)`

- **CanvasGroup 컴포넌트 추가**
  - Alpha: `0` (시작 시 보이지 않음)
  - Interactable: `false`
  - Block Raycasts: `false`

- **Image 컴포넌트** (선택사항 - 배경)
  - Color: 반투명 검정 `(0, 0, 0, 180)`

- **자식: TutorialText (TextMeshProUGUI)**
  - Font: 사용 중인 한글 폰트 (예: pokemon-dppt SDF 또는 NotoSansKR)
  - Font Size: `48`
  - Alignment: Center & Middle
  - Color: White `(255, 255, 255, 255)`
  - RectTransform: Stretch All (부모 크기에 맞춤)

### 2.3 AnyStatePanel
- **TutorialPanel 복제**
- **RectTransform**
  - 동일한 위치와 크기

- **CanvasGroup**
  - Alpha: `0` (시작 시 꺼짐)

- **GameObject**
  - Active: `false` (체크 해제)

- **중요: Layer 순서**
  - Hierarchy에서 TutorialPanel보다 **아래**에 위치시킴
  - 하지만 Canvas Sort Order를 높여서 실제로는 위에 렌더링되도록 설정:
    - AnyStatePanel에 별도의 Canvas 컴포넌트 추가
    - Override Sorting: `true`
    - Sort Order: `11`

### 2.4 SkipButton
- **RectTransform**
  - Anchor: Top Right
  - Position: `(-50, -50, 0)` (우측 상단)
  - Size: `(200, 80)`

- **Button 컴포넌트**
  - OnClick 이벤트는 TutorialManager에서 코드로 연결됨

- **자식: Text (TextMeshProUGUI)**
  - Text: `"튜토리얼 건너뛰기"`
  - Font Size: `32`
  - Alignment: Center & Middle

## 3. TutorialManager 설정

### 3.1 GameObject 생성
1. Hierarchy에서 빈 GameObject 생성
2. 이름: `TutorialManager`
3. `TutorialManager.cs` 스크립트 추가

### 3.2 Inspector 설정

#### UI References
- **Tutorial Panel**: `Tutorial_Canvas/TutorialPanel` (CanvasGroup)
- **Tutorial Text**: `Tutorial_Canvas/TutorialPanel/TutorialText` (TextMeshProUGUI)
- **Any State Panel**: `Tutorial_Canvas/AnyStatePanel` (CanvasGroup)
- **Any State Text**: `Tutorial_Canvas/AnyStatePanel/AnyStateText` (TextMeshProUGUI)
- **Skip Button**: `Tutorial_Canvas/SkipButton` (Button)

#### Prefabs
- **Weapon Tier2 Prefab**: `Assets/Prefabs/Item_WeaponTier2_World.prefab`
- **Bat Prefab**: `Assets/Prefabs/Bat.prefab`

#### Player References (자동 탐색 - 비워두세요!)
- **Player Controller**: 비워두기 (DontDestroyOnLoad에서 자동 검색)
- **Player Health**: 비워두기 (DontDestroyOnLoad에서 자동 검색)
- **Player Stamina**: 비워두기 (DontDestroyOnLoad에서 자동 검색)
- **Inventory**: 비워두기 (Inventory.instance 싱글톤 사용)

> ⚠️ **중요**: Player는 01_InitialScene에서 DontDestroyOnLoad로 넘어오므로 Inspector에서 직접 할당할 수 없습니다. TutorialManager가 자동으로 FindObjectOfType을 사용하여 최대 5초간 Player를 찾습니다.

#### Camera
- **Main Camera**: 비워두기 (Camera.main 자동 검색)

#### Settings
- **Fade Time**: `1` (페이드 인/아웃 시간, 초 단위)

## 4. Build Settings 확인

### 4.1 씬 빌드 순서
File > Build Settings에서 씬 순서 확인:
```
0. 00_TitleScene
1. 01_InitialScene
2. TutorialScene          ← 추가됨
3. 02_VillageScene
4. 03_ForestScene
5. ...
```

### 4.2 SceneInitializer 수정 필요
`Assets/Scripts/SceneInitializer.cs` 파일을 열어서 첫 씬을 TutorialScene으로 변경:

```csharp
public void LoadFirstScene()
{
    // 원래: SceneManager.LoadScene("02_VillageScene");
    SceneManager.LoadScene("TutorialScene");  // 튜토리얼부터 시작
}
```

**또는** 튜토리얼을 선택적으로 만들려면:
- PlayerPrefs로 "TutorialCompleted" 플래그 저장
- 완료했으면 바로 02_VillageScene으로 이동

## 5. Player 스폰 위치 설정

TutorialScene으로 전환할 때 Player가 올바르게 스폰되도록 설정:

### 5.1 방법 1: PlayerSpawn 오브젝트 (권장)
TutorialScene Hierarchy에 빈 GameObject 생성:
- **이름**: `PlayerSpawn`
- **Position**: `(0, 0, 0)` (원하는 시작 위치)
- **Tag**: 태그 없음 (PlayerPersistent가 이름으로 찾음)

> PlayerPersistent.cs의 `TryFindSpawnPoint` 메서드가 자동으로 "PlayerSpawn" 이름을 가진 오브젝트를 찾습니다.

### 5.2 방법 2: Portal을 통한 진입 (선택)
01_InitialScene에서 PortalTrigger를 통해 TutorialScene으로 진입하는 경우:
- PortalTrigger의 `targetSpawnPointName` 설정
- TutorialScene에 해당 이름의 스폰 포인트 배치
- PortalSpawnPoint 컴포넌트로 오프셋 설정 가능

### 5.3 자동 동작 확인
PlayerPersistent는 씬 로드 시 자동으로:
1. PlayerPrefs에서 "TargetSpawnPoint" 확인
2. 해당 이름의 오브젝트 찾기
3. 없으면 "PlayerSpawn" 이름으로 재시도
4. 그래도 없으면 기본 위치 (0, 0, 0) 유지

## 6. 테스트 체크리스트

### 6.1 튜토리얼 진행 순서
- [ ] "튜토리얼" 페이드 인/아웃 (2초)
- [ ] "WASD로 움직이세요." → WASD 입력 대기 (페이드 완료 후)
- [ ] "좋습니다!" (2초, 페이드 인/아웃)
- [ ] "Shift로 2초간 달리세요." → 2초 대시 대기 (페이드 완료 후)
- [ ] "좋습니다!" (2초, 페이드 인/아웃)
- [ ] 플레이어 워프 (-5, 0, 0) → 검 스폰 (0, 0, 0)
- [ ] "다가가서 검을 획득하세요" → 획득 대기 (페이드 완료 후)
- [ ] "좋습니다!" (1초)
- [ ] "검은 반드시 아이템 슬롯 1번에 위치해야..." (1초 대기)
- [ ] "드래그해서 아이템을..." (페이드 아웃)
- [ ] 플레이어 워프 (0, 3.5, 0) → 적 3마리 스폰 + 카메라 워크
- [ ] "적이 있습니다! 좌클릭으로 공격하세요."
- [ ] 3마리 처치 대기
- [ ] "처치했습니다!" (2초, 페이드 인/아웃)
- [ ] "tab 토글로 설정 창을..." (2초, 페이드 인/아웃)
- [ ] 시간 멈춤 + "튜토리얼 완료!" (1초)
- [ ] "게임을 시작합니다" (1초)
- [ ] 인벤토리/핫바/체력 초기화 → 02_VillageScene 전환

### 6.2 AnyState 팝업
- [ ] HP가 95 이하로 떨어지면 → "HP가 0이되면 GameOver..." (2초, 시간 멈춤)
- [ ] Stamina가 50 이하로 떨어지면 →
  - "달리기와 공격은 스테미나를..." (2초, 시간 멈춤)
  - "스테미나가 소모되지 않는..." (2초, 시간 멈춤)

### 6.3 건너뛰기 버튼
- [ ] 우측 상단에 버튼 표시
- [ ] 클릭 시 즉시 02_VillageScene으로 전환
- [ ] 시간 복원 (Time.timeScale = 1)

## 7. 추가 개선 사항 (선택)

### 7.1 튜토리얼 완료 플래그 저장
TutorialManager의 SkipTutorial()과 씬 전환 시:
```csharp
PlayerPrefs.SetInt("TutorialCompleted", 1);
PlayerPrefs.Save();
```

SceneInitializer에서 체크:
```csharp
if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
{
    SceneManager.LoadScene("02_VillageScene");
}
else
{
    SceneManager.LoadScene("TutorialScene");
}
```

### 7.2 카메라 워크 개선
현재는 Cinemachine Transposer의 offset을 변경하는 방식입니다.
더 부드러운 효과를 원하면:
- Cinemachine Target Group 사용
- 또는 Impulse Source로 카메라 흔들기

### 7.3 적 배치 시각 효과
적이 스폰될 때 이펙트 추가:
- Particle System
- Scale 애니메이션 (작게 시작 → 크게)

## 8. 문제 해결

### 8.1 "TutorialPanel이 보이지 않아요"
- CanvasGroup의 Alpha 확인 (0에서 시작)
- Canvas Sort Order 확인 (10 이상)
- TutorialManager의 tutorialPanel 레퍼런스 연결 확인

### 8.2 "적을 죽여도 카운트가 안 올라가요"
- EnemyHealth.cs에 `OnDeath` 이벤트가 추가되었는지 확인
- Bat 프리팹이 올바른지 확인
- TutorialManager의 OnEnemyKilled() 메서드 연결 확인

### 8.3 "Player를 찾을 수 없다는 에러가 나요"
- Console에서 "✅ TutorialManager: Player references found successfully!" 메시지 확인
- 01_InitialScene이 먼저 로드되어 Player가 생성되었는지 확인
- Player GameObject에 PlayerController, PlayerHealth, PlayerStamina 컴포넌트가 있는지 확인
- TutorialManager의 InitializeTutorial 코루틴이 최대 5초간 대기함
- 에러 발생 시 Build Settings에서 씬 순서 확인: 01_InitialScene → TutorialScene

### 8.4 "카메라가 안 움직여요"
- Camera.main이 씬에 있는지 확인 (Main Camera 태그 필요)
- TutorialManager의 mainCamera 레퍼런스가 자동으로 할당되었는지 확인
- Console에서 카메라 관련 로그 확인

### 8.5 "시간이 멈춘 채로 있어요"
- OnDestroy()에서 Time.timeScale = 1로 복원
- 에러 발생 시에도 복원되도록 try-finally 추가 고려

### 8.6 "HP가 계속 100으로 유지돼요"
- MonitorPlayerStats() 코루틴이 실행 중인지 확인
- PlayerHealth.GetCurrentHealth() 메서드 확인

## 9. 완성 확인

모든 설정이 완료되면:
1. Unity에서 TutorialScene 열기
2. Play 버튼 눌러 테스트
3. 모든 튜토리얼 단계 진행
4. 02_VillageScene으로 정상 전환 확인
5. 건너뛰기 버튼 테스트

---

작성자: Claude Code
최종 수정일: 2025-12-06
