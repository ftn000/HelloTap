/**
 * Интеграция Yandex Games SDK v2 для HelloTap
 * Поддерживает:
 * - Рекламу за вознаграждение (Rewarded Video) для бустов и Time Warp
 * - Полноэкранную рекламу (Interstitial)
 * - Облачные сохранения в профиль игрока Яндекс (Cloud Save)
 * - Лидерборды Яндекс Игр (рейтинг по строкам кода)
 * - Безопасную работу в локальном браузере без SDK (mock fallback)
 */

import { yandexPayments } from './yandexPayments';

interface YandexPlayer {
  getData: (keys?: string[]) => Promise<Record<string, unknown>>;
  setData: (data: Record<string, unknown>, flush?: boolean) => Promise<void>;
  getName: () => string;
  getPhoto: (size: 'small' | 'medium' | 'large') => string;
  getUniqueID: () => string;
}

export interface LeaderboardEntry {
  rank: number;
  name: string;
  score: number;
  photo?: string;
  isPlayer?: boolean;
}

interface YandexLeaderboard {
  setLeaderboardScore: (leaderboardName: string, score: number) => Promise<void>;
  getLeaderboardPlayerEntry: (leaderboardName: string) => Promise<unknown>;
  getLeaderboardEntries?: (
    leaderboardName: string,
    options?: {
      includeUser?: boolean;
      quantityAround?: number;
      quantityTop?: number;
      avatarSize?: 'small' | 'medium' | 'large';
    }
  ) => Promise<{
    entries: Array<{
      score: number;
      rank: number;
      player: {
        getAvatarSrc: (size: string) => string;
        getName: () => string;
        uniqueID: string;
      };
    }>;
    userRank?: number;
  }>;
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
    getBannerAdvStatus?: () => Promise<{ stickyAdvIsShowing: boolean; reason?: string }>;
    showBannerAdv?: () => Promise<{ stickyAdvIsShowing: boolean; reason?: string }>;
    hideBannerAdv?: () => Promise<void>;
  };
  feedback?: {
    canReview: () => Promise<{ value: boolean; reason?: string }>;
    requestReview: () => Promise<{ feedbackSent: boolean }>;
  };
  shortcut?: {
    canShowPrompt: () => Promise<{ canShow: boolean }>;
    showPrompt: () => Promise<{ outcome: 'accepted' | 'dismissed' }>;
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

        try {
          // Инициализируем сервис внутриигровых покупок
          await yandexPayments.init(this.ysdk);
        } catch (e) {
          console.warn("[YandexSDK] Could not init payments:", e);
        }

        console.log("[YandexSDK] Initialized successfully");
        return true;
      }
    } catch (err) {
      console.warn("[YandexSDK] Initialization skipped (running outside Yandex):", err);
    }
    return false;
  }

  /**
   * Проверка возможности оставить отзыв об игре
   */
  public async canReview(): Promise<boolean> {
    if (!this.ysdk?.feedback) return false;
    try {
      const res = await this.ysdk.feedback.canReview();
      return !!res.value;
    } catch {
      return false;
    }
  }

  /**
   * Запрос нативного попапа оценки игры в каталоге Яндекс Игр
   */
  public async requestReview(): Promise<boolean> {
    if (!this.ysdk?.feedback) return false;
    try {
      const res = await this.ysdk.feedback.requestReview();
      return !!res.feedbackSent;
    } catch {
      return false;
    }
  }

  /**
   * Проверка возможности добавления ярлыка игры на рабочий стол
   */
  public async canShowShortcut(): Promise<boolean> {
    if (!this.ysdk?.shortcut) return false;
    try {
      const res = await this.ysdk.shortcut.canShowPrompt();
      return !!res.canShow;
    } catch {
      return false;
    }
  }

  /**
   * Диалог добавления ярлыка на рабочий стол
   */
  public async showShortcut(): Promise<boolean> {
    if (!this.ysdk?.shortcut) return false;
    try {
      const res = await this.ysdk.shortcut.showPrompt();
      return res.outcome === 'accepted';
    } catch {
      return false;
    }
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
   * Показать адаптивный RTB Sticky-баннер Яндекс Игр
   */
  public async showStickyBanner(): Promise<boolean> {
    if (!this.ysdk || !this.ysdk.adv?.showBannerAdv) return false;
    try {
      const res = await this.ysdk.adv.showBannerAdv();
      return !!res?.stickyAdvIsShowing;
    } catch (err) {
      console.warn("[YandexSDK] Sticky banner show error:", err);
      return false;
    }
  }

  /**
   * Скрыть RTB Sticky-баннер (например, при покупке No-Ads)
   */
  public async hideStickyBanner(): Promise<boolean> {
    if (!this.ysdk || !this.ysdk.adv?.hideBannerAdv) return false;
    try {
      await this.ysdk.adv.hideBannerAdv();
      return true;
    } catch (err) {
      console.warn("[YandexSDK] Sticky banner hide error:", err);
      return false;
    }
  }

  /**
   * Отправка счета в лидерборд Яндекс Игр
   */
  public async submitLeaderboardScore(score: number): Promise<void> {
    if (!this.ysdk) return;
    try {
      const lb = await this.ysdk.getLeaderboards();
      const s = Math.floor(score);
      // Записываем в основной лидерборд CodeTap и legacy HelloTap
      await Promise.allSettled([
        lb.setLeaderboardScore("codetap_score", s),
        lb.setLeaderboardScore("HelloTapCodeLines", s)
      ]);
    } catch (err) {
      console.warn("[YandexSDK] Leaderboard score submit error:", err);
    }
  }

  /**
   * Получение топа игроков из лидерборда
   */
  public async getLeaderboardEntries(limit: number = 10, currentScore: number = 0): Promise<LeaderboardEntry[]> {
    if (this.ysdk) {
      try {
        const lb = await this.ysdk.getLeaderboards();
        if (lb.getLeaderboardEntries) {
          const res = await lb.getLeaderboardEntries("codetap_score", {
            quantityTop: limit,
            includeUser: true
          });

          if (res && res.entries && res.entries.length > 0) {
            const playerUid = this.player ? this.player.getUniqueID() : null;
            return res.entries.map(e => ({
              rank: e.rank,
              name: e.player.getName() || "Anonymous Coder",
              score: e.score,
              photo: e.player.getAvatarSrc ? e.player.getAvatarSrc('small') : undefined,
              isPlayer: playerUid ? e.player.uniqueID === playerUid : false
            }));
          }
        }
      } catch (err) {
        console.warn("[YandexSDK] Could not fetch Yandex leaderboards, using local fallback:", err);
      }
    }

    // Fallback / Mock Leaderboard для автономной игры и локального тестирования
    const mockCoders = [
      { name: "Linus_Kernel", score: 25000000 },
      { name: "Satoshi_N", score: 18500000 },
      { name: "Carmack_Doom", score: 12400000 },
      { name: "Guido_Python", score: 8900000 },
      { name: "Anders_TS", score: 5600000 },
      { name: "Wozniak_Apple", score: 3200000 },
      { name: "Ada_Lovelace", score: 1800000 },
      { name: "Dennis_Ritchie", score: 950000 },
      { name: "Turing_Enigma", score: 450000 },
      { name: "CyberNinja_99", score: 120000 }
    ];

    const currentName = this.player ? this.player.getName() : "Вы (Player)";
    const list = [...mockCoders, { name: currentName, score: Math.floor(currentScore), isPlayer: true }];
    list.sort((a, b) => b.score - a.score);

    return list.slice(0, limit).map((entry, index) => ({
      rank: index + 1,
      name: entry.name,
      score: entry.score,
      isPlayer: !!(entry as { isPlayer?: boolean }).isPlayer
    }));
  }
}

export const yandexSdk = new YandexGamesService();
