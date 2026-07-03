# 개발 사양서

## 1. 문서 목적

이 문서는 `몬스터 헌트 MVP`의 Unity 개발 기준을 정의한다. 게임 기획서의 기능을 실제 구현 단위로 분리하고, 씬 구성, 프리팹, 스크립트 책임, CSV 데이터 구조, 레벨 공식, 테스트 기준을 명확히 한다.

관련 문서:

- [game_design_document_ko.md](game_design_document_ko.md)
- [monster_data_sample.csv](monster_data_sample.csv)

## 2. 개발 환경

- 엔진: Unity
- 언어: C#
- 렌더링: Unity 기본 3D 렌더 파이프라인 기준
- 입력: Unity Input Manager 또는 프로젝트 설정에 맞춘 Input System
- 대상 플랫폼: Windows PC
- 화면 비율: 16:9 우선 지원

## 3. 권장 폴더 구조

```text
Assets/
  Art/
    Materials/
    Models/
    Textures/
  Audio/
    BGM/
    SFX/
  Data/
    Monsters/
      monster_data.csv
  Prefabs/
    Player/
    Monsters/
    Zones/
    UI/
  Scenes/
  Scripts/
    Core/
    Data/
    Player/
    Monsters/
    Rewards/
    Zones/
    UI/
```

## 4. 씬 구성

### TitleScene

역할:

- 게임 시작 진입점
- 새 게임 시작
- 종료 버튼 제공

필수 오브젝트:

- `Main Camera`
- `Directional Light`
- `TitleCanvas`
- `SceneLoader`

### HuntScene

역할:

- 실제 사냥 플레이 진행
- 4개 구역 관리
- 몬스터 스폰과 보상 지급

필수 오브젝트:

- `Main Camera`
- `Directional Light`
- `GameManager`
- `ZoneManager`
- `RewardManager`
- `Player`
- `GameHUD`
- `EasyZone`
- `NormalZone`
- `HardZone`
- `BossZone`

### ResultScene

역할:

- 최종 결과 표시
- 다시 시작, 타이틀 이동 처리

필수 오브젝트:

- `Main Camera`
- `Directional Light`
- `ResultCanvas`
- `ResultController`

## 5. 구역 사양

| 구역 ID | 표시명 | 권장 레벨 | 기능 |
| --- | --- | ---: | --- |
| easy | 쉬움 | 1~3 | 기본 몬스터 스폰 |
| normal | 중간 | 3~6 | 중간 난이도 몬스터 스폰 |
| hard | 어려움 | 6~9 | 높은 체력과 피해량 몬스터 스폰 |
| boss | 보스 | 9~10 | 보스 몬스터 스폰 |

구역 구현 기준:

- 각 구역은 `ZoneTrigger` Collider로 범위를 가진다.
- 플레이어가 구역에 진입하면 HUD의 현재 구역 표시를 갱신한다.
- 몬스터 스폰은 `MonsterSpawner`가 CSV의 `zone` 값을 기준으로 처리한다.
- MVP에서는 구역 입장 제한을 두지 않는다.

## 6. 프리팹 사양

### Player

구성 컴포넌트:

- `Transform`
- `Rigidbody`
- `CapsuleCollider`
- `PlayerController`
- `PlayerCombat`
- `PlayerHealth`
- `PlayerLevel`

책임:

- 이동 입력 처리
- 점프 처리
- 박치기 공격 처리
- 점프 밟기 판정 전달
- 체력과 레벨 관리

### Monster

구성 컴포넌트:

- `Transform`
- `Rigidbody`
- `CapsuleCollider`
- `MonsterController`
- `MonsterHealth`
- `MonsterReward`

책임:

- CSV 데이터 기반 스탯 초기화
- 배회 및 추격
- 플레이어 접촉 피해 처리
- 피격 및 사망 처리
- 사망 시 보상 지급 요청

### BossMonster

`Monster` 프리팹을 기반으로 하되 다음 차이를 둔다.

- 더 큰 스케일
- 높은 체력
- 높은 접촉 피해
- 넓은 추격 범위
- `is_boss = true` 데이터 사용

### MonsterSpawner

책임:

- CSV에서 로드된 몬스터 데이터 목록을 참조한다.
- 자신의 구역 ID와 일치하는 몬스터를 생성한다.
- `spawn_count`만큼 몬스터를 배치한다.

