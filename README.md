# Dead Air — Рейдер со стелсом

Тестовое, вариант 3. Unity 6 (6000.2), URP.

Открыть `Assets/_DeadAir/Scenes/Crater.unity` и нажать Play.

Видео: [Video/raider.mp4](Video/raider.mp4)

## Управление

Кнопки задаются в `Assets/_DeadAir/Scripts/Controls/DeadAirControls.inputactions` (Input System), есть клавиатура+мышь и геймпад.

| Действие | Клавиатура и мышь | Геймпад |
| --- | --- | --- |
| Ходьба | WASD | левый стик |
| Камера | мышь | правый стик |
| Бег | Shift | нажать левый стик |
| Прыжок | Space | A |
| Выстрел | ЛКМ | RT |
| Заклеить пробой скафандра | зажать T | зажать X |
| Отладка рейдера вкл/выкл | F1 | Select |
| Отпустить курсор | Esc | — |
| Рестарт после смерти | R | Start |

## Что сделано

- Патруль по точкам на NavMesh
- Конус зрения с проверкой стен, шкала обнаружения (вблизи растёт быстрее)
- Состояния: патруль, подозрение, поиск по последней позиции, бой, возврат к патрулю
- Выстрел слышно в радиусе 40 м — рейдер сразу идёт искать
- Не убегает, при HP ≤ 30% и если остался один — сдаётся
- Отладка: конус, состояние и шкала над головой, маркер последней позиции

Дополнительно по описанию игры: кислород и пробой скафандра (заклеивается изолентой), три режима гравитации (в демо базовая), два солнца, HUD в цветах из гайда со шрифтом Inter.

## Структура

```
Assets/_DeadAir
├── Art/Materials, Art/Textures, Art/UI
├── Audio, Fonts
├── Data/RaiderConfig.asset      — настройки рейдера (ScriptableObject)
├── Prefabs/Characters           — Player, PlayerCamera, Raider
├── Prefabs/Environment          — стены, ящики, бочки, камни
├── Prefabs/FX, Prefabs/UI       — трассер, вспышка, HUD
├── Scenes/Crater.unity
└── Scripts                      — по сборке (asmdef) на модуль
    ├── Core      — здоровье, оружие, шина выстрелов
    ├── Controls  — Input Actions и PlayerInputHandler
    ├── Player    — движение, стрельба, кислород, камера
    ├── Raider    — RaiderBrain, States/, зрение, слух, бой, отладка
    ├── Game      — GameSession: рестарт, отладка, курсор
    └── UI        — HUD
```

## Ассеты

Текстуры — Poly Haven (CC0), звуки — OpenGameArt (CC0) и Mixkit, шрифт Inter (OFL). Подробнее в `Assets/_DeadAir/ATTRIBUTION.md`.
