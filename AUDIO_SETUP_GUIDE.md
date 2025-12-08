# 🎵 Puang-Adventure 오디오 시스템 설치 가이드

## 📋 목차
1. [폴더 구조 생성](#1-폴더-구조-생성)
2. [AudioMixer 설정](#2-audiomixer-설정)
3. [AudioManager 설정](#3-audiomanager-설정)
4. [AudioClipData 생성](#4-audioclipdata-생성)
5. [필요한 사운드 목록](#5-필요한-사운드-목록)
6. [게임에 사운드 통합](#6-게임에-사운드-통합)

---

## 1. 폴더 구조 생성

Unity 프로젝트의 `Assets` 폴더에 다음 구조를 생성하세요:

```
Assets/
├── Audio/
│   ├── BGM/              ← 배경음악 파일 (mp3, wav, ogg)
│   ├── SFX/              ← 효과음 파일
│   │   ├── Player/       ← 플레이어 관련 효과음
│   │   ├── Enemy/        ← 적 관련 효과음
│   │   ├── UI/           ← UI 효과음
│   │   └── Ambient/      ← 주변음/환경음
│   └── Voice/            ← 보이스 (선택사항)
└── Scripts/
    └── Audio/            ← 이미 생성됨 (5개 스크립트)
```

---

## 2. AudioMixer 설정

### 2-1. AudioMixer 생성
1. Unity 에디터에서 `Assets` 폴더에 우클릭
2. **Create > Audio Mixer** 선택
3. 이름을 `MasterAudioMixer`로 변경

### 2-2. AudioMixer Groups 설정
1. `MasterAudioMixer`를 더블클릭하여 AudioMixer 창 열기
2. 다음과 같이 Groups 생성:

```
Master
├── BGM
└── SFX
```

3. 각 그룹 우클릭 > **Add exposed parameter**:
   - **BGM** 그룹: "BGMVolume" (이름 정확히 입력!)
   - **SFX** 그룹: "SFXVolume" (이름 정확히 입력!)

> ⚠️ **중요**: 파라미터 이름은 `SettingsPanelController.cs`에서 사용하는 이름과 동일해야 합니다!

### 2-3. SettingsPanelController 연결
1. 씬에서 `SettingsPanelController` 오브젝트 선택
2. Inspector에서 **Audio Mixer** 필드에 `MasterAudioMixer` 드래그
3. **BGM Mixer Parameter**에 `BGMVolume` 입력
4. **SFX Mixer Parameter**에 `SFXVolume` 입력   

---

## 3. AudioManager 설정

### 3-1. 씬에 AudioManager 추가
1. `01_InitialScene` 씬 열기 (DontDestroyOnLoad 오브젝트가 생성되는 씬)
2. Hierarchy에서 우클릭 > **Create Empty**
3. 이름을 `AudioManager`로 변경
4. `AudioManager.cs` 스크립트 추가

### 3-2. Inspector 설정
AudioManager의 Inspector에서 다음을 설정:

#### **Audio Mixer 섹션:**
- **Master Mixer**: `MasterAudioMixer` 드래그
- **BGM Mixer Group**: AudioMixer 창에서 **BGM** 그룹 드래그
- **SFX Mixer Group**: AudioMixer 창에서 **SFX** 그룹 드래그

#### **Scene BGM Mapping 섹션:**
나중에 BGM 오디오 파일을 구한 후 각 씬에 맞는 BGM을 할당:
- **Title BGM**: 타이틀 화면용 BGM
- **Village BGM**: 마을/튜토리얼용 BGM
- **Forest BGM**: 숲 씬용 BGM
- **Cave BGM**: 동굴 씬용 BGM
- **Boss Intense BGM**: 06_UnkillableBossScene용
- **Boss Epic BGM**: 07_BossScene용 (최종 보스)
- **Ending BGM**: 엔딩 씬용 BGM

#### **Settings 섹션:**
- **Scene Transition Fade Time**: `2` (초 단위)
- **Show Debug Logs**: ✅ 체크 (디버깅 용도)

### 3-3. BGMController/SFXController 자동 생성
- AudioManager를 씬에 추가하면 Awake()에서 자동으로 생성됩니다
- 또는 수동으로 자식 오브젝트로 만들어 연결할 수도 있습니다

---

## 4. AudioClipData 생성

### 4-1. ScriptableObject 생성
1. `Assets/Audio` 폴더에서 우클릭
2. **Create > Audio > Audio Clip Data** 선택
3. 이름을 `GameAudioClips`로 변경

### 4-2. AudioManager에 연결
1. AudioManager 오브젝트 선택
2. Inspector의 **Audio Clips** 섹션에 `GameAudioClips` 드래그

### 4-3. 오디오 클립 할당
`GameAudioClips` ScriptableObject를 선택하고 Inspector에서 각 필드에 오디오 파일을 할당합니다.

---

## 5. 필요한 사운드 목록

### 📂 BGM (Background Music) - 7개

| 파일명 | 설명 | 추천 길이 | 저장 위치 |
|--------|------|-----------|-----------|
| `Title_BGM` | 타이틀 화면 음악 (평화롭고 웅장) | 2-3분 (루프) | `Assets/Audio/BGM/` |
| `Village_BGM` | 마을/튜토리얼 음악 (따뜻하고 친근) | 2-3분 (루프) | `Assets/Audio/BGM/` |
| `Forest_BGM` | 숲 탐험 음악 (모험적, 약간 긴장감) | 2-3분 (루프) | `Assets/Audio/BGM/` |
| `Cave_BGM` | 동굴 음악 (어둡고 긴장감) | 2-3분 (루프) | `Assets/Audio/BGM/` |
| `Boss_Intense_BGM` | 첫 보스전 음악 (빠르고 긴박) | 2-3분 (루프) | `Assets/Audio/BGM/` |
| `Boss_Epic_BGM` | 최종 보스전 음악 (서사적, 강렬) | 3-4분 (루프) | `Assets/Audio/BGM/` |
| `Ending_BGM` | 엔딩 음악 (감동적, 희망적) | 2-3분 | `Assets/Audio/BGM/` |

**추천 포맷:** OGG (용량 효율), MP3 (호환성), WAV (고품질)

---

### 🔊 SFX (Sound Effects)

#### **플레이어 사운드** (10개) - `Assets/Audio/SFX/Player/`

| 파일명 | 설명 | AudioClipData 필드명 |
|--------|------|---------------------|
| `Sword_Slash.wav` | 검 휘두르기 소리 (공격 시) | `swordSlash` |
| `Dash_Whoosh.wav` | 대시 효과음 (휙~ 하는 소리) | `dashWhoosh` |
| `Footstep_Walk.wav` | 걷기 발소리 | `footstepWalk` |
| `Footstep_Run.wav` | 달리기 발소리 | `footstepRun` |
| `Player_Hit.wav` | 플레이어 피격음 (아야!) | `playerHit` |
| `Player_Death.wav` | 플레이어 사망 소리 | `playerDeath` |
| `Player_Heal.wav` | 회복 소리 (반짝이는 힐링) | `playerHeal` |
| `Ultimate_Slash.wav` | 궁극기 슬래시 (강력한 베기) | `ultimateSlash` |
| `Weapon_Equip.wav` | 무기 장착 소리 | `weaponEquip` |
| `Stamina_Depleted.wav` | 스태미나 소진 경고음 | `staminaDepleted` |

---

#### **적 사운드** (5개) - `Assets/Audio/SFX/Enemy/`

| 파일명 | 설명 | AudioClipData 필드명 |
|--------|------|---------------------|
| `Enemy_Death.wav` | 적 사망 소리 | `enemyDeath` |
| `Enemy_Hit.wav` | 적 피격음 | `enemyHit` |
| `Enemy_Attack.wav` | 적 공격 소리 | `enemyAttack` |
| `Boss_Appear.wav` | 보스 등장 팡파레 | `bossAppear` |
| `Boss_Victory.wav` | 보스 승리 음악/효과음 | `bossVictory` |

---

#### **UI 사운드** (8개) - `Assets/Audio/SFX/UI/`

| 파일명 | 설명 | AudioClipData 필드명 |
|--------|------|---------------------|
| `UI_Click.wav` | 버튼 클릭 소리 (딸깍) | `uiClick` |
| `UI_Panel_Open.wav` | 패널 열기 소리 (슈욱~) | `uiPanelOpen` |
| `UI_Panel_Close.wav` | 패널 닫기 소리 | `uiPanelClose` |
| `Dialogue_Open.wav` | 다이얼로그 창 열기 | `dialogueOpen` |
| `Dialogue_Close.wav` | 다이얼로그 창 닫기 | `dialogueClose` |
| `Dialogue_Advance.wav` | 다이얼로그 텍스트 진행 (띡) | `dialogueAdvance` |
| `Slider_Adjust.wav` | 슬라이더 조정 소리 | `sliderAdjust` |
| `Tab_Switch.wav` | 탭 전환 소리 | `tabSwitch` |

---

#### **게임플레이 사운드** (7개) - `Assets/Audio/SFX/`

| 파일명 | 설명 | AudioClipData 필드명 |
|--------|------|---------------------|
| `Portal_Enter.wav` | 포털 진입 소리 (워프) | `portalEnter` |
| `Quest_Progress.wav` | 퀘스트 진행/완료 (징~ 하는 소리) | `questProgress` |
| `Item_Pickup.wav` | 아이템 획득 소리 (반짝!) | `itemPickup` |
| `Item_Drop.wav` | 아이템 드롭 소리 | `itemDrop` |
| `Scene_Transition.wav` | 씬 전환 효과음 | `sceneTransition` |
| `Game_Over.wav` | 게임 오버 사운드 | `gameOver` |
| `Ability_Unlock.wav` | 능력 해금 소리 (팡파레) | `abilityUnlock` |

---

#### **주변음** (3개) - `Assets/Audio/SFX/Ambient/`

| 파일명 | 설명 | AudioClipData 필드명 |
|--------|------|---------------------|
| `Ambient_Village.wav` | 마을 주변 소리 (새소리 등) | `ambientVillage` |
| `Ambient_Forest.wav` | 숲 주변 소리 (바람 소리 등) | `ambientForest` |
| `Ambient_Cave.wav` | 동굴 주변 소리 (물방울 등) | `ambientCave` |

---

### 📊 전체 요약
- **BGM**: 7개
- **SFX**: 33개
- **총 오디오 파일**: **40개**

---

## 6. 게임에 사운드 통합

오디오 시스템이 설정되었으면 이제 게임 로직에 통합해야 합니다.

### 6-1. 자주 사용하는 패턴

#### **플레이어 공격음** (PlayerController.cs)
```csharp
void Attack()
{
    // 기존 공격 로직...
    AudioManager.I.PlayPlayerAttackSound();
}
```

#### **플레이어 피격음** (PlayerHealth.cs)
```csharp
public void TakeDamage(int damage)
{
    health -= damage;
    AudioManager.I.PlayPlayerHitSound();
    // 나머지 로직...
}
```

#### **적 사망음** (EnemyHealth.cs)
```csharp
void Die()
{
    AudioManager.I.PlayEnemyDeathSound(transform.position); // 3D 사운드
    // 사망 로직...
}
```

#### **UI 클릭음** (UI_MasterController.cs)
```csharp
public void OnButtonClick()
{
    AudioManager.I.PlayUIClickSound();
}
```

#### **다이얼로그 사운드** (DialogueManager.cs)
```csharp
public void StartDialogue(Dialogue dialogue)
{
    AudioManager.I.PlayDialogueOpenSound();
    AudioManager.I.DuckBGM(); // BGM 볼륨 감소
    // 다이얼로그 시작 로직...
}

public void EndDialogue()
{
    AudioManager.I.PlayDialogueCloseSound();
    AudioManager.I.RestoreBGM(); // BGM 볼륨 복구
    // 다이얼로그 종료 로직...
}
```

---

## 7. 테스트 방법

### 7-1. AudioManager 디버그 메뉴
AudioManager를 선택하고 Inspector에서 우클릭:
- **Debug: Log Audio System Status** - 현재 오디오 상태 출력
- **Debug: Stop All Audio** - 모든 오디오 정지

### 7-2. SFXController 디버그 메뉴
SFXController에서 우클릭:
- **Debug: Log SFX Statistics** - 효과음 통계 출력
- **Debug: Stop All SFX** - 모든 효과음 정지

### 7-3. SettingsPanel 볼륨 조절 테스트
1. 게임 실행
2. 설정 패널 열기
3. BGM/SFX 슬라이더 조정
4. 실시간으로 볼륨 변경 확인

---

## 8. 무료 오디오 리소스 추천

### BGM
- [Incompetech](https://incompetech.com/music/) - 무료 BGM (CC BY 라이선스)
- [Purple Planet](https://www.purple-planet.com/) - 로열티 프리 음악
- [Free Music Archive](https://freemusicarchive.org/) - 다양한 장르

### SFX
- [Freesound.org](https://freesound.org/) - 커뮤니티 제작 효과음
- [Zapsplat](https://www.zapsplat.com/) - 프리미엄 품질 SFX
- [Mixkit](https://mixkit.co/free-sound-effects/) - 무료 게임 효과음

---

## 9. 문제 해결

### 문제: 사운드가 재생되지 않음
- AudioManager가 `01_InitialScene`에 있는지 확인
- AudioClipData가 할당되었는지 확인
- AudioMixer Groups가 올바르게 연결되었는지 확인
- Console에서 에러 메시지 확인

### 문제: 볼륨 슬라이더가 작동하지 않음
- SettingsPanelController에 AudioMixer가 할당되었는지 확인
- AudioMixer 파라미터 이름이 정확한지 확인 (대소문자 구분)
- AudioMixer의 Exposed Parameters 확인

### 문제: BGM이 씬 전환 시 끊김
- AudioManager가 DontDestroyOnLoad인지 확인
- SceneManager.sceneLoaded 이벤트가 제대로 연결되었는지 확인

---

## 완료! 🎉

이제 Puang-Adventure에 완전한 오디오 시스템이 구축되었습니다!

다음 단계:
1. 오디오 파일 수집/제작
2. AudioClipData에 할당
3. 게임 로직에 사운드 호출 추가
4. 테스트 및 볼륨 밸런싱