### GameHUD

표시 항목:

- 체력
- 레벨
- 경험치
- 골드
- 현재 구역
- 처치 수
- 박치기 쿨다운

## 7. 데이터 사양

### MonsterData

CSV 1행은 하나의 몬스터 타입을 의미한다.

```csharp
public class MonsterData
{
    public string monsterId;
    public string name;
    public string zone;
    public int maxHp;
    public int contactDamage;
    public float moveSpeed;
    public float chaseRange;
    public float headbuttDamageMultiplier;
    public float stompDamageMultiplier;
    public int expReward;
    public int goldReward;
    public int spawnCount;
    public bool isBoss;
}
```

CSV 컬럼:

```text
monster_id,name,zone,max_hp,contact_damage,move_speed,chase_range,headbutt_damage_multiplier,stomp_damage_multiplier,exp_reward,gold_reward,spawn_count,is_boss
```

컬럼 규칙:

- `monster_id`: 고유 문자열. 예: `easy_slime`
- `zone`: `easy`, `normal`, `hard`, `boss` 중 하나
- `max_hp`: 1 이상 정수
- `contact_damage`: 0 이상 정수
- `move_speed`: 0 이상 실수
- `chase_range`: 0 이상 실수
- `headbutt_damage_multiplier`: 박치기 기본 피해에 곱하는 값
- `stomp_damage_multiplier`: 점프 밟기 기본 피해에 곱하는 값
- `exp_reward`: 처치 시 지급 경험치
- `gold_reward`: 처치 시 지급 골드
- `spawn_count`: 해당 타입 생성 수
- `is_boss`: `true` 또는 `false`

## 8. CSV 로드 사양

### MonsterCsvLoader

역할:

- `Assets/Data/Monsters/monster_data.csv`를 로드한다.
- CSV 행을 `MonsterData` 객체로 변환한다.
- 잘못된 데이터는 에러 로그를 남기고 스폰 대상에서 제외한다.

주요 메서드:

```csharp
public IReadOnlyList<MonsterData> Load(string resourcePath);
private bool TryParseRow(string[] columns, out MonsterData data);
```

예외 처리:

- 파일이 없으면 에러 로그를 출력하고 빈 목록을 반환한다.
- 필수 컬럼 개수가 부족하면 해당 행을 무시한다.
- 숫자 변환 실패 시 해당 행을 무시한다.
- 알 수 없는 `zone` 값은 해당 행을 무시한다.

MVP 구현에서는 쉼표가 포함된 문자열을 사용하지 않는다는 전제로 단순 CSV 파서를 허용한다. 추후 로컬라이징 텍스트나 복잡한 필드가 필요해지면 CSV 파서 라이브러리 도입을 검토한다.

## 9. 플레이어 스크립트 사양

### PlayerController

역할:

- 이동과 점프 처리

주요 필드:

```csharp
public float moveSpeed;
public float jumpForce;
public LayerMask groundLayer;
```

주요 메서드:

```csharp
private void Update();
private void FixedUpdate();
private bool IsGrounded();
```

규칙:

- 입력 수집은 `Update`에서 처리한다.
- Rigidbody 이동은 `FixedUpdate`에서 처리한다.
- 점프는 지면 판정이 참일 때만 실행한다.

### PlayerCombat

역할:

- 박치기 공격
- 점프 밟기 판정
- 공격 상태 관리

주요 필드:

```csharp
public int baseHeadbuttDamage;
public int baseStompDamage;
public float headbuttSpeed;
public float headbuttDuration;
public float headbuttCooldown;
public bool IsHeadbutting { get; private set; }
public float HeadbuttCooldownRatio { get; private set; }
```

주요 메서드:

```csharp
public void TryHeadbutt();
public int GetHeadbuttDamage(float multiplier);
public int GetStompDamage(float multiplier);
private void OnCollisionEnter(Collision collision);
```

규칙:

- 박치기 중 몬스터와 충돌하면 `MonsterHealth.TakeDamage()`를 호출한다.
- 플레이어가 하강 중이고 몬스터 상단 판정에 닿으면 점프 밟기로 처리한다.
- 점프 밟기 성공 시 플레이어에게 반동 점프를 적용한다.

### PlayerHealth

역할:

- 체력과 무적 시간 관리

주요 필드:

