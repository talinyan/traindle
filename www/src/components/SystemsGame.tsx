import { getSystemOptions, guessSystem, guessRandomSystem } from '../api/client';
import GuessInput from './GuessInput';
import type { GameMode, GameSystemOption, SystemGuess } from '../api/types';
import { useGameState } from '../api/gameState';
import { Cell, CompareCell, LocationCell, Modes, ResetGuesses, ResetPuzzle, WinBanner } from './HintCells';

export default function SystemsGame({ mode }: { mode: GameMode }) {
  const game = useGameState('systems', mode, getSystemOptions, guessSystem, guessRandomSystem);

  if (!game.ready) {
    if (game.error) return <p>{game.error}</p>;
    return <p>Loading...</p>;
  }

  const guessedIds: number[] = game.guesses.map((g) => g.guessId);

  const getSystemId = (system: GameSystemOption): number => system.id;
  const getSearchText = (system: GameSystemOption): (string | null)[] => [system.name, system.city, system.country];
  const renderOption = (system: GameSystemOption) => {
    let place = system.city;
    if (system.country) {
      place = system.city + ', ' + system.country;
    }

    return (
      <>
        <span className='text-primary'>{system.name}</span>
        <span className='truncate text-xs text-secondary'>{place}</span>
      </>
    );
  };

  const newestFirst = [...game.guesses].reverse();

  return (
    <div className='flex flex-col gap-6'>
      <div className='flex items-center justify-between gap-4'>
        <h1 className='text-2xl font-semibold'>Traindle Systems: {game.title}</h1>

        <div className='flex gap-2'>
          {game.mode === 'random' && <ResetPuzzle onReset={game.newPuzzle} />}

          <ResetGuesses onReset={game.resetGuesses} />
        </div>
      </div>

      {game.error && <p className='text-sm text-red-400'>{game.error}</p>}

      {game.won ? (
        <WinBanner guesses={game.guesses} game='systems' title={game.title!} />
      ) : (
        <GuessInput
          options={game.options}
          guessedIds={guessedIds}
          getId={getSystemId}
          searchText={getSearchText}
          renderOption={renderOption}
          onGuess={game.submitGuess}
          disabled={game.submitting}
          placeholder='Guess a system, city or country…'
        />
      )}

      {game.guesses.length > 0 && (
        <div className='overflow-x-auto'>
          <table className='w-full min-w-160 border-separate border-spacing-1 text-sm'>
            <thead className='text-xs text-secondary'>
              <tr>
                <th className='text-left font-normal'>System</th>
                <th className='font-normal'>To Target</th>
                <th className='font-normal'>System Length</th>
                <th className='font-normal'>Lines</th>
                <th className='font-normal'>Stations</th>
                <th className='font-normal'>Opened</th>
                <th className='font-normal'>Modes</th>
              </tr>
            </thead>
            <tbody>
              {newestFirst.map((guess) => {
                const system = game.options.find((g) => g.id === guess.guessId);
                if (system === undefined) return null;

                return <SystemGuessRow key={guess.guessId} guess={guess} system={system} />;
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// One row of the table: the guessed system's values, coloured by how close they are
function SystemGuessRow({ guess, system }: { guess: SystemGuess; system: GameSystemOption }) {
  return (
    <tr>
      {/* System */}
      <Cell correct={guess.correct} align={'left'}>
        <span className='text-primary'>{system.name}</span>
        <span className='block text-xs text-secondary'>
          {system.city}, {system.country} ({system.continent})
        </span>
      </Cell>

      {/* Location */}
      <LocationCell correct={guess.correct} km={guess.distanceKm} bearing={guess.bearing} />

      {/* Length */}
      <CompareCell result={guess.length}>{(system.length / 1000).toFixed(1)} km</CompareCell>

      {/* Line ct */}
      <CompareCell result={guess.lineCount}>{system.lineCount}</CompareCell>

      {/* Station ct */}
      <CompareCell result={guess.stationCount}>{system.stationCount}</CompareCell>

      {/* Opening year */}
      <CompareCell result={guess.openingYear}>{system.openingYear}</CompareCell>

      {/* Modes */}
      <Cell correct={guess.modeMatch === 'Exact'}>
        <Modes correct={guess.correctModes} guessed={system.modes} />
      </Cell>
    </tr>
  );
}
