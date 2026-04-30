# Background Mute Helper

> 백그라운드 음소거를 지원하지 않는 게임/프로그램을 위한 자동 음소거 도구.
> Auto-mute helper for games and programs that don't have a built-in background-mute option.

대상 프로그램이 포커스를 잃으면 자동으로 해당 프로그램의 오디오 세션만 음소거하고,
다시 포커스를 받으면 음소거를 해제합니다. 시스템 전체 볼륨이나 다른 앱은 건드리지 않습니다.

When a target program loses focus, only its audio session is muted; when it regains focus,
mute is released. Other applications and the system master volume are never touched.

---

## 한국어

### 동작 방식
- Windows 포그라운드 변경 이벤트(`SetWinEventHook` / `EVENT_SYSTEM_FOREGROUND`) 를 후킹하여, 포커스 전환이 일어난 순간에만 음소거 상태를 갱신합니다. 폴링 루프가 없어 평소엔 CPU를 거의 쓰지 않습니다.
- 오디오 세션 관리는 [NAudio](https://github.com/naudio/NAudio) 의 `AudioSessionManager` 를 사용하며, 5초마다 한 번 세션을 재스캔하여 새로 실행되거나 종료된 대상 프로그램을 반영합니다.
- 음소거는 프로세스 단위 오디오 세션의 `SimpleAudioVolume.Mute` 만 토글합니다.

### 사용법
1. 빌드된 실행 파일을 실행하면 시스템 트레이에 아이콘이 등록됩니다.
2. 트레이 아이콘을 더블클릭하거나, 우클릭 → **설정 열기** 로 설정 창을 엽니다.
3. 목록에는 현재 오디오 세션이 잡히는 프로세스가 표시됩니다. 음소거 대상으로 삼을 프로세스를 체크하고 **저장** 을 누르면 즉시 적용됩니다.
4. 목록에 보이지 않는 프로세스(예: 아직 실행되지 않은 게임)는 **수동 추가** 로 프로세스 이름(`.exe` 확장자 제외, 예: `StarRail`)을 직접 입력해 등록할 수 있습니다.
5. 종료는 트레이 아이콘 우클릭 → **Exit**.

### 프로세스 이름 확인
작업 관리자 → 세부 정보 탭에서 보이는 이름이 프로세스 이름입니다. 예) Honkai: Star Rail → `StarRail.exe` 이므로 `StarRail` 을 등록.

### 설정 파일 (선택)
설정은 실행 파일 옆 `setting.json` 에 저장됩니다. GUI 가 자동으로 관리하므로 직접 편집할 필요는 없지만, 형식은 다음과 같습니다.

---

## English

### Usage
1. Run the built executable — it appears in the system tray.
2. Double-click the tray icon (or right-click → **설정 열기 / Open Settings**) to open the settings window.
3. The list shows processes currently holding an audio session. Check the ones you want auto-muted in the background and click **저장 (Save)** — changes apply immediately.
4. For programs not yet running (so not in the list), use **수동 추가 (Add manually)** and type the process name (without `.exe`, e.g. `StarRail`).
5. To quit, right-click the tray icon → **Exit**.

### Finding a process name
Open Task Manager → Details tab. The image name there is the process name. e.g. Honkai: Star Rail → `StarRail.exe`, so register `StarRail`.