import React, { useState, useEffect, useRef } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { Play, RotateCw, CheckCircle2, XCircle, AlertTriangle, ShieldCheck, Zap, Sparkles, Terminal } from 'lucide-react';
import confetti from 'canvas-confetti';
import { sounds } from '../utils/soundEffects';

interface PipelineJob {
  id: string;
  name: string;
  icon: string;
  status: 'idle' | 'running' | 'success' | 'failed';
}

const INITIAL_JOBS: PipelineJob[] = [
  { id: 'lint', name: 'Lint & Style', icon: '🧹', status: 'idle' },
  { id: 'test', name: 'Unit Tests', icon: '🧪', status: 'idle' },
  { id: 'build', name: 'Docker Pack', icon: '📦', status: 'idle' },
  { id: 'deploy', name: 'Auto Deploy', icon: '🚀', status: 'idle' }
];

export const CICDPipeline: React.FC = () => {
  const { 
    codePerSec, 
    codePerClick, 
    moneyPerSec, 
    handleClick, 
    unlockedSkills, 
    recordContribution, 
    recordPipelinePass, 
    pipelinesPassed,
    t 
  } = useGame();

  const [jobs, setJobs] = useState<PipelineJob[]>(INITIAL_JOBS);
  const [pipelineState, setPipelineState] = useState<'idle' | 'running' | 'success' | 'broken'>('idle');
  const [pipelineRunId, setPipelineRunId] = useState<number>(1024 + pipelinesPassed);
  const [failedJobName, setFailedJobName] = useState<string>('');
  const [statusMessage, setStatusMessage] = useState<string>('CI/CD Workflow idle');

  // Талант ускорения и наград CI/CD
  const hasteMultiplier = 1.0 + (unlockedSkills['skill_github_actions'] || 0) * 0.25;
  const bountyBonus = 1.0 + (unlockedSkills['skill_github_actions'] || 0) * 0.50;

  const isRunningRef = useRef<boolean>(false);

  // Запуск пайплайна
  const runPipeline = (forceSuccess: boolean = false) => {
    if (isRunningRef.current || pipelineState === 'broken') return;

    isRunningRef.current = true;
    setPipelineState('running');
    const newRunId = pipelineRunId + 1;
    setPipelineRunId(newRunId);
    setStatusMessage(`Running Pipeline #${newRunId}...`);

    // Сброс статусов
    setJobs(prev => prev.map(j => ({ ...j, status: 'idle' })));

    const stageDuration = Math.max(400, Math.round(900 / hasteMultiplier));

    // Выбираем, упадет ли билд (12% шанс на test или build)
    const willFail = !forceSuccess && Math.random() < 0.14;
    const failStageIdx = willFail ? (Math.random() < 0.6 ? 1 : 2) : -1;

    // Последовательный прогон 4 шагов
    const executeStage = (idx: number) => {
      if (idx >= INITIAL_JOBS.length) {
        // Успех всего пайплайна!
        setPipelineState('success');
        isRunningRef.current = false;
        setStatusMessage(`✓ Pipeline #${newRunId} passed! 4/4 jobs green`);
        sounds.playPipelinePass();
        sounds.triggerHaptic('success');
        recordPipelinePass();
        return;
      }

      // Начинаем шаг
      setJobs(prev => prev.map((j, i) => i === idx ? { ...j, status: 'running' } : j));

      setTimeout(() => {
        if (idx === failStageIdx) {
          // Авария сборки (Broken Build)!
          setJobs(prev => prev.map((j, i) => i === idx ? { ...j, status: 'failed' } : j));
          setPipelineState('broken');
          setFailedJobName(INITIAL_JOBS[idx].name);
          setStatusMessage(`❌ Broken Build: Pipeline #${newRunId} failed at [${INITIAL_JOBS[idx].name}]`);
          isRunningRef.current = false;
          sounds.playPipelineFail();
          sounds.triggerHaptic('heavy');
          return;
        }

        // Шаг успешен
        setJobs(prev => prev.map((j, i) => i === idx ? { ...j, status: 'success' } : j));
        executeStage(idx + 1);
      }, stageDuration);
    };

    executeStage(0);
  };

  // Автоматический запуск пайплайна каждые 28 секунд
  useEffect(() => {
    const timer = setInterval(() => {
      if (!isRunningRef.current && pipelineState !== 'broken') {
        runPipeline();
      }
    }, 28000);

    return () => clearInterval(timer);
  }, [pipelineState, hasteMultiplier]);

  // Слушатель для вызова из Command Palette
  useEffect(() => {
    const handleRunCmd = () => runPipeline(true);
    const handleFixCmd = () => {
      if (pipelineState === 'broken') {
        handleFixBrokenBuild();
      }
    };

    window.addEventListener('codetap_run_pipeline', handleRunCmd);
    window.addEventListener('codetap_fix_pipeline', handleFixCmd);
    return () => {
      window.removeEventListener('codetap_run_pipeline', handleRunCmd);
      window.removeEventListener('codetap_fix_pipeline', handleFixCmd);
    };
  }, [pipelineState]);

  // Фикс сломанной сборки
  const handleFixBrokenBuild = () => {
    const bountyCode = Math.round(Math.max(400, codePerClick * 30, codePerSec * 6) * bountyBonus);
    const bountyMoney = Math.round(Math.max(500, moneyPerSec * 8) * bountyBonus);

    handleClick();
    sounds.playQuickFix();
    sounds.triggerHaptic('success');

    confetti({
      particleCount: 55,
      spread: 75,
      origin: { y: 0.65 },
      colors: ['#EF4444', '#10B981', '#38BDF8', '#F59E0B']
    });

    setJobs(prev => prev.map(j => ({ ...j, status: 'success' })));
    setPipelineState('success');
    setStatusMessage(`✓ Hotfix deployed: Pipeline #${pipelineRunId} restored (+₽${formatNumber(bountyMoney)})`);
    recordContribution(2);
    recordPipelinePass();
  };

  return (
    <div className="w-full my-2 p-2.5 rounded-2xl bg-slate-950/70 border border-slate-800/80 font-mono select-none">
      {/* Шапка статуса пайплайна */}
      <div className="flex items-center justify-between text-[11px] mb-2 px-1">
        <div className="flex items-center gap-1.5 overflow-hidden">
          <span className="w-2 h-2 rounded-full inline-block animate-ping mr-0.5 shrink-0 bg-cyan-400" />
          <span className="text-slate-400 font-bold flex items-center gap-1 truncate">
            <span>Actions CI/CD</span>
            <span className="text-slate-600">#{pipelineRunId}</span>
          </span>
        </div>

        <div className="flex items-center gap-1.5 shrink-0">
          {pipelineState === 'broken' ? (
            <button
              onClick={handleFixBrokenBuild}
              className="px-2 py-0.5 rounded-lg bg-red-500 hover:bg-red-400 text-black font-black text-[10px] tracking-wider animate-bounce shadow-[0_0_12px_rgba(239,68,68,0.7)] flex items-center gap-1 cursor-pointer"
            >
              <AlertTriangle className="w-3 h-3 text-black" />
              <span>FIX BUILD!</span>
            </button>
          ) : (
            <button
              onClick={() => runPipeline()}
              disabled={pipelineState === 'running'}
              className="px-2 py-0.5 rounded-lg bg-slate-800/80 hover:bg-slate-700/80 text-cyan-300 text-[10px] font-semibold flex items-center gap-1 border border-slate-700/60 transition-colors disabled:opacity-50 cursor-pointer"
              title="Запустить ручной прогон CI/CD пайплайна"
            >
              <RotateCw className={`w-2.5 h-2.5 ${pipelineState === 'running' ? 'animate-spin text-cyan-400' : ''}`} />
              <span>Run</span>
            </button>
          )}
        </div>
      </div>

      {/* Линейка из 4 шагов */}
      <div className="grid grid-cols-4 gap-1.5">
        {jobs.map((job) => {
          let statusStyle = 'bg-slate-900/60 border-slate-800/60 text-slate-400';
          let iconBadge = null;

          if (job.status === 'running') {
            statusStyle = 'bg-cyan-950/70 border-cyan-500/50 text-cyan-300 animate-pulse shadow-[0_0_8px_rgba(6,182,212,0.3)]';
            iconBadge = <RotateCw className="w-2.5 h-2.5 animate-spin text-cyan-400" />;
          } else if (job.status === 'success') {
            statusStyle = 'bg-emerald-950/50 border-emerald-500/40 text-emerald-300 shadow-[0_0_6px_rgba(16,185,129,0.2)]';
            iconBadge = <CheckCircle2 className="w-2.5 h-2.5 text-emerald-400" />;
          } else if (job.status === 'failed') {
            statusStyle = 'bg-red-950/80 border-red-500 text-red-200 animate-pulse shadow-[0_0_10px_rgba(239,68,68,0.5)]';
            iconBadge = <XCircle className="w-2.5 h-2.5 text-red-400" />;
          }

          return (
            <div
              key={job.id}
              className={`p-1.5 rounded-xl border flex flex-col items-center justify-center text-center transition-all ${statusStyle}`}
            >
              <div className="flex items-center gap-1">
                <span className="text-[11px]">{job.icon}</span>
                {iconBadge}
              </div>
              <span className="text-[9px] font-bold mt-0.5 truncate w-full">
                {job.name}
              </span>
            </div>
          );
        })}
      </div>

      {/* Нижний бегущий лог статуса */}
      <div className="mt-1.5 pt-1.5 border-t border-slate-800/60 flex items-center justify-between text-[9px] text-slate-400">
        <span className="truncate max-w-[80%] font-mono">
          {statusMessage}
        </span>
        <span className="text-slate-500 font-mono shrink-0">
          {pipelinesPassed} deploys
        </span>
      </div>
    </div>
  );
};
