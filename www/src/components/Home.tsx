import { Link, useSearchParams } from 'react-router';

const cardClass =
  'group flex flex-col gap-1 rounded-lg border border-border bg-white/5 px-4 py-5 text-primary transition-colors hover:border-neon focus-visible:border-neon focus-visible:outline-none';

const chipBase =
  'cursor-pointer shrink-0 rounded-sm border px-3 py-1.5 text-xs font-medium whitespace-nowrap transition-colors focus-visible:outline-none focus-visible:border-neon';
const chipIdle = 'border-border bg-white/5 text-secondary hover:border-neon hover:text-primary';
const chipActive = 'border-neon bg-neon/10 text-primary';

const CONTINENTS = [
  { value: 'Africa', label: 'Africa', src: '/africa.svg' },
  { value: 'Asia', label: 'Asia', src: '/asia.svg' },
  { value: 'Europe', label: 'Europe', src: '/europe.svg' },
  { value: 'NorthAmerica', label: 'North America', src: '/north_america.svg' },
  { value: 'Oceania', label: 'Oceania', src: '/oceania.svg' },
  { value: 'SouthAmerica', label: 'South America', src: '/south_america.svg' }
];

const MODES = [
  { path: 'systems', title: 'Systems', description: 'Guess the rail network' },
  { path: 'lines', title: 'Lines', description: 'Guess the individual line' }
];

function RegionPicker({ value, onChange }: { value: string | null; onChange: (value: string | null) => void }) {
  return (
    <div role='group' aria-label='Region' className='-mx-4 flex gap-2 overflow-x-auto px-4 pb-1 scrollbar-none'>
      <button
        type='button'
        aria-pressed={!value}
        onClick={() => onChange(null)}
        className={`${chipBase} ${!value ? chipActive : chipIdle} inline-flex items-center gap-1.5`}
      >
        <img src='/globe.svg' alt='' className='h-4 w-4 shrink-0' />
        Global
      </button>

      <span aria-hidden className='mx-1 w-px shrink-0 self-stretch bg-border' />

      {CONTINENTS.map((c) => {
        const active = value === c.value;
        return (
          <button
            key={c.value}
            type='button'
            aria-pressed={active}
            onClick={() => onChange(c.value)}
            className={`${chipBase} ${active ? chipActive : chipIdle} inline-flex items-center gap-1.5`}
          >
            <img src={c.src} alt='' className='h-6 w-6 shrink-0' />
            {c.label}
          </button>
        );
      })}
    </div>
  );
}

function ModeCards({ kind, query = '' }: { kind: 'daily' | 'endless'; query?: string }) {
  return (
    <div className='grid gap-4 sm:grid-cols-2'>
      {MODES.map((m) => (
        <Link key={m.path} to={`/${kind}/${m.path}${query}`} className={cardClass}>
          <span className='font-semibold'>{m.title}</span>
          <span className='text-sm text-secondary'>{m.description}</span>
        </Link>
      ))}
    </div>
  );
}

export default function Home() {
  const [searchParams, setSearchParams] = useSearchParams();
  const continent = searchParams.get('continent');

  const setContinent = (value: string | null) =>
    setSearchParams(value ? { continent: value } : {}, { replace: true, preventScrollReset: true });

  const query = continent ? `?continent=${continent}` : '';

  return (
    <div className='flex flex-col gap-10'>
      <section className='flex flex-col gap-3'>
        <h2 className='text-xl font-semibold'>Daily</h2>
        <p className='text-sm text-secondary'>One puzzle per day - the same for everyone.</p>
        <ModeCards kind='daily' />
      </section>

      <section className='flex flex-col gap-3'>
        <h2 className='text-xl font-semibold'>Endless</h2>
        <p className='text-sm text-secondary'>Random puzzles, as many as you like!</p>
        <RegionPicker value={continent} onChange={setContinent} />
        <ModeCards kind='endless' query={query} />
      </section>
    </div>
  );
}
