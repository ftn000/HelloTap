/**
 * Web Audio API синтезатор звуковых эффектов для HelloTap / CodeTap
 * Процедурный аналоговый звук с нулевым весом файлов, встроенным лимитером (компрессором)
 * и фильтром для мягкого, теплого и приятного для ушей звучания без клиппинга и пищания.
 */
export type SoundProfile = 'asmr' | 'classic' | 'cyber' | 'mute';

class SoundEngine {
  private ctx: AudioContext | null = null;
  private masterGain: GainNode | null = null;
  private masterFilter: BiquadFilterNode | null = null;
  private compressor: DynamicsCompressorNode | null = null;
  public isMuted: boolean = false;
  private isTabVisible: boolean = true;
  public soundProfile: SoundProfile = 'asmr';
  public sfxVolume: number = 0.5;

  constructor() {
    if (typeof window !== 'undefined' && typeof document !== 'undefined') {
      try {
        const savedVol = localStorage.getItem("CODETAP_SFX_VOLUME");
        if (savedVol !== null) {
          const parsed = parseFloat(savedVol);
          if (!isNaN(parsed)) this.sfxVolume = Math.max(0, Math.min(1, parsed));
        }
        const savedProf = localStorage.getItem("CODETAP_SOUND_PROFILE") as SoundProfile;
        if (savedProf && ['asmr', 'classic', 'cyber', 'mute'].includes(savedProf)) {
          this.soundProfile = savedProf;
          this.isMuted = savedProf === 'mute';
        }
      } catch {}

      document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
          this.isTabVisible = false;
          if (this.ctx && this.ctx.state === 'running') {
            this.ctx.suspend();
          }
        } else {
          this.isTabVisible = true;
          if (this.ctx && this.ctx.state === 'suspended' && !this.isMuted) {
            this.ctx.resume();
          }
        }
      });

      window.addEventListener('blur', () => {
        if (this.ctx && this.ctx.state === 'running') {
          this.ctx.suspend();
        }
      });

      window.addEventListener('focus', () => {
        if (this.ctx && this.ctx.state === 'suspended' && !this.isMuted && this.isTabVisible) {
          this.ctx.resume();
        }
      });
    }
  }

  private initMasterBus(ctx: AudioContext): void {
    if (this.masterGain && this.compressor && this.masterFilter) return;

    // 1. Мастер-компрессор (лимитер): предотвращает любой клиппинг и перегруз ("бас-буст")
    const comp = ctx.createDynamicsCompressor();
    comp.threshold.setValueAtTime(-10, ctx.currentTime);
    comp.knee.setValueAtTime(12, ctx.currentTime);
    comp.ratio.setValueAtTime(10, ctx.currentTime);
    comp.attack.setValueAtTime(0.003, ctx.currentTime);
    comp.release.setValueAtTime(0.12, ctx.currentTime);
    this.compressor = comp;

    // 2. Мягкий Low-Pass фильтр: срез высоких частот в зависимости от звукового профиля
    const filter = ctx.createBiquadFilter();
    filter.type = 'lowpass';
    filter.frequency.setValueAtTime(this.soundProfile === 'asmr' ? 2200 : this.soundProfile === 'cyber' ? 5200 : 3600, ctx.currentTime);
    filter.Q.setValueAtTime(this.soundProfile === 'cyber' ? 1.4 : 0.7, ctx.currentTime);
    this.masterFilter = filter;

    // 3. Мастер-громкость с учетом SFX громкости и профиля
    const master = ctx.createGain();
    const effectiveGain = this.isMuted ? 0 : this.sfxVolume * 0.45;
    master.gain.setValueAtTime(effectiveGain, ctx.currentTime);
    this.masterGain = master;

    // Маршрутизация: filter -> compressor -> masterGain -> destination
    filter.connect(comp);
    comp.connect(master);
    master.connect(ctx.destination);
  }

  private applyProfileSettings(): void {
    if (!this.ctx) return;
    const now = this.ctx.currentTime;
    if (this.masterFilter) {
      switch (this.soundProfile) {
        case 'asmr':
          this.masterFilter.frequency.setValueAtTime(2200, now);
          this.masterFilter.Q.setValueAtTime(0.5, now);
          break;
        case 'classic':
          this.masterFilter.frequency.setValueAtTime(3600, now);
          this.masterFilter.Q.setValueAtTime(0.7, now);
          break;
        case 'cyber':
          this.masterFilter.frequency.setValueAtTime(5200, now);
          this.masterFilter.Q.setValueAtTime(1.4, now);
          break;
        case 'mute':
          break;
      }
    }
    if (this.masterGain) {
      const effectiveGain = this.isMuted ? 0 : this.sfxVolume * 0.45;
      this.masterGain.gain.setValueAtTime(effectiveGain, now);
    }
  }

  public setVolume(val: number): void {
    this.sfxVolume = Math.max(0, Math.min(1, val));
    try {
      localStorage.setItem("CODETAP_SFX_VOLUME", this.sfxVolume.toString());
    } catch {}
    if (this.masterGain && this.ctx) {
      const effectiveGain = this.isMuted ? 0 : this.sfxVolume * 0.45;
      this.masterGain.gain.setValueAtTime(effectiveGain, this.ctx.currentTime);
    }
  }

  public getVolume(): number {
    return this.sfxVolume;
  }

  public setProfile(profile: SoundProfile): void {
    this.soundProfile = profile;
    this.isMuted = profile === 'mute';
    try {
      localStorage.setItem("CODETAP_SOUND_PROFILE", profile);
    } catch {}
    this.applyProfileSettings();
  }

  public getProfile(): SoundProfile {
    return this.soundProfile;
  }

  /**
   * Приятный хрустальный / стеклянный колокольчик при проке крита или входе в состояние Flow.
   * Физически смоделированный звон хрустального бокала/колокола с чистыми синусоидальными обертонами.
   */
  public playGlassBell(type: 'crit' | 'flow' = 'crit'): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    if (type === 'crit') {
      // Изящный звонкий хрустальный колокольчик (A6 1760 Гц + E7 2637 Гц + кристальный обертон ~3950 Гц)
      const freqs = [1760.0, 2637.0, 3951.0];
      const gains = [0.08, 0.05, 0.025];
      const decay = 0.55;

      freqs.forEach((freq, idx) => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        const startTime = ctx.currentTime;

        osc.type = 'sine';
        osc.frequency.setValueAtTime(freq, startTime);

        gain.gain.setValueAtTime(0.0001, startTime);
        gain.gain.linearRampToValueAtTime(gains[idx], startTime + 0.002);
        gain.gain.exponentialRampToValueAtTime(0.00001, startTime + decay);

        osc.connect(gain);
        gain.connect(dest);

        osc.start(startTime);
        osc.stop(startTime + decay);
      });
    } else {
      // Восходящий кристальный каскад колокольчиков при входе в состояние Flow (C6, E6, G6, C7, E7)
      const notes = [1046.5, 1318.5, 1567.98, 2093.0, 2637.0];
      notes.forEach((freq, idx) => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        const startTime = ctx.currentTime + idx * 0.045;
        const decay = 0.7;

        osc.type = 'sine';
        osc.frequency.setValueAtTime(freq, startTime);

        gain.gain.setValueAtTime(0.0001, startTime);
        gain.gain.linearRampToValueAtTime(0.065, startTime + 0.004);
        gain.gain.exponentialRampToValueAtTime(0.00001, startTime + decay);

        osc.connect(gain);
        gain.connect(dest);

        osc.start(startTime);
        osc.stop(startTime + decay);
      });
    }
  }

  private getContext(): AudioContext | null {
    if (this.isMuted || !this.isTabVisible) return null;
    if (!this.ctx) {
      const AudioCtx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      if (AudioCtx) {
        this.ctx = new AudioCtx();
        this.initMasterBus(this.ctx);
      }
    }
    if (this.ctx && this.ctx.state === 'suspended' && this.isTabVisible) {
      this.ctx.resume();
    }
    if (this.ctx && !this.masterFilter) {
      this.initMasterBus(this.ctx);
    }
    return this.ctx;
  }

  private getMasterNode(): AudioNode | null {
    if (!this.ctx) return null;
    return this.masterFilter || this.ctx.destination;
  }

  public switchType: 'blue' | 'red' | 'brown' | 'laser' | 'typewriter' = 'blue';

  /** Мягкий и приятный клик клавиш (ASMR стиль механической клавиатуры) */
  public playKeyClick(isCrit: boolean = false): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    let baseFreq = 420;
    let waveType: OscillatorType = 'triangle';
    let duration = 0.035;

    switch (this.switchType) {
      case 'blue': // Мягкий тактильный щелчок
        baseFreq = isCrit ? 640 : 420 + Math.random() * 40;
        waveType = 'triangle';
        duration = isCrit ? 0.06 : 0.025;
        break;
      case 'red': // Глухой линейный стук
        baseFreq = isCrit ? 480 : 220 + Math.random() * 25;
        waveType = 'sine';
        duration = isCrit ? 0.05 : 0.03;
        break;
      case 'brown': // Теплый механический отклик
        baseFreq = isCrit ? 560 : 320 + Math.random() * 30;
        waveType = 'triangle';
        duration = isCrit ? 0.055 : 0.028;
        break;
      case 'laser': // Мягкий кибер-синт
        baseFreq = isCrit ? 780 : 520 + Math.random() * 50;
        waveType = 'sine';
        duration = isCrit ? 0.08 : 0.035;
        break;
      case 'typewriter': // Винтажный мягкий стук
        baseFreq = isCrit ? 880 : 260 + Math.random() * 30;
        waveType = 'triangle';
        duration = isCrit ? 0.09 : 0.028;
        break;
    }

    osc.type = waveType;
    osc.frequency.setValueAtTime(baseFreq, ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(baseFreq * 0.6, ctx.currentTime + duration);

    const targetGain = isCrit ? 0.14 : 0.07;
    gain.gain.setValueAtTime(targetGain, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + duration);

    osc.connect(gain);
    gain.connect(dest);

    osc.start();
    osc.stop(ctx.currentTime + duration);
  }

  /** Мягкий диагностический аккорд для исправления линтера / багов */
  public playQuickFix(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    // Теплый колокольчик: D5 -> A5
    const notes = [587.33, 880.0];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.06;
      const duration = 0.22;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.09, startTime + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Награда за апгрейд студии: мягкое восходящее мажорное трезвучие */
  public playUpgrade(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const notes = [440, 554.37, 659.25];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.045;
      const duration = 0.25;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.08, startTime + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Награда за релиз системы или обычный бонус */
  public playRelease(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const notes = [523.25, 659.25, 783.99];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.055;
      const duration = 0.28;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.09, startTime + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Награда за офлайн-прогресс (теплый мягкий аккорд электропиано Fender Rhodes) */
  public playOfflineReward(isDouble: boolean = false): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    // Теплый мажорный аккорд E-Major в среднем регистре: E4, G#4, B4, E5 (без резких пищалок!)
    const notes = isDouble 
      ? [329.63, 415.30, 493.88, 659.25, 987.77] 
      : [329.63, 415.30, 493.88, 659.25];

    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.045;
      const duration = isDouble ? 0.45 : 0.35;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      // Плавная атака без щелчка и мягкий спад
      gain.gain.setValueAtTime(0.0001, startTime);
      gain.gain.linearRampToValueAtTime(0.08, startTime + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Оповещение о редком событии студии (деликатный двойной перезвон, играет строго 1 раз) */
  public playEventAlert(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    // Нежный двойной chime: F5 (698 Hz) -> A5 (880 Hz)
    const notes = [698.46, 880.00];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.08;
      const duration = 0.24;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.0001, startTime);
      gain.gain.linearRampToValueAtTime(0.07, startTime + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Слияние ветки и PR: благородный аккорд F Major 7 */
  public playBranchMerge(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const notes = [349.23, 440.0, 523.25, 659.25];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.04;
      const duration = 0.32;

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.08, startTime + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Успех 10x Blitz: триумфальный мягкий арпеджио */
  public playBlitzSuccess(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const notes = [523.25, 659.25, 783.99, 1046.50];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.05;
      const duration = 0.35;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.09, startTime + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Покупка / Успешная крупная награда */
  public playPurchaseSuccess(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const notes = [523.25, 659.25, 783.99, 1046.50];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.05;
      const duration = 0.32;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.08, startTime + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Ультракороткий мягкий тихий клик автокликера */
  public playAutoClickTick(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    const duration = 0.015;

    osc.type = 'sine';
    osc.frequency.setValueAtTime(440, ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(220, ctx.currentTime + duration);

    gain.gain.setValueAtTime(0.02, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + duration);

    osc.connect(gain);
    gain.connect(dest);

    osc.start();
    osc.stop(ctx.currentTime + duration);
  }

  /** Успех CI/CD пайплайна: мелодичный чистый аккорд */
  public playPipelinePass(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const notes = [523.25, 659.25, 783.99];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.045;
      const duration = 0.22;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.07, startTime + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  /** Сбой в CI/CD: мягкий приглушенный диагностический двойной стук (без резкого жужжания) */
  public playPipelineFail(): void {
    const ctx = this.getContext();
    const dest = this.getMasterNode();
    if (!ctx || !dest) return;

    const notes = [220, 185];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.09;
      const duration = 0.12;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);
      osc.frequency.exponentialRampToValueAtTime(freq * 0.8, startTime + duration);

      gain.gain.setValueAtTime(0.001, startTime);
      gain.gain.linearRampToValueAtTime(0.06, startTime + 0.01);
      gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

      osc.connect(gain);
      gain.connect(dest);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  public triggerHaptic(type: 'light' | 'medium' | 'heavy' | 'success' = 'light'): void {
    const tg = (window as unknown as { Telegram?: { WebApp?: { HapticFeedback?: { impactOccurred: (s: string) => void; notificationOccurred: (s: string) => void } } } }).Telegram?.WebApp;
    if (tg && tg.HapticFeedback) {
      if (type === 'success') {
        tg.HapticFeedback.notificationOccurred('success');
      } else {
        tg.HapticFeedback.impactOccurred(type);
      }
      return;
    }

    if (navigator.vibrate) {
      if (type === 'light') navigator.vibrate(10);
      else if (type === 'medium') navigator.vibrate(20);
      else if (type === 'heavy') navigator.vibrate(35);
      else if (type === 'success') navigator.vibrate([12, 25, 18]);
    }
  }
}

export const sounds = new SoundEngine();
