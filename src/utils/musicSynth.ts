/**
 * Генеративный Lo-Fi / Synthwave Web Audio Синтезатор для CodeTap
 * 100% процедурный звук с нулевым весом файлов, теплым аналоговым фильтром,
 * lo-fi виниловым шумом и соблюдением политики автопаузы Яндекс Игр.
 */

// Ноты и частоты
const NOTE_FREQS: Record<string, number> = {
  'A1': 55.00, 'C2': 65.41, 'D2': 73.42, 'E2': 82.41, 'F2': 87.31, 'G2': 98.00,
  'A2': 110.00, 'B2': 123.47, 'C3': 130.81, 'D3': 146.83, 'E3': 164.81, 'F3': 174.61, 'G3': 196.00,
  'A3': 220.00, 'B3': 246.94, 'C4': 261.63, 'D4': 293.66, 'E4': 329.63, 'F4': 349.23, 'G4': 392.00,
  'A4': 440.00, 'B4': 493.88, 'C5': 523.25, 'D5': 587.33, 'E5': 659.25, 'G5': 783.99
};

// 4-тактовая Lo-Fi / Synthwave гармония
export interface ChordProgression {
  bass: string;
  pad: string[];
  pluckNotes: string[];
}

export interface SynthTrack {
  id: string;
  name: string;
  genre: string;
  bpm: number;
  filterCutoff: number;
  stepDuration: number;
  progression: ChordProgression[];
}

export const SYNTH_TRACKS: SynthTrack[] = [
  // 1. Lo-Fi Coder Beats: Глубокое погружение в программирование
  {
    id: 'lofi_beats',
    name: 'Lo-Fi Coder Beats',
    genre: 'Generative Lo-Fi Ambient',
    bpm: 72,
    filterCutoff: 680,
    stepDuration: 3.6,
    progression: [
      { bass: 'D2', pad: ['D3', 'F3', 'A3', 'C4', 'E4'], pluckNotes: ['A4', 'C5', 'E5', 'G5', 'E5', 'C5'] },
      { bass: 'G1', pad: ['G2', 'B2', 'F3', 'A3', 'E4'], pluckNotes: ['B4', 'D5', 'E5', 'G5', 'E5', 'D5'] },
      { bass: 'C2', pad: ['C3', 'E3', 'G3', 'B3', 'D4'], pluckNotes: ['G4', 'B4', 'D5', 'E5', 'D5', 'B4'] },
      { bass: 'A1', pad: ['A2', 'C3', 'E3', 'G3', 'B3'], pluckNotes: ['E4', 'G4', 'A4', 'C5', 'B4', 'G4'] }
    ]
  },
  // 2. Chill Lo-Fi: Ночной кодинг
  {
    id: 'midnight',
    name: 'Midnight Coding',
    genre: 'Lo-Fi Chill',
    bpm: 67,
    filterCutoff: 850,
    stepDuration: 3.6,
    progression: [
      { bass: 'A1', pad: ['A2', 'C3', 'E3', 'G3', 'B3'], pluckNotes: ['E4', 'G4', 'B4', 'C5', 'B4', 'G4'] },
      { bass: 'F2', pad: ['F2', 'C3', 'E3', 'A3', 'B3'], pluckNotes: ['A4', 'C5', 'E5', 'B4', 'A4', 'E4'] },
      { bass: 'C2', pad: ['C2', 'G2', 'D3', 'E3', 'B3'], pluckNotes: ['G4', 'B4', 'D5', 'E5', 'D5', 'B4'] },
      { bass: 'E2', pad: ['E2', 'B2', 'D3', 'G3', 'B3'], pluckNotes: ['E4', 'G4', 'B4', 'D5', 'B4', 'G4'] }
    ]
  },
  // 3. Synthwave: Неоновый киберпанк
  {
    id: 'cyberpunk',
    name: 'Neon Cyberpunk Rain',
    genre: 'Synthwave',
    bpm: 78,
    filterCutoff: 1250,
    stepDuration: 3.0,
    progression: [
      { bass: 'D2', pad: ['D3', 'F3', 'A3', 'C4'], pluckNotes: ['A4', 'C5', 'D5', 'F5', 'D5', 'A4'] },
      { bass: 'B2', pad: ['B2', 'D3', 'F3', 'A3'], pluckNotes: ['F4', 'A4', 'C5', 'D5', 'C5', 'A4'] },
      { bass: 'G2', pad: ['G2', 'B2', 'D3', 'F3'], pluckNotes: ['D4', 'G4', 'B4', 'D5', 'B4', 'G4'] },
      { bass: 'A2', pad: ['A2', 'C3', 'E3', 'G3'], pluckNotes: ['E4', 'A4', 'C5', 'E5', 'C5', 'A4'] }
    ]
  },
  // 4. Warm Acoustic: Утренний кофе и чистый код
  {
    id: 'coffee',
    name: 'Coffee & Clean Code',
    genre: 'Warm Acoustic',
    bpm: 60,
    filterCutoff: 720,
    stepDuration: 4.0,
    progression: [
      { bass: 'C2', pad: ['C3', 'E3', 'G3', 'B3'], pluckNotes: ['E4', 'G4', 'B4', 'D5', 'B4', 'G4'] },
      { bass: 'A1', pad: ['A2', 'C3', 'E3', 'G3'], pluckNotes: ['C4', 'E4', 'G4', 'C5', 'G4', 'E4'] },
      { bass: 'F2', pad: ['F2', 'A2', 'C3', 'E3'], pluckNotes: ['A4', 'C5', 'E5', 'G4', 'E4', 'C4'] },
      { bass: 'G2', pad: ['G2', 'B2', 'D3', 'F3'], pluckNotes: ['B4', 'D5', 'F4', 'G4', 'D4', 'B3'] }
    ]
  }
];

