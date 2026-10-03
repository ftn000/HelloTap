# Руководство по публикации «CodeTap: Симулятор Программиста» в Яндекс Играх

Документ содержит все необходимые метаданные, тексты, артикулы внутриигровых покупок (In-App Purchases) и технические параметры для быстрой настройки игры в [Консоли разработчика Яндекс Игр](https://yandex.ru/dev/games/console/).

---

## 1. Основная информация

| Параметр | Значение (RU) | Value (EN) | Değer (TR) |
|---|---|---|---|
| **Название** | `CodeTap: Симулятор Программиста` | `CodeTap: Programmer Simulator & Tycoon` | `CodeTap: Yazılımcı Simülatörü` |
| **Короткое описание** | `Пишите код, развивайте IT-студию, покупайте серверы и выходите на биржу IPO!` | `Write code, scale your IT studio, buy servers and go public in high-tech IPO!` | `Kod yazın, BT stüdyonuzu büyütün, sunucular alın ve halka arz (IPO) yapın!` |
| **Возрастной рейтинг** | 6+ (или 0+) | 6+ | 6+ |
| **Категории** | Кликеры, Симуляторы, Экономические | Clickers, Simulators, Tycoon | Tıklayıcı, Simülatör, Şirket |
| **Теги** | `кликер`, `симулятор`, `программист`, `it`, `код`, `тайкун`, `бизнес`, `офис`, `деньги` | `clicker`, `simulator`, `coder`, `programming`, `tycoon`, `dev`, `idle`, `tech` | `tıklayıcı`, `simülatör`, `yazılımcı`, `kod`, `şirket`, `ofis`, `para`, `it` |

### Полное описание (RU):
```text
Почувствуйте себя основателем IT-стартапа! В «CodeTap: Симулятор Программиста» каждый клик — это компиляция чистого кода на C# и генерация прибыли.

Особенности игры:
⚡ Механика компиляции: набирайте код вручную или автоматизируйте процессы ботами и квантовыми серверами.
⌨️ Синтезатор клавиатурных свитчей: переключайтесь в реальном времени между звуковыми профилями Blue, Red, Brown и Laser!
🔥 Состояние Потока (Flow State): удерживайте темп набора для активации комбо с x3.0 множителем.
🏢 Hub Студии: развивайте офисы, маркетинг, кибербезопасность и заведите офисных котиков.
📈 Выход на IPO: привлекайте венчурных инвесторов, продавайте акции и получайте постоянный буст престижа.
🏆 Глобальный Зал Славы: соревнуйтесь с другими разработчиками в таблице лидеров Яндекс Игр!
```

### Full Description (EN):
```text
Become the founder of a tech startup! In "CodeTap: Programmer Simulator & Tycoon", every tap compiles clean code and generates revenue.

Game features:
⚡ Code compilation mechanics: write code manually or automate workflows with bots and quantum servers.
⌨️ Mechanical switch synthesizer: switch between Blue, Red, Brown, and Laser sound presets in real-time!
🔥 Flow State: keep your typing pace to activate a massive x3.0 multiplier combo.
🏢 Studio Hub OS: upgrade offices, marketing, cybersecurity, and adopt office cats.
📈 IPO Prestige: attract venture investors, sell shares, and unlock permanent prestige perks.
🏆 Global Hall of Fame: compete against coders worldwide on the Yandex Games leaderboard!
```

### Tam Açıklama (TR):
```text
Bir teknoloji girişiminin kurucusu olun! "CodeTap: Yazılımcı Simülatörü"nde her tıklama temiz kod derler ve gelir üretir.

Oyun özellikleri:
⚡ Derleme mekaniği: kodu manuel yazın veya botlar ve kuantum sunucularıyla otomasyona bağlayın.
⌨️ Mekanik anahtar sentezleyici: Blue, Red, Brown ve Laser ses profilleri arasında anında geçiş yapın!
🔥 Akış Hali (Flow State): x3.0 çarpan kombolarını tetiklemek için yazma hızınızı koruyun.
🏢 Stüdyo Merkezi: ofisleri, pazarlamayı, siber güvenliği geliştirin ve ofis kedileri sahiplenin.
📈 Halka Arz (IPO): girişim sermayedarlarını çekin, hisse satın ve kalıcı prestij bonusları kazanın.
🏆 Küresel Onur Listesi: Yandex Games liderlik tablosunda dünyanın dört bir yanındaki geliştiricilerle yarışın!
```

---

## 2. Графика и промо-материалы

Все необходимые промо-материалы уже сгенерированы в максимальном качестве и находятся в папке [`promo/`](../promo/):

* **Иконка игры (512x512 px, 1:1):** [`promo/icon_512x512.png`](../promo/icon_512x512.png) (или [`public/icon.png`](../public/icon.png)).
* **Обложка карточки в каталоге (800x600 px, 4:3):** [`promo/cover_800x600.png`](../promo/cover_800x600.png) (HD: [`promo/cover_1200x900.png`](../promo/cover_1200x900.png)).
* **Широкий промо-баннер (1920x1080 px, 16:9):** [`promo/banner_1920x1080.png`](../promo/banner_1920x1080.png) (или [`promo/banner_1280x720.png`](../promo/banner_1280x720.png)).
* **Мастер-арт (1024x1024 px):** [`promo/icon_1024x1024.png`](../promo/icon_1024x1024.png).
* **Фавиконка браузера:** [`public/favicon.png`](../public/favicon.png) и [`public/icon-192.png`](../public/icon-192.png).
* **Ориентация:** Поддерживается как альбомная (Desktop), так и портретная (Mobile).
* **Подробное руководство:** [`promo/README.md`](../promo/README.md).

---

## 3. Загрузка исходного кода игры

1. Выполните команду сборки в терминале:
   ```bash
   npm run build:yandex
   ```
2. Скрипт соберет TypeScript-бандл и сформирует готовый архив:
   ```text
   hellotap-yandex.zip
   ```
3. В Консоли разработчика Яндекс Игр перейдите во вкладку **«Исходный код»** и загрузите `hellotap-yandex.zip`.

---

## 4. Спецификация внутриигровых покупок (In-App Purchases)

В Консоли Яндекс Игр перейдите в раздел **«Покупки»** -> **«Добавить покупку»** и внесите следующие товары:

### 1. Вечный Множитель x2 (Non-consumable)
* **ID артикула:** `codetap_vip_x2`
* **Тип:** Нерасходуемый (постоянный)
* **Цена:** `129 Янов`
* **Название (RU):** `Вечный Множитель x2`
* **Название (EN):** `Permanent x2 Multiplier`
* **Название (TR):** `Kalıcı x2 Çarpanı`
* **Описание (RU):** `Удваивает всю генерацию C# и доход студии навсегда (стакается с рекламой до x4)`
* **Описание (EN):** `Doubles all C# production and money income forever (stacks with ads to x4)`
* **Описание (TR):** `Tüm kod üretimini ve stüdyo gelirini kalıcı olarak ikiye katlar (reklamla x4'e katlanır)`

### 2. Авто-Кликер Bot Pro 10 CPS (Non-consumable)
* **ID артикула:** `codetap_autoclicker`
* **Тип:** Нерасходуемый (постоянный)
* **Цена:** `149 Янов`
* **Название (RU):** `Авто-Кликер Bot Pro (10 CPS)`
* **Название (EN):** `Auto-Clicker Bot Pro (10 CPS)`
* **Название (TR):** `Otomatik Tıklayıcı Bot Pro (10 CPS)`
* **Описание (RU):** `Кликает автоматически 10 раз в секунду без участия игрока`
* **Описание (EN):** `Clicks automatically 10 times per second in the background`
* **Описание (TR):** `Oyuncunun müdahalesi olmadan saniyede 10 kez otomatik tıklar`

### 3. Отключение Рекламы / No-Ads Pass (Non-consumable)
* **ID артикула:** `codetap_noads`
* **Тип:** Нерасходуемый (постоянный)
* **Цена:** `149 Янов`
* **Название (RU):** `Отключение Рекламы (No-Ads Pass)`
* **Название (EN):** `No-Ads Pass`
* **Название (TR):** `Reklamsız Geçiş (No-Ads)`
* **Описание (RU):** `Отключает всплывающую рекламу (Interstitial) и нижний баннер (Sticky Banner)`
* **Описание (EN):** `Removes interstitial popups and bottom sticky banner`
* **Описание (TR):** `Geçiş reklamlarını ve alt yapışkan başlığı kaldırır`

### 4. Стартовый Венчурный Грант (Consumable)
* **ID артикула:** `codetap_stocks_25`
* **Тип:** Расходуемый (многоразовый)
* **Цена:** `79 Янов`
* **Название (RU):** `Венчурный грант (25 Акций)`
* **Название (EN):** `Venture Grant (25 Stocks)`
* **Название (TR):** `Girişim Hibesi (25 Hisse)`
* **Описание (RU):** `Мгновенно начисляет 25 токенов акций для раннего открытия престиж-апгрейдов`
* **Описание (EN):** `Instantly awards 25 stock tokens to unlock early prestige perks`
* **Описание (TR):** `Erken prestij geliştirmeleri için anında 25 hisse jetonu verir`

### 5. Серия А — Раунд Развития (Consumable)
* **ID артикула:** `codetap_stocks_100`
* **Тип:** Расходуемый (многоразовый)
* **Цена:** `250 Янов`
* **Название (RU):** `Серия А (100 Акций)`
* **Название (EN):** `Series A (100 Stocks)`
* **Название (TR):** `Seri A (100 Hisse)`
* **Описание (RU):** `Крупный пул токенов акций (100 токенов) для масштабной экспансии`
* **Описание (EN):** `Massive pool of 100 stock tokens for rapid studio expansion`
* **Описание (TR):** `Hızlı stüdyo büyümesi için 100 hisse jetonluk büyük fon`

### 6. Чемодан Инвестора (Consumable)
* **ID артикула:** `codetap_money_100k`
* **Тип:** Расходуемый (многоразовый)
* **Цена:** `49 Янов`
* **Название (RU):** `Чемодан Инвестора (100,000 ₽)`
* **Название (EN):** `Investor Briefcase (100,000 ₽)`
* **Название (TR):** `Yatırımcı Çantası (100,000 ₽)`
* **Описание (RU):** `Оборотные средства для быстрого найма сотрудников и покупки серверов`
* **Описание (EN):** `Working capital for quick hiring and server hardware purchases`
* **Описание (TR):** `Hızlı personel alımı ve sunucu donanımı için işletme sermayesi`

### 7. Крупный Инвест-Раунд (Consumable)
* **ID артикула:** `codetap_money_1m`
* **Тип:** Расходуемый (многоразовый)
* **Цена:** `300 Янов`
* **Название (RU):** `Крупный инвест-раунд (1,000,000 ₽)`
* **Название (EN):** `Mega Investment Round (1,000,000 ₽)`
* **Название (TR):** `Büyük Yatırım Turu (1,000,000 ₽)`
* **Описание (RU):** `Огромный финансовый капитал (1,000,000 ₽) для мгновенного разгона капитализации`
* **Описание (EN):** `Substantial financial capital (1,000,000 ₽) to rocket your market cap`
* **Описание (TR):** `Piyasa değerini hızla uçuracak devasa finansal sermaye (1,000,000 ₽)`

---

## 5. Настройка Лидерборда (Leaderboards)

В Консоли перейдите в раздел **«Таблицы лидеров»** -> **«Добавить таблицу»**:

* **Техническое имя:** `codetap_score`
* **Название таблицы (RU):** `Скомпилировано строк кода`
* **Название таблицы (EN):** `Compiled Code Lines`
* **Название таблицы (TR):** `Derlenen Kod Satırları`
* **Тип:** `Числовой` (Numeric)
* **Сортировка:** `По убыванию` (Descending — чем больше очков, тем выше место)
* **Десятичные знаки:** `0`

---

## 6. Настройка Рекламы и RTB-блоков

1. **Реклама за вознаграждение (Rewarded Video):**
   * Буст x2 на 3 минуты в окне Hub.
   * Сброс кулдауна Time Warp.
   * Удвоение оффлайн-прибыли при входе.
   * Игроки могут стакать x2 буст с вечным множителем x2 до суммарного x4.
2. **Полноэкранная реклама (Interstitial):**
   * Показывается при выходе на IPO (престиж), с интервалом не чаще 1 раза в 180 секунд.
   * Автоматически отключается при покупке No-Ads Pass.
3. **Липкий баннер (Sticky Banner):**
   * Подключен вызов `showStickyBanner()`. Автоматически скрывается при покупке No-Ads Pass.

---

## 7. Чек-лист проверки перед отправкой на модерацию

- [x] Звук останавливается при переключении вкладки или сворачивании браузера (`visibilitychange`).
- [x] Вызов `ysdk.features.LoadingAPI.ready()` выполняется сразу после монтирования приложения.
- [x] Облачные сохранения привязаны к профилю игрока Яндекс Игр.
- [x] Отсутствуют внешние неразрешенные ссылки и социальные сети.
- [x] Все кнопки адаптированы под мобильные тач-экраны и мышь.
