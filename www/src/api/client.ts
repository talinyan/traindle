import type {
  DailyPuzzle,
  GameKind,
  GameLine,
  GameLineOption,
  GameSystem,
  GameSystemOption,
  LineGuess,
  SystemGuess
} from "./types";

// export const API_URL = "http://localhost:5135/api";
// export const API_URL = "https://localhost:7102/api";
export const API_URL = import.meta.env.VITE_API_URL ?? "/api";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, init);
  if (!res.ok) throw new Error(`${init?.method ?? "GET"} ${path} failed: ${res.status}`);
  return res.json();
}

const post = (body: unknown): RequestInit => ({
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(body)
});

// Every playable option for autocomplete
export const getSystemOptions = () => request<GameSystemOption[]>("/gamedata/systems/options");
export const getLineOptions = () => request<GameLineOption[]>("/gamedata/lines/options");

export const getDaily = (game: GameKind, date: string) => request<DailyPuzzle>(`/game/${game}/daily?date=${date}`);

export const guessSystem = (date: string, systemId: number) =>
  request<SystemGuess>("/game/systems/guess", post({ date, systemId }));

export const guessLine = (date: string, lineId: number) =>
  request<LineGuess>("/game/lines/guess", post({ date, lineId }));

export const revealSystem = (date: string) => request<GameSystem>(`/game/systems/reveal?date=${date}`);

export const revealLine = (date: string) => request<GameLine>(`/game/lines/reveal?date=${date}`);
