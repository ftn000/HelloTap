/**
 * Модуль интеграции внутриигровых покупок (In-App Purchases) Яндекс Игр для CodeTap
 * Документация Yandex Games Payments: https://yandex.ru/dev/games/doc/ru/sdk/sdk-purchases
 */

export interface InAppProduct {
  id: string;
  titleRu: string;
  titleEn: string;
  titleTr: string;
  descRu: string;
  descEn: string;
  descTr: string;
  priceYans: number;
  icon: string;
  isConsumable: boolean;
}

export const IN_APP_PRODUCTS: InAppProduct[] = [
  {
    id: "codetap_vip_x2",
    titleRu: "Вечный Множитель x2",
    titleEn: "Permanent x2 Multiplier",
    titleTr: "Kalıcı x2 Çarpanı",
    descRu: "Удваивает весь клик, пассивный C# и доход студии навсегда (стакается с рекламой до x4)",
    descEn: "Doubles all C# production and money income forever (stacks with ad boost to x4)",
    descTr: "Tüm C# üretimini ve stüdyo gelirini kalıcı olarak ikiye katlar (reklamla x4 olur)",
    priceYans: 129,
    icon: "👑",
    isConsumable: false
  },
  {
    id: "codetap_autoclicker",
    titleRu: "Авто-Кликер Bot Pro (10 CPS)",
    titleEn: "Auto-Clicker Bot Pro (10 CPS)",
    titleTr: "Otomatik Tıklayıcı Bot Pro (10 CPS)",
    descRu: "Кликает автоматически 10 раз в секунду без участия игрока",
    descEn: "Clicks automatically 10 times per second in the background",
    descTr: "Oyuncunun müdahalesi olmadan saniyede 10 kez otomatik tıklar",
    priceYans: 149,
    icon: "⚡",
    isConsumable: false
  },
  {
    id: "codetap_noads",
    titleRu: "Отключение Рекламы (No-Ads Pass)",
    titleEn: "No-Ads Pass",
    titleTr: "Reklamsız Geçiş (No-Ads Pass)",
    descRu: "Полностью отключает баннеры и полноэкранную рекламу при IPO",
    descEn: "Completely disables sticky banners and interstitial ads on IPO",
    descTr: "Sabit banner'ları ve halka arz geçiş reklamlarını tamamen devre dışı bırakır",
    priceYans: 149,
    icon: "🚫",
    isConsumable: false
  },
  {
    id: "codetap_stocks_25",
    titleRu: "Венчурный Грант (25 Токенов)",
    titleEn: "Venture Grant (25 Tokens)",
    titleTr: "Girişim Hibesi (25 Jeton)",
    descRu: "+25 Токенов Акций для мощного старта и прокачки ключевых перков",
    descEn: "+25 IPO Stock Tokens for a rapid boost and key talent unlocks",
    descTr: "Hızlı bir başlangıç ve temel yetenekler için +25 Halka Arz Jetonu",
    priceYans: 79,
    icon: "📜",
    isConsumable: true
  },
  {
    id: "codetap_stocks_100",
    titleRu: "Инвест-Пакет Серия А (100 Токенов)",
    titleEn: "Series A Package (100 Tokens)",
    titleTr: "Seri A Paketi (100 Jeton)",
    descRu: "+100 Токенов Акций и колоссальный буст +500% ко всей студии",
    descEn: "+100 IPO Stock Tokens and massive permanent +500% studio boost",
    descTr: "+100 Halka Arz Jetonu ve tüm stüdyoya devasa kalıcı +%500 takviye",
    priceYans: 250,
    icon: "📈",
    isConsumable: true
  },
  {
    id: "codetap_money_100k",
    titleRu: "Чемодан Инвестора (100,000 ₽)",
    titleEn: "Investor Briefcase (100,000 ₽)",
    titleTr: "Yatırımcı Çantası (100.000 ₽)",
    descRu: "Стартовые оборотные средства для закупки первого парка серверов",
    descEn: "Initial working capital to purchase your first server racks",
    descTr: "İlk sunucu parkınızı satın almak için başlangıç işletme sermayesi",
    priceYans: 49,
    icon: "💼",
    isConsumable: true
  },
  {
    id: "codetap_money_1m",
    titleRu: "Крупный Инвест-Раунд (1,000,000 ₽)",
    titleEn: "Major Investment Round (1,000,000 ₽)",
    titleTr: "Büyük Yatırım Turu (1.000.000 ₽)",
    descRu: "Крупный капитал для масштабирования в топ дата-центры и ИИ-кластеры",
    descEn: "Major capital funding to scale into top datacenters and AI clusters",
    descTr: "En iyi veri merkezlerine ve yapay zeka kümelerine ölçeklenmek için büyük sermaye",
    priceYans: 300,
    icon: "🏦",
    isConsumable: true
  }
];

