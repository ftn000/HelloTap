import React, { useState } from 'react';
import { useGame } from '../context/GameContext';
import { SKILL_NODES } from '../utils/skillsList';
import { SkillBranch, SkillNode } from '../types/skills';
import { X, GitBranch, Zap, Sparkles, Check, Lock, RotateCcw } from 'lucide-react';
import confetti from 'canvas-confetti';
import { sounds } from '../utils/soundEffects';

interface TechTreeModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const TechTreeModal: React.FC<TechTreeModalProps> = ({ isOpen, onClose }) => {
  const { skillPoints, unlockedSkills, upgradeSkill, resetSkills } = useGame();
  const [activeBranch, setActiveBranch] = useState<SkillBranch | 'all'>('all');

  if (!isOpen) return null;

  const branches: { id: SkillBranch; name: string; icon: string; color: string }[] = [
    { id: 'frontend', name: 'Frontend & UX', icon: '🖥️', color: 'from-pink-500/20 to-purple-500/20 border-pink-500/40 text-pink-300' },
    { id: 'backend', name: 'Backend & Cloud', icon: '⚙️', color: 'from-cyan-500/20 to-blue-500/20 border-cyan-500/40 text-cyan-300' },
    { id: 'devops', name: 'DevOps & CI/CD', icon: '🚀', color: 'from-emerald-500/20 to-teal-500/20 border-emerald-500/40 text-emerald-300' },
    { id: 'business', name: 'Startup Tycoon', icon: '💼', color: 'from-amber-500/20 to-yellow-500/20 border-amber-500/40 text-amber-300' },
    { id: 'ai', name: 'AI & Neural Nets', icon: '🧠', color: 'from-purple-500/20 to-emerald-500/20 border-purple-500/40 text-purple-300' }
  ];

  const filteredNodes = activeBranch === 'all'
    ? SKILL_NODES
    : SKILL_NODES.filter(n => n.branch === activeBranch);

  const handleUpgrade = (node: SkillNode) => {
    const currentLvl = unlockedSkills[node.id] || 0;
    if (currentLvl >= node.maxLevel) return;
    if (skillPoints < node.costPerLevel) return;

    if (node.reqSkillId) {
      const reqLvl = unlockedSkills[node.reqSkillId] || 0;
      if (reqLvl < 1) return;
    }

    const success = upgradeSkill(node.id, node.costPerLevel);
    if (success) {
      confetti({
        particleCount: 25,
        spread: 50,
        origin: { y: 0.6 }
      });
      sounds.playUpgrade();
      sounds.triggerHaptic('success');
    }
  };

  const handleReset = () => {
    if (window.confirm('Сбросить все вложенные очки талантов и вернуть их на баланс?')) {
      resetSkills();
      sounds.playRelease();
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-black/85 backdrop-blur-sm animate-in fade-in duration-150 font-mono">
      <div className="relative w-full max-w-2xl bg-cyber-card border border-cyber-border rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[92vh]">
        {/* Хедер */}
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-cyber-border bg-slate-900/90">
          <div className="flex items-center gap-2.5">
            <GitBranch className="w-5 h-5 text-indigo-400" />
            <div>
              <h2 className="text-sm sm:text-base font-bold text-white tracking-wide">
                ДЕРЕВО IT-НАВЫКОВ И ТАЛАНТОВ
              </h2>
              <p className="text-[10px] text-slate-400 font-sans">
                Очки талантов начисляются при выходе на IPO и закрытии ачивок
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="px-3 py-1 rounded-full bg-indigo-500/20 border border-indigo-500/40 text-xs text-indigo-300 font-bold flex items-center gap-1.5 shadow-[0_0_12px_rgba(99,102,241,0.3)]">
              <Sparkles className="w-3.5 h-3.5 text-indigo-400" />
              <span>Очков: {skillPoints}</span>
            </div>

            <button
              onClick={onClose}
              className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Фильтры веток */}
        <div className="flex items-center justify-between gap-1 p-2 bg-slate-950/70 border-b border-cyber-border text-xs">
          <div className="flex items-center gap-1 overflow-x-auto py-0.5">
            <button
              onClick={() => setActiveBranch('all')}
              className={`px-3 py-1.5 rounded-xl font-semibold transition-all ${
                activeBranch === 'all' ? 'bg-indigo-600 text-white' : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              Все ветки
            </button>
            {branches.map(b => (
              <button
                key={b.id}
                onClick={() => setActiveBranch(b.id)}
                className={`px-3 py-1.5 rounded-xl font-semibold transition-all flex items-center gap-1.5 ${
                  activeBranch === b.id ? 'bg-slate-800 text-white border border-slate-700' : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                <span>{b.icon}</span>
                <span className="hidden sm:inline">{b.name}</span>
              </button>
            ))}
          </div>

