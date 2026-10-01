/**
 * Web Audio API синтезатор звуковых эффектов для HelloTap
 * Нулевой вес файлов, отсутствие лагов загрузки и мгновенный отклик при клике
 */
class SoundEngine {
  private ctx: AudioContext | null = null;
  public isMuted: boolean = false;
  private isTabVisible: boolean = true;

  constructor() {
    if (typeof window !== 'undefined' && typeof document !== 'undefined') {
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

  private getContext(): AudioContext | null {
    if (this.isMuted || !this.isTabVisible) return null;
    if (!this.ctx) {
      const AudioCtx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      if (AudioCtx) {
        this.ctx = new AudioCtx();
      }
    }
    if (this.ctx && this.ctx.state === 'suspended' && this.isTabVisible) {
      this.ctx.resume();
    }
    return this.ctx;
  }

  public switchType: 'blue' | 'red' | 'brown' | 'laser' | 'typewriter' = 'blue';

  public playKeyClick(isCrit: boolean = false): void {
    const ctx = this.getContext();
    if (!ctx) return;

    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    let baseFreq = 420;
    let waveType: OscillatorType = 'sine';
    let duration = 0.04;

    switch (this.switchType) {
      case 'blue': // Clicky & Bright
        baseFreq = isCrit ? 920 : 540 + Math.random() * 80;
        waveType = isCrit ? 'triangle' : 'square';
        duration = isCrit ? 0.12 : 0.035;
        break;
      case 'red': // Linear & Soft
        baseFreq = isCrit ? 700 : 260 + Math.random() * 40;
        waveType = 'sine';
        duration = isCrit ? 0.09 : 0.045;
        break;
      case 'brown': // Tactile bump
        baseFreq = isCrit ? 800 : 380 + Math.random() * 60;
        waveType = 'triangle';
        duration = isCrit ? 0.10 : 0.04;
        break;
      case 'laser': // Cyber Synth
        baseFreq = isCrit ? 1200 : 750 + Math.random() * 120;
        waveType = 'sawtooth';
        duration = isCrit ? 0.15 : 0.05;
        break;
      case 'typewriter': // Vintage Mechanical Strike + Ding Bell on Crit
        baseFreq = isCrit ? 2200 : 180 + Math.random() * 50;
        waveType = isCrit ? 'sine' : 'sawtooth';
        duration = isCrit ? 0.40 : 0.025;
        break;
    }

    osc.type = waveType;
    osc.frequency.setValueAtTime(baseFreq, ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(baseFreq * 0.25, ctx.currentTime + duration);

    gain.gain.setValueAtTime(isCrit ? 0.35 : 0.18, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + duration);

    osc.connect(gain);
    gain.connect(ctx.destination);

    osc.start();
    osc.stop(ctx.currentTime + duration);
  }

  public playUpgrade(): void {
    const ctx = this.getContext();
    if (!ctx) return;

    const notes = [440, 554.37, 659.25, 880];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.05;

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.18, startTime);
      gain.gain.exponentialRampToValueAtTime(0.001, startTime + 0.15);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(startTime);
      osc.stop(startTime + 0.15);
    });
  }

  public playRelease(): void {
    const ctx = this.getContext();
    if (!ctx) return;

    const notes = [523.25, 659.25, 783.99, 1046.50];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.07;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.25, startTime);
      gain.gain.exponentialRampToValueAtTime(0.001, startTime + 0.3);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(startTime);
      osc.stop(startTime + 0.3);
    });
  }

  public playPurchaseSuccess(): void {
    const ctx = this.getContext();
    if (!ctx) return;

    // Золотой арпеджио каскад в C Major 9: C5, E5, G5, B5, C6, E6
    const notes = [523.25, 659.25, 783.99, 987.77, 1046.50, 1318.51];
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const startTime = ctx.currentTime + idx * 0.055;
      const duration = 0.35;

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, startTime);

      gain.gain.setValueAtTime(0.22, startTime);
      gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(startTime);
      osc.stop(startTime + duration);
    });
  }

  public playAutoClickTick(): void {
    const ctx = this.getContext();
    if (!ctx) return;

    // Очень мягкий и ультракороткий клик
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    const duration = 0.018;

    osc.type = 'triangle';
    osc.frequency.setValueAtTime(560, ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(180, ctx.currentTime + duration);

    gain.gain.setValueAtTime(0.035, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + duration);

    osc.connect(gain);
    gain.connect(ctx.destination);

    osc.start();
    osc.stop(ctx.currentTime + duration);
  }

  public triggerHaptic(type: 'light' | 'medium' | 'heavy' | 'success' = 'light'): void {
    // 1. Проверяем Telegram WebApp HapticFeedback
    const tg = (window as unknown as { Telegram?: { WebApp?: { HapticFeedback?: { impactOccurred: (s: string) => void; notificationOccurred: (s: string) => void } } } }).Telegram?.WebApp;
    if (tg && tg.HapticFeedback) {
      if (type === 'success') {
        tg.HapticFeedback.notificationOccurred('success');
      } else {
        tg.HapticFeedback.impactOccurred(type);
      }
      return;
    }

    // 2. Стандартный Web Vibration API
    if (navigator.vibrate) {
      if (type === 'light') navigator.vibrate(10);
      else if (type === 'medium') navigator.vibrate(25);
      else if (type === 'heavy') navigator.vibrate(45);
      else if (type === 'success') navigator.vibrate([15, 30, 20]);
    }
  }
}

export const sounds = new SoundEngine();