interface YandexPurchase {
  productID: string;
  purchaseToken: string;
}

interface YandexPaymentsService {
  getPurchases: () => Promise<YandexPurchase[]>;
  purchase: (options: { id: string }) => Promise<YandexPurchase>;
  consumePurchase: (purchaseToken: string) => Promise<void>;
}

class PaymentsManager {
  private payments: YandexPaymentsService | null = null;
  private isInitialized: boolean = false;

  public async init(ysdkInstance?: unknown): Promise<boolean> {
    if (this.isInitialized) return true;

    try {
      const ysdk = ysdkInstance || (window as unknown as { ysdk?: { getPayments: (opt: { signed?: boolean }) => Promise<YandexPaymentsService> } }).ysdk;
      if (ysdk && typeof (ysdk as { getPayments?: unknown }).getPayments === 'function') {
        this.payments = await (ysdk as { getPayments: (opt: { signed?: boolean }) => Promise<YandexPaymentsService> }).getPayments({ signed: true });
        this.isInitialized = true;
        console.log("[YandexPayments] Payments service initialized");
        return true;
      }
    } catch (err) {
      console.warn("[YandexPayments] Payment service unavailable, running mock mode:", err);
    }
    return false;
  }

  /**
   * Запрос списка уже совершенных покупок игрока
   */
  public async getActivePurchases(): Promise<string[]> {
    if (!this.payments) {
      // Mock: читаем из localStorage
      try {
        const raw = localStorage.getItem("CODETAP_MOCK_PURCHASES");
        return raw ? JSON.parse(raw) : [];
      } catch {
        return [];
      }
    }

    try {
      const purchases = await this.payments.getPurchases();
      return purchases.map(p => p.productID);
    } catch (err) {
      console.error("[YandexPayments] Failed to get purchases:", err);
      return [];
    }
  }

  /**
   * Покупка товара
   */
  public async buyProduct(productId: string): Promise<{ success: boolean; isConsumable: boolean }> {
    const productDef = IN_APP_PRODUCTS.find(p => p.id === productId);
    const isConsumable = productDef ? productDef.isConsumable : false;

    if (!this.payments) {
      console.log(`[YandexPayments Mock] Purchased ${productId} (Dev/Mock mode)`);
      if (!isConsumable) {
        try {
          const raw = localStorage.getItem("CODETAP_MOCK_PURCHASES");
          const arr: string[] = raw ? JSON.parse(raw) : [];
          if (!arr.includes(productId)) {
            arr.push(productId);
            localStorage.setItem("CODETAP_MOCK_PURCHASES", JSON.stringify(arr));
          }
        } catch {}
      }
      return { success: true, isConsumable };
    }

    try {
      const purchase = await this.payments.purchase({ id: productId });
      if (purchase) {
        if (isConsumable && purchase.purchaseToken) {
          // Расходуемый товар нужно сразу потребить
          await this.payments.consumePurchase(purchase.purchaseToken);
        }
        return { success: true, isConsumable };
      }
    } catch (err) {
      console.warn("[YandexPayments] Purchase canceled or failed:", err);
    }

    return { success: false, isConsumable };
  }
}

export const yandexPayments = new PaymentsManager();
