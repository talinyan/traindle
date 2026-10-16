export interface GameSystem {
  id: number;
  name: string;
  city: string;
  country: string | null;
  countryState: string | null;
  continent: Continent | null;
  latitude: number | null;
  longitude: number | null;
  modes: string[];
  lineCount: number;
  stationCount: number;
  interchangeCount: number;
  length: number;
  openingYear: number;
  lines: LineSummary[];
}

export interface GameSystemOption {
  id: number;
  name: string;
  city: string;
  country: string | null;
  continent: Continent | null;
  modes: string[];
  lineCount: number;
  stationCount: number;
  length: number; // metres
  openingYear: number;
}

export interface GameLine {
  id: number;
  name: string;
  colour: string;
  mode: string;
  stationCount: number;
  length: number;
  openingYear: number;
  geometry: string;
  systemId: number;
  system: SystemSummary;
}

export interface GameLineOption {
  id: number;
  name: string;
  colour: string | null;
  mode: string;
  stationCount: number;
  length: number; // metres
  openingYear: number;
  systemId: number;
  systemName: string;
  city: string;
  country: string | null;
}

export interface LineSummary {
  id: number;
  name: string;
  colour: string;
  mode: string;
}

export interface SystemSummary {
  id: number;
  name: string;
  city: string;
  country: string | null;
  continent: Continent | null;
}

export type Continent = 'Africa' | 'Asia' | 'Europe' | 'North America' | 'South America' | 'Oceania';

export type GameKind = 'systems' | 'lines';
export type GameMode = 'daily' | 'random';

export interface DailyPuzzle {
  date: string; // yyyy-MM-dd -> new Date().toLocaleDateString("en-CA");
  puzzleNumber: number;
}

export interface RandomPuzzle {
  game: GameKind;
  id: number;
}

export type Comparison = 'Lower' | 'Equal' | 'Higher';
export type ModeMatch = 'None' | 'Partial' | 'Exact';

export interface SystemGuess {
  guessId: number;
  correct: boolean;
  lineCount: Comparison;
  distanceKm: number | null;
  bearing: number | null; // degrees clockwise from north, guess -> answer
  openingYear: Comparison;
  length: Comparison;
  stationCount: Comparison;
  modeMatch: ModeMatch;
  correctModes: string[];
}

export interface LineGuess {
  guessId: number;
  correct: boolean;
  distanceKm: number | null;
  bearing: number | null;
  openingYear: Comparison;
  modeCorrect: boolean;
  length: Comparison;
  stationCount: Comparison;
}
