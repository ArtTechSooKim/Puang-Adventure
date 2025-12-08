# Boss 공격 시스템 설정 가이드

Boss는 **Player와 충돌 시 즉시 공격 애니메이션을 재생하고 데미지를 줍니다.**

---

## 1️⃣ Boss GameObject 컴포넌트 설정

Boss GameObject를 선택하고 다음 컴포넌트들을 확인/설정하세요:

### ✅ 필수 컴포넌트:
1. **Animator** - 애니메이션 재생
2. **EnemyAI** - 이동 및 방향 정보
3. **EnemyHealth** - 체력 관리
4. **BossWakeUp** (선택) - 깨어나는 연출
5. **BossAttack** (새로 추가!) - Boss 전용 공격 시스템
6. **Rigidbody2D** - 물리 시뮬레이션
7. **Polygon Collider 2D** (또는 다른 Collider) - Boss의 몸통 충돌 영역

### ❌ 제거할 컴포넌트:
- **EnemyAttack** - Boss에서는 사용하지 않음! (일반 Enemy만 사용)
  - Inspector에서 EnemyAttack 컴포넌트 우클릭 → "Remove Component"

---

## 2️⃣ Boss의 Collider 설정

### Boss 몸통 Collider (Polygon Collider 2D):
Boss의 Collider 설정에 따라 충돌 방식이 달라집니다:

#### 옵션 1: Is Trigger 체크 ✅ (권장)
- Player와 겹칠 수 있음 (물리적으로 밀리지 않음)
- 충돌 시 `OnTriggerEnter2D` 호출
- Boss가 Player를 통과하면서 공격할 수 있음

#### 옵션 2: Is Trigger 체크 해제 ❌
- Player와 물리적으로 충돌 (서로 밀림)
- 충돌 시 `OnCollisionEnter2D` 호출
- Boss와 Player가 서로 부딪쳐서 밀려남

**추천**: Is Trigger ✅ 체크 (보스전에서 더 자연스러움)

---

## 3️⃣ BossAttack 컴포넌트 설정

Boss GameObject의 BossAttack 컴포넌트 설정:

### Attack Settings:
- **Damage**: 20 (Boss 공격 데미지, 조정 가능)
- **Attack Cooldown**: 2.0 (공격 쿨다운, 초 단위)

### Attack Animation:
- **Attack Duration**: 0.5 (공격 애니메이션 지속 시간, 초 단위)

---

## 4️⃣ Boss 작동 방식

### 공격 로직:
1. **충돌 감지**: Boss의 Collider와 Player가 충돌 (`OnCollisionEnter2D` 또는 `OnTriggerEnter2D`)
2. **조건 체크**: Boss 깨어남 확인, 쿨다운 확인, Player 태그 확인
3. **애니메이션 재생**: Attack 트리거 발동 → MoveX/MoveY 방향 설정
4. **데미지 적용**: Player에게 즉시 데미지
5. **쿨다운**: `attackCooldown` 동안 대기 (이 시간 동안 추가 공격 불가)

### 일반 Enemy vs Boss 비교:

| | 일반 Enemy | Boss |
|---|---|---|
| **컴포넌트** | EnemyAttack | BossAttack |
| **공격 방식** | 몸통 충돌 시 즉시 데미지 | 몸통 충돌 시 즉시 데미지 + 애니메이션 |
| **공격 트리거** | Player와 충돌 시 | Player와 충돌 시 |
| **애니메이션** | 충돌 시 Attack 트리거 | 충돌 시 Attack 트리거 |
| **쿨다운** | 0.5초 (빠름) | 2.0초 (느림) |
| **특징** | 간단한 적 | 보스, WakeUp 애니메이션 지원 |

---

## 5️⃣ Animator Controller 설정

Boss의 Animator Controller에 다음 파라미터가 있어야 합니다:

### Parameters:
- **IsWalking** (Bool) - Idle/Walk 전환
- **MoveX** (Float) - 좌우 방향
- **MoveY** (Float) - 상하 방향
- **Attack** (Trigger) - 공격 애니메이션
- **Dead** (Trigger) - 사망 애니메이션
- **WakeUp** (Trigger, 선택) - 깨어나는 애니메이션

### Attack Transition:
- Idle/Walk → Attack
- Condition: **Attack** (Trigger)
- Settings:
  - **Has Exit Time**: ❌ 체크 해제
  - **Transition Duration**: 0

---

## 6️⃣ 계층 구조 예시

```
Boss (GameObject)
├─ Animator
├─ EnemyAI
├─ EnemyHealth
├─ BossWakeUp (선택)
├─ BossAttack ← 새로 추가!
├─ Rigidbody2D
└─ Polygon Collider 2D (Is Trigger: ✅ 권장)
```

**중요**: AttackArea 자식 오브젝트는 더 이상 필요하지 않습니다! Boss 몸통 Collider가 직접 충돌을 감지합니다.

---

## 7️⃣ 테스트 방법

### Console 로그 확인:
```
🔵 BossAttack (Boss이름): OnTriggerEnter2D - 트리거 감지됨! Target: Player, Tag: Player
🎯 BossAttack (Boss이름): TryAttack 호출됨! Target: Player
✅ BossAttack (Boss이름): 공격 조건 통과! 공격 시작
⚔️ BossAttack (Boss이름): 공격 시작!
✅ BossAttack (Boss이름): 공격 애니메이션 트리거 발동! 방향: (1, 0)
💥 BossAttack (Boss이름): Player에게 20 데미지 적용!
✅ BossAttack (Boss이름): 공격 종료
```

---

## 8️⃣ 조정 가능한 값

### BossAttack 컴포넌트:
- **Damage**: 20 → Boss 공격력 (Player 체력이 100이므로 5번에 죽음)
- **Attack Cooldown**: 2.0 → 공격 간격 (너무 짧으면 너무 어려움)
- **Attack Duration**: 0.5 → 공격 애니메이션 지속 시간 (애니메이션 길이와 맞춰야 함)

---

## 9️⃣ 문제 해결

### Boss가 공격을 안 함:
- Console에 "충돌 감지됨" 또는 "트리거 감지됨" 로그가 뜨는지 확인
- Boss Collider가 있는지 확인 (Polygon Collider 2D)
- Boss Rigidbody2D가 있는지 확인
- BossWakeUp 사용 시 Boss가 깨어났는지 확인 ("Boss 완전히 깨어남" 로그)

### 공격 모션은 나오는데 데미지가 안 들어감:
- Console에 "PlayerHealth를 찾을 수 없습니다" 로그가 있는지 확인
- Player GameObject에 PlayerHealth 컴포넌트가 있는지 확인

### Boss 몸통에 닿기만 해도 데미지가 연속으로 들어감:
- Attack Cooldown이 너무 짧지 않은지 확인 (최소 1.0초 이상 권장)
- Boss의 EnemyAttack 컴포넌트를 제거했는지 확인

### "쿨다운 중" 로그만 계속 뜸:
- 정상입니다! Boss는 공격 후 쿨다운 시간 동안 다시 공격하지 않습니다
- Attack Cooldown 시간이 지나면 다시 공격합니다

---

이제 Boss는 **Player와 충돌 시 즉시 공격 모션**을 보여주는 간단하고 효과적인 보스로 작동합니다! 🎉
