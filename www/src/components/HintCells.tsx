import type { ReactNode } from 'react';
import type { Comparison, LineGuess, SystemGuess } from '../api/types';
import { ArrowDown, ArrowUp, Check } from 'lucide-react';
import { buttonClass } from '../ui';

export function Cell({
  correct,
  children,
  align
}: {
  correct: boolean;
  children: ReactNode;
  align?: 'left' | 'center';
}) {
  const colour = correct ? 'border-neon bg-neon/10' : 'border-border bg-white/5';
  const alignment = align === 'left' ? 'text-left' : 'text-center';

  return <td className={`rounded-md border px-2 py-2 ${colour} ${alignment}`}>{children}</td>;
}

export function CompareCell({ result, children }: { result: Comparison; children: ReactNode }) {
  return (
    <Cell correct={result === 'Equal'}>
      <span className='inline-flex items-center gap-1 text-primary'>
        {children}
        {result === 'Higher' && <ArrowUp size={14} />}
        {result === 'Lower' && <ArrowDown size={14} />}
        {result === 'Equal' && <Check size={14} className='text-neon' />}
      </span>
    </Cell>
  );
}

export function ResetButton({ onReset }: { onReset: () => void }) {
  return (
    <button type='button' onClick={onReset} className={buttonClass}>
      Reset Guesses
    </button>
  );
}

export function ResetPuzzle({ onReset }: { onReset: () => void }) {
  return (
    <button type='button' onClick={onReset} className={buttonClass}>
      New Puzzle
    </button>
  );
}

type Props = { title: string } & (
  | { game: 'systems'; guesses: SystemGuess[] }
  | { game: 'lines'; guesses: LineGuess[] }
);
export function WinBanner({ guesses, game, title }: Props) {
  const shareResults = () => {
    const right = '🟩';
    const wrong = '🟥';
    const partial = '🟨';
    const higher = '⬆️';
    const lower = '⬇️';

    let results = `Traindle ${game.slice(0, 1).toUpperCase() + game.slice(1)}: ${title}\n`;

    if (game === 'systems') {
      guesses.forEach((g) => {
        results += g.correct ? right : wrong;
        results += g.distanceKm === 0 ? right : wrong;
        results += g.length === 'Lower' ? lower : g.length === 'Higher' ? higher : right;
        results += g.lineCount === 'Lower' ? lower : g.lineCount === 'Higher' ? higher : right;
        results += g.openingYear === 'Lower' ? lower : g.openingYear === 'Higher' ? higher : right;
        results += g.modeMatch === 'Partial' ? partial : g.modeMatch === 'None' ? wrong : right;
        results += '\n';
      });
    } else {
      guesses.forEach((g) => {
        results += g.correct ? right : wrong;
        results += g.distanceKm === 0 ? right : wrong;
        results += g.length === 'Lower' ? lower : g.length === 'Higher' ? higher : right;
        results += g.stationCount === 'Lower' ? lower : g.stationCount === 'Higher' ? higher : right;
        results += g.openingYear === 'Lower' ? lower : g.openingYear === 'Higher' ? higher : right;
        results += g.modeCorrect ? right : wrong;
        results += '\n';
      });
    }

    console.log(guesses);
    navigator.clipboard.writeText(results);
  };

  return (
    <div className='flex flex-wrap items-center justify-between gap-3 rounded-lg border border-neon/40 bg-neon/10 px-4 py-3'>
      <p className='text-primary'>
        You got it in {guesses.length} {guesses.length === 1 ? 'guess' : 'guesses'}!
      </p>

      <button className={buttonClass} onClick={shareResults}>
        Copy Results
      </button>
    </div>
  );
}

const DIRECTIONS = [
  { label: 'N', name: 'North' },
  { label: 'NE', name: 'Northeast' },
  { label: 'E', name: 'East' },
  { label: 'SE', name: 'Southeast' },
  { label: 'S', name: 'South' },
  { label: 'SW', name: 'Southwest' },
  { label: 'W', name: 'West' },
  { label: 'NW', name: 'Northwest' }
];

export function bearingToDirection(bearing: number) {
  const normalized = ((bearing % 360) + 360) % 360; // handles negatives and values > 360
  const index = Math.round(normalized / 45) % 8; // 337.5°+ wraps back to N
  return { ...DIRECTIONS[index], degrees: index * 45 };
}

export function LocationCell({
  correct,
  km,
  bearing
}: {
  correct: boolean;
  km: number | null | undefined;
  bearing: number | null | undefined;
}) {
  const dir = bearing != null ? bearingToDirection(bearing) : null;

  return (
    <Cell correct={correct || km === 0}>
      {km == null ? (
        <span className='text-secondary'>?</span>
      ) : km === 0 ? (
        <Check size={14} className='mx-auto text-neon' aria-label='same place' />
      ) : (
        <span className='inline-flex items-center gap-1 text-primary'>
          {km.toLocaleString()} km
          {dir && <span title={dir.name}>({dir.label})</span>}
        </span>
      )}
    </Cell>
  );
}

export function Modes({ correct, guessed }: { correct: string[]; guessed: string[] }) {
  return (
    <span className='flex flex-wrap justify-center gap-1'>
      {guessed.map((mode) => {
        const right = correct.includes(mode);
        return (
          <span
            key={mode}
            className={`rounded px-1.5 py-0.5 text-xs ${right ? 'bg-neon/20 text-neon' : 'bg-red-500/20 text-red-300'}`}
          >
            {mode.replaceAll('_', ' ')}
          </span>
        );
      })}
    </span>
  );
}