          <button
            onClick={handleReset}
            className="px-2.5 py-1.5 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-800 text-slate-400 hover:text-red-400 text-[11px] flex items-center gap-1 transition-colors shrink-0"
            title="Сбросить все навыки и вернуть очки"
          >
            <RotateCcw className="w-3 h-3" />
            <span className="hidden sm:inline">Сброс</span>
          </button>
        </div>

        {/* Список навыков */}
        <div className="p-4 overflow-y-auto space-y-2.5 max-h-[65vh]">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
            {filteredNodes.map(node => {
              const currentLvl = unlockedSkills[node.id] || 0;
              const isMax = currentLvl >= node.maxLevel;
              const reqMet = !node.reqSkillId || (unlockedSkills[node.reqSkillId] || 0) > 0;
              const canAfford = skillPoints >= node.costPerLevel;
              const canUpgrade = !isMax && reqMet && canAfford;

              return (
                <div
                  key={node.id}
                  className={`p-3.5 rounded-2xl border text-left flex flex-col justify-between gap-2.5 transition-all ${
                    isMax
                      ? 'bg-indigo-950/30 border-indigo-500/40 shadow-[0_0_12px_rgba(99,102,241,0.15)]'
                      : !reqMet
                      ? 'bg-slate-950/60 border-slate-900 opacity-60'
                      : 'bg-slate-900/90 border-slate-800 hover:border-slate-700'
                  }`}
                >
                  <div className="space-y-1">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <span className="text-xl">{node.icon}</span>
                        <div>
                          <h3 className="font-bold text-slate-100 text-xs sm:text-sm">
                            {node.name}
                          </h3>
                          <span className="text-[10px] text-indigo-400 font-mono">
                            Уровень {currentLvl}/{node.maxLevel}
                          </span>
                        </div>
                      </div>

                      {isMax && (
                        <span className="p-1 rounded-full bg-emerald-500/20 text-emerald-400">
                          <Check className="w-4 h-4" />
                        </span>
                      )}
                      {!reqMet && (
                        <span className="p-1 rounded-full bg-slate-800 text-slate-500">
                          <Lock className="w-3.5 h-3.5" />
                        </span>
                      )}
                    </div>

                    <p className="text-[11px] text-slate-400 font-sans leading-relaxed pt-1">
                      {node.description}
                    </p>

                    {!reqMet && node.reqSkillId && (
                      <p className="text-[10px] text-amber-400/80 font-mono">
                        🔒 Требуется открыть базовый навык ветки
                      </p>
                    )}
                  </div>

                  <button
                    onClick={() => handleUpgrade(node)}
                    disabled={!canUpgrade}
                    className={`w-full py-2 px-3 rounded-xl font-bold text-xs uppercase tracking-wider transition-all flex items-center justify-center gap-1.5 ${
                      isMax
                        ? 'bg-slate-800 text-slate-500 cursor-default'
                        : canUpgrade
                        ? 'bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 text-white shadow-md active:scale-98'
                        : 'bg-slate-800/80 text-slate-600 cursor-not-allowed border border-slate-800'
                    }`}
                  >
                    {isMax ? (
                      'МАКСИМУМ'
                    ) : (
                      <>
                        <Zap className="w-3.5 h-3.5 text-amber-400" />
                        <span>Прокачать ({node.costPerLevel} очк.)</span>
                      </>
                    )}
                  </button>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
};
