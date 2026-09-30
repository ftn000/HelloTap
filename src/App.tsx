import React, { useState } from 'react';
import { GameProvider } from './context/GameContext';
import { HeaderHUD } from './components/HeaderHUD';
import { MainClicker } from './components/MainClicker';
import { ShopPanel } from './components/ShopPanel';
import { StudioHubModal } from './components/StudioHubModal';
import { LeaderboardModal } from './components/LeaderboardModal';
import { AchievementsModal } from './components/AchievementsModal';
import { AchievementToast } from './components/AchievementToast';
import { RandomEventModal } from './components/RandomEventModal';
import { useGame } from './context/GameContext';

const GameApp: React.FC = () => {
  const { 
    achievementToasts, 
    dismissAchievementToast,
    activeEvent,
    dismissEvent,
    handleEventOption
  } = useGame();
  const [hubOpen, setHubOpen] = useState<boolean>(false);
  const [hubTab, setHubTab] = useState<'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save'>('systems');
  const [leaderboardOpen, setLeaderboardOpen] = useState<boolean>(false);
  const [achievementsOpen, setAchievementsOpen] = useState<boolean>(false);

  const openHubWithTab = (tab: 'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save') => {
    setHubTab(tab);
    setHubOpen(true);
  };

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
      />

      {/* Основной контент */}
      <main className="flex-1 flex flex-col items-center justify-start max-w-xl mx-auto w-full pt-2">
        <MainClicker />
        <ShopPanel />
      </main>

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
