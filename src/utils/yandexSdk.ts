/**
 * Интеграция Yandex Games SDK v2 для HelloTap
 * Поддерживает:
 * - Рекламу за вознаграждение (Rewarded Video) для бустов и Time Warp
 * - Полноэкранную рекламу (Interstitial)
 * - Облачные сохранения в профиль игрока Яндекс (Cloud Save)
 * - Лидерборды Яндекс Игр (рейтинг по строкам кода)
 * - Безопасную работу в локальном браузере без SDK (mock fallback)
 */

interface YandexPlayer {
  getData: (keys?: string[]) => Promise<Record<string, unknown>>;
  setData: (data: Record<string, unknown>, flush?: boolean) => Promise<void>;
  getName: () => string;
  getPhoto: (size: 'small' | 'medium' | 'large') => string;
  getUniqueID: () => string;
}

interface YandexLeaderboard {
  setLeaderboardScore: (leaderboardName: string, score: number) => Promise<void>;
  getLeaderboardPlayerEntry: (leaderboardName: string) => Promise<unknown>;
}

interface YandexSDK {
  features: {
    LoadingAPI?: {
      ready: () => void;
    };
  };
  adv: {
    showFullscreenAdv: (callbacks: {
      onOpen?: () => void;
      onClose?: (wasShown: boolean) => void;
      onError?: (error: unknown) => void;
    }) => void;
    showRewardedVideo: (callbacks: {
      onOpen?: () => void;
      onRewarded?: () => void;
      onClose?: () => void;
      onError?: (error: unknown) => void;
    }) => void;
  };
  getPlayer: (options?: { scopes?: boolean }) => Promise<YandexPlayer>;
  getLeaderboards: () => Promise<YandexLeaderboard>;
  deviceInfo: {
    isMobile: () => boolean;
    isTablet: () => boolean;
    isDesktop: () => boolean;
  };
}

declare global {
  interface Window {
    YaGames?: {
      init: () => Promise<YandexSDK>;
    };
  }
}

class YandexGamesService {
  private ysdk: YandexSDK | null = null;
  private player: YandexPlayer | null = null;
  public isInitialized: boolean = false;
  private lastInterstitialTime: number = 0;

  public async init(): Promise<boolean> {
    if (this.isInitialized) return true;

    try {
      if (typeof window !== 'undefined' && window.YaGames) {
        this.ysdk = await window.YaGames.init();
        this.isInitialized = true;

        // Сообщаем платформе, что игра загрузилась
        if (this.ysdk.features?.LoadingAPI) {
          this.ysdk.features.LoadingAPI.ready();
        }

        try {
          this.player = await this.ysdk.getPlayer({ scopes: false });
        } catch {
          console.log("[YandexSDK] Guest player mode");
        }

        console.log("[YandexSDK] Initialized successfully");
        return true;
      }
    } catch (err) {
      console.warn("[YandexSDK] Initialization skipped (running outside Yandex):", err);
    }
    return false;
  }

  public isMobile(): boolean {
    return this.ysdk?.deviceInfo.isMobile() ?? /Mobi|Android/i.test(navigator.userAgent);
  }

  /**
   * Показ рекламы за вознаграждение (Rewarded Video)
   */
  public showRewardedVideo(onReward: () => void, onError?: () => void): void {
    if (!this.ysdk) {
      console.log("[YandexSDK Mock] Rewarded video triggered in dev mode");
      // В режиме разработки или вне Яндекса мгновенно выдаем награду
      onReward();
      return;
    }

    try {
      this.ysdk.adv.showRewardedVideo({
        onRewarded: () => {
          console.log("[YandexSDK] Video rewarded!");
          onReward();
        },
        onError: (err) => {
          console.warn("[YandexSDK] Video ad error:", err);
          if (onError) onError();
        }
      });
    } catch (e) {
      console.error("[YandexSDK] showRewardedVideo exception:", e);
      if (onError) onError();
    }
  }

  /**
   * Межстраничная полноэкранная реклама (Interstitial с кулдауном 2 минуты)
   */
  public showInterstitial(onClose?: () => void): void {
    const now = Date.now();
    if (now - this.lastInterstitialTime < 120000) {
      if (onClose) onClose();
      return;
    }

    if (!this.ysdk) {
      if (onClose) onClose();
      return;
    }

    try {
      this.ysdk.adv.showFullscreenAdv({
        onClose: () => {
          this.lastInterstitialTime = Date.now();
          if (onClose) onClose();
        },
        onError: () => {
          if (onClose) onClose();
        }
      });
    } catch {
      if (onClose) onClose();
    }
  }

  /**
   * Облачное сохранение в профиль игрока Яндекс
   */
  public async saveToCloud(dataKey: string, payload: unknown): Promise<boolean> {
    if (!this.player) return false;
    try {
      await this.player.setData({ [dataKey]: payload }, true);
      return true;
    } catch (err) {
      console.warn("[YandexSDK] Cloud save error:", err);
      return false;
    }
  }

  /**
   * Облачная загрузка из профиля Яндекс
   */
  public async loadFromCloud(dataKey: string): Promise<unknown | null> {
    if (!this.player) return null;
    try {
      const data = await this.player.getData([dataKey]);
      return data[dataKey] ?? null;
    } catch (err) {
      console.warn("[YandexSDK] Cloud load error:", err);
      return null;
    }
  }

  /**
   * Отправка счета в лидерборд Яндекс Игр
   */
  public async submitLeaderboardScore(score: number): Promise<void> {
    if (!this.ysdk) return;
    try {
      const lb = await this.ysdk.getLeaderboards();
      await lb.setLeaderboardScore("HelloTapCodeLines", Math.floor(score));
    } catch (err) {
      console.warn("[YandexSDK] Leaderboard score submit error:", err);
    }
  }
}

export const yandexSdk = new YandexGamesService();
