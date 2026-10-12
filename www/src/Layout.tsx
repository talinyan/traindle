import { Link, NavLink, Outlet } from "react-router";

const navClass = ({ isActive }: { isActive: boolean }) =>
  `relative flex h-full items-center px-1 text-sm transition-colors ${
    isActive
      ? "text-primary after:absolute after:inset-x-0 after:bottom-0 after:h-0.5 after:rounded-full after:bg-purple"
      : "text-secondary hover:text-primary"
  }`;

export default function Layout() {
  return (
    <div className="flex min-h-svh flex-col">
      {/* Above the map and the search dropdown (z-1000); translucent so content blurs underneath */}
      <header className="sticky top-0 z-1100 border-b border-border bg-bg/85 backdrop-blur">
        <div className="mx-auto flex h-14 max-w-6xl items-center justify-between gap-4 px-4 sm:px-6">
          <Link
            to="/"
            className="flex items-center gap-2 font-display text-lg font-semibold text-fg"
          >
            <span
              aria-hidden
              className="h-2.5 w-2.5 rounded-full bg-neon shadow-[0_0_12px] shadow-neon/60"
            />
            Traindle
          </Link>
          <nav className="flex h-full gap-4 sm:gap-7">
            <NavLink to="/" end className={navClass}>
              Home
            </NavLink>
          </nav>
        </div>
      </header>

      <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-6 sm:px-6 sm:py-10">
        <Outlet />
      </main>

      <footer className="border-t border-border">
        <p className="mx-auto max-w-6xl px-4 py-6 text-xs text-dim sm:px-6">
          Data from{" "}
          <a
            href="https://www.citylines.co/"
            target="_blank"
            rel="noreferrer"
            className="text-primary"
          >
            Citylines.co
          </a>
          . This site is not affiliated in any way.
        </p>
      </footer>
    </div>
  );
}
