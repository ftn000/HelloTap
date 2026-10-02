import React, { useState, useEffect } from 'react';
import { GameProvider } from './context/GameContext';
import { HeaderHUD } from './components/HeaderHUD';
import { MainClicker } from './components/MainClicker';
import { ShopPanel } from './components/ShopPanel';
import { StudioHubModal } from './components/StudioHubModal';
import { LeaderboardModal } from './components/LeaderboardModal';
import { AchievementsModal } from './components/AchievementsModal';
import { AchievementToast } from './components/AchievementToast';
import { RandomEventModal } from './components/RandomEventModal';
import { TechTreeModal } from './components/TechTreeModal';
import { OfflineProgressModal } from './components/OfflineProgressModal';
import { BottomNav } from './components/BottomNav';
import { CommandPaletteModal } from './components/CommandPaletteModal';
import { GitHubHeatmapModal } from './components/GitHubHeatmapModal';
import { useGame } from './context/GameContext';
import { ThemeId } from './types/themes';

const GameApp: React.FC = () => {
  const { 
    achievementToasts, 
    dismissAchievementToast,
    activeEvent,
    dismissEvent,
    handleEventOption,
    handleClick,
    toggleMusic,
    themeId,
    setThemeId,
    offlineReport,
    claimOfflineEarnings
  } = useGame();

  const [hubOpen, setHubOpen] = useState<boolean>(false);
  const [hubTab, setHubTab] = useState<'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save'>('systems');
  const [leaderboardOpen, setLeaderboardOpen] = useState<boolean>(false);
  const [achievementsOpen, setAchievementsOpen] = useState<boolean>(false);
  const [techTreeOpen, setTechTreeOpen] = useState<boolean>(false);
  const [heatmapOpen, setHeatmapOpen] = useState<boolean>(false);

  const openHubWithTab = (tab: 'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save') => {
    setHubTab(tab);
    setHubOpen(true);
  };

  // Полноценная поддержка физической клавиатуры и горячих клавиш (True Coder Mode)
  useEffect(() => {
    const handleOpenHeatmapEvent = () => setHeatmapOpen(true);
    window.addEventListener('codetap_open_heatmap', handleOpenHeatmapEvent);

    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement;
      if (target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA')) {
        return;
      }

      if (e.key === 'Escape') {
        setHubOpen(false);
        setLeaderboardOpen(false);
        setAchievementsOpen(false);
        setTechTreeOpen(false);
        setHeatmapOpen(false);
        dismissEvent();
        return;
      }

      if (e.code === 'KeyB') {
        openHubWithTab('shop');
        return;
      }
      if (e.code === 'KeyH') {
        setHubOpen(prev => !prev);
        return;
      }
      if (e.code === 'KeyK') {
        setTechTreeOpen(prev => !prev);
        return;
      }
      if (e.code === 'KeyL') {
        setLeaderboardOpen(prev => !prev);
        return;
      }
      if (e.code === 'KeyA') {
        setAchievementsOpen(prev => !prev);
        return;
      }
      if (e.code === 'KeyM') {
        toggleMusic();
        return;
      }
      if (e.code === 'KeyT') {
        const themeKeys: ThemeId[] = ['cyberpunk', 'monokai', 'dracula', 'matrix', 'nordic', 'tokyonight'];
        const currentIdx = themeKeys.indexOf(themeId);
        const nextTheme = themeKeys[(currentIdx + 1) % themeKeys.length];
        setThemeId(nextTheme);
        return;
      }

      // Печать кода / клик любой буквой или пробелом при закрытых окнах
      if (
        !hubOpen && 
        !leaderboardOpen && 
        !achievementsOpen && 
        !techTreeOpen && 
        !activeEvent && 
        !offlineReport?.isOpen
      ) {
        if (
          e.code === 'Space' || 
          e.code === 'Enter' || 
          (e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey)
        ) {
          if (e.code === 'Space') e.preventDefault();
          handleClick();
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('codetap_open_heatmap', handleOpenHeatmapEvent);
    };
  }, [hubOpen, leaderboardOpen, achievementsOpen, techTreeOpen, heatmapOpen, activeEvent, offlineReport, themeId, handleClick, toggleMusic, setThemeId, dismissEvent]);

  return (
    <div className="min-h-screen bg-cyber-bg text-slate-100 flex flex-col font-sans selection:bg-cyan-500 selection:text-black">
      {/* Всплывающие тосты ачивок */}
      <AchievementToast
        toasts={achievementToasts}
        onDismiss={dismissAchievementToast}
      />

      {/* Верхний статус-бар */}
      <HeaderHUD
        onOpenHub={() => openHubWithTab('systems')}
        onOpenShop={() => openHubWithTab('shop')}
        onOpenDigest={() => openHubWithTab('digest')}
        onOpenSave={() => openHubWithTab('save')}
        onOpenLeaderboard={() => setLeaderboardOpen(true)}
        onOpenAchievements={() => setAchievementsOpen(true)}
        onOpenTechTree={() => setTechTreeOpen(true)}
        onOpenHeatmap={() => setHeatmapOpen(true)}
      />

      {/* Основной контент */}
      <main className="flex-1 flex flex-col items-center justify-start max-w-xl mx-auto w-full pt-2 pb-20 sm:pb-6">
        <MainClicker />
        <ShopPanel />
      </main>

      {/* Нижняя мобильная панель навигации */}
      <BottomNav
        onOpenHub={() => openHubWithTab('systems')}
        onOpenAchievements={() => setAchievementsOpen(true)}
        onOpenTechTree={() => setTechTreeOpen(true)}
        onOpenLeaderboard={() => setLeaderboardOpen(true)}
      />

      {/* Модальное окно Studio Hub OS */}
      <StudioHubModal
        isOpen={hubOpen}
        onClose={() => setHubOpen(false)}
        defaultTab={hubTab}
      />

      {/* Модальное окно Рейтинга Лидерборда */}
      <LeaderboardModal
        isOpen={leaderboardOpen}
        onClose={() => setLeaderboardOpen(false)}
      />

      {/* Модальное окно Достижений (24 ачивки по 3 уровня) */}
      <AchievementsModal
        isOpen={achievementsOpen}
        onClose={() => setAchievementsOpen(false)}
      />

      {/* Случайные интерактивные мини-события студии */}
      <RandomEventModal
        event={activeEvent}
        onSelectOption={handleEventOption}
        onClose={dismissEvent}
      />

      {/* Дерево IT-навыков и талантов */}
      <TechTreeModal
        isOpen={techTreeOpen}
        onClose={() => setTechTreeOpen(false)}
      />

      {/* Окно Офлайн-прогресса при входе */}
      <OfflineProgressModal
        isOpen={Boolean(offlineReport?.isOpen)}
        offlineSec={offlineReport?.seconds || 0}
        codeEarned={offlineReport?.codeEarned || 0}
        moneyEarned={offlineReport?.moneyEarned || 0}
        onClaim={claimOfflineEarnings}
      />

      {/* GitHub Contribution Heatmap */}
      <GitHubHeatmapModal
        isOpen={heatmapOpen}
        onClose={() => setHeatmapOpen(false)}
      />

      {/* VS Code Command Palette (Ctrl+Shift+P / F1) */}
      <CommandPaletteModal />
    </div>
  );
};

export const App: React.FC = () => {
  return (
    <GameProvider>
      <GameApp />
    </GameProvider>
  );
};

export default App;
