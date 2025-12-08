# Boss 공격 애니메이션 디버그 가이드

## 문제: Boss 공격 모션이 재생되지 않음

Boss가 Player와 충돌해도 Attack 애니메이션이 재생되지 않는 문제입니다.

---

## 1️⃣ Unity Console에서 로그 확인

게임을 실행한 후 Boss가 Player와 충돌할 때 Console에 나타나는 로그를 확인하세요:

### ✅ 정상적으로 작동하는 경우:
```
🔵 EnemyAttack (Boss이름): OnCollisionEnter2D - 충돌 감지됨! Target: Player, Tag: Player
🎯 EnemyAttack (Boss이름): TryHit 호출됨! Target: Player
✅ EnemyAttack (Boss이름): 공격 조건 통과! 데미지 적용 시작
✅ EnemyAttack (Boss이름): 공격 애니메이션 트리거 발동! 방향: (1, 0)
💥 EnemyAttack (Boss이름): Player에게 10 데미지 적용!
```

### ❌ 문제가 있는 경우:

#### Case 1: 충돌 자체가 감지되지 않음
- 로그에 `🔵 OnCollisionEnter2D` 또는 `🔵 OnTriggerEnter2D` 메시지가 없음
- **원인**: Collider 설정 문제
- **해결 방법**: 아래 "2️⃣ Boss Collider 설정 확인" 참조

#### Case 2: Player 태그 인식 안 됨
```
🔵 EnemyAttack (Boss이름): OnCollisionEnter2D - 충돌 감지됨! Target: Player, Tag: Untagged
❌ EnemyAttack (Boss이름): Player 태그가 아님! (Tag: Untagged)
```
- **원인**: Player GameObject의 Tag가 "Player"로 설정되지 않음
- **해결 방법**: Player GameObject 선택 → Inspector 상단 Tag 드롭다운 → "Player" 선택

#### Case 3: 쿨다운 중
```
⏱️ EnemyAttack (Boss이름): 쿨다운 중! (남은 시간: 0.3초)
```
- **원인**: 이전 공격 후 hitCooldown 시간이 지나지 않음 (정상 동작)
- **해결 방법**: 잠시 기다리면 다시 공격 가능

#### Case 4: 애니메이션 컴포넌트 누락
```
⚠️ EnemyAttack (Boss이름): 애니메이션 재생 실패! playAttackAnimation: True, anim: False, enemyAI: True
```
- **원인**: Animator 컴포넌트가 없음
- **해결 방법**: 아래 "3️⃣ Boss 필수 컴포넌트 확인" 참조

---

## 2️⃣ Boss Collider 설정 확인

Boss GameObject를 선택하고 Inspector에서 다음을 확인하세요:

### Polygon Collider 2D (또는 다른 Collider)
- **Is Trigger**: ✅ 체크됨 → `OnTriggerEnter2D` 사용
- **Is Trigger**: ❌ 체크 안 됨 → `OnCollisionEnter2D` 사용

### Rigidbody2D (필수!)
- **Body Type**: Dynamic 또는 Kinematic
- ⚠️ **중요**: Collider만 있고 Rigidbody2D가 없으면 충돌 감지가 안 될 수 있습니다!

### Collision Matrix 확인
- Edit → Project Settings → Physics 2D
- Boss의 Layer와 Player의 Layer가 서로 충돌 가능하도록 설정되어 있는지 확인

---

## 3️⃣ Boss 필수 컴포넌트 확인

Boss GameObject에 다음 컴포넌트들이 있는지 확인하세요:

### ✅ 필수 컴포넌트:
1. **Animator** - Attack 트리거를 재생하기 위해 필요
2. **EnemyAI** - 이동 방향 정보를 가져오기 위해 필요
3. **EnemyAttack** (현재 스크립트)
4. **Rigidbody2D** - 충돌 감지를 위해 필요
5. **Collider2D** (Polygon/Circle/Box) - 충돌 영역 정의

### EnemyAttack 컴포넌트 설정:
- **Damage**: 10 (원하는 데미지 값)
- **Hit Cooldown**: 0.5 (공격 간격, 초 단위)
- **Play Attack Animation**: ✅ 체크됨 (반드시!)

---

## 4️⃣ Animator Controller 설정 확인

Boss의 Animator Controller를 열고 다음을 확인하세요:

