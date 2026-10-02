import React, { useEffect, useState, useRef } from 'react';
import { GameRandomEvent, GameEventOption } from '../types/events';
import { Clock, Zap, Sparkles } from 'lucide-react';
import confetti from 'canvas-confetti';
import { sounds } from '../utils/soundEffects';

interface RandomEventModalProps {
  event: GameRandomEvent | null;
  onSelectOption: (option: GameEventOption) => void;
  onClose: () => void;
}

export const RandomEventModal: React.FC<RandomEventModalProps> = ({ event, onSelectOption, onClose }) => {
  const [remainingSec, setRemainingSec] = useState<number>(15);
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;
  const lastEventIdRef = useRef<string | null>(null);

  useEffect(() => {
    if (!event) {
      lastEventIdRef.current = null;
      return;
    }

    // Воспроизводим деликатный звук строго один раз при появлении нового события
    if (lastEventIdRef.current !== event.id) {
      lastEventIdRef.current = event.id;
      setRemainingSec(event.timeoutSec);
      sounds.playEventAlert();
    }

    const interval = setInterval(() => {
      setRemainingSec(prev => {
        if (prev <= 1) {
          clearInterval(interval);
          onCloseRef.current();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [event?.id, event?.timeoutSec]);

  if (!event) return null;

  const handleChoose = (opt: GameEventOption) => {
    confetti({
      particleCount: 50,
      spread: 70,
      origin: { y: 0.5 },
      colors: ['#F59E0B', '#10B981', '#06B6D4', '#EC4899']
    });
    sounds.playPurchaseSuccess();
    onSelectOption(opt);
  };

  const progressPercent = (remainingSec / event.timeoutSec) * 100;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div
        className={`relative w-full max-w-lg bg-gradient-to-b ${event.gradient} border ${event.borderColor} rounded-3xl p-5 sm:p-6 shadow-2xl flex flex-col gap-4 font-mono text-slate-100 overflow-hidden`}
        style={{ boxShadow: `0 0 40px ${event.glowColor}` }}
      >
        {/* Анимированный фоновый блик */}
        <div className="absolute -top-24 -right-24 w-48 h-48 bg-white/5 rounded-full blur-3xl pointer-events-none" />

        {/* Верхняя панель и таймер */}
        <div className="flex items-center justify-between border-b border-white/10 pb-3">
          <div className="flex items-center gap-2.5">
            <span className="text-2xl sm:text-3xl">{event.icon}</span>
            <div>
              <h2 className="text-sm sm:text-base font-bold text-white tracking-wide">
                {event.title}
              </h2>
              <p className="text-[11px] text-slate-400 font-sans">
                {event.subtitle}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-1.5 px-3 py-1 rounded-full bg-slate-900/90 border border-white/10 text-xs text-amber-300 font-bold shrink-0">
            <Clock className="w-3.5 h-3.5 text-amber-400 animate-spin" />
            <span>{remainingSec}с</span>
          </div>
        </div>

        {/* Полоска таймера */}
        <div className="w-full h-1.5 bg-slate-950/80 rounded-full overflow-hidden border border-white/10">
          <div
            className="h-full bg-gradient-to-r from-amber-500 via-orange-500 to-red-500 transition-all duration-1000 ease-linear"
            style={{ width: `${progressPercent}%` }}
          />
        </div>

        {/* Описание события */}
        <p className="text-xs sm:text-sm text-slate-300 font-sans leading-relaxed">
          {event.description}
        </p>

        {/* Кнопки выбора опций */}
        <div className="flex flex-col gap-2.5 pt-1">
          {event.options.map(opt => (
            <button
              key={opt.id}
              onClick={() => handleChoose(opt)}
              className="group p-3.5 rounded-2xl bg-slate-900/90 hover:bg-slate-800/95 border border-white/10 hover:border-white/30 text-left transition-all active:scale-98 shadow-md flex items-center justify-between gap-3"
            >
              <div className="space-y-1">
                <div className="text-xs sm:text-sm font-bold text-slate-100 group-hover:text-cyan-300 transition-colors flex items-center gap-1.5">
                  <Zap className="w-3.5 h-3.5 text-amber-400" />
                  <span>{opt.title}</span>
                </div>
                <p className="text-[11px] text-slate-400 font-sans">
                  {opt.description}
                </p>
              </div>

              {opt.badge && (
                <span className={`px-2.5 py-1 rounded-xl text-[10px] font-bold border shrink-0 ${opt.badgeColor || 'bg-cyan-500/20 text-cyan-300 border-cyan-500/30'}`}>
                  {opt.badge}
                </span>
              )}
            </button>
          ))}
        </div>

        {/* Подсказка внизу */}
        <div className="flex items-center justify-between text-[10px] text-slate-500 pt-1">
          <span className="flex items-center gap-1">
            <Sparkles className="w-3 h-3 text-cyan-400" />
            Быстрый выбор дает преимущество студии
          </span>
          <button
            onClick={onClose}
            className="hover:text-slate-300 underline underline-offset-2 transition-colors"
          >
            Пропустить
          </button>
        </div>
      </div>
    </div>
  );
};
