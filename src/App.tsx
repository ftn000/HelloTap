import React, { useState } from 'react';
import { GameProvider } from './context/GameContext';
import { HeaderHUD } from './components/HeaderHUD';
import { MainClicker } from './components/MainClicker';
import { ShopPanel } from './components/ShopPanel';
import { StudioHubModal } from './components/StudioHubModal';

const GameApp: React.FC = () => {
  const [hubOpen, setHubOpen] = useState<boolean>(false);
  const [hubTab, setHubTab] = useState<'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save'>('systems');

  const openHubWithTab = (tab: 'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save') => {
    setHubTab(tab);
    setHubOpen(true);
  };

  return (
    <div className="min-h-screen bg-cyber-bg text-slate-100 flex flex-col font-sans selection:bg-cyan-500 selection:text-black">
      {/* Верхний статус-бар */}
      <HeaderHUD
        onOpenHub={() => openHubWithTab('systems')}
        onOpenShop={() => openHubWithTab('shop')}
        onOpenDigest={() => openHubWithTab('digest')}
        onOpenSave={() => openHubWithTab('save')}
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
