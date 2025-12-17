// ...existing code...
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    private Vector2 movementInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.6f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private float lastDashTime = -99f;
    private bool isDashEnabled = false; // 대시 기능 활성화 여부

    [Header("Ultimate")]
    private bool isUltActive = false; // 궁극기 사용 중 여부 (PlayerUlt에서 제어)

    [Header("Attack")]
    [Tooltip("Hierarchy의 AttackArea(자식) Collider2D를 할당하세요. Is Trigger 체크 필요")]
    [SerializeField] private Collider2D attackAreaCollider;
    [SerializeField] private float attackDuration = 0.12f;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Attack Effect (Sword Slash)")]
    [Tooltip("검기 이펙트 SpriteRenderer들을 할당하세요 (여러 방향별 이펙트 가능)")]
    [SerializeField] private SpriteRenderer[] swordSlashEffects;
    [Tooltip("검기 이펙트 Animator (SlashEffect 오브젝트의 Animator)")]
    [SerializeField] private Animator slashEffectAnimator;
    [Tooltip("검기 이펙트 컨트롤러 (자동으로 이펙트 숨김 처리)")]
    [SerializeField] private SlashEffectController slashEffectController;

    [Header("Attack Range by Weapon Tier")]
    [SerializeField] private float defaultAttackRadius = 0.7f;  // 무기 없거나 다른 아이템일 때
    [SerializeField] private float tier0AttackRadius = 1.0f;    // Tier 0 무기 (칼자루 - Item_WeaponTier0)
    [SerializeField] private float tier1AttackRadius = 1.2f;    // Tier 1 무기 (숲의 검 - Item_WeaponTier1)
    [SerializeField] private float tier2AttackRadius = 1.5f;    // Tier 2 무기 (중붕이의 검 - Item_WeaponTier2)

    [Header("Attack Damage by Weapon Tier")]
    [SerializeField] private int defaultAttackDamage = 5;       // 무기 없거나 다른 아이템일 때 (주먹)
    [SerializeField] private int tier0AttackDamage = 10;        // Tier 0 무기 (칼자루)
    [SerializeField] private int tier1AttackDamage = 20;        // Tier 1 무기 (숲의 검)
    [SerializeField] private int tier2AttackDamage = 35;        // Tier 2 무기 (중붕이의 검)

    [Header("Sword Slash Effect Colors by Weapon Tier")]
    [SerializeField] private Color tier0SlashColor = Color.white;           // Tier 0 검기 색상 (흰색)
    [SerializeField] private Color tier1SlashColor = Color.green;           // Tier 1 검기 색상 (초록색)
    [SerializeField] private Color tier2SlashColor = new Color(1f, 0.84f, 0f); // Tier 2 검기 색상 (금색)

    private bool isAttacking = false;
    private float lastAttackTime = -99f;
    private ContactFilter2D attackFilter;
    private readonly List<Collider2D> overlapResults = new List<Collider2D>();
    private ItemData currentWeapon = null; // 현재 장착된 무기 추적

    // Stamina reference
    private PlayerStamina stamina;

    //추가부분.김주은
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector3 lastInputDirection = Vector3.down;

    /// <summary>
    /// 방향을 4방향(상/하/좌/우) 중 가장 가까운 방향으로 스냅합니다.
    /// </summary>
    private Vector2 SnapToFourDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            return Vector2.down; // 기본값

        // 상하좌우 중 가장 큰 성분을 선택
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absY > absX)
        {
            // 상 또는 하
            return direction.y > 0 ? Vector2.up : Vector2.down;
        }
        else
        {
            // 좌 또는 우
            return direction.x > 0 ? Vector2.right : Vector2.left;
        }
    }

    private void Awake()
    {
        if (GetComponent<PlayerInput>() == null)
            Debug.Log("⚠ PlayerInput 컴포넌트가 없습니다. 새 Input System 사용 시 PlayerInput 추가를 권장합니다.");

        attackFilter = new ContactFilter2D();
        attackFilter.SetLayerMask(enemyLayer);
        attackFilter.useTriggers = true;

        stamina = GetComponent<PlayerStamina>();
        if (stamina == null)
            Debug.LogWarning("PlayerStamina 컴포넌트가 없습니다. 스태미나 연동 기능 비활성화됩니다.");


        // Animator와 SpriteRenderer 추가부분.김주은
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            Debug.LogError("PlayerController: Animator 컴포넌트를 찾을 수 없습니다. Player 오브젝트에 Animator 컴포넌트를 추가했는지 확인하세요.");

        // 검기 이펙트 자동 찾기 (할당되지 않은 경우)
        if (swordSlashEffects == null || swordSlashEffects.Length == 0)
        {
            AutoFindSlashEffects();
        }

        // 검기 이펙트 Animator 자동 찾기 (할당되지 않은 경우)
        if (slashEffectAnimator == null)
        {
            AutoFindSlashEffectAnimator();
        }

        // 검기 이펙트 컨트롤러 자동 찾기 (할당되지 않은 경우)
        if (slashEffectController == null)
        {
            AutoFindSlashEffectController();
        }
    }

    /// <summary>
    /// 자식 오브젝트에서 검기 이펙트를 자동으로 찾습니다.
    /// 이름에 "slash", "effect", "sword" 등이 포함된 SpriteRenderer를 찾습니다.
    /// </summary>
    private void AutoFindSlashEffects()
    {
        List<SpriteRenderer> foundEffects = new List<SpriteRenderer>();

        // 모든 자식 오브젝트의 SpriteRenderer 검색 (비활성화된 것도 포함)
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        Debug.Log($"[PlayerController] 검기 이펙트 검색 시작... 총 {allRenderers.Length}개의 SpriteRenderer 발견");

        foreach (var renderer in allRenderers)
        {
            // Player 자신의 SpriteRenderer는 제외
            if (renderer == spriteRenderer)
            {
                Debug.Log($"[PlayerController]   - {renderer.gameObject.name} (Player 본인, 제외)");
                continue;
            }

            string objName = renderer.gameObject.name.ToLower();
            Debug.Log($"[PlayerController]   - 검사 중: {renderer.gameObject.name}");

            // 검기 이펙트로 추정되는 오브젝트 이름 패턴
            if (objName.Contains("slash") ||
                objName.Contains("effect") ||
                objName.Contains("sword") ||
                objName.Contains("attack"))
            {
                foundEffects.Add(renderer);
                Debug.Log($"[PlayerController] ✅ 검기 이펙트 발견: {renderer.gameObject.name} (활성화: {renderer.gameObject.activeSelf})");
            }
        }

        if (foundEffects.Count > 0)
        {
            swordSlashEffects = foundEffects.ToArray();
            Debug.Log($"[PlayerController] ✅ 총 {swordSlashEffects.Length}개의 검기 이펙트를 자동으로 찾았습니다.");

            // 초기 색상 및 크기 설정 (기본 흰색, 크기 1.0배)
            UpdateSwordSlashColor(tier0SlashColor, 1.0f);
        }
        else
        {
            Debug.LogWarning("[PlayerController] ⚠ 검기 이펙트를 찾지 못했습니다. Player의 자식 오브젝트 이름에 'slash', 'effect', 'sword', 'attack' 등을 포함시키거나 Inspector에서 수동으로 할당하세요.");
        }
    }

    /// <summary>
    /// 자식 오브젝트에서 검기 이펙트 Animator를 자동으로 찾습니다.
    /// 이름에 "slash", "effect", "sword" 등이 포함된 Animator를 찾습니다.
    /// </summary>
    private void AutoFindSlashEffectAnimator()
    {
        // 모든 자식 오브젝트의 Animator 검색
        Animator[] allAnimators = GetComponentsInChildren<Animator>(true);

        foreach (var animator in allAnimators)
        {
            // Player 자신의 Animator는 제외
            if (animator == anim)
                continue;

            string objName = animator.gameObject.name.ToLower();

            // 검기 이펙트로 추정되는 오브젝트 이름 패턴
            if (objName.Contains("slash") ||
                objName.Contains("effect") ||
                objName.Contains("sword") ||
                objName.Contains("attack"))
            {
                slashEffectAnimator = animator;
                Debug.Log($"[PlayerController] 검기 이펙트 Animator 발견: {animator.gameObject.name}");
                return;
            }
        }

        Debug.LogWarning("[PlayerController] 검기 이펙트 Animator를 찾지 못했습니다. SlashEffect 오브젝트의 Animator를 Inspector에서 수동으로 할당하세요.");
    }

    /// <summary>
    /// 자식 오브젝트에서 검기 이펙트 컨트롤러를 자동으로 찾습니다.
    /// </summary>
    private void AutoFindSlashEffectController()
    {
        // 모든 자식 오브젝트의 SlashEffectController 검색
        SlashEffectController[] allControllers = GetComponentsInChildren<SlashEffectController>(true);

        if (allControllers.Length > 0)
        {
            slashEffectController = allControllers[0];
            Debug.Log($"[PlayerController] 검기 이펙트 컨트롤러 발견: {slashEffectController.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[PlayerController] 검기 이펙트 컨트롤러를 찾지 못했습니다. SlashEffect 오브젝트에 SlashEffectController를 추가하세요.");
        }
    }

    private void Update()
    {
        // 궁극기 사용 중에는 이동/공격 차단
        if (!isDashing && !isAttacking && !isUltActive)
            MovePlayer();

        // Hotbar 1번 칸의 무기에 따라 공격 범위 업데이트
        UpdateAttackRange();
    }

    // ===================== Input System 콜백 (InputAction.CallbackContext) =====================
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        // 궁극기 사용 중에는 공격 차단
        if (isUltActive) return;

        if (context.performed)
            Attack();
    }

    // Dash: 대시 시 스태미나 소비 시도
    public void OnDash(InputAction.CallbackContext context)
    {
        // 궁극기 사용 중에는 대시 차단
        if (isUltActive) return;

        // Block dash input if dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            Debug.Log("[PlayerController] 대시 입력 차단: 대화 중");
            return;
        }

        // Block dash if not enabled
        if (!isDashEnabled)
        {
            Debug.Log($"[PlayerController] 대시 입력 차단: 대시 기능 비활성화 상태 (isDashEnabled={isDashEnabled})");
            return;
        }

        if (context.performed)
        {
            Debug.Log("[PlayerController] 대시 입력 수신 - TryDash() 호출");
            TryDash();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (stamina == null) return;

        if (context.started)
        {
        // Shift를 누르기 시작한 순간
            stamina.SetSprint(true);
        }
        else if (context.canceled)
        {
        // Shift를 뗀 순간
            stamina.SetSprint(false);
        }
    }


    // ===================== Send Messages 호환 오버로드 (PlayerInput Behavior: Send Messages) =====================
    public void OnMove(InputValue value) => movementInput = value.Get<Vector2>();
    public void OnAttack(InputValue value)
    {
        if (value.Get<float>() > 0f) Attack();
    }
    public void OnDash(InputValue value)
    {
        // Block dash input if dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            Debug.Log("[PlayerController] 대시 입력 차단: 대화 중");
            return;
        }

        // Block dash if not enabled
        if (!isDashEnabled)
        {
            Debug.Log($"[PlayerController] 대시 입력 차단: 대시 기능 비활성화 상태 (isDashEnabled={isDashEnabled})");
            return;
        }

        if (value.Get<float>() > 0f)
        {
            Debug.Log("[PlayerController] 대시 입력 수신 - TryDash() 호출");
            TryDash();
        }
    }
    public void OnSprint(InputValue value)
    {
        if (stamina == null) return;
        bool sprinting = value.isPressed;
        stamina.SetSprint(sprinting);
    }


    // ===================== 이동 =====================
    private void MovePlayer()
    {
        Vector3 dir = new Vector3(movementInput.x, movementInput.y, 0f);
        bool isWalking = dir.sqrMagnitude >= 0.0001f;

        if (!isWalking)
        {
            // walking 상태 전달
            if (stamina != null) stamina.SetWalking(false);

            // Animator 업데이트 (멈춰있을 때 Idle로 전환)
            if (anim != null)
            {
                anim.SetBool("IsWalking", false);
                // 마지막 방향 유지
                anim.SetFloat("MoveX", lastInputDirection.x);
                anim.SetFloat("MoveY", lastInputDirection.y);
            }
            return;
        }

        dir = dir.normalized;

        // sprint 상태는 Stamina에서 읽음(존재하지 않으면 기존 로컬 동작)
        bool sprinting = (stamina != null) ? stamina.IsSprinting : false;
        float baseSpeed = moveSpeed * (sprinting ? sprintMultiplier : 1f);

        // 스태미나 고갈 시 속도 보정
        float exhaustedMult = (stamina != null) ? stamina.GetExhaustedSpeedMultiplier() : 1f;
        float speed = baseSpeed * exhaustedMult;

        transform.Translate(dir * speed * Time.deltaTime, Space.World);

        //김주은.추가
        if (isWalking)
        {
        // 실제로 움직이고 있을 때만 방향 저장
        lastInputDirection = dir.normalized;
        }


        // walking 상태 전달 (스프린트 중이면 walking=false)
        if (stamina != null) stamina.SetWalking(!sprinting && isWalking);


        //애니메이션 처리 - MoveX/MoveY Float 방식 (간단한 방향 처리)
        if (anim != null)
        {
            // 1. IsWalking Bool (Idle <-> Walk 전환) - Speed보다 명확함
            anim.SetBool("IsWalking", isWalking);

            // 2. 방향 처리 (MoveX, MoveY)
            if (isWalking)
            {
                // 움직일 때는 입력 방향을 그대로 전달
                anim.SetFloat("MoveX", movementInput.x);
                anim.SetFloat("MoveY", movementInput.y);
            }
            // Idle 상태에서는 마지막 방향 유지 (lastInputDirection 사용)
            else
            {
                anim.SetFloat("MoveX", lastInputDirection.x);
                anim.SetFloat("MoveY", lastInputDirection.y);
            }
        }
    }

    // ===================== 대시 =====================
    private void TryDash()
    {
        if (Time.time < lastDashTime + dashCooldown) return;

        Vector3 dir = lastInputDirection;
        if (dir.sqrMagnitude < 0.01f) return;

        // 스태미나가 있으면 대시 비용 소모 시도
        if (stamina != null)
        {
            if (!stamina.TryConsumeDash())
            {
                Debug.Log("대시 실패: 스태미나 부족");
                return;
            }
        }

        StartCoroutine(Dash(dir.normalized));
    }

    private IEnumerator Dash(Vector3 direction)
    {
        isDashing = true;

        // 🔊 대시 사운드 재생
        AudioManager.I?.PlayPlayerDashSound();

        //김주은 추가부분
        if (anim != null)
        {
            anim.SetTrigger("Dash"); // 대시 트리거 발동
            // 대시 방향을 4방향으로 스냅 (자연스러운 전환)
            Vector2 snappedDir = SnapToFourDirection(direction);
            anim.SetFloat("MoveX", snappedDir.x);
            anim.SetFloat("MoveY", snappedDir.y);
        }
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }


        float start = Time.time;

        while (Time.time < start + dashDuration)
        {
            transform.Translate(direction * dashSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        isDashing = false;

        //김주은 추가부분
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // 물리 시스템 복구
        }


        lastDashTime = Time.time;
    }

    // ===================== 공격 (AttackArea 사용) =====================
    private void Attack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        // 스태미나가 있으면 공격 비용 소모 시도
        if (stamina != null)
        {
            if (!stamina.TryConsumeAttack())
            {
                Debug.Log("공격 실패: 스태미나 부족");
                return;
            }
        }

        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        // 🔊 공격 사운드 재생
        AudioManager.I?.PlayPlayerAttackSound();

        //김주은 추가부분
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (anim != null)
        {
            anim.SetTrigger("Attack"); // 공격 시작 시 Attack Trigger 발동 (Player)
            // 공격 방향을 4방향으로 스냅 (자연스러운 전환)
            Vector2 snappedDir = SnapToFourDirection(lastInputDirection);
            anim.SetFloat("MoveX", snappedDir.x);
            anim.SetFloat("MoveY", snappedDir.y);
        }

        // 🔹 검기 이펙트 Animator 트리거 (SlashEffect)
        // 무기가 있을 때만 검기 이펙트 재생
        // SlashEffectController가 Animator 애니메이션을 자동으로 감지하여 표시/숨김 처리
        if (slashEffectAnimator != null && currentWeapon != null && currentWeapon.isWeapon)
        {
            slashEffectAnimator.SetTrigger("Attack");
            Debug.Log("[PlayerController] 검기 이펙트 애니메이션 트리거 발동!");
        }
        else if (slashEffectAnimator != null)
        {
            Debug.Log("[PlayerController] 무기가 없어서 검기 이펙트를 재생하지 않습니다.");
        }
        // 공격 시 Lunge (마지막 방향으로 돌진)
        if (rb != null)
        {
            float lungeDistance = 0.3f;

            // 마지막 입력 방향으로 Lunge 실행 (훨씬 간단!)
            if (lastInputDirection.sqrMagnitude > 0.1f)
            {
                transform.Translate(lastInputDirection.normalized * lungeDistance, Space.World);
            }
        }


        // 공격 지속 시간 동안 기다림 (애니메이션 동기화 용이)
        yield return new WaitForSeconds(attackDuration);

        //김주은 추가
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // 물리 시스템 복구
        }

        // 현재 무기에 따른 공격력 계산
        int damage = GetCurrentWeaponDamage();

        // 공격 판정: AttackArea 콜라이더 범위 내의 적 검색
        if (attackAreaCollider != null)
        {
            overlapResults.Clear();
            attackAreaCollider.Overlap(attackFilter, overlapResults);

            foreach (var col in overlapResults)
            {
                if (col == null) continue;

                EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"공격 성공: {col.name} (데미지: {damage})");
                }
                else
                {
                    Debug.LogWarning($"⚠ {col.name}에 EnemyHealth 컴포넌트가 없습니다!");
                }
            }
        }
        else
        {
            // fallback: 기존 방식 (AttackArea가 없을 때)
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 1.0f, enemyLayer);
            foreach (var enemy in hitEnemies)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"공격 성공 (fallback): {enemy.name} (데미지: {damage})");
                }
            }
        }

        isAttacking = false;
    }

    /// <summary>
    /// 현재 장착된 무기에 따른 공격력 반환
    /// </summary>
    private int GetCurrentWeaponDamage()
    {
        // 무기가 있고 isWeapon이 true인 경우
        if (currentWeapon != null && currentWeapon.isWeapon)
        {
            switch (currentWeapon.weaponTier)
            {
                case 0:
                    return tier0AttackDamage; // 칼자루: 10 데미지
                case 1:
                    return tier1AttackDamage; // 숲의 검: 20 데미지
                case 2:
                    return tier2AttackDamage; // 중붕이의 검: 35 데미지
                default:
                    return defaultAttackDamage; // 알 수 없는 티어: 기본 데미지
            }
        }
        else
        {
            // 무기가 없거나 무기가 아닌 아이템: 주먹 공격 (5 데미지)
            return defaultAttackDamage;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackAreaCollider is CircleCollider2D cc)
        {
            Gizmos.DrawWireSphere(cc.transform.position + (Vector3)cc.offset, cc.radius * cc.transform.lossyScale.x);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1.0f);
        }
    }

    // ===================== 공격 범위 업데이트 =====================
    /// <summary>
    /// Hotbar 1번 칸의 무기에 따라 AttackArea의 Radius와 검기 색상을 업데이트합니다.
    /// </summary>
    private void UpdateAttackRange()
    {
        // // Inventory와 AttackArea가 없으면 리턴
        // if (Inventory.instance == null || attackAreaCollider == null)
        // {
        //     Debug.LogWarning("[PlayerController] UpdateAttackRange: Inventory 또는 AttackArea가 없습니다.");
        //     return;
        // }

        // Hotbar 1번 칸(index 0) 체크
        ItemData hotbarSlot0 = null;
        if (Inventory.instance.items != null && Inventory.instance.items.Length > 0)
        {
            hotbarSlot0 = Inventory.instance.items[0];
        }

        // 무기가 변경되었는지 체크 (최적화를 위해)
        if (hotbarSlot0 == currentWeapon)
            return;

        Debug.Log($"[PlayerController] 🔄 무기 변경 감지: {currentWeapon?.itemName ?? "없음"} → {hotbarSlot0?.itemName ?? "없음"}");
        currentWeapon = hotbarSlot0;

        // AttackArea가 CircleCollider2D인지 확인
        if (attackAreaCollider is CircleCollider2D circleCollider)
        {
            float newRadius = defaultAttackRadius;
            Color newSlashColor = tier0SlashColor; // 기본 색상

            // 무기가 있고 isWeapon이 true인 경우
            if (currentWeapon != null && currentWeapon.isWeapon)
            {
                switch (currentWeapon.weaponTier)
                {
                    case 0:
                        newRadius = tier0AttackRadius;
                        newSlashColor = tier0SlashColor;
                        Debug.Log($"[PlayerController] 무기 Tier 0 (칼자루) 장착: Attack Radius = {newRadius}, 검기 색상 = 흰색");
                        break;
                    case 1:
                        newRadius = tier1AttackRadius;
                        newSlashColor = tier1SlashColor;
                        Debug.Log($"[PlayerController] 무기 Tier 1 (숲의 검) 장착: Attack Radius = {newRadius}, 검기 색상 = 초록색");
                        break;
                    case 2:
                        newRadius = tier2AttackRadius;
                        newSlashColor = tier2SlashColor;
                        Debug.Log($"[PlayerController] 무기 Tier 2 (중붕이의 검) 장착: Attack Radius = {newRadius}, 검기 색상 = 금색");
                        break;
                    default:
                        newRadius = defaultAttackRadius;
                        newSlashColor = tier0SlashColor;
                        Debug.Log($"[PlayerController] 알 수 없는 무기 Tier ({currentWeapon.weaponTier}): Attack Radius = {newRadius}");
                        break;
                }
            }
            else
            {
                // 무기가 아니거나 아무것도 없을 때
                if (currentWeapon != null)
                    Debug.Log($"[PlayerController] 무기가 아닌 아이템 장착 ({currentWeapon.itemName}): Attack Radius = {newRadius}");
                else
                    Debug.Log($"[PlayerController] Hotbar 1번 칸 비어있음: Attack Radius = {newRadius}");
            }

            circleCollider.radius = newRadius;

            // 검기 이펙트 색상 및 크기 업데이트
            float sizeMultiplier = newRadius / defaultAttackRadius; // 기본 크기 대비 배율 계산
            UpdateSwordSlashColor(newSlashColor, sizeMultiplier);
        }
        else
        {
            Debug.LogWarning("[PlayerController] AttackArea가 CircleCollider2D가 아닙니다!");
        }
    }

    /// <summary>
    /// 검기 이펙트의 색상과 크기를 업데이트하고, 무기 유무에 따라 활성화/비활성화합니다.
    /// </summary>
    /// <param name="color">검기 색상</param>
    /// <param name="sizeMultiplier">크기 배율 (기본값 1.0)</param>
    private void UpdateSwordSlashColor(Color color, float sizeMultiplier = 1.0f)
    {
        if (swordSlashEffects == null || swordSlashEffects.Length == 0)
        {
            Debug.LogWarning("[PlayerController] 검기 이펙트 SpriteRenderer가 할당되지 않았습니다.");
            return;
        }

        // 무기가 없거나 비무기 아이템이면 검기 이펙트 비활성화
        bool hasWeapon = currentWeapon != null && currentWeapon.isWeapon;

        int colorChangedCount = 0;
        foreach (var slashEffect in swordSlashEffects)
        {
            if (slashEffect != null)
            {
                // 무기 유무에 따라 SlashEffect 오브젝트 활성화/비활성화
                slashEffect.gameObject.SetActive(hasWeapon);

                if (hasWeapon)
                {
                    // SpriteRenderer.color 설정 (가장 확실한 방법)
                    slashEffect.color = color;

                    // 검기 크기 조정 (공격 범위에 비례)
                    slashEffect.transform.localScale = Vector3.one * sizeMultiplier;

                    // Material Shader 정보 로그
                    if (slashEffect.material != null)
                    {
                        Debug.Log($"[PlayerController]     Material: {slashEffect.material.name}, Shader: {slashEffect.material.shader.name}");
                    }

                    // PropertyBlock을 사용한 색상 설정 (여러 속성 이름 시도)
                    MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                    slashEffect.GetPropertyBlock(propertyBlock);

                    // 다양한 Shader 속성 이름 시도
                    propertyBlock.SetColor("_Color", color);
                    propertyBlock.SetColor("_MainColor", color);
                    propertyBlock.SetColor("_TintColor", color);

                    slashEffect.SetPropertyBlock(propertyBlock);

                    colorChangedCount++;
                    Debug.Log($"[PlayerController]   - {slashEffect.gameObject.name} 활성화");
                    Debug.Log($"[PlayerController]     목표 색상: RGB({color.r:F2}, {color.g:F2}, {color.b:F2})");
                    Debug.Log($"[PlayerController]     크기 배율: {sizeMultiplier:F2}x (localScale: {slashEffect.transform.localScale})");
                    Debug.Log($"[PlayerController]     현재 색상: RGB({slashEffect.color.r:F2}, {slashEffect.color.g:F2}, {slashEffect.color.b:F2})");
                }
                else
                {
                    Debug.Log($"[PlayerController]   - {slashEffect.gameObject.name} 비활성화 (무기 없음)");
                }
            }
        }

        if (hasWeapon && colorChangedCount > 0)
        {
            Debug.Log($"[PlayerController] ✅ {colorChangedCount}개 검기 이펙트 활성화 및 색상 변경 완료: RGB({color.r:F2}, {color.g:F2}, {color.b:F2})");
        }
        else if (!hasWeapon)
        {
            Debug.Log($"[PlayerController] ✅ 검기 이펙트 비활성화 완료 (무기 없음)");
        }
        else
        {
            Debug.LogWarning("[PlayerController] ⚠ 유효한 검기 이펙트를 찾지 못했습니다.");
        }
    }

    // ===================== 궁극기 상태 제어 (외부 호출용) =====================
    /// <summary>
    /// 궁극기 활성 상태를 설정합니다. (PlayerUlt에서 호출)
    /// </summary>
    public void SetUltActive(bool active)
    {
        isUltActive = active;
        Debug.Log($"[PlayerController] 궁극기 상태 변경: {active}");
    }

    /// <summary>
    /// 현재 궁극기 활성 상태를 반환합니다.
    /// </summary>
    public bool IsUltActive()
    {
        return isUltActive;
    }

    // ===================== 대시 활성화/비활성화 (외부 호출용) =====================
    /// <summary>
    /// 대시 기능을 활성화합니다.
    /// </summary>
    public void EnableDash()
    {
        Debug.Log($"[PlayerController] EnableDash() 호출됨 - 현재 상태: {isDashEnabled}");
        isDashEnabled = true;
        Debug.Log($"[PlayerController] ✅ 대시 기능이 활성화되었습니다! 새 상태: {isDashEnabled}");
        Debug.Log($"[PlayerController] 이제 Space 키를 눌러 대시를 사용할 수 있습니다.");
    }

    /// <summary>
    /// 대시 기능을 비활성화합니다.
    /// </summary>
    public void DisableDash()
    {
        Debug.Log($"[PlayerController] DisableDash() 호출됨 - 현재 상태: {isDashEnabled}");
        isDashEnabled = false;
        Debug.Log($"[PlayerController] ❌ 대시 기능이 비활성화되었습니다. 새 상태: {isDashEnabled}");
    }

    /// <summary>
    /// 대시 활성화 상태를 반환합니다.
    /// </summary>
    public bool IsDashEnabled()
    {
        return isDashEnabled;
    }
}
// ...existing code...