class LoFiSynthwaveEngine {
  private ctx: AudioContext | null = null;
  private masterGain: GainNode | null = null;
  private filterNode: BiquadFilterNode | null = null;
  private isPlaying: boolean = false;
  private loopInterval: number | null = null;
  private currentStep: number = 0;
  private isTabVisible: boolean = true;
  private userMuted: boolean = false;
  public currentTrackIndex: number = 0;
  public musicVolume: number = 0.35;

  constructor() {
    if (typeof window !== 'undefined' && typeof document !== 'undefined') {
      try {
        const saved = localStorage.getItem("CODETAP_MUSIC_MUTED");
        if (saved === 'true') {
          this.userMuted = true;
        }
        const savedVol = localStorage.getItem("CODETAP_MUSIC_VOLUME");
        if (savedVol !== null) {
          const parsed = parseFloat(savedVol);
          if (!isNaN(parsed)) this.musicVolume = Math.max(0, Math.min(1, parsed));
        }
      } catch {}

      // Автопауза при неактивной вкладке (Yandex Games Compliance)
      document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
          this.isTabVisible = false;
          if (this.ctx && this.ctx.state === 'running') {
            this.ctx.suspend();
          }
        } else {
          this.isTabVisible = true;
          if (this.ctx && this.ctx.state === 'suspended' && this.isPlaying && !this.userMuted) {
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
        if (this.ctx && this.ctx.state === 'suspended' && this.isPlaying && !this.userMuted && this.isTabVisible) {
          this.ctx.resume();
        }
      });
    }
  }

  private initAudio(): boolean {
    if (!this.ctx) {
      const AudioCtx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      if (!AudioCtx) return false;
      this.ctx = new AudioCtx();

      // Master Gain
      this.masterGain = this.ctx.createGain();
      this.masterGain.gain.setValueAtTime(0.001, this.ctx.currentTime);

      // Теплый Low-Pass фильтр (Lo-Fi характер: срез около 850 Гц)
      this.filterNode = this.ctx.createBiquadFilter();
      this.filterNode.type = 'lowpass';
      this.filterNode.frequency.setValueAtTime(850, this.ctx.currentTime);
      this.filterNode.Q.setValueAtTime(2.2, this.ctx.currentTime);

      this.filterNode.connect(this.masterGain);
      this.masterGain.connect(this.ctx.destination);
    }
    return true;
  }

  // Играем один аккордовый такт
  private playChordStep(step: number): void {
    if (!this.ctx || !this.filterNode || this.ctx.state !== 'running') return;
    const track = SYNTH_TRACKS[this.currentTrackIndex % SYNTH_TRACKS.length];
    const chord = track.progression[step % track.progression.length];
    const now = this.ctx.currentTime;
    const stepDuration = track.stepDuration;

    // Регулируем частоту среза фильтра под трек
    this.filterNode.frequency.setValueAtTime(track.filterCutoff, now);

    // 1. Теплый аналоговый пэд (Pad)
    chord.pad.forEach((noteName) => {
      if (!this.ctx || !this.filterNode) return;
      const freq = NOTE_FREQS[noteName];
      if (!freq) return;

      const osc = this.ctx.createOscillator();
      const oscDetune = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(freq, now);

      // Детюн для насыщенного стереохоруса
      oscDetune.type = 'triangle';
      oscDetune.frequency.setValueAtTime(freq * 1.002, now);

      gain.gain.setValueAtTime(0.001, now);
      gain.gain.linearRampToValueAtTime(0.035, now + 0.9);
      gain.gain.setValueAtTime(0.035, now + stepDuration - 0.7);
      gain.gain.linearRampToValueAtTime(0.001, now + stepDuration);

      osc.connect(gain);
      oscDetune.connect(gain);
      gain.connect(this.filterNode);

      osc.start(now);
      oscDetune.start(now);
      osc.stop(now + stepDuration);
      oscDetune.stop(now + stepDuration);
    });

    // 2. Глубокий аналоговый бас (Sub Bass)
    const bassFreq = NOTE_FREQS[chord.bass];
    if (bassFreq) {
      const bassOsc = this.ctx.createOscillator();
      const bassGain = this.ctx.createGain();

      bassOsc.type = 'sine';
      bassOsc.frequency.setValueAtTime(bassFreq, now);

      bassGain.gain.setValueAtTime(0.001, now);
      bassGain.gain.linearRampToValueAtTime(0.08, now + 0.2);
      bassGain.gain.exponentialRampToValueAtTime(0.001, now + stepDuration - 0.2);

      bassOsc.connect(bassGain);
      bassGain.connect(this.filterNode);

      bassOsc.start(now);
      bassOsc.stop(now + stepDuration);
    }

    // 3. Деликатные Lo-Fi синтезаторные плаки (Arp Plucks)
    const pluckInterval = (stepDuration - 0.8) / Math.max(1, chord.pluckNotes.length);
    chord.pluckNotes.forEach((pluckNote, pIdx) => {
      if (!this.ctx || !this.filterNode) return;
      const pluckFreq = NOTE_FREQS[pluckNote];
      if (!pluckFreq) return;

      const pluckTime = now + 0.35 + pIdx * pluckInterval;
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(pluckFreq, pluckTime);

      gain.gain.setValueAtTime(0.001, pluckTime);
      gain.gain.linearRampToValueAtTime(0.022, pluckTime + 0.04);
      gain.gain.exponentialRampToValueAtTime(0.0001, pluckTime + 0.45);

      osc.connect(gain);
      gain.connect(this.filterNode);

      osc.start(pluckTime);
      osc.stop(pluckTime + 0.5);
    });

    // 4. Мягкий Lo-Fi ритм для Coder Beats (теплый саб-кик и деликатный виниловый щелчок)
    if (track.id === 'lofi_beats') {
      // Суб-кик на сильную долю (низкий бархатный бас-толчок)
      const kickOsc = this.ctx.createOscillator();
      const kickGain = this.ctx.createGain();
      kickOsc.type = 'sine';
      kickOsc.frequency.setValueAtTime(85, now);
      kickOsc.frequency.exponentialRampToValueAtTime(38, now + 0.16);

      kickGain.gain.setValueAtTime(0.001, now);
      kickGain.gain.linearRampToValueAtTime(0.04, now + 0.015);
      kickGain.gain.exponentialRampToValueAtTime(0.0001, now + 0.22);

      kickOsc.connect(kickGain);
      kickGain.connect(this.filterNode);
      kickOsc.start(now);
      kickOsc.stop(now + 0.25);

      // Теплый мягкий снейр/щелчок в середине такта
      const snareTime = now + stepDuration * 0.5;
      const snareOsc = this.ctx.createOscillator();
      const snareGain = this.ctx.createGain();
      snareOsc.type = 'triangle';
      snareOsc.frequency.setValueAtTime(260, snareTime);
      snareOsc.frequency.exponentialRampToValueAtTime(130, snareTime + 0.08);

      snareGain.gain.setValueAtTime(0.001, snareTime);
      snareGain.gain.linearRampToValueAtTime(0.022, snareTime + 0.01);
      snareGain.gain.exponentialRampToValueAtTime(0.0001, snareTime + 0.12);

      snareOsc.connect(snareGain);
      snareGain.connect(this.filterNode);
      snareOsc.start(snareTime);
      snareOsc.stop(snareTime + 0.15);
    }
  }

  public setVolume(vol: number): void {
    this.musicVolume = Math.max(0, Math.min(1, vol));
    try {
      localStorage.setItem("CODETAP_MUSIC_VOLUME", this.musicVolume.toString());
    } catch {}
    if (this.masterGain && this.ctx && this.isPlaying) {
      this.masterGain.gain.setValueAtTime(this.musicVolume * 0.4, this.ctx.currentTime);
    }
  }

  public getVolume(): number {
    return this.musicVolume;
  }

  public start(): boolean {
    if (this.userMuted) return false;
    if (!this.initAudio()) return false;
    if (this.isPlaying) return true;

    if (this.ctx && this.ctx.state === 'suspended') {
      this.ctx.resume();
    }

    this.isPlaying = true;
    if (this.masterGain && this.ctx) {
      const targetGain = this.musicVolume * 0.4;
      this.masterGain.gain.linearRampToValueAtTime(targetGain, this.ctx.currentTime + 1.2);
    }

    const track = SYNTH_TRACKS[this.currentTrackIndex % SYNTH_TRACKS.length];
    this.playChordStep(this.currentStep);
    this.currentStep++;

    this.loopInterval = window.setInterval(() => {
      this.playChordStep(this.currentStep);
      this.currentStep++;
    }, Math.round(track.stepDuration * 1000));

    return true;
  }

  public setTrack(index: number): string {
    this.currentTrackIndex = (index + SYNTH_TRACKS.length) % SYNTH_TRACKS.length;
    this.currentStep = 0;
    if (this.isPlaying) {
      this.stop();
      this.start();
    }
    return this.getTrackName();
  }

  public nextTrack(): string {
    return this.setTrack(this.currentTrackIndex + 1);
  }

  public getTrackName(): string {
    const track = SYNTH_TRACKS[this.currentTrackIndex % SYNTH_TRACKS.length];
    return `${track.name} (${track.genre})`;
  }

  public stop(): void {
    if (!this.isPlaying) return;
    this.isPlaying = false;

    if (this.loopInterval) {
      clearInterval(this.loopInterval);
      this.loopInterval = null;
    }

    if (this.masterGain && this.ctx) {
      this.masterGain.gain.linearRampToValueAtTime(0.001, this.ctx.currentTime + 0.8);
    }
  }

  public toggle(): boolean {
    if (this.isPlaying) {
      this.userMuted = true;
      try {
        localStorage.setItem("CODETAP_MUSIC_MUTED", "true");
      } catch {}
      this.stop();
      return false;
    } else {
      this.userMuted = false;
      try {
        localStorage.setItem("CODETAP_MUSIC_MUTED", "false");
      } catch {}
      return this.start();
    }
  }

  public getIsPlaying(): boolean {
    return this.isPlaying;
  }

  public getIsUserMuted(): boolean {
    return this.userMuted;
  }
}

export const musicSynth = new LoFiSynthwaveEngine();
