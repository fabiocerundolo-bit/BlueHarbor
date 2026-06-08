import { useApp } from '../context/AppContext'

export default function Header() {
  const { role, setRole, currentDay, dayLoading, advanceDay, clearError } = useApp()

  const handleNextDay = async () => {
    clearError()
    await advanceDay()
  }

  return (
    <header className="bg-navy text-white shadow-lg">
      <div className="max-w-screen-2xl mx-auto px-6 h-16 flex items-center justify-between gap-4">

        {/* Logo */}
        <div className="flex items-center gap-3 flex-shrink-0">
          <svg className="w-7 h-7 opacity-90" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 2a3 3 0 1 0 0 6 3 3 0 0 0 0-6zm0 2a1 1 0 1 1 0 2 1 1 0 0 1 0-2zm-1 5v1H7a1 1 0 0 0 0 2h4v7.27A7.002 7.002 0 0 1 5.08 13H7a1 1 0 0 0 0-2H3a1 1 0 0 0-1 1 9 9 0 0 0 18 0 1 1 0 0 0-1-1h-4a1 1 0 0 0 0 2h1.92A7.002 7.002 0 0 1 13 19.27V12h4a1 1 0 0 0 0-2h-4V9a1 1 0 0 0-2 0z"/>
          </svg>
          <div>
            <span className="text-lg font-bold tracking-tight">BlueHarbor</span>
            <span className="text-navy-200 text-xs ml-2 font-normal hidden sm:inline">Terminal Operations</span>
          </div>
        </div>

        {/* Day indicator */}
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 bg-white/10 rounded-lg px-4 py-1.5">
            <svg className="w-4 h-4 text-blue-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <span className="text-xs text-blue-200 font-medium">Giorno Virtuale</span>
            <span className="text-xl font-bold tabular-nums">
              {currentDay !== null ? currentDay : '—'}
            </span>
          </div>

          <button
            onClick={handleNextDay}
            disabled={dayLoading}
            className="flex items-center gap-2 bg-harbor-accent hover:bg-blue-500 active:bg-blue-700
                       text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors
                       disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {dayLoading ? (
              <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
              </svg>
            ) : (
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M13 5l7 7-7 7M5 5l7 7-7 7" />
              </svg>
            )}
            Next Day
          </button>
        </div>

        {/* Role selector */}
        <div className="flex items-center gap-1 bg-white/10 rounded-lg p-1 flex-shrink-0">
          <button
            onClick={() => setRole('Operatore')}
            className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all duration-150 ${
              role === 'Operatore'
                ? 'bg-white text-navy shadow-sm'
                : 'text-white/70 hover:text-white hover:bg-white/10'
            }`}
          >
            Operatore
          </button>
          <button
            onClick={() => setRole('Scheduler')}
            className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all duration-150 ${
              role === 'Scheduler'
                ? 'bg-white text-navy shadow-sm'
                : 'text-white/70 hover:text-white hover:bg-white/10'
            }`}
          >
            Scheduler
          </button>
        </div>

      </div>
    </header>
  )
}
