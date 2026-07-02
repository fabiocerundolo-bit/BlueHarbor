import { useApp } from './context/AppContext'
import Header from './components/Header'
import OperatorPage from './pages/OperatorPage'
import SchedulerPage from './pages/SchedulerPage'

/**
 * Component for displaying a global red banner
 * when errors occur during harbor API calls.
 */
function ErrorBanner() {
  const { error, clearError } = useApp()
  if (!error) return null
  return (
    <div className="bg-red-50 border-b border-red-200">
      <div className="w-[80%] mx-auto py-2.5 flex items-center justify-between gap-4">
        <div className="flex items-center gap-2 text-sm text-red-700">
          <svg className="w-4 h-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          {error}
        </div>
        <button onClick={clearError} className="text-red-400 hover:text-red-600 flex-shrink-0">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </div>
  )
}

/**
 * Main React application component.
 * Manages the main layout structure, conditional view rendering
 * based on the active user role (Operator vs Scheduler), and the navigation bar/footer.
 */
export default function App() {
  const { role } = useApp()

  return (
    <div className="min-h-screen flex flex-col">
      {/* Header containing role switching and day advancement */}
      <Header />
      
      {/* Global error notification banner */}
      <ErrorBanner />
      
      {/* Main layout section: switches views based on the current role */}
      <main className="flex-1 w-[80%] mx-auto py-6">
        {role === 'Operator' ? <OperatorPage /> : <SchedulerPage />}
      </main>
      
      {/* Informational footer for the BlueHarbor terminal */}
      <footer className="border-t border-slate-100 bg-white mt-auto">
        <div className="w-[80%] mx-auto py-3 flex items-center justify-between text-xs text-slate-400">
          <span>BlueHarbor Terminal Operations &copy; {new Date().getFullYear()}</span>
          <span className={`px-2 py-0.5 rounded-full font-medium ${
            role === 'Operator'
              ? 'bg-amber-100 text-amber-700'
              : 'bg-blue-100 text-blue-700'
           }`}>
            Session: {role}
          </span>
        </div>
      </footer>
    </div>
  )
}
