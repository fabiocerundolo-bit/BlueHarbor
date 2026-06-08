const STYLES = {
  XL: 'bg-red-100 text-red-700 border border-red-200',
  L:  'bg-orange-100 text-orange-700 border border-orange-200',
  M:  'bg-blue-100 text-blue-700 border border-blue-200',
  S:  'bg-violet-100 text-violet-700 border border-violet-200',
}

export default function SizeBadge({ size }) {
  const style = STYLES[size] ?? 'bg-slate-100 text-slate-600'
  return (
    <span className={`inline-flex items-center justify-center w-7 h-7 rounded-md text-xs font-bold ${style}`}>
      {size}
    </span>
  )
}
