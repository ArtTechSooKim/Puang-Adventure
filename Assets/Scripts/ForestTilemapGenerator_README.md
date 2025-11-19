# Forest Tilemap Generator 사용 가이드

## 개요
Forest Scene을 위한 자동 타일맵 생성 시스템입니다. 허브 역할을 하는 숲 맵에 각 포탈별로 테마가 있는 지역을 자동으로 배치합니다.

## 주요 기능

### 🎨 테마별 지역 생성
1. **Village Portal 구역** (Forest → Village)
   - 나무로 많이 가려진 숲길
   - 밀집된 나무 배치
   - 은밀한 통로 느낌

2. **Cave Portal 구역** (Forest → Cave)
   - 동굴 입구가 보이는 지역
   - 바위와 돌이 많은 지형
   - 어두운 분위기
   - 박쥐 Enemy 배치 예정 지역

3. **Boss Portal 구역** (Forest → Boss)
   - 신비스러운 느낌
   - 마법적인 장식 요소
   - 특별한 분위기

### 🗺️ 자동 생성 레이어
- **Background Layer**: 어두운 숲 배경
- **Ground Layer**: 잔디와 흙길
- **Collision Layer**: 충돌 레이어 (Ground와 동일)
- **Decoration Layer**: 바위, 덤불, 꽃 등
- **WalkBehind Layer**: 플레이어 뒤에 보이는 나무 등

## 설치 및 설정

### 1단계: Generator 컴포넌트 추가

#### 방법 A: 메뉴에서 추가 (추천)
1. Unity Editor에서 `03_ForestScene` 열기
2. 메뉴: **Tools → Puang Adventure → Setup Forest Tilemap Generator**
3. 자동으로 Grid 오브젝트에 컴포넌트가 추가되고 Tilemap들이 연결됩니다

#### 방법 B: 수동으로 추가
1. Hierarchy에서 `Grid` 오브젝트 선택
2. Inspector에서 **Add Component** 클릭
3. `ForestTilemapGenerator` 검색 후 추가

### 2단계: Tile Assets 할당

Inspector에서 다음 타일들을 할당해야 합니다:

#### 필수 타일 (Ground)
- **Grass Tiles**: 기본 잔디 타일
  - 위치: `Assets/Tilemaps/TilesetFloor_*.asset` 또는 `TilesetField_*.asset`
  - 여러 개 할당 가능 (랜덤 배치)

- **Dirt Path Tiles**: 흙길 타일
  - 포탈을 연결하는 길에 사용

- **Stone Tiles**: 돌바닥 타일 (Cave 지역용)

#### 장식 타일 (Nature)
- **Tree Tiles**: 나무 타일 (Village 지역에 밀집 배치)
  - 위치: `Assets/Tilemaps/TilesetNature_*.asset`

- **Bush Tiles**: 덤불 타일

- **Flower Tiles**: 꽃 타일

- **Rock Tiles**: 바위 타일 (Cave 지역에 많이 배치)

#### 특수 타일 (Special)
- **Cave Entrance Tiles**: 동굴 입구 타일 (선택사항)

- **Mystical Tiles**: 신비로운 장식 타일 (Boss 지역용, 선택사항)

- **Dark Forest Tiles**: 어두운 숲 배경 타일

### 3단계: 포탈 위치 설정

Inspector의 **Portal Locations** 섹션에서 각 포탈의 위치를 설정합니다:

```
Village Portal Position: (-8, 2, 0)
Cave Portal Position: (8, -3, 0)
Boss Portal Position: (0, 6, 0)
Player Spawn Position: (0, 0, 0)
```

이 위치들은 Scene View에서 Gizmo로 표시됩니다:
- 🟢 초록색: Village Portal
- ⚫ 회색: Cave Portal
- 🟣 보라색: Boss Portal
- 🔵 청록색: Player Spawn

### 4단계: 생성 설정 조정

#### Map Size (맵 크기)
- 기본값: (20, 14)
- 필요에 따라 조정 가능

#### Map Origin (맵 시작점)
- 기본값: (-10, -7)
- 맵의 왼쪽 아래 모서리 위치

#### Decoration Density (장식 밀도)
- 범위: 0.0 ~ 1.0
- 기본값: 0.3 (30%)
- 높을수록 바위, 덤불이 많이 배치됨

## 사용 방법

### 타일맵 생성
1. Grid 오브젝트 선택
2. Inspector에서 **🌲 Generate Forest Map** 버튼 클릭
3. 확인 다이얼로그에서 "Yes, Generate" 클릭
4. 자동으로 모든 레이어에 타일이 배치됩니다

### 타일맵 초기화
- **🧹 Clear All Tilemaps** 버튼 클릭
- 모든 타일이 제거됩니다 (Undo 가능)

