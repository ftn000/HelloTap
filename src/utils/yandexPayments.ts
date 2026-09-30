/**
 * Модуль интеграции внутриигровых покупок (In-App Purchases) Яндекс Игр для CodeTap
 * Документация Yandex Games Payments: https://yandex.ru/dev/games/doc/ru/sdk/sdk-purchases
 */

export interface InAppProduct {
  id: string;
  titleRu: string;
  titleEn: string;
  descRu: string;
  descEn: string;
  priceYans: number;
  icon: string;
  isConsumable: boolean;
}

export const IN_APP_PRODUCTS: InAppProduct[] = [
  {
    id: "codetap_vip_x2",
    titleRu: "Вечный Множитель x2",
    titleEn: "Permanent x2 Multiplier",
    descRu: "Удваивает всю генерацию C# и доход студии навсегда",
    descEn: "Doubles all C# production and money income forever",
    priceYans: 99,
    icon: "👑",
    isConsumable: false
  },
  {
    id: "codetap_autoclicker",
    titleRu: "Авто-Кликер Bot Pro (10 CPS)",
    titleEn: "Auto-Clicker Bot Pro (10 CPS)",
    descRu: "Кликает автоматически 10 раз в секунду без участия игрока",
    descEn: "Clicks automatically 10 times per second in the background",
    priceYans: 149,
    icon: "⚡",
    isConsumable: false
  },
  {
    id: "codetap_noads",
    titleRu: "Отключение Рекламы (No-Ads Pass)",
    titleEn: "No-Ads Pass",
    descRu: "Все рекламные бонусы (x2 буст, Time Warp) мгновенно без просмотра видео",
    descEn: "All ad bonuses (x2 boost, Time Warp) activate instantly with no video",
    priceYans: 199,
    icon: "🚫",
    isConsumable: false
  },
  {
    id: "codetap_stocks_100",
    titleRu: "Пакет 100 Токенов Акций",
    titleEn: "100 Stock Tokens Pack",
    descRu: "Мгновенное начисление 100 токенов акций для взрывного старта",
    descEn: "Instantly grant 100 IPO Stock Tokens for an explosive boost",
    priceYans: 49,
    icon: "📈",
    isConsumable: true
  },
  {
    id: "codetap_money_1m",
    titleRu: "Чемодан Инвестора (1,000,000 ₽)",
    titleEn: "Investor Briefcase (1,000,000 ₽)",
    descRu: "Мгновенные оборотные средства для закупки топового оборудования",
    descEn: "Instant working capital to purchase top-tier studio hardware",
    priceYans: 29,
    icon: "💼",
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
