# 🃏 Stitched Souls (Prototype)

Unity 기반의 **로그라이크 덱 빌딩 턴제 전략 게임**입니다.
그리드 위에서 펼쳐지는 전략적인 타일 이동과 덱 빌딩 시스템을 결합하여 깊이 있는 전투 경험을 제공하며, 노드 기반의 맵 탐험을 통해 매 런(Run)마다 새로운 성장과 선택의 재미를 느낄 수 있습니다.

<img width="889" height="498" alt="Image" src="https://github.com/user-attachments/assets/baecb259-b581-4ff2-b6f1-73e2149dad48" />

## 🎥 Project Overview

- **Engine:** Unity 6.3 LTS
- **Language:** C#
- **Key Libraries:** DOTween (Animation/UI Effects)

## 🎮 Key Features (핵심 기능)

### 1. Grid & Queue-based Battle System (그리드 예약 전투)

- **Queue Action:** 카드를 슬롯에 순서대로 등록하여 행동을 예약하고, 턴 실행 시 시퀀스(Sequence)로 일괄 실행하는 전략적 전투 시스템입니다.
- **자원 관리:** 이동력(Move Points)을 소모하여 그리드를 이동하고, PP(Play Points)를 소모해 카드를 사용하는 입체적 자원 시스템을 구축했습니다.
- **궁극기(Ultimate) 시스템:** 전투 중 게이지를 모아 전황을 뒤집을 수 있는 강력한 궁극기 기능을 포함하고 있습니다.

### 2. Roguelike Map Progression (로그라이크 맵 탐험)

- **노드 기반 맵 시스템:** 일반 전투, 엘리트, 상점, 보스 등 다양한 노드를 선택하며 전진하는 분기형 진행 방식입니다.
- **디자인 유연성:** 유니티 에디터 인스펙터에서 노드 순서와 맵 구조를 자유롭게 디자인할 수 있는 커스텀 데이터 구조를 적용했습니다.

### 3. Deck Building & Relic System (성장 시스템)

- **카드 덱 관리:** 전투 보상과 상점을 통해 카드를 획득하고, 실시간으로 덱(Draw Pile)과 무덤(Discard Pile)의 매수를 확인할 수 있습니다.
- **유물(Relic) 시스템:** 획득 시 플레이어의 스탯(최대 체력, 공격력, 방어력 등)을 영구적으로 강화하는 수집 요소를 구현했습니다.

### 4. Polished UI/UX & Sound System (사용자 경험)

- **마스터 사운드 제어:** 독립적인 `SoundManager`를 통해 BGM 및 SFX를 분리하고, 하나의 마스터 슬라이더로 통합 제어가 가능한 환경 설정 기능을 제공합니다.
- **동적 전투 결과창:** 전투 종료 시 가한 피해량, 입은 피해량, 획득 골드 등을 직관적으로 보여주는 툴팁 시스템을 갖추고 있습니다.
- **인벤토리형 유물 팝업:** 전투 화면 가독성을 위해 필요할 때만 껐다 켤 수 있는 독립형 유물 팝업 UI를 도입했습니다.

## 🛠️ Technical Highlights (기술적 특징)

- **Event-Driven Architecture:** 옵저버 패턴(`event Action`)을 활용하여 `GameManager`의 데이터가 변경될 때만 UI가 스스로 갱신되도록 설계하여 성능 최적화와 낮은 결합도를 달성했습니다.
- **Modular UI Prefab & Dynamic Generation:** 유물 패널을 독립된 프리팹으로 설계하여 어떤 씬에서도 재사용이 가능하며, 데이터에 따라 슬롯이 동적으로 생성(`Instantiate`)되도록 자동화했습니다.
- **ScriptableObject 중심 설계:** 카드와 유물 데이터를 ScriptableObject로 관리하여 데이터 확장 및 밸런스 수정이 용이한 구조를 확립했습니다.

## 🕹️ Controls (조작법)

| Action                     | Key / Input                      |
| :------------------------- | :------------------------------- |
| **Move (그리드 이동)**     | `Arrow Keys (↑, ↓, ←, →)`        |
| **Use Card (카드 등록)**   | Mouse Drag & Drop (Hand -> Slot) |
| **Execute Turn (턴 실행)** | `Space Bar`                      |
| **Camera**                 | Fixed Quarter View               |

## 📂 Project Structure

- **Managers:** `GameManager`, `BattleManager`, `SoundManager`, `SettingManager`
- **Map & Progression:** `MapManager`, `MapNode`, `RewardUIManager`
- **Entities:** `Unit`, `CardData`, `ItemData` (ScriptableObject)
- **UI:** `BattleUIManager`, `DeckViewUI`, `RelicPanelUI`, `ItemSlotUI`

---

_Created by RE : SAY_WIZ_