```csharp
public int maxHealth;
public float invincibleDuration;
public int CurrentHealth { get; private set; }
public bool IsInvincible { get; private set; }
```

주요 메서드:

```csharp
public void TakeDamage(int amount);
public void Heal(int amount);
public void ResetHealth();
```

규칙:

- 피해를 받으면 무적 시간이 시작된다.
- 무적 시간 중 추가 피해는 무시한다.
- 체력이 0 이하가 되면 `GameManager.GameOver()`를 호출한다.

### PlayerLevel

역할:

- 경험치와 레벨 관리
- 레벨업 시 능력치 증가 적용

주요 필드:

```csharp
public int Level { get; private set; }
public int CurrentExp { get; private set; }
public int Gold { get; private set; }
public int KillCount { get; private set; }
```

레벨 테이블:

```csharp
private readonly int[] requiredExpByLevel =
{
    0, 50, 120, 220, 360, 540, 760, 1020, 1320, 1660
};
```

주요 메서드:

```csharp
public void AddReward(int exp, int gold);
private void TryLevelUp();
public int GetRequiredExpForNextLevel();
```

규칙:

- 최대 레벨은 10이다.
- 레벨 10 이후에도 경험치는 누적할 수 있지만 레벨은 증가하지 않는다.
- 레벨업 시 최대 체력, 박치기 피해, 점프 밟기 피해를 갱신한다.

## 10. 몬스터 스크립트 사양

### MonsterController

역할:

- 배회
- 추격
- 플레이어 접촉 감지

주요 필드:

```csharp
public MonsterData Data { get; private set; }
public Transform target;
```

주요 메서드:

```csharp
public void Initialize(MonsterData data);
private void Update();
private void Patrol();
private void Chase();
private bool IsTargetInChaseRange();
```

규칙:

- 플레이어가 `chase_range` 안에 있으면 추격한다.
- 추격 범위 밖이면 스폰 위치 근처를 배회한다.
- 공격 판정은 별도 애니메이션 없이 접촉 피해로 처리한다.

### MonsterHealth

역할:

- 몬스터 체력 관리
- 사망 처리

주요 필드:

```csharp
public int CurrentHp { get; private set; }
public bool IsDead { get; private set; }
```

주요 메서드:

```csharp
public void Initialize(MonsterData data);
public void TakeDamage(int amount);
private void Die();
```

규칙:

- 체력이 0 이하가 되면 한 번만 사망 처리한다.
- 사망 시 `RewardManager.GrantMonsterReward()`를 호출한다.
- 사망 후 Collider를 비활성화한다.

## 11. 보상 사양

### RewardManager

역할:

- 몬스터 처치 보상 지급
- 처치 수 누적
- 보스 처치 여부 기록

주요 메서드:

```csharp
public void GrantMonsterReward(MonsterData data);
```

규칙:

- 보상은 플레이어의 `PlayerLevel.AddReward()`로 전달한다.
- `is_boss`가 참인 몬스터가 죽으면 `GameManager.ClearGame()`을 호출한다.

## 12. 전투 판정 기준

### 박치기

조건:

- `PlayerCombat.IsHeadbutting == true`
- 충돌 대상이 `Monster` 태그를 가진다.

처리:

1. 몬스터의 `headbutt_damage_multiplier`를 읽는다.
2. `baseHeadbuttDamage * multiplier`로 피해량을 계산한다.
3. `MonsterHealth.TakeDamage()`를 호출한다.
4. 박치기 히트 효과를 재생한다.

### 점프 밟기

조건:

- 플레이어의 Y축 속도가 음수이다.
- 몬스터 상단 판정에 접촉했다.
- 충돌 대상이 `Monster` 태그를 가진다.

처리:

1. 몬스터의 `stomp_damage_multiplier`를 읽는다.
2. `baseStompDamage * multiplier`로 피해량을 계산한다.
3. `MonsterHealth.TakeDamage()`를 호출한다.
4. 플레이어에게 반동 점프를 적용한다.

### 플레이어 피격

조건:

- 플레이어가 박치기 중이 아니다.
- 점프 밟기 조건을 만족하지 않는다.
- 몬스터와 접촉했다.

처리:

1. 몬스터의 `contact_damage`를 읽는다.
2. `PlayerHealth.TakeDamage()`를 호출한다.

