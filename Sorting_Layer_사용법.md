# Sorting Layer 빠른 사용 가이드

비디오 플레이어에 **Sorting Layer 지원 기능**이 추가되었습니다!

## 🎯 언제 사용하나요?

### Sorting Layer가 필요한 경우
- ✅ 비디오 위에 UI를 표시하고 싶을 때
- ✅ 비디오 앞/뒤에 스프라이트를 배치하고 싶을 때
- ✅ 여러 레이어 간 렌더링 순서를 정밀하게 제어하고 싶을 때

### Sorting Layer가 불필요한 경우
- ❌ 비디오만 전체화면으로 표시하고 다른 요소가 없을 때
- ❌ 성능이 중요하고 렌더링 순서가 상관없을 때

---

## 🚀 빠른 설정 (3단계)

### 1단계: Sorting Layer 생성

Unity 에디터에서:
1. **Edit → Project Settings → Tags and Layers**
2. **Sorting Layers** 섹션 펼치기
3. **+** 버튼으로 새 레이어 추가

권장 레이어 구조:
```
- Background    (-100)  ← 배경
- Default       (0)     ← 게임 오브젝트
- Video         (50)    ← 시네마틱 영상
- UI            (100)   ← 게임 UI
- Overlay       (200)   ← 최상단 UI
```

### 2단계: VideoPlayer 설정

각 씬의 VideoPlayer GameObject를 선택하고 Inspector에서:

1. **Render Mode** → `RenderTexture` 선택
2. **Sorting Layer Name** → 원하는 레이어 이름 입력 (예: "Video")
3. **Sorting Order** → 숫자 입력 (높을수록 위에 표시)

### 3단계: 테스트

플레이 버튼을 눌러 확인:
- ✅ 비디오가 올바른 순서로 표시되는지 확인
- ✅ UI 요소가 비디오 위/아래에 표시되는지 확인

---

## 📋 씬별 권장 설정

### 05_PeuangSadScene (시네마틱)
```
Render Mode: RenderTexture
Sorting Layer Name: "Video"
Sorting Order: 100
```
이유: UI나 대화창 위에 비디오 표시

### 08_EndingScene (엔딩)
```
Render Mode: RenderTexture
Sorting Layer Name: "Video"
Sorting Order: 50
```
이유: 크레딧 UI와 함께 사용 가능

### 00_TitleScene (타이틀)
```
Render Mode: RenderTexture
Sorting Layer Name: "Background"
Sorting Order: -10
```
이유: 타이틀 UI **아래**에 배경으로 표시

---

## 🎨 실전 예시

### 예시 1: 시네마틱 비디오 + 스킵 버튼

비디오 위에 "스킵" 버튼을 표시하고 싶을 때:

```
Video GameObject (CinematicVideoPlayer)
├─ Render Mode: RenderTexture
├─ Sorting Layer: "Video"
└─ Sorting Order: 50

Skip Button Canvas
├─ Render Mode: Screen Space - Camera
├─ Sorting Layer: "UI"
└─ Sorting Order: 100  (비디오보다 높음 → 위에 표시)
```

### 예시 2: 타이틀 배경 비디오 + UI

타이틀 UI 뒤에 배경 비디오를 표시하고 싶을 때:

```
Video GameObject (TitleLoopVideoPlayer)
├─ Render Mode: RenderTexture
├─ Sorting Layer: "Background"
└─ Sorting Order: -10

Title UI Canvas
├─ Render Mode: Screen Space - Camera
├─ Sorting Layer: "UI"
└─ Sorting Order: 0  (비디오보다 높음 → 위에 표시)
```

### 예시 3: 엔딩 비디오 + 크레딧

엔딩 비디오 위에 크레딧 텍스트를 표시하고 싶을 때:

```
Video GameObject (EndingVideoPlayer)
├─ Render Mode: RenderTexture
├─ Sorting Layer: "Video"
└─ Sorting Order: 0

Credits Canvas
├─ Render Mode: Screen Space - Camera
├─ Sorting Layer: "UI"
└─ Sorting Order: 10  (비디오보다 높음 → 위에 표시)
```

---

## ⚙️ 고급 설정

### Canvas 자동 생성

**Target Raw Image**를 비워두면 스크립트가 자동으로:
1. Canvas GameObject 생성
2. RawImage GameObject 생성
3. 전체화면으로 RectTransform 설정
4. Sorting Layer 설정 적용

### 수동 Canvas 사용

이미 존재하는 Canvas를 사용하려면:
1. 씬에 Canvas와 RawImage 수동 생성
2. RawImage를 **Target Raw Image**에 할당
3. 스크립트가 자동으로 해당 Canvas의 Sorting Layer 설정

---

## 🐛 문제 해결

### 비디오가 UI 아래에 숨어요
→ VideoPlayer의 **Sorting Order**를 UI보다 **높은 값**으로 설정

### 비디오가 UI 위에 표시돼요 (의도하지 않게)
→ VideoPlayer의 **Sorting Order**를 UI보다 **낮은 값**으로 설정

### Sorting Layer 이름을 찾을 수 없다는 오류
→ **Edit → Project Settings → Tags and Layers**에서 해당 레이어를 먼저 생성

### RenderTexture 모드에서 비디오가 안 보여요
1. Render Mode가 **RenderTexture**로 설정되었는지 확인
2. Main Camera가 존재하는지 확인
3. Console에서 에러 메시지 확인

---

## 💡 팁

1. **성능 우선**: Sorting Layer가 필요 없으면 `CameraNearPlane` 모드 사용
2. **Order 간격**: Sorting Order는 10~100 단위로 띄워서 설정하면 나중에 조정하기 편함
3. **디버그 로그**: Inspector에서 "Show Debug Messages" 체크하면 설정 확인 가능
4. **테스트**: Scene 뷰와 Game 뷰 모두에서 확인

---

## 📞 도움이 더 필요하신가요?

전체 문서는 **[비디오_플레이어_사용법.md](비디오_플레이어_사용법.md)**를 참고하세요!
