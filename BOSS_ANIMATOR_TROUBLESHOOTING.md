# Boss Animator 문제 해결 가이드

## 증상: Attack 모션이 안 나오고 AttackArea만 켜짐

이 문제는 Animator Controller의 Transition 설정이 잘못되었을 가능성이 높습니다.

---

## 1️⃣ Console 로그 확인

게임을 실행하고 Console에서 다음 로그들을 확인하세요:

### BossWakeUp 로그 (씬 시작 시):
```
🌙 BossWakeUp (Boss이름): Start 호출됨 - Boss 잠들어있는 상태
✅ BossWakeUp (Boss이름): Boss AI 비활성화 (잠든 상태)
⏰ BossWakeUp (Boss이름): WakeUpSequence 시작 - 0.5초 대기 중...
💤 BossWakeUp (Boss이름): 대기 완료 - 이제 깨어나기 시작!
✅ BossWakeUp (Boss이름): WakeUp 트리거 발동! (Animator: Boss)
⏳ BossWakeUp (Boss이름): 깨어나기 애니메이션 재생 중... (2.0초 대기)
👁️ BossWakeUp (Boss이름): 깨어나기 애니메이션 완료!
✅ BossWakeUp (Boss이름): Boss AI 활성화 (깨어남) - 이제 Player를 추적합니다!
🎉 BossWakeUp (Boss이름): Boss 완전히 깨어남! (hasWokenUp = true)
```

### BossAttack 로그 (Player가 범위 안에 들어왔을 때):
```
🎯 BossAttack (Boss이름): Player가 공격 범위 안에 있음! (거리: 1.2)
⚔️ BossAttack (Boss이름): 공격 시작!
✅ BossAttack (Boss이름): 공격 애니메이션 트리거 발동! 방향: (1, 0)
✅ BossAttack (Boss이름): AttackArea 활성화
```

### ⚠️ 문제 확인:
- **WakeUp 트리거는 발동하는데 애니메이션이 안 나옴** → Animator Controller Transition 문제
- **Attack 트리거는 발동하는데 애니메이션이 안 나옴** → Animator Controller Transition 문제

---

## 2️⃣ Animator Controller 구조 확인

Boss의 Animator Controller를 열고 다음 구조가 있는지 확인하세요:

### 필수 States:
```
Entry → Enemy_sleep (또는 Enemy_idle)
        ↓
    Enemy_idle ←→ Enemy_Walk
        ↓           ↓
        ↓        Enemy_attack
        ↓           ↓
    Enemy_awake     ↓
        ↓           ↓
    Enemy_idle ←----+
        ↓
    Enemy_dead
```

### 필수 Parameters:
- **IsWalking** (Bool) - Idle/Walk 전환
- **MoveX** (Float) - 방향
- **MoveY** (Float) - 방향
- **WakeUp** (Trigger) - 깨어나기
- **Attack** (Trigger) - 공격
- **Dead** (Trigger) - 사망

---

## 3️⃣ WakeUp Transition 설정

### ❌ 잘못된 설정 (Any State → Enemy_awake):
- Any State에서 WakeUp으로 가면 다른 애니메이션 재생 중 언제든 깨어나기 실행
- **문제**: 이미 Idle/Walk 상태에서 바로 깨어나기로 점프할 수 없음

### ✅ 올바른 설정:

#### 방법 1: Entry → Enemy_sleep → Enemy_awake
```
Entry
  ↓
Enemy_sleep (잠든 모습 - 1프레임 또는 정지 상태)
  ↓ (Transition)
  Condition: WakeUp (Trigger)
  Has Exit Time: ❌
  Transition Duration: 0
  ↓
Enemy_awake (깨어나는 애니메이션)
  ↓ (Transition)
  Condition: 없음
  Has Exit Time: ✅ (애니메이션 끝나면 자동 전환)
  Transition Duration: 0
  Exit Time: 1.0 (애니메이션 끝)
  ↓
Enemy_idle
```

#### 방법 2: Entry → Enemy_idle (시작부터 Idle)
```
Entry
  ↓
Enemy_idle
  ↓ (Transition to Enemy_awake)
  Condition: WakeUp (Trigger)
  Has Exit Time: ❌
  Transition Duration: 0
  ↓
Enemy_awake (깨어나는 애니메이션)
  ↓ (Transition to Enemy_idle)
  Condition: 없음
  Has Exit Time: ✅
  Transition Duration: 0
  ↓
Enemy_idle
```

---

## 4️⃣ Attack Transition 설정

### Attack 애니메이션이 재생되지 않는 경우:

#### ✅ 올바른 설정:

1. **Enemy_idle → Enemy_attack**
   - Condition: **Attack** (Trigger)
   - Has Exit Time: ❌ 체크 해제
   - Transition Duration: 0
   - Interruption Source: Current State

2. **Enemy_Walk → Enemy_attack**
   - Condition: **Attack** (Trigger)
   - Has Exit Time: ❌ 체크 해제
   - Transition Duration: 0
   - Interruption Source: Current State

3. **Enemy_attack → Enemy_idle**
   - Condition: 없음
   - Has Exit Time: ✅ 체크
   - Exit Time: 1.0 (애니메이션 끝)
   - Transition Duration: 0.1 ~ 0.2 (자연스러운 전환)

