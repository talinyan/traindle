import { useEffect, useState } from 'react';
import type { GameKind, GameMode } from './types';
import { getDaily, getRandom } from './client';

interface BaseGuess {
  guessId: number;
  correct: boolean;
}

// What we keep in localStorage for an endless game
interface RandomSave<TGuess> {
  answerId: number;
  guesses: TGuess[];
}

const getStorageKey = (game: GameKind, date: string) => 'traindle:' + game + ':' + date;
const getRandomStorageKey = (game: GameKind) => 'traindle:random:' + game;
const getTodaysDate = () => new Date().toLocaleDateString('en-CA');

const loadGuesses = <TGuess>(game: GameKind, date: string): TGuess[] => {
  try {
    const text = localStorage.getItem(getStorageKey(game, date));
    if (text === null) {
      return [];
    }

    const saved = JSON.parse(text);
    if (!Array.isArray(saved)) {
      return [];
    }

    return saved;
  } catch {
    return [];
  }
};

const saveGuesses = <TGuess>(game: GameKind, date: string, guesses: TGuess[]) => {
  try {
    localStorage.setItem(getStorageKey(game, date), JSON.stringify(guesses));
  } catch {}
};

const loadRandom = <TGuess>(game: GameKind): RandomSave<TGuess> | null => {
  try {
    const text = localStorage.getItem(getRandomStorageKey(game));
    if (text === null) {
      return null;
    }

    const saved = JSON.parse(text);
    if (typeof saved?.answerId !== 'number' || !Array.isArray(saved.guesses)) {
      return null;
    }

    return saved;
  } catch {
    return null;
  }
};

const saveRandom = <TGuess>(game: GameKind, save: RandomSave<TGuess>) => {
  try {
    localStorage.setItem(getRandomStorageKey(game), JSON.stringify(save));
  } catch {}
};

const clearRandom = (game: GameKind) => {
  try {
    localStorage.removeItem(getRandomStorageKey(game));
  } catch {}
};

export const useGameState = <TOption, TGuess extends BaseGuess>(
  game: GameKind,
  mode: GameMode,
  loadOptions: () => Promise<TOption[]>,
  guessDaily: (date: string, id: number) => Promise<TGuess>,
  guessRandom: (answerId: number, id: number) => Promise<TGuess>
) => {
  const [date] = useState(getTodaysDate);
  // Endless: the answer we're guessing. Restored from storage so a refresh keeps the same puzzle
  const [answerId, setAnswerId] = useState<number | null>(() =>
    mode === 'random' ? (loadRandom<TGuess>(game)?.answerId ?? null) : null
  );
  const [guesses, setGuesses] = useState<TGuess[]>(() =>
    mode === 'random' ? (loadRandom<TGuess>(game)?.guesses ?? []) : loadGuesses<TGuess>(game, date)
  );
  const [puzzleNumber, setPuzzleNumber] = useState<number | null>(null);
  const [options, setOptions] = useState<TOption[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [won, setWon] = useState(false);

  // Every time the guesses change: save them
  useEffect(() => {
    if (mode === 'daily') {
      saveGuesses(game, date, guesses);
    } else if (answerId !== null) {
      saveRandom(game, { answerId, guesses });
    }
  }, [game, mode, date, answerId, guesses]);

  // Every time the guesses change: check for correctness
  useEffect(() => {
    setWon(guesses.some((g) => g.correct));
  }, [guesses]);

  // Once on page load: fetch the list of options
  useEffect(() => {
    // If the player leaves the page before the data arrives, we must not update state
    let cancelled = false;

    const load = async () => {
      try {
        const loadedOptions = await loadOptions();
        if (cancelled) return;

        setOptions(loadedOptions);
      } catch {
        if (cancelled) return;
        setError("Couldn't load the puzzle.");
      }
    };

    load();

    // React runs this when the page is closed
    return () => {
      cancelled = true;
    };
  }, [loadOptions]);

  // On page load (and after newPuzzle in endless): fetch the puzzle
  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        if (mode === 'daily') {
          const loadedPuzzle = await getDaily(game, date);
          if (cancelled) return;

          setPuzzleNumber(loadedPuzzle.puzzleNumber);
        } else if (answerId === null) {
          const loadedPuzzle = await getRandom(game);
          if (cancelled) return;

          setAnswerId(loadedPuzzle.id);
        }
      } catch {
        if (cancelled) return;
        setError("Couldn't load the puzzle.");
      }
    };

    load();

    return () => {
      cancelled = true;
    };
  }, [game, mode, date, answerId]);

  const resetGuesses = () => {
    setGuesses([]);
  };

  // Endless: throw away the current puzzle; the effect above fetches a new one
  const newPuzzle = () => {
    clearRandom(game);
    setGuesses([]);
    setAnswerId(null);
  };

  // Send a guess
  const submitGuess = async (id: number): Promise<boolean> => {
    if (mode === 'random' && answerId === null) return false;

    setSubmitting(true);
    setError(null);

    try {
      const newGuess = mode === 'daily' ? await guessDaily(date, id) : await guessRandom(answerId!, id);
      if (!guesses.some((g) => g.guessId === newGuess.guessId)) {
        setGuesses([...guesses, newGuess]);
      }

      return true;
    } catch {
      setError('Guess failed. Please try again.');
      return false;
    } finally {
      setSubmitting(false);
    }
  };

  // null until the puzzle has loaded
  let title: string | null = null;
  if (mode === 'daily' && puzzleNumber !== null) {
    title = 'Day #' + puzzleNumber;
  } else if (mode === 'random' && answerId !== null) {
    title = 'Endless';
  }

  return {
    ready: title !== null && options !== null,
    title,
    options: options ?? [],
    guesses,
    won,
    error,
    submitting,
    submitGuess,
    resetGuesses,
    newPuzzle,
    mode
  };
};
