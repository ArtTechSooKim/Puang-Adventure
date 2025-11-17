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

    [Header("Attack")]
    [Tooltip("Hierarchy의 AttackArea(자식) Collider2D를 할당하세요. Is Trigger 체크 필요")]
    [SerializeField] private Collider2D attackAreaCollider;
    [SerializeField] private float attackDuration = 0.12f;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    private bool isAttacking = false;
    private float lastAttackTime = -99f;
    private ContactFilter2D attackFilter;
    private readonly List<Collider2D> overlapResults = new List<Collider2D>();

    // Stamina reference
    private PlayerStamina stamina;

    //추가부분.김주은
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector3 lastInputDirection = Vector3.down;

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
    }

    private void Update()
    {
        if (!isDashing && !isAttacking)
            MovePlayer();
    }

    // ===================== Input System 콜백 (InputAction.CallbackContext) =====================
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
            Attack();
    }

    // Dash: 대시 시 스태미나 소비 시도
    public void OnDash(InputAction.CallbackContext context)
    {
        // Block dash input if dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
            return;

        if (context.performed)
            TryDash();
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
            return;

        if (value.Get<float>() > 0f) TryDash();
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


        //애니메이션 처리(김주은.추가부분)
        if (anim != null)
        {
            // 1. Speed (Idle <-> Walk 전환)
            // movementInput.magnitude는 속도의 크기입니다.
            anim.SetFloat("Speed", movementInput.magnitude);

            // 2. 방향 처리 (FacingBack, FacingFront, flipX)
            if (isWalking) // 움직일 때만 방향 처리
            {
                // Y축 입력이 X축 입력보다 크거나 같을 때 Y축 우선 (대각선 포함, 0.01은 민감도)
                if (Mathf.Abs(movementInput.y) >= Mathf.Abs(movementInput.x) && Mathf.Abs(movementInput.y) > 0.01f)
                {
                    // W 입력 (뒷모습)
                    if (movementInput.y > 0) 
                    {
                        anim.SetBool("FacingBack", true);
                        anim.SetBool("FacingFront", false);
                    }
                    // S 입력 (앞모습)
                    else 
                    {
                        anim.SetBool("FacingBack", false);
                        anim.SetBool("FacingFront", true);
                    }
                }
                // X축 입력이 Y축보다 클 때 (측면)
                else
                {
                    // 상하 방향 Bool 초기화 (측면 애니메이션이 재생되도록)
                    anim.SetBool("FacingBack", false);
                    anim.SetBool("FacingFront", false);

                    // A/D 입력에 따른 좌우 반전(Flipping) 처리
                    if (movementInput.x > 0)
                    {
                        spriteRenderer.flipX = false; // 오른쪽
                    }
                    else if (movementInput.x < 0)
                    {
                        spriteRenderer.flipX = true; // 왼쪽
                    }
                }
            }
            else // 멈췄을 때 (Idle 상태)
            {
                // 멈췄을 때도 캐릭터는 마지막 방향을 유지해야 합니다.
                // Idle 상태에서는 방향 Bool 값을 유지하고, Walk 상태에서만 갱신됩니다.
                
                // 만약 Idle 상태에서 방향을 초기화하고 싶다면 다음 코드를 사용하세요:
                // anim.SetBool("FacingBack", false);
                // anim.SetBool("FacingFront", false);
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

        //김주은 추가부분
        if (anim != null) anim.SetBool("Dash", true); // 대시 시작 시 Dash Bool을 True로
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
        if (anim != null) anim.SetBool("Dash", false); // 대시 종료 시 Dash Bool을 False로
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

        //김주은 추가부분
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (anim != null) anim.SetTrigger("Attack"); // 공격 시작 시 Attack Trigger 발동
        if (rb != null) 
        {
            float lungeDistance = 0.3f; 
            Vector3 lungeDirection = Vector3.zero;

            // 1. 현재 바라보는 방향을 확인합니다. (가장 최신 정보를 사용)
            if (anim.GetBool("FacingBack"))
            {
                lungeDirection = Vector3.up; // W 방향 (뒷모습)
            }
            else if (anim.GetBool("FacingFront"))
            {
                lungeDirection = Vector3.down; // S 방향 (앞모습)
            }
            else // FacingFront와 FacingBack이 모두 False일 때 (측면)
            {
                // SpriteRenderer의 flipX 상태를 확인합니다.
                if (spriteRenderer.flipX)
                    lungeDirection = Vector3.left; // A 방향 (왼쪽)
                else
                    lungeDirection = Vector3.right; // D 방향 (오른쪽)
            }

            // 2. 입력이 없었더라도 시야 방향으로 Lunge를 실행합니다.
            // *만약 움직이고 있었다면, 움직이던 방향으로 Lunge 실행*
            if (lastInputDirection.sqrMagnitude > 0.1f) {
                lungeDirection = lastInputDirection; // 움직이고 있었으면 그 방향으로 Lunge
            }
            
            // 3. 계산된 방향으로 Lunge 실행
            if (lungeDirection.sqrMagnitude > 0.1f)
            {
                transform.Translate(lungeDirection.normalized * lungeDistance, Space.World);
            }
        }


        // 공격 지속 시간 동안 기다림 (애니메이션 동기화 용이)
        yield return new WaitForSeconds(attackDuration);

        //김주은 추가
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // 물리 시스템 복구
        }

        // 공격 판정: AttackArea 콜라이더 범위 내의 적 검색
        if (attackAreaCollider != null)
        {
            overlapResults.Clear();
            attackAreaCollider.Overlap(attackFilter, overlapResults);

            foreach (var col in overlapResults)
            {
                if (col == null) continue;
                Debug.Log("공격 성공: " + col.name);
                // 실제로는 EnemyHealth.TakeDamage(...) 호출 권장
                Destroy(col.gameObject);
            }
        }
        else
        {
            // fallback: 기존 방식
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 1.0f, enemyLayer);
            foreach (var enemy in hitEnemies)
            {
                Debug.Log("공격 성공 (fallback): " + enemy.name);
                Destroy(enemy.gameObject);
            }
        }

        isAttacking = false;
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
}
// ...existing code...