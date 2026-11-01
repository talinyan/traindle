import type {
  Continent,
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
export const getSystemOptions = async (continent: Continent | null = null) =>
  request<GameSystemOption[]>(`/gamedata/systems/options${continent ? `?continent=${continent}` : ''}`);
export const getLineOptions = async (continent: Continent | null = null) =>
  request<GameLineOption[]>(`/gamedata/lines/options${continent ? `?continent=${continent}` : ''}`);

// Daily (no continent = the global puzzle)
const continentParam = (continent: Continent | null) => (continent ? `&continent=${continent}` : '');

export const getDaily = (game: GameKind, date: string, continent: Continent | null = null) =>
  request<DailyPuzzle>(`/dailygame/${game}/daily?date=${date}${continentParam(continent)}`);

export const guessSystem = (date: string, continent: Continent | null, systemId: number) =>
  request<SystemGuess>('/dailygame/systems/guess', post({ date, continent, systemId }));

export const guessLine = (date: string, continent: Continent | null, lineId: number) =>
  request<LineGuess>('/dailygame/lines/guess', post({ date, continent, lineId }));

export const revealSystem = (date: string, continent: Continent | null = null) =>
  request<GameSystem>(`/dailygame/systems/reveal?date=${date}${continentParam(continent)}`);

export const revealLine = (date: string, continent: Continent | null = null) =>
  request<GameLine>(`/dailygame/lines/reveal?date=${date}${continentParam(continent)}`);

// Random
export const getRandom = (game: GameKind, continent: Continent | null = null) =>
  request<RandomPuzzle>(`/randomgame/${game}/random${continent ? `?continent=${continent}` : ''}`);

export const guessRandomSystem = (answerId: number, systemId: number) =>
  request<SystemGuess>('/randomgame/systems/guess', post({ answerId, systemId }));

export const guessRandomLine = (answerId: number, lineId: number) =>
  request<LineGuess>('/randomgame/lines/guess', post({ answerId, lineId }));
