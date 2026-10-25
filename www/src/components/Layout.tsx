import { NavLink, Outlet } from 'react-router';
import { Flip, ToastContainer } from 'react-toastify';

const navClass = ({ isActive }: { isActive: boolean }) =>
  `relative flex h-full items-center px-1 text-sm transition-colors ${
    isActive
      ? 'text-primary after:absolute after:inset-x-0 after:bottom-0 after:h-0.5 after:rounded-full after:bg-purple'
      : 'text-secondary hover:text-primary'
  }`;

export default function Layout() {
  return (
    <div className='flex min-h-svh flex-col'>
      <header className='sticky top-0 z-1100 border-b border-border bg-bg/85 backdrop-blur'>
        <div className='mx-auto grid h-14 max-w-6xl grid-cols-[1fr_auto_1fr] items-center gap-4 px-4 sm:px-6'>
          <div className='flex items-center gap-2 justify-self-start font-display text-lg font-semibold text-fg'>
            <span aria-hidden className='h-2.5 w-2.5 rounded-full bg-neon shadow-[0_0_12px] shadow-neon/60' />
            Traindle
          </div>

          <nav className='flex h-full gap-4 sm:gap-7'>
            <NavLink to='/' end className={navClass}>
              Play Traindle
            </NavLink>
          </nav>

          <nav className='flex h-full justify-self-end'>
            <NavLink to='/changelog' end className={navClass}>
              Changes
            </NavLink>
          </nav>
        </div>
      </header>

      <main className='mx-auto w-full max-w-6xl flex-1 px-4 py-6 sm:px-6 sm:py-10'>
        <Outlet />
      </main>

      <footer className='border-t border-border'>
        <p className='mx-auto max-w-6xl px-4 py-6 text-xs text-dim sm:px-6'>
          Data from{' '}
          <a href='https://www.citylines.co/' target='_blank' rel='noreferrer' className='text-primary'>
            Citylines.co
          </a>
          . This site is not affiliated in any way.
        </p>
      </footer>

      <ToastContainer
        theme='dark'
        position='bottom-right'
        hideProgressBar
        closeOnClick
        autoClose={1500}
        transition={Flip}
      />
    </div>
  );
}
