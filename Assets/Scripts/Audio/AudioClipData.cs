using UnityEngine;

/// <summary>
/// ScriptableObject to organize and categorize audio clips.
/// Inspector에서 AudioClip을 할당하여 사용합니다.
/// </summary>
[CreateAssetMenu(fileName = "AudioClipData", menuName = "Audio/Audio Clip Data")]
public class AudioClipData : ScriptableObject
{
    [Header("=== Player Sounds ===")]
    [Tooltip("검 휘두르기 소리 (공격 시)")]
    public AudioClip swordSlash;

    [Tooltip("대시 효과음 (휙~ 하는 소리)")]
    public AudioClip dashWhoosh;

    [Tooltip("걷기 발소리")]
    public AudioClip footstepWalk;

    [Tooltip("달리기 발소리")]
    public AudioClip footstepRun;

    [Tooltip("플레이어 피격음 (아파!)")]
    public AudioClip playerHit;

    [Tooltip("플레이어 사망 소리")]
    public AudioClip playerDeath;

    [Tooltip("회복 소리 (힐링 효과음)")]
    public AudioClip playerHeal;

    [Tooltip("궁극기 슬래시 소리 (8연속 베기)")]
    public AudioClip ultimateSlash;

    [Tooltip("무기 장착 소리")]
    public AudioClip weaponEquip;

    [Tooltip("스태미나 소진 경고음")]
    public AudioClip staminaDepleted;

    [Header("=== Enemy Sounds ===")]
    [Tooltip("적 사망 소리")]
    public AudioClip enemyDeath;

    [Tooltip("적 피격음")]
    public AudioClip enemyHit;

    [Tooltip("적 공격 소리")]
    public AudioClip enemyAttack;

    [Tooltip("보스 등장 팡파레")]
    public AudioClip bossAppear;

    [Tooltip("보스 사망 소리")]
    public AudioClip bossDeath;

    [Tooltip("보스 사망/승리 음악")]
    public AudioClip bossVictory;

    [Header("=== UI Sounds ===")]
    [Tooltip("버튼 클릭 소리")]
    public AudioClip uiClick;

    [Tooltip("패널 열기 소리")]
    public AudioClip uiPanelOpen;

    [Tooltip("패널 닫기 소리")]
    public AudioClip uiPanelClose;

    [Tooltip("다이얼로그 창 열기")]
    public AudioClip dialogueOpen;

    [Tooltip("다이얼로그 창 닫기")]
    public AudioClip dialogueClose;

    [Tooltip("다이얼로그 텍스트 진행 (띡)")]
    public AudioClip dialogueAdvance;

    [Tooltip("슬라이더 조정 소리")]
    public AudioClip sliderAdjust;

    [Tooltip("탭 전환 소리")]
    public AudioClip tabSwitch;

    [Header("=== Gameplay Sounds ===")]
    [Tooltip("포털 진입 소리")]
    public AudioClip portalEnter;

    [Tooltip("퀘스트 진행/완료 소리")]
    public AudioClip questProgress;

    [Tooltip("아이템 획득 소리 (반짝!)")]
    public AudioClip itemPickup;

    [Tooltip("아이템 드롭 소리")]
    public AudioClip itemDrop;

    [Tooltip("씬 전환 소리 (트랜지션)")]
    public AudioClip sceneTransition;

    [Tooltip("게임 오버 사운드")]
    public AudioClip gameOver;

    [Tooltip("능력 해금 소리 (Dash/Ultimate 획득 시)")]
    public AudioClip abilityUnlock;

    [Header("=== Ambient Sounds ===")]
    [Tooltip("마을 주변 소리 (새소리 등)")]
    public AudioClip ambientVillage;

    [Tooltip("숲 주변 소리 (바람 소리 등)")]
    public AudioClip ambientForest;

    [Tooltip("동굴 주변 소리 (물방울 등)")]
    public AudioClip ambientCave;
}
