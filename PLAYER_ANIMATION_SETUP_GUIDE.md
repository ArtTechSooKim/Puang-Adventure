# 🎬 Puang-Adventure 플레이어 애니메이션 설정 가이드

## 📋 목차
1. [애니메이션 시스템 개요](#1-애니메이션-시스템-개요)
2. [필요한 애니메이션 클립 목록](#2-필요한-애니메이션-클립-목록)
3. [Animator Controller 설정](#3-animator-controller-설정)
4. [Animator Parameters 설정](#4-animator-parameters-설정)
5. [State Machine 구조](#5-state-machine-구조)
6. [Transition 설정](#6-transition-설정)
7. [PlayerController 연동](#7-playercontroller-연동)
8. [검기 이펙트 애니메이션](#8-검기-이펙트-애니메이션)

---

## 1. 애니메이션 시스템 개요

### **PlayerController.cs 애니메이션 로직 분석**

PlayerController는 다음과 같은 애니메이션 파라미터를 사용합니다:

```csharp
// Line 330: Speed (Float) - Idle ↔ Walk 전환
anim.SetFloat("Speed", movementInput.magnitude);

// Line 341, 347: FacingBack (Bool) - 뒷모습 (W 방향)
anim.SetBool("FacingBack", true/false);

// Line 348, 356: FacingFront (Bool) - 앞모습 (S 방향)
anim.SetBool("FacingFront", true/false);

// Line 361, 365: SpriteRenderer.flipX - 좌우 반전 (A/D 방향)
spriteRenderer.flipX = true/false;

// Line 407, 427: Dash (Bool) - 대시 애니메이션
anim.SetBool("Dash", true/false);

// Line 467: Attack (Trigger) - 공격 애니메이션
anim.SetTrigger("Attack");
```

### **애니메이션 방향 시스템**

플레이어 애니메이션은 **4방향** 시스템을 사용합니다:
- **↑ (W)**: `FacingBack = true` - 뒷모습
- **↓ (S)**: `FacingFront = true` - 앞모습
- **← (A)**: `FacingBack = false, FacingFront = false, flipX = true` - 측면 (왼쪽)
- **→ (D)**: `FacingBack = false, FacingFront = false, flipX = false` - 측면 (오른쪽)

---

## 2. 필요한 애니메이션 클립 목록

### **현재 프로젝트에 있는 플레이어 애니메이션 클립**

`Assets/Animation/` 폴더에 다음 클립들이 있습니다:

#### **Idle 애니메이션 (4방향)**
```
player_idle.anim           - 측면 Idle (기본)
player_idle_front.anim     - 앞모습 Idle (S 방향)
player_idle_back.anim      - 뒷모습 Idle (W 방향)
player_idle_left.anim      - 왼쪽 Idle (사용 안함 - flipX로 처리)
```

#### **Walk 애니메이션 (3방향)**
```
player_walk.anim           - 측면 Walk (A/D 방향, flipX 사용)
player_walk_front.anim     - 앞모습 Walk (S 방향)
player_walk_back.anim      - 뒷모습 Walk (W 방향)
```

#### **Attack 애니메이션 (3방향)**
```
player_attack.anim         - 측면 Attack (A/D 방향, flipX 사용)
player_attack_front.anim   - 앞모습 Attack (S 방향)
player_attack_back.anim    - 뒷모습 Attack (W 방향)
```

#### **Dash 애니메이션 (3방향)**
```
player_dash.anim           - 측면 Dash (A/D 방향, flipX 사용)
player_dash_front.anim     - 앞모습 Dash (S 방향)
player_dash_back.anim      - 뒷모습 Dash (W 방향)
```

---

## 3. Animator Controller 설정

### **3-1. Player Animator Controller 생성/확인**

1. `Assets/Animation/Player.controller` 파일이 이미 존재합니다
2. Player 게임 오브젝트 선택
3. Inspector에서 **Animator 컴포넌트** 확인
4. **Controller** 필드에 `Player.controller` 할당

### **3-2. Animator 컴포넌트 설정**

Player 오브젝트의 Animator 컴포넌트:
```
Animator
├── Controller: Player.controller
├── Avatar: None (2D 게임이므로)
├── Apply Root Motion: ✅ 체크 해제
├── Update Mode: Normal
├── Culling Mode: Always Animate
```

---

## 4. Animator Parameters 설정

Player.controller를 더블클릭하여 Animator 창을 열고 **Parameters** 탭에서 다음을 추가합니다:

### **Parameter 목록**

| Parameter Name | Type | Default Value | 설명 |
|----------------|------|---------------|------|
| `Speed` | Float | `0` | 이동 속도 (0 = Idle, >0 = Walk) |
| `FacingBack` | Bool | `false` | 뒷모습 애니메이션 (W 방향) |
| `FacingFront` | Bool | `false` | 앞모습 애니메이션 (S 방향) |
| `Dash` | Bool | `false` | 대시 중인지 여부 |
| `Attack` | Trigger | - | 공격 트리거 (1회성) |

### **Parameter 추가 방법**

1. Animator 창에서 **Parameters** 탭 클릭
2. **+** 버튼 클릭
3. 타입 선택 (Float, Bool, Trigger)
4. 이름 입력 (대소문자 정확히!)

---

## 5. State Machine 구조

### **전체 State Machine 구조**

```
Player Animator Controller
│
├── Entry → Idle_Side (기본 상태)
│
├── Idle States (속도 기반 전환)
│   ├── Idle_Side           (측면 대기)
│   ├── Idle_Front          (앞모습 대기)
│   └── Idle_Back           (뒷모습 대기)
│
├── Walk States (속도 기반 전환)
│   ├── Walk_Side           (측면 걷기)
│   ├── Walk_Front          (앞모습 걷기)
│   └── Walk_Back           (뒷모습 걷기)
│
├── Dash States (방향 기반)
│   ├── Dash_Side           (측면 대시)
│   ├── Dash_Front          (앞모습 대시)
│   └── Dash_Back           (뒷모습 대시)
│
└── Attack States (방향 기반)
    ├── Attack_Side         (측면 공격)
    ├── Attack_Front        (앞모습 공격)
    └── Attack_Back         (뒷모습 공격)
```

### **State 생성 방법**

1. Animator 창에서 우클릭 > **Create State > Empty**
2. State 이름 변경 (예: `Idle_Side`)
3. Inspector에서 **Motion** 필드에 애니메이션 클립 드래그
   - 예: `Idle_Side` → `player_idle.anim`

---

## 6. Transition 설정

### **6-1. Idle ↔ Walk Transition (속도 기반)**

#### **Idle_Side → Walk_Side**
```
Conditions:
- Speed Greater 0.01

Settings:
- Has Exit Time: ✅ 체크 해제
- Exit Time: 0
- Fixed Duration: ✅ 체크
- Transition Duration: 0.1 (부드러운 전환)
- Transition Offset: 0
- Interruption Source: Current State
- Ordered Interruption: ✅ 체크
```

#### **Walk_Side → Idle_Side**
```
Conditions:
- Speed Less 0.01

Settings:
- Has Exit Time: ✅ 체크 해제
- Exit Time: 0
- Fixed Duration: ✅ 체크
- Transition Duration: 0.1
- Transition Offset: 0
```

> **주의**: `Idle_Front ↔ Walk_Front`, `Idle_Back ↔ Walk_Back`도 동일하게 설정!

---

### **6-2. 방향 전환 Transition (FacingBack/FacingFront 기반)**

#### **Idle_Side → Idle_Back (뒷모습으로 전환)**
```
Conditions:
- FacingBack Equals true

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0 (즉시 전환)
```

#### **Idle_Back → Idle_Side (측면으로 복귀)**
```
Conditions:
- FacingBack Equals false

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0
```

#### **Idle_Side → Idle_Front (앞모습으로 전환)**
```
Conditions:
- FacingFront Equals true

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0
```

#### **Idle_Front → Idle_Side (측면으로 복귀)**
```
Conditions:
- FacingFront Equals false

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0
```

> **주의**: Walk 상태들도 동일한 방향 전환 로직 적용!

---

### **6-3. Dash Transition**

#### **Any State → Dash_Side**
```
Conditions:
- Dash Equals true
- FacingBack Equals false
- FacingFront Equals false

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0 (즉시 대시)
- Can Transition To Self: ✅ 체크 해제
```

#### **Any State → Dash_Front**
```
Conditions:
- Dash Equals true
- FacingFront Equals true

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0
```

#### **Any State → Dash_Back**
```
Conditions:
- Dash Equals true
- FacingBack Equals true

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0
```

#### **Dash States → Idle (대시 종료)**
```
Conditions:
- Dash Equals false

Settings:
- Has Exit Time: ✅ 체크 (애니메이션 완료 후 전환)
- Exit Time: 0.9 (애니메이션 90% 완료 시)
- Transition Duration: 0.1
```

---

### **6-4. Attack Transition**

#### **Any State → Attack_Side**
```
Conditions:
- Attack (Trigger)
- FacingBack Equals false
- FacingFront Equals false

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0 (즉시 공격)
- Can Transition To Self: ✅ 체크 (연속 공격 가능)
```

#### **Any State → Attack_Front**
```
Conditions:
- Attack (Trigger)
- FacingFront Equals true

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0
```

#### **Any State → Attack_Back**
```
Conditions:
- Attack (Trigger)
- FacingBack Equals true

Settings:
- Has Exit Time: ✅ 체크 해제
- Transition Duration: 0
```

#### **Attack States → Idle (공격 종료)**
```
Conditions:
- (없음 - Exit Time만 사용)

Settings:
- Has Exit Time: ✅ 체크
- Exit Time: 1.0 (애니메이션 100% 완료 후)
- Transition Duration: 0.1
```

---

## 7. PlayerController 연동

### **7-1. 애니메이션 파라미터 설정 위치**

PlayerController.cs에서 이미 다음 위치에서 파라미터를 설정하고 있습니다:

#### **이동 애니메이션 (Line 326-378)**
```csharp
// Speed 파라미터 설정
anim.SetFloat("Speed", movementInput.magnitude);

// 방향 파라미터 설정
if (movementInput.y > 0)
{
    anim.SetBool("FacingBack", true);   // W 방향
    anim.SetBool("FacingFront", false);
}
else if (movementInput.y < 0)
{
    anim.SetBool("FacingBack", false);
    anim.SetBool("FacingFront", true);  // S 방향
}
else // X축 입력 (측면)
{
    anim.SetBool("FacingBack", false);
    anim.SetBool("FacingFront", false);

    // flipX로 좌우 반전
    spriteRenderer.flipX = (movementInput.x < 0);
}
```

#### **대시 애니메이션 (Line 407, 427)**
```csharp
// 대시 시작
anim.SetBool("Dash", true);

// 대시 종료
anim.SetBool("Dash", false);
```

#### **공격 애니메이션 (Line 467)**
```csharp
// 공격 트리거
anim.SetTrigger("Attack");
```

### **7-2. 필요한 설정 확인**

1. **Player 오브젝트에 Animator 컴포넌트 추가**
   - PlayerController.cs의 `Awake()`에서 자동으로 가져옴 (Line 81)

2. **Player 오브젝트에 SpriteRenderer 컴포넌트 필요**
   - flipX로 좌우 반전 처리 (Line 82)

3. **Rigidbody2D 컴포넌트 필요**
   - 대시/공격 시 물리 일시정지 (Line 83)

---

## 8. 검기 이펙트 애니메이션

### **8-1. SlashEffect Animator 설정**

검기 이펙트는 별도의 Animator Controller를 사용합니다:
- **Controller**: `Assets/Animation/SlashEffect.controller`
- **애니메이션 클립**: `Assets/Animation/SlashFx/SlashAnim_01~06.anim`

### **8-2. SlashEffect 오브젝트 구조**

```
Player
└── SlashEffect (자식 오브젝트)
    ├── Animator (SlashEffect.controller)
    ├── SpriteRenderer (검기 스프라이트)
    └── (여러 방향별 이펙트 가능)
```

### **8-3. SlashEffect Animator Parameters**

| Parameter | Type | 설명 |
|-----------|------|------|
| `Attack` | Trigger | 검기 이펙트 재생 트리거 |

### **8-4. SlashEffect State Machine**

```
SlashEffect Animator
│
├── Entry → Idle (비활성 상태)
│
└── SlashAnimation
    ├── Any State → Slash (Attack 트리거 시)
    └── Slash → Idle (애니메이션 완료 후)
```

### **8-5. PlayerController 연동 (Line 469-479)**

```csharp
// 검기 이펙트 재생 (무기가 있을 때만)
if (slashEffectAnimator != null && currentWeapon != null && currentWeapon.isWeapon)
{
    slashEffectAnimator.SetTrigger("Attack");
}
```

### **8-6. Inspector 설정**

PlayerController에서:
1. **Sword Slash Effects**: SlashEffect의 SpriteRenderer 배열 할당
2. **Slash Effect Animator**: SlashEffect의 Animator 할당

---

## 9. 단계별 설정 가이드 (처음부터)

### **Step 1: Animator Controller 생성**
```
1. Assets/Animation 폴더에서 우클릭
2. Create > Animator Controller
3. 이름: Player
```

### **Step 2: Parameters 추가**
```
1. Player.controller 더블클릭
2. Parameters 탭에서 + 버튼
3. 추가할 파라미터:
   - Speed (Float)
   - FacingBack (Bool)
   - FacingFront (Bool)
   - Dash (Bool)
   - Attack (Trigger)
```

### **Step 3: States 생성**
```
1. Animator 창에서 우클릭 > Create State > Empty

Idle States:
- Idle_Side → player_idle.anim
- Idle_Front → player_idle_front.anim
- Idle_Back → player_idle_back.anim

Walk States:
- Walk_Side → player_walk.anim
- Walk_Front → player_walk_front.anim
- Walk_Back → player_walk_back.anim

Dash States:
- Dash_Side → player_dash.anim
- Dash_Front → player_dash_front.anim
- Dash_Back → player_dash_back.anim

Attack States:
- Attack_Side → player_attack.anim
- Attack_Front → player_attack_front.anim
- Attack_Back → player_attack_back.anim
```

### **Step 4: Default State 설정**
```
1. Idle_Side 우클릭
2. Set as Layer Default State (주황색으로 변경됨)
```

### **Step 5: Transitions 연결**

#### **Idle ↔ Walk (속도 기반)**
```
Idle_Side → Walk_Side
- Condition: Speed Greater 0.01
- Has Exit Time: 체크 해제

Walk_Side → Idle_Side
- Condition: Speed Less 0.01
- Has Exit Time: 체크 해제

(Front, Back도 동일하게 설정)
```

#### **방향 전환 (FacingBack/Front 기반)**
```
Idle_Side → Idle_Back
- Condition: FacingBack Equals true
- Transition Duration: 0

Idle_Back → Idle_Side
- Condition: FacingBack Equals false
- Transition Duration: 0

Idle_Side → Idle_Front
- Condition: FacingFront Equals true
- Transition Duration: 0

Idle_Front → Idle_Side
- Condition: FacingFront Equals false
- Transition Duration: 0

(Walk States도 동일하게 설정)
```

#### **Dash (Any State에서)**
```
Any State → Dash_Side
- Conditions: Dash=true, FacingBack=false, FacingFront=false
- Transition Duration: 0

Any State → Dash_Front
- Conditions: Dash=true, FacingFront=true
- Transition Duration: 0

Any State → Dash_Back
- Conditions: Dash=true, FacingBack=true
- Transition Duration: 0

Dash_* → Idle_*
- Condition: Dash=false
- Has Exit Time: 체크
- Exit Time: 0.9
```

#### **Attack (Any State에서)**
```
Any State → Attack_Side
- Conditions: Attack (Trigger), FacingBack=false, FacingFront=false
- Transition Duration: 0

Any State → Attack_Front
- Conditions: Attack (Trigger), FacingFront=true
- Transition Duration: 0

Any State → Attack_Back
- Conditions: Attack (Trigger), FacingBack=true
- Transition Duration: 0

Attack_* → Idle_*
- Condition: (없음)
- Has Exit Time: 체크
- Exit Time: 1.0
```

### **Step 6: Player 오브젝트 설정**
```
1. Player 게임 오브젝트 선택
2. Animator 컴포넌트 추가
3. Controller 필드에 Player.controller 드래그
4. Apply Root Motion: 체크 해제
5. SpriteRenderer 확인 (flipX 사용)
6. Rigidbody2D 확인 (물리 제어)
```

---

## 10. 디버깅 팁

### **애니메이션이 재생되지 않을 때**

1. **Animator 창에서 실시간 확인**
   - 게임 실행 중 Animator 창에서 현재 State 확인
   - Parameter 값이 변경되는지 확인

2. **Parameter 이름 확인**
   - 대소문자 정확히 일치해야 함
   - `Speed` ≠ `speed`

3. **Transition Conditions 확인**
   - Condition이 제대로 설정되었는지
   - Has Exit Time 설정 확인

4. **애니메이션 클립 할당 확인**
   - State의 Motion 필드에 클립이 할당되었는지

### **방향 전환이 안 될 때**

1. **Console 로그 확인**
   - PlayerController는 방향 변경 시 로그 출력 안함
   - Animator Parameters 탭에서 실시간 값 확인

2. **FacingBack/FacingFront 우선순위**
   - 둘 다 false면 측면 (flipX 사용)
   - Y축 입력이 X축보다 우선 (Line 336)

### **Dash/Attack 애니메이션이 중복될 때**

1. **Transition 우선순위**
   - Any State에서의 Transition이 우선
   - Can Transition To Self 설정 확인

2. **Exit Time 설정**
   - Dash: Exit Time 0.9 (90% 완료 후)
   - Attack: Exit Time 1.0 (100% 완료 후)

---

## 11. 완료 체크리스트

- [ ] Animator Controller 생성 (`Player.controller`)
- [ ] Parameters 5개 추가 (Speed, FacingBack, FacingFront, Dash, Attack)
- [ ] Idle States 3개 생성 (Side, Front, Back)
- [ ] Walk States 3개 생성 (Side, Front, Back)
- [ ] Dash States 3개 생성 (Side, Front, Back)
- [ ] Attack States 3개 생성 (Side, Front, Back)
- [ ] Idle ↔ Walk Transitions 설정 (속도 기반)
- [ ] 방향 전환 Transitions 설정 (FacingBack/Front 기반)
- [ ] Any State → Dash Transitions 설정
- [ ] Any State → Attack Transitions 설정
- [ ] Player 오브젝트에 Animator 컴포넌트 추가
- [ ] Player.controller 할당
- [ ] SpriteRenderer 확인 (flipX)
- [ ] Rigidbody2D 확인
- [ ] SlashEffect Animator 설정 (검기 이펙트)
- [ ] 게임 실행 테스트

---

## 완료! 🎉

이제 PlayerController와 완벽하게 연동되는 애니메이션 시스템이 구축되었습니다!

**테스트 방법:**
1. 게임 실행
2. WASD로 이동 → Walk 애니메이션 재생, 방향 전환 확인
3. Space로 대시 → Dash 애니메이션 재생
4. 좌클릭으로 공격 → Attack 애니메이션 재생
5. Animator 창에서 실시간 Parameter 값 확인