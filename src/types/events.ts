export type RandomEventType = 'hacker_attack' | 'angel_investor' | 'bug_bounty' | 'coffee_rush';

export interface GameEventOption {
  id: string;
  title: string;
  description: string;
  badge?: string;
  badgeColor?: string;
  actionType: 'boost_flow' | 'grant_money' | 'grant_code' | 'boost_cps' | 'grant_token';
  value: number; // multiplier or amount
  durationSec?: number;
}

export interface GameRandomEvent {
  id: string;
  type: RandomEventType;
  title: string;
  subtitle: string;
  description: string;
  icon: string;
  gradient: string;
  borderColor: string;
  glowColor: string;
  timeoutSec: number;
  options: GameEventOption[];
}
