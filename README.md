# 🃏 Stitched Souls (Prototype)

Unity 기반의 **덱 빌딩 턴제 전략 게임** 프로토타입입니다.
그리드 위에서 펼쳐지는 전략적인 이동과 카드 시스템을 결합한 전투 시스템을 구현했습니다.
<img width="889" height="498" alt="Image" src="https://github.com/user-attachments/assets/781e292b-1e9e-495b-aa38-0566a28d1136" />

## 🎥 Project Overview

-   **Engine:** Unity 6.3 LTS
-   **Language:** C#
-   **Key Libraries:** DOTween (Animation)

## 🎮 Key Features (핵심 기능)

### 1. Turn-Based Battle System (턴제 전투)

-   플레이어와 적이 번갈아가며 행동하는 턴 시스템 구현
-   **행동력(Action Points)** 및 **이동력(Move Points)** 자원 관리
-   승리(적 전멸) 및 패배(플레이어 사망) 조건에 따른 게임 오버/승리 연출

### 2. Grid Movement & Interaction (그리드 이동)

-   타일 기반의 그리드 맵 시스템
-   유닛 간 충돌 처리 및 이동 불가 구역 설정
-   키보드 방향키를 통한 직관적인 캐릭터 이동

### 3. Card Deck Building System (카드 시스템)

-   **ScriptableObject** 기반의 확장 가능한 카드 데이터 설계
-   **Drag & Drop UI**: 카드를 드래그하여 슬롯에 등록하는 직관적인 조작
-   **Queue Action**: 슬롯에 등록된 카드 순서대로 행동을 예약하고 일괄 실행(Sequence)

### 4. Interactive UI & Feedback

-   **World Space UI**: 유닛 머리 위에 실시간 체력(HP) 바 표시
-   데미지 피격 시 쉐이크(Shake) 및 색상 변경 연출
-   전투 결과 팝업 및 재시작(Restart) 기능 구현

## 🕹️ Controls (조작법)

| Action                     | Key / Input                      |
| :------------------------- | :------------------------------- |
| **Move (이동)**            | `Arrow Keys (↑, ↓, ←, →)`        |
| **Use Card (카드 등록)**   | Mouse Drag & Drop (Hand -> Slot) |
| **Execute Turn (턴 실행)** | `Space Bar`                      |
| **Camera**                 | Fixed Quarter View               |

## 📂 Project Structure

-   **Controllers:** `BattleManager` (Game Loop), `GridManager`
-   **Entities:** `Unit`, `CardData` (ScriptableObject)
-   **UI:** `BattleUIManager`, `DraggableCard`, `ActionSlot`

---

_Created by RE : SAY_WIZ_