## 13. 태그와 레이어

### 태그

| 태그 | 용도 |
| --- | --- |
| Player | 플레이어 판별 |
| Monster | 일반 몬스터와 보스 판별 |
| Zone | 구역 판별 |
| Ground | 지면 판별 |

### 레이어

| 레이어 | 용도 |
| --- | --- |
| Player | 플레이어 물리 |
| Monster | 몬스터 물리 |
| Ground | 지면 판정 |
| Zone | 구역 Trigger |

## 14. UI 갱신 방식

권장 방식:

- 상태 변경 시 이벤트를 발생시키고 UI가 구독한다.
- MVP에서는 `GameManager` 또는 `PlayerLevel`이 명시적으로 `GameHudController` 메서드를 호출해도 된다.

주요 메서드:

```csharp
public void SetHealth(int current, int max);
public void SetLevel(int level);
public void SetExp(int currentExp, int nextLevelExp);
public void SetGold(int gold);
public void SetZone(string zoneName);
public void SetKillCount(int killCount);
public void SetHeadbuttCooldown(float ratio);
```

## 15. 결과 데이터

```csharp
public class HuntResult
{
    public bool isCleared;
    public int finalLevel;
    public int totalExp;
    public int totalGold;
    public int killCount;
    public bool bossKilled;
}
```

결과 표시 기준:

- 보스를 처치하면 클리어
- 체력이 0이 되면 실패
- 실패해도 획득 골드와 경험치는 결과 화면에 표시한다.

## 16. 예외 처리 기준

- CSV 파일이 없으면 기본 몬스터 없이 씬을 시작하지 않고 에러 로그를 출력한다.
- CSV 행 파싱 실패 시 해당 행만 무시한다.
- 스폰 가능한 몬스터가 없는 구역은 경고 로그를 출력한다.
- `RewardManager` 또는 `PlayerLevel` 참조가 없으면 보상 지급을 중단하고 에러 로그를 출력한다.
- 보스 데이터가 없으면 결과 클리어가 불가능하므로 경고 로그를 출력한다.

## 17. 개발 단계

### 단계 1: 전투 가능한 최소 버전

- 플레이어 이동
- 점프
- 박치기
- 점프 밟기
- 몬스터 피격과 사망

### 단계 2: 성장과 보상

- 경험치 지급
- 골드 지급
- 레벨 1~10
- HUD 표시

### 단계 3: 구역과 데이터

- 4개 구역 구성
- CSV 로드
- 구역별 스폰
- 기본 몬스터 데이터 적용

### 단계 4: 보스와 결과

- 보스 몬스터
- 보스 처치 클리어
- 게임 오버
- 결과 화면

## 18. 테스트 기준

### 기능 테스트

- CSV 데이터가 정상 로드된다.
- 각 구역에 `spawn_count`만큼 몬스터가 생성된다.
- 박치기 공격으로 몬스터 체력이 감소한다.
- 점프 밟기로 몬스터 체력이 감소한다.
- 공격 상태가 아닐 때 몬스터 접촉 시 플레이어 체력이 감소한다.
- 몬스터 처치 시 경험치와 골드가 증가한다.
- 누적 경험치에 따라 레벨이 10까지 상승한다.
- 보스 처치 시 클리어 결과가 표시된다.
- 플레이어 체력 0 시 실패 결과가 표시된다.

### 밸런스 테스트

- 쉬움 구역에서 레벨 3까지 2분 내외로 도달할 수 있다.
- 중간 구역에서 레벨 6까지 자연스럽게 성장할 수 있다.
- 어려움 구역에서 레벨 9까지 도달 가능하다.
- 레벨 9~10 플레이어가 보스를 처치할 수 있다.
- 레벨이 낮은 상태에서 보스에게 접근하면 위험이 충분히 크다.

## 19. 완료 기준

MVP 완료 조건:

- `HuntScene`에서 4개 구역을 이동할 수 있다.
- CSV 기반으로 몬스터가 스폰된다.
- 이동, 박치기, 점프 밟기로 몬스터를 처치할 수 있다.
- 몬스터 처치 보상으로 골드와 경험치를 획득한다.
- 레벨 1~10 성장이 동작한다.
- 보스 처치 시 클리어 결과 화면이 표시된다.
- 플레이어 체력 0 시 실패 결과 화면이 표시된다.
