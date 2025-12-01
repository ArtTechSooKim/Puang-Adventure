# 타이틀 UI 페이드인 설정 가이드

비디오 재생 후 Play, Load, Save 버튼이 자동으로 페이드인되도록 설정하는 방법입니다.

## 🎯 기능

- ✅ 타이틀 씬 시작 시 비디오가 먼저 재생됨
- ✅ 비디오 재생 시작 후 2초 대기
- ✅ UI 버튼들이 1초 동안 페이드인 효과로 나타남
- ✅ 대기 시간과 페이드인 속도 조절 가능

---

## 🚀 설정 방법 (3단계)

### 1단계: UI에 CanvasGroup 추가

1. **00_TitleScene** 씬을 엽니다
2. Hierarchy에서 **Play, Load, Save 버튼을 포함하는 부모 GameObject**를 선택합니다
   - 예: "TitleButtons" 또는 "MainMenuUI" 등
3. Inspector에서 **Add Component** 클릭
4. **CanvasGroup** 컴포넌트를 추가합니다

> **💡 팁**: 개별 버튼이 아닌 **버튼들을 모두 포함하는 부모 오브젝트**에 CanvasGroup을 추가하세요!

### 2단계: TitleLoopVideoPlayer 설정

TitleLoopVideoPlayer가 있는 GameObject를 선택하고 Inspector에서:

#### UI Fade In 섹션
- **Title UI Canvas Group**: 1단계에서 CanvasGroup을 추가한 GameObject를 드래그 앤 드롭
- **UI Fade In Delay**: `2` (비디오 시작 후 2초 대기)
- **UI Fade In Duration**: `1` (1초 동안 페이드인)

### 3단계: 테스트

플레이 버튼을 눌러 확인:
1. ✅ 씬 시작 시 UI가 투명한 상태로 시작
2. ✅ 비디오가 재생됨
3. ✅ 2초 후 UI가 서서히 나타남
4. ✅ 1초 후 UI가 완전히 보임

---

## 📋 Hierarchy 구조 예시

```
00_TitleScene
├── TitleVideoPlayer (TitleLoopVideoPlayer 스크립트)
├── Main Camera
└── Canvas
    └── TitleUI (← 여기에 CanvasGroup 추가!)
        ├── PlayButton
        ├── LoadButton
        └── SaveButton
```

또는

```
00_TitleScene
├── TitleVideoPlayer (TitleLoopVideoPlayer 스크립트)
├── Main Camera
└── Canvas
    ├── Logo
    ├── Background
    └── Buttons (← 여기에 CanvasGroup 추가!)
        ├── PlayButton
        ├── LoadButton
        └── SaveButton
```

---

## ⚙️ Inspector 설정 상세

### TitleLoopVideoPlayer 컴포넌트

```
┌─────────────────────────────────────┐
│ UI Fade In                          │
├─────────────────────────────────────┤
│ Title UI Canvas Group               │
│   └─ [TitleUI GameObject]           │  ← 드래그 앤 드롭
│                                     │
│ UI Fade In Delay        2           │  ← 2초 대기
│ UI Fade In Duration     1           │  ← 1초 페이드인
└─────────────────────────────────────┘
```

### 시간 조절

| 설정 | 기본값 | 설명 |
|------|--------|------|
| **UI Fade In Delay** | 2초 | 비디오 시작 후 UI 페이드인까지 대기 시간 |
| **UI Fade In Duration** | 1초 | UI가 완전히 나타나는데 걸리는 시간 |

#### 예시 조합

**빠른 페이드인**
- Delay: `1초`
- Duration: `0.5초`
→ 비디오 시작 1초 후, 0.5초 만에 UI 나타남

**느린 페이드인**
- Delay: `3초`
- Duration: `2초`
→ 비디오 시작 3초 후, 2초 동안 천천히 UI 나타남

**즉시 표시**
- Delay: `0초`
- Duration: `0초`
→ 비디오와 동시에 UI 즉시 표시

---

## 🎨 고급 설정

### 여러 UI 요소를 다른 타이밍에 페이드인

각 UI 그룹마다 별도의 CanvasGroup을 만들 수 있습니다:

```
Canvas
├── Logo (CanvasGroup) ← 즉시 표시
├── Buttons (CanvasGroup) ← 2초 후 페이드인
└── Footer (CanvasGroup) ← 4초 후 페이드인
```

이 경우:
1. TitleLoopVideoPlayer의 **Title UI Canvas Group**에는 "Buttons"를 할당
2. 다른 UI는 별도 스크립트나 Animator로 제어

### CanvasGroup의 다른 옵션들

CanvasGroup에는 페이드 외에도 유용한 옵션이 있습니다:

- **Alpha**: 투명도 (0 = 투명, 1 = 불투명)
- **Interactable**: 체크 해제하면 버튼 클릭 불가
- **Block Raycasts**: 체크 해제하면 클릭 이벤트 차단

초기 설정:
```
Alpha: 1 (스크립트가 자동으로 0으로 설정함)
Interactable: ✓ (체크)
Block Raycasts: ✓ (체크)
```

---

## 🐛 문제 해결

### UI가 페이드인되지 않아요
1. **Title UI Canvas Group**에 올바른 GameObject가 할당되었는지 확인
2. 해당 GameObject에 **CanvasGroup** 컴포넌트가 있는지 확인
3. Console에서 디버그 로그 확인 ("Show Debug Messages" 체크)

### UI가 처음부터 보여요
1. CanvasGroup이 **버튼의 부모 오브젝트**에 있는지 확인 (개별 버튼에 있으면 안됨)
2. 스크립트가 올바르게 작동하는지 Console 로그 확인

### 페이드인이 너무 빨라요/느려요
- **UI Fade In Delay**: 대기 시간 조절
- **UI Fade In Duration**: 페이드인 속도 조절

### 비디오는 나오는데 UI가 안 나와요
1. CanvasGroup의 **Interactable** 체크 확인
2. CanvasGroup의 **Block Raycasts** 체크 확인
3. UI GameObject가 **Active** 상태인지 확인

---

## 📞 디버그 로그

"Show Debug Messages"를 체크하면 다음과 같은 로그가 표시됩니다:

```
✅ TitleLoopVideoPlayer: Title UI set to invisible (will fade in after video)
▶ Video started playing in loop mode
⏳ Waiting 2s before fading in title UI...
✨ Fading in title UI over 1s...
✅ Title UI fade in completed!
```

---

## ✅ 완료 체크리스트

- [ ] UI 버튼들의 부모 GameObject에 CanvasGroup 추가
- [ ] TitleLoopVideoPlayer의 Title UI Canvas Group에 할당
- [ ] UI Fade In Delay 설정 (기본 2초)
- [ ] UI Fade In Duration 설정 (기본 1초)
- [ ] 테스트 플레이로 페이드인 효과 확인
- [ ] 타이밍이 마음에 들 때까지 조절

---

## 💡 팁

1. **부드러운 효과**: Duration을 1~2초로 설정
2. **빠른 전환**: Duration을 0.3~0.5초로 설정
3. **드라마틱한 효과**: Delay를 3~4초로 설정
4. **즉시 시작**: Delay를 0초로 설정

원하는 느낌을 찾을 때까지 실험해보세요! 🎬
