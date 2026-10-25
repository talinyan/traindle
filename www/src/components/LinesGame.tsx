import { getLineOptions, guessLine, guessRandomLine } from '../api/client';
import GuessInput from './GuessInput';
import type { GameLineOption, GameMode, LineGuess } from '../api/types';
import { useGameState } from '../api/gameState';
import { Cell, CompareCell, LocationCell, Modes, ResetGuesses, WinBanner, ResetPuzzle } from './HintCells';

export default function LinesGame({ mode }: { mode: GameMode }) {
  const game = useGameState('lines', mode, getLineOptions, guessLine, guessRandomLine);

  if (!game.ready) {
    if (game.error) return <p>{game.error}</p>;
    return <p>Loading...</p>;
  }

  const guessedIds: number[] = game.guesses.map((g) => g.guessId);

  const getLineId = (line: GameLineOption): number => line.id;
  const getSearchText = (line: GameLineOption): (string | null)[] => [
    line.name,
    line.systemName,
    line.city,
    line.country
  ];

  const renderOption = (line: GameLineOption) => {
    let place = line.systemName + ', ' + line.city;
    if (line.systemName === line.city) {
      place = line.city;
    }

    return (
      <>
        <span className='flex items-center gap-2 text-primary'>
          <Swatch colour={line.colour} />
          {line.name}
        </span>
        <span className='truncate text-xs text-secondary'>{place}</span>
      </>
    );
  };

  const newestFirst = [...game.guesses].reverse();

  return (
    <div className='flex flex-col gap-6'>
      <div className='flex items-center justify-between gap-4'>
        <h1 className='text-2xl font-semibold'>Traindle Lines: {game.title}</h1>

        <div className='flex gap-2'>
          {game.mode === 'random' && <ResetPuzzle onReset={game.newPuzzle} />}

          <ResetGuesses onReset={game.resetGuesses} />
        </div>
      </div>

      {game.error && <p className='text-sm text-red-400'>{game.error}</p>}

      {game.won ? (
        <WinBanner guesses={game.guesses} game='lines' title={game.title!} />
      ) : (
        <GuessInput
          options={game.options}
          guessedIds={guessedIds}
          getId={getLineId}
          searchText={getSearchText}
          renderOption={renderOption}
          onGuess={game.submitGuess}
          disabled={game.submitting}
          placeholder='Guess a line, system or city…'
        />
      )}

      {game.guesses.length > 0 && (
        <div className='overflow-x-auto'>
          <table className='w-full min-w-160 border-separate border-spacing-1 text-sm'>
            <thead className='text-xs text-secondary'>
              <tr>
                <th className='text-left font-normal'>Line</th>
                <th className='font-normal'>To Target</th>
                <th className='font-normal'>Line Length</th>
                <th className='font-normal'>Stations</th>
                <th className='font-normal'>Opened</th>
                <th className='font-normal'>Mode</th>
              </tr>
            </thead>
            <tbody>
              {newestFirst.map((guess) => {
                const line = game.options.find((g) => g.id === guess.guessId);
                if (line === undefined) return null;

                return <LineGuessRow key={guess.guessId} guess={guess} line={line} />;
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// One row of the table: the guessed line's values, coloured by how close they are
function LineGuessRow({ guess, line }: { guess: LineGuess; line: GameLineOption }) {
  return (
    <tr>
      {/* Line */}
      <Cell correct={guess.correct} align='left'>
        <span className='flex items-center gap-2 text-primary'>
          <Swatch colour={line.colour} />
          {line.name} ({line.systemName})
        </span>
        <span className='block text-xs text-secondary'>
          {line.city}, {line.country} ({line.continent})
        </span>
      </Cell>

      {/* Location */}
      <LocationCell correct={guess.correct} km={guess.distanceKm} bearing={guess.bearing} />

      {/* Length */}
      <CompareCell result={guess.length}>{(line.length / 1000).toFixed(1)} km</CompareCell>

      {/* Station ct*/}
      <CompareCell result={guess.stationCount}>{line.stationCount}</CompareCell>

      {/* Opening year */}
      <CompareCell result={guess.openingYear}>{line.openingYear}</CompareCell>

      {/* Mode */}
      <Cell correct={guess.modeCorrect}>
        <Modes guessed={[line.mode]} correct={guess.modeCorrect ? [line.mode] : []} />
      </Cell>
    </tr>
  );
}

// The line's own colour; Citylines colours are hex like "#ff0000"
function Swatch({ colour }: { colour: string | null }) {
  return (
    <span
      aria-hidden
      className='h-2.5 w-2.5 shrink-0 rounded-full border border-white/20'
      style={{ background: colour ?? 'transparent' }}
    />
  );
}