### 수동 편집
생성된 타일맵은 Unity의 Tilemap Editor로 추가 편집 가능합니다:
1. Window → 2D → Tile Palette 열기
2. 원하는 Tilemap 레이어 선택
3. 타일 추가/제거/수정

## 타일 할당 가이드

### 빠른 할당 방법

#### 1. Grass Tiles 할당
```
Assets/Tilemaps 폴더에서:
- TilesetFloor_0 ~ TilesetFloor_10 (잔디 타일들)
- 또는 TilesetField_0 ~ TilesetField_10
```
여러 개를 선택해서 배열에 드래그 앤 드롭

#### 2. Nature Tiles 할당
```
Assets/Tilemaps 폴더에서:
- TilesetNature_0 ~ TilesetNature_50 (나무 타일들)
```

#### 3. 타일 찾기 팁
- Project 창에서 검색: `t:Tile`
- TilesetFloor, TilesetNature, TilesetField 등으로 필터링

## 생성 후 작업

### 1. 포탈 배치
생성된 맵에 실제 Portal GameObject를 배치합니다:
```csharp
// 각 포탈 위치에 PortalTrigger 오브젝트 생성
Village Portal: (-8, 2, 0) → targetScene: "VillageScene"
Cave Portal: (8, -3, 0) → targetScene: "CaveScene"
Boss Portal: (0, 6, 0) → targetScene: "BossScene"
```

### 2. Enemy 배치
Cave 포탈 주변에 박쥐 Enemy 배치:
```
위치: Cave Portal 주변 (8±3, -3±2, 0)
Enemy Type: Bat
Patrol Area: Cave 입구 주변
```

### 3. 추가 장식
- NPC 배치
- 아이템 배치
- 추가 장식 오브젝트 배치

## 트러블슈팅

### 타일이 생성되지 않을 때
1. **Tilemap 참조 확인**
   - Inspector에서 모든 Tilemap 필드가 할당되었는지 확인

2. **Tile Assets 확인**
   - 최소한 Grass Tiles는 할당되어야 함
   - 배열이 비어있지 않은지 확인

3. **Console 확인**
   - 경고/에러 메시지 확인
   - `⚠` 경고는 선택사항, `❌` 에러는 필수 해결

### 타일이 이상하게 배치될 때
1. **Map Origin/Size 확인**
   - Scene View에서 Gizmo로 표시된 맵 범위 확인

2. **Portal 위치 확인**
   - 포탈 위치가 맵 범위 안에 있는지 확인

3. **다시 생성**
   - Clear All Tilemaps → Generate Forest Map

## 시스템 구조

### 생성 순서
```
1. Background Layer (배경)
2. Ground Layer (바닥 + 길)
3. Collision Layer (충돌)
4. Decoration Layer (장식)
5. Themed Zones (테마 지역)
   - Village Zone
   - Cave Zone
   - Boss Zone
```

### 파일 구조
```
Assets/Scripts/
├── ForestTilemapGenerator.cs          # 메인 생성 스크립트
├── Editor/
│   └── ForestTilemapGeneratorEditor.cs # Editor UI
└── ForestTilemapGenerator_README.md   # 이 문서
```

## 확장 및 커스터마이징

### 새로운 테마 지역 추가
`ForestTilemapGenerator.cs`에서:
```csharp
private void GenerateCustomZone()
{
    // 커스텀 지역 생성 로직
}
```

### 길 생성 알고리즘 수정
`IsOnPath()` 메서드를 수정하여 길 패턴 변경

### 장식 패턴 변경
`GenerateDecorationLayer()`에서 `decorationDensity` 및 배치 로직 수정

## 참고 사항

- Undo/Redo 지원: 생성/삭제 작업은 Ctrl+Z로 되돌릴 수 있습니다
- Scene View에서 실시간 프리뷰: Gizmos를 통해 맵 범위와 포탈 위치 확인
- Git 친화적: .unity 파일만 수정되므로 버전 관리 용이

## 라이선스 및 크레딧

- Tile Assets: Ninja Adventure Asset Pack
- Generator Script: Puang Adventure Team

---

## 빠른 시작 체크리스트

- [ ] Forest Scene 열기
- [ ] Tools → Setup Forest Tilemap Generator 실행
- [ ] Grass Tiles 할당 (10개 이상 추천)
- [ ] Tree Tiles 할당 (나무 타일 여러 개)
- [ ] Rock Tiles 할당 (바위 타일 여러 개)
- [ ] 포탈 위치 확인 (Gizmo로 표시됨)
- [ ] Generate Forest Map 클릭
- [ ] 결과 확인 및 수동 편집
- [ ] 포탈 GameObject 배치
- [ ] Enemy 배치 (Cave 주변에 박쥐)
- [ ] 테스트 플레이

완료! 🎉
