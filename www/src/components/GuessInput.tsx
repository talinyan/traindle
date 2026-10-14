import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react';

interface GuessInputProps<T> {
  options: T[];
  guessedIds: number[];
  getId: (option: T) => number;
  searchText: (option: T) => (string | null)[];
  renderOption: (option: T) => ReactNode;
  onGuess: (id: number) => Promise<boolean>;
  disabled: boolean;
  placeholder: string;
}

export default function GuessInput<T>({
  options,
  guessedIds,
  getId,
  searchText,
  renderOption,
  onGuess,
  disabled,
  placeholder
}: GuessInputProps<T>) {
  const [query, setQuery] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);

  // Disabling the input while a guess submits drops focus, so restore it once re-enabled
  useEffect(() => {
    if (!disabled) inputRef.current?.focus();
  }, [disabled]);

  // Build suggestions based on query
  const suggestions = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return [];
    const guessed = new Set(guessedIds);
    return options
      .filter((o) => !guessed.has(getId(o)) && searchText(o).some((t) => t?.toLowerCase().includes(q)))
      .slice(0, 10);
  }, [query, options, guessedIds, getId, searchText]);

  async function pick(option: T) {
    if (disabled) return;
    if (await onGuess(getId(option))) setQuery('');
    inputRef.current?.focus();
  }

  return (
    <div className='relative max-w-md'>
      <input
        ref={inputRef}
        type='text'
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === 'Enter' && suggestions[0]) pick(suggestions[0]);
        }}
        disabled={disabled}
        placeholder={placeholder}
        aria-label={placeholder}
        className='w-full rounded-md border border-border bg-white/5 px-3 py-2 text-primary placeholder:text-muted/60 focus:border-purple focus:outline-none disabled:opacity-60'
      />
      {suggestions.length > 0 && (
        <ul className='absolute inset-x-0 top-full z-1000 mt-1 overflow-hidden rounded-md border border-border bg-bg shadow-lg'>
          {suggestions.map((o) => (
            <li key={getId(o)}>
              <button
                type='button'
                onClick={() => pick(o)}
                className='flex w-full items-baseline justify-between gap-3 px-3 py-2 text-left hover:bg-white/5'
              >
                {renderOption(o)}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
