import { Link } from 'react-router';

const cardClass =
  'rounded-lg border border-border bg-white/5 px-4 py-5 text-primary transition-colors hover:border-neon';

export default function Home() {
  return (
    <div className='flex flex-col gap-10'>
      <section className='flex flex-col gap-3'>
        <h2 className='text-xl font-semibold'>Daily</h2>
        <p className='text-sm text-secondary'>One puzzle per day - the same for everyone.</p>
        <div className='grid gap-4 sm:grid-cols-2'>
          <Link to='/daily/systems' className={cardClass}>
            Systems
          </Link>
          <Link to='/daily/lines' className={cardClass}>
            Lines
          </Link>
        </div>
      </section>

      <section className='flex flex-col gap-3'>
        <h2 className='text-xl font-semibold'>Endless</h2>
        <p className='text-sm text-secondary'>Random puzzles, as many as you like!</p>
        <div className='grid gap-4 sm:grid-cols-2'>
          <Link to='/endless/systems' className={cardClass}>
            Systems
          </Link>
          <Link to='/endless/lines' className={cardClass}>
            Lines
          </Link>
        </div>
      </section>
    </div>
  );
}
