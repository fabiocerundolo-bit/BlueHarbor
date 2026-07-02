const LABELS = {
  Pending:  'Pending',
  Assigned: 'Assigned',
  Departed: 'Departed',
}

const STYLES = {
  Pending:  'bg-amber-100 text-amber-700 border border-amber-200',
  Assigned: 'bg-blue-100 text-blue-700 border border-blue-200',
  Departed: 'bg-emerald-100 text-emerald-700 border border-emerald-200',
}

export default function StatusBadge({ status }) {
  const style = STYLES[status] ?? 'bg-slate-100 text-slate-600'
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${style}`}>
      {LABELS[status] ?? status}
    </span>
  )
}