### Attack Parameter 존재 여부:
- Parameters 탭에 **"Attack"** Trigger가 있어야 함
- Type: **Trigger** (Bool이나 Float이 아님!)

### Attack Transition 설정:
- Idle/Walk → Attack 트랜지션이 있어야 함
- Conditions: **Attack** Trigger
- Settings:
  - **Has Exit Time**: ❌ 체크 해제
  - **Transition Duration**: 0 (즉시 전환)

### MoveX, MoveY Parameter 존재 여부:
- **MoveX**: Float
- **MoveY**: Float
- 이 값들이 Attack 애니메이션 방향을 결정합니다

---

## 5️⃣ BossWakeUp 스크립트와의 충돌 가능성

Boss에 `BossWakeUp.cs`가 있다면:

### 확인 사항:
- Boss가 아직 깨어나지 않았을 수 있음
- `WakeUpSequence()` 완료 전에는 `EnemyAI`가 비활성화되어 있음
- `enemyAI.enabled = false` → Attack 애니메이션 방향 설정 불가

### 해결 방법:
1. Console에서 "Boss AI 활성화 (깨어남)" 로그 확인
2. 또는 BossWakeUp의 `delayBeforeWakeUp`과 `wakeUpDuration`을 줄여서 빠르게 테스트

---

## 6️⃣ 추가 테스트 방법

### Player 대신 다른 방법으로 즉시 공격 트리거:
Boss GameObject 선택 → Inspector → EnemyAttack 컴포넌트 우클릭 → "Copy Component"
그리고 임시 스크립트로 강제 트리거:

```csharp
// 테스트용 스크립트 (Boss에 임시로 추가)
using UnityEngine;

public class BossAttackTest : MonoBehaviour
{
    private Animator anim;
    private EnemyAI enemyAI;

    void Start()
    {
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
    }

    void Update()
    {
        // T 키를 누르면 즉시 Attack 트리거
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🧪 테스트: Attack 트리거 강제 발동!");
            anim.SetTrigger("Attack");

            if (enemyAI != null)
            {
                Vector2 dir = enemyAI.SnapToFourDirection(Vector2.down);
                anim.SetFloat("MoveX", dir.x);
                anim.SetFloat("MoveY", dir.y);
            }
        }
    }
}
```

게임 실행 → T 키 눌러서 Attack 애니메이션이 재생되는지 확인

---

## 예상되는 문제 및 해결책 요약

| 증상 | 원인 | 해결 방법 |
|------|------|-----------|
| 충돌 로그가 전혀 안 뜸 | Collider/Rigidbody 없음 | Rigidbody2D 추가, Collider 확인 |
| "Player 태그가 아님" 로그 | Player Tag 미설정 | Player GameObject Tag → "Player" |
| 데미지는 들어가지만 애니메이션 없음 | playAttackAnimation 체크 해제 | EnemyAttack 컴포넌트에서 체크 |
| "Animator를 찾을 수 없습니다" | Animator 컴포넌트 없음 | Boss에 Animator 추가 |
| "애니메이션 재생 실패 enemyAI: False" | EnemyAI 비활성화됨 | BossWakeUp 완료 대기 또는 EnemyAI.enabled = true |
| Attack 트리거는 발동하지만 애니메이션 없음 | Animator Controller 설정 오류 | Attack Parameter/Transition 확인 |

---

## 최종 체크리스트

- [ ] Boss GameObject에 Rigidbody2D 컴포넌트 있음
- [ ] Boss GameObject에 Polygon Collider 2D 있음
- [ ] Boss GameObject에 Animator 컴포넌트 있음
- [ ] Boss GameObject에 EnemyAI 스크립트 있음
- [ ] Boss GameObject에 EnemyAttack 스크립트 있음
- [ ] EnemyAttack의 "Play Attack Animation" 체크됨
- [ ] Player GameObject의 Tag가 "Player"로 설정됨
- [ ] Animator Controller에 "Attack" Trigger 파라미터 있음
- [ ] Animator Controller에 Attack 트랜지션 설정됨
- [ ] BossWakeUp 사용 시 Boss가 깨어난 상태인지 확인

---

이 가이드를 따라 확인하고, Unity Console에 나타나는 로그를 참고하여 문제를 해결하세요!