### ⚠️ 주의사항:
- **Has Exit Time을 체크하면** 애니메이션이 끝날 때까지 기다림 → Attack 시작이 느려짐
- **Transition Duration이 크면** 애니메이션 블렌딩 시간이 길어짐 → Attack이 천천히 시작됨

---

## 5️⃣ 사진으로 확인하기

Unity Animator 창에서 Boss의 Animator Controller를 열고:

### Any State 확인:
1. Any State 우클릭 → Make Transition → Enemy_attack 연결이 있나요?
   - **있으면**: 이 Transition을 삭제하고 Idle/Walk → Attack으로 직접 연결하세요
   - **없으면**: 정상입니다

### Enemy_idle → Enemy_attack Transition 클릭:
- Inspector에서 다음 확인:
  - Conditions: **Attack** (Trigger) 있어야 함
  - Has Exit Time: ❌ 체크 해제
  - Transition Duration (s): 0 또는 매우 작은 값 (0.05)

### Enemy_attack → Enemy_idle Transition 클릭:
- Inspector에서 다음 확인:
  - Conditions: 없음 (비어있어야 함)
  - Has Exit Time: ✅ 체크
  - Exit Time: 1.0
  - Transition Duration (s): 0.1 ~ 0.2

---

## 6️⃣ 공통 문제 해결

### 문제 1: WakeUp 트리거는 발동하는데 애니메이션이 안 나옴
**원인**: Animator Controller에 WakeUp Transition이 없거나 조건이 맞지 않음

**해결 방법**:
1. Animator Controller 열기
2. Enemy_sleep 또는 Entry에서 Enemy_awake로 가는 Transition 만들기
3. Condition: WakeUp (Trigger)
4. Has Exit Time: ❌
5. Transition Duration: 0

### 문제 2: Attack 트리거는 발동하는데 애니메이션이 안 나옴
**원인**: Any State → Attack 또는 Idle/Walk → Attack Transition이 없거나 조건이 맞지 않음

**해결 방법**:
1. Enemy_idle → Enemy_attack Transition 만들기
2. Enemy_Walk → Enemy_attack Transition 만들기
3. 둘 다 Condition: Attack (Trigger), Has Exit Time: ❌

### 문제 3: Attack 애니메이션이 재생되다 말고 끊김
**원인**: Enemy_attack → Enemy_idle Transition에 Has Exit Time이 체크 해제됨

**해결 방법**:
1. Enemy_attack → Enemy_idle Transition 선택
2. Has Exit Time: ✅ 체크
3. Exit Time: 1.0

### 문제 4: Boss가 깨어나지 않고 계속 잠듦
**원인**: BossWakeUp 스크립트가 없거나 delayBeforeWakeUp/wakeUpDuration이 너무 김

**해결 방법**:
1. Boss GameObject에 BossWakeUp 스크립트 추가
2. Delay Before Wake Up: 0.5
3. Wake Up Duration: 2.0 (애니메이션 길이와 맞춰야 함)

### 문제 5: AttackArea는 활성화되는데 데미지가 안 들어감
**원인**: AttackArea에 BossAttackArea 스크립트가 없거나 Player Tag가 잘못됨

**해결 방법**:
1. AttackArea GameObject에 BossAttackArea 스크립트 추가
2. Player GameObject Tag → "Player" 확인

---

## 7️⃣ 테스트 단계

### 1단계: WakeUp 테스트
1. 게임 실행
2. Console에서 "WakeUp 트리거 발동" 로그 확인
3. Boss가 깨어나는 애니메이션이 재생되는지 확인
4. 애니메이션 후 Boss가 Player를 따라오는지 확인

### 2단계: Attack 테스트
1. Player를 Boss 근처로 이동 (빨간 원 범위 안)
2. Console에서 "공격 애니메이션 트리거 발동" 로그 확인
3. Boss가 공격 애니메이션을 재생하는지 확인
4. Player가 데미지를 받는지 확인 (HP 감소)

---

## 8️⃣ 최종 체크리스트

- [ ] Animator Controller에 WakeUp (Trigger) 파라미터 있음
- [ ] Animator Controller에 Attack (Trigger) 파라미터 있음
- [ ] Entry → Enemy_sleep 또는 Enemy_idle 연결됨
- [ ] Enemy_sleep/Idle → Enemy_awake Transition 있음 (Condition: WakeUp)
- [ ] Enemy_awake → Enemy_idle Transition 있음 (Has Exit Time: ✅)
- [ ] Enemy_idle → Enemy_attack Transition 있음 (Condition: Attack)
- [ ] Enemy_Walk → Enemy_attack Transition 있음 (Condition: Attack)
- [ ] Enemy_attack → Enemy_idle Transition 있음 (Has Exit Time: ✅)
- [ ] Boss에 BossWakeUp 스크립트 있음
- [ ] Boss에 BossAttack 스크립트 있음
- [ ] AttackArea에 BossAttackArea 스크립트 있음
- [ ] Console 로그에서 "WakeUp 트리거 발동" 확인됨
- [ ] Console 로그에서 "공격 애니메이션 트리거 발동" 확인됨

---

이 가이드를 따라 Animator Controller를 설정하면 Boss가 정상적으로 깨어나고 공격 애니메이션을 재생할 것입니다! 🎉
