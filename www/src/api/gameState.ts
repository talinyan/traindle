import { useEffect, useState } from "react";
import type { DailyPuzzle, GameKind } from "./types";
import { getDaily } from "./client";

interface BaseGuess {
  guessId: number;
  correct: boolean;
}

const getStorageKey = (game: GameKind, date: string) => "traindle:" + game + ":" + date;
const getTodaysDate = () => new Date().toLocaleDateString("en-CA");

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

const deleteOldGames = (game: GameKind, date: string) => {
  try {
    const todaysKey = getStorageKey(game, date);
    const prefix = "traindle:" + game + ":";

    for (const key of Object.keys(localStorage)) {
      const isThisGame = key.startsWith(prefix);
      const isToday = key === todaysKey;
      if (isThisGame && !isToday) {
        localStorage.removeItem(key);
      }
    }
  } catch {}
};

export const useGameState = <TOption, TGuess extends BaseGuess>(
  game: GameKind,
  loadOptions: () => Promise<TOption[]>,
  makeGuess: (date: string, id: number) => Promise<TGuess>
) => {
  const [date] = useState(getTodaysDate);
  const [guesses, setGuesses] = useState<TGuess[]>(() => loadGuesses<TGuess>(game, date));
  const [puzzle, setPuzzle] = useState<DailyPuzzle | null>(null);
  const [options, setOptions] = useState<TOption[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [won, setWon] = useState(false);

  // Once on page load: clear out saves from previous days
  useEffect(() => {
    deleteOldGames(game, date);
  }, [game, date]);

  // Every time the guesses change: save them
  useEffect(() => {
    saveGuesses(game, date, guesses);
  }, [game, date, guesses]);

  // Every time the guesses change: check for correctness
  useEffect(() => {
    setWon(guesses.some((g) => g.correct));
  }, [guesses]);

  // Once on page load: fetch today's puzzle and the list of options
  useEffect(() => {
    // If the player leaves the page before the data arrives, we must not update state
    let cancelled = false;

    const load = async () => {
      try {
        const loadedPuzzle = await getDaily(game, date);
        const loadedOptions = await loadOptions();
        if (cancelled) return;

        setPuzzle(loadedPuzzle);
        setOptions(loadedOptions);
      } catch {
        if (cancelled) return;
        setError("Couldn't load today's puzzle.");
      }
    };

    load();

    // React runs this when the page is closed
    return () => {
      cancelled = true;
    };
  }, [game, date, loadOptions]);

  const resetGuesses = () => {
    setGuesses([]);
  };

  // Send a guess
  const submitGuess = async (id: number): Promise<boolean> => {
    setSubmitting(true);
    setError(null);

    try {
      const newGuess = await makeGuess(date, id);
      if (!guesses.some((g) => g.guessId === newGuess.guessId)) {
        setGuesses([...guesses, newGuess]);
      }

      return true;
    } catch {
      setError("Guess failed. Please try again.");
      return false;
    } finally {
      setSubmitting(false);
    }
  };

  return {
    puzzle,
    options,
    guesses,
    won,
    error,
    submitting,
    submitGuess,
    resetGuesses
  };
};
