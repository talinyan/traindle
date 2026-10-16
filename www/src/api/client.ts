import type {
  DailyPuzzle,
  GameKind,
  GameLine,
  GameLineOption,
  GameSystem,
  GameSystemOption,
  LineGuess,
  RandomPuzzle,
  SystemGuess
} from './types';

export const API_URL = import.meta.env.VITE_API_URL ?? '/api';

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, init);
  if (!res.ok) throw new Error(`${init?.method ?? 'GET'} ${path} failed: ${res.status}`);
  return res.json();
}

const post = (body: unknown): RequestInit => ({
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body)
});

// Every playable option for autocomplete
export const getSystemOptions = () => request<GameSystemOption[]>('/gamedata/systems/options');
export const getLineOptions = () => request<GameLineOption[]>('/gamedata/lines/options');

// Daily
export const getDaily = (game: GameKind, date: string) => request<DailyPuzzle>(`/dailygame/${game}/daily?date=${date}`);

export const guessSystem = (date: string, systemId: number) =>
  request<SystemGuess>('/dailygame/systems/guess', post({ date, systemId }));

export const guessLine = (date: string, lineId: number) =>
  request<LineGuess>('/dailygame/lines/guess', post({ date, lineId }));

export const revealSystem = (date: string) => request<GameSystem>(`/dailygame/systems/reveal?date=${date}`);

export const revealLine = (date: string) => request<GameLine>(`/dailygame/lines/reveal?date=${date}`);

// Random
export const getRandom = (game: GameKind) => request<RandomPuzzle>(`/randomgame/${game}/random`);

export const guessRandomSystem = (answerId: number, systemId: number) =>
  request<SystemGuess>('/randomgame/systems/guess', post({ answerId, systemId }));

export const guessRandomLine = (answerId: number, lineId: number) =>
  request<LineGuess>('/randomgame/lines/guess', post({ answerId, lineId }));
