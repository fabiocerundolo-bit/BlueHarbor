import { useMemo, useState, useEffect } from "react"
import { useApp } from "../../context/AppContext"

const SIZE_COLORS = {
  XL: "#dc2626",
  L:  "#d97706",
  M:  "#2563eb",
  S:  "#7c3aed",
}
const SIZE_WEIGHTS = { XL: 4, L: 3, M: 2, S: 1.5 }

const CONTAINER_COLORS = [
  "#b91c1c", "#c2410c", "#a16207", "#15803d",
  "#0e7490", "#1d4ed8", "#6d28d9", "#9f1239",
  "#854d0e", "#065f46",
]

const SHIP_SPEC = {
  XL: { cols: [10, 12], rows: 3, length: 320, cranes: 2 },
  L:  { cols: [8,  9],  rows: 3, length: 260, cranes: 2 },
  M:  { cols: [6,  7],  rows: 2, length: 200, cranes: 1 },
  S:  { cols: [4,  5],  rows: 2, length: 150, cranes: 1 },
}

function hash(seed, salt) {
  let h = 2166136261
  const s = seed + ":" + salt
  for (let i = 0; i < s.length; i++) {
    h ^= s.charCodeAt(i)
    h = Math.imul(h, 16777619)
  }
  return ((h >>> 0) % 10000) / 10000
}

function ContainerShip({ name, size, pending = false, delay = 0, arriving = false }) {
  const spec = SHIP_SPEC[size] ?? SHIP_SPEC.M
  const accent = SIZE_COLORS[size] ?? "#64748b"

  const { cols, stack } = useMemo(() => {
    const c = Math.floor(hash(name, 0) * (spec.cols[1] - spec.cols[0] + 1)) + spec.cols[0]
    const s = []
    for (let r = 0; r < spec.rows; r++) {
      const row = []
      for (let i = 0; i < c; i++) {
        row.push(CONTAINER_COLORS[Math.floor(hash(name, 1 + r * 100 + i) * CONTAINER_COLORS.length)])
      }
      s.push(row)
    }
    return { cols: c, stack: s }
  }, [name, spec.cols, spec.rows])

  const L = spec.length
  const H = L * 0.085
  const containerW = (L * 0.62) / cols
  const containerH = containerW * 0.62
  const containerGap = 1.2
  const deckLeft = L * 0.16
  const deckRight = L * 0.78
  const deckW = deckRight - deckLeft
  const stackH = stack.length * (containerH + containerGap) - containerGap
  const bridgeW = L * 0.10
  const bridgeH = H * 1.8
  const bridgeX = L * 0.80
  const funnelW = bridgeW * 0.35
  const funnelH = bridgeH * 0.55

  const VBW = L + 4
  const VBH = H + stackH + bridgeH + 24

  const pxWidth = (pending ? 0.55 : 1) * (size === "XL" ? 240 : size === "L" ? 200 : size === "M" ? 160 : 130)
  const pxHeight = pxWidth * (VBH / VBW)

  const hullTop = pending ? "#1f2937" : "#0e2a4a"
  const hullMid = pending ? "#111827" : "#091b30"
  const hullBot = pending ? "#0a0f17" : "#04101c"

  const visibleRows = pending ? Math.max(1, stack.length - 1) : stack.length
  const visibleStack = stack.slice(0, visibleRows)

  const hullY0 = bridgeH + stackH + 4
  const hullY1 = hullY0 + H
  const bowTip = 0
  const sternBack = L
  const bowDeck = L * 0.16

  const hullPath = `
    M ${bowTip + 2} ${hullY1 - 2}
    Q ${bowTip} ${hullY1 - H * 0.4}, ${bowDeck * 0.4} ${hullY0 + H * 0.15}
    L ${bowDeck} ${hullY0}
    L ${sternBack - 2} ${hullY0}
    L ${sternBack} ${hullY0 + H * 0.25}
    L ${sternBack} ${hullY1 - 2}
    Q ${sternBack * 0.6} ${hullY1 + 1}, ${bowTip + 2} ${hullY1 - 2}
    Z
  `

  const animStyle = arriving
    ? { animation: `hv-arrive 1.9s cubic-bezier(0.22,1,0.36,1) ${delay * 0.18}s both` }
    : undefined

  return (
    <div
      className="relative inline-block animate-ship-bob"
      style={{
        width: pxWidth,
        height: pxHeight,
        animationDelay: `${delay}s`,
        filter: pending
          ? "saturate(0.65) brightness(0.7)"
          : `drop-shadow(0 8px 14px ${accent}40) drop-shadow(0 2px 4px rgba(0,0,0,0.6))`,
        opacity: pending ? 0.9 : 1,
        ...animStyle,
      }}
    >
      <svg viewBox={`0 0 ${VBW} ${VBH}`} width="100%" height="100%" style={{ display: "block", overflow: "visible" }}>
        <defs>
          <linearGradient id={`hull-${name.replace(/\s/g,'')}`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%"   stopColor={hullTop} />
            <stop offset="55%"  stopColor={hullMid} />
            <stop offset="100%" stopColor={hullBot} />
          </linearGradient>
          <linearGradient id={`bridge-${name.replace(/\s/g,'')}`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%"   stopColor="#f1f5f9" />
            <stop offset="60%"  stopColor="#94a3b8" />
            <stop offset="100%" stopColor="#475569" />
          </linearGradient>
        </defs>

        {/* Cranes */}
        {Array.from({ length: spec.cranes }).map((_, i) => {
          const cx = deckLeft + deckW * (spec.cranes === 1 ? 0.5 : i === 0 ? 0.28 : 0.62)
          const cTop = hullY0 - stackH - 10
          const cBot = hullY0 - 2
          return (
            <g key={i} stroke="#94a3b8" strokeWidth={0.8} fill="none">
              <line x1={cx} y1={cBot} x2={cx} y2={cTop} />
              <line x1={cx - 10} y1={cTop} x2={cx + 10} y2={cTop} />
              <line x1={cx - 8}  y1={cTop} x2={cx - 8}  y2={cTop + 5} />
              <line x1={cx + 8}  y1={cTop} x2={cx + 8}  y2={cTop + 4} />
            </g>
          )
        })}

        {/* Containers */}
        {visibleStack.map((row, ri) => {
          const rowY = hullY0 - (visibleStack.length - ri) * (containerH + containerGap)
          return row.map((c, ci) => {
            const x = deckLeft + ci * (containerW + containerGap)
            return (
              <g key={`${ri}-${ci}`}>
                <rect x={x} y={rowY} width={containerW} height={containerH} fill={c} />
                <rect x={x} y={rowY} width={containerW} height={Math.max(0.6, containerH * 0.18)} fill="rgba(255,255,255,0.18)" />
                <rect x={x} y={rowY + containerH - Math.max(0.6, containerH * 0.18)} width={containerW} height={Math.max(0.6, containerH * 0.18)} fill="rgba(0,0,0,0.35)" />
                {!pending && containerW > 6 && (
                  <>
                    <line x1={x + containerW * 0.33} y1={rowY + containerH * 0.25} x2={x + containerW * 0.33} y2={rowY + containerH * 0.78} stroke="rgba(0,0,0,0.2)" strokeWidth={0.3} />
                    <line x1={x + containerW * 0.66} y1={rowY + containerH * 0.25} x2={x + containerW * 0.66} y2={rowY + containerH * 0.78} stroke="rgba(0,0,0,0.2)" strokeWidth={0.3} />
                  </>
                )}
                <line x1={x + containerW} y1={rowY} x2={x + containerW} y2={rowY + containerH} stroke="rgba(0,0,0,0.5)" strokeWidth={0.4} />
              </g>
            )
          })
        })}

        {/* Bridge */}
        <g>
          <rect x={bridgeX} y={hullY0 - bridgeH} width={bridgeW} height={bridgeH} fill={`url(#bridge-${name.replace(/\s/g,'')})`} />
          {Array.from({ length: 4 }).map((_, i) => (
            <line key={i}
              x1={bridgeX}              y1={hullY0 - bridgeH + (bridgeH / 5) * (i + 1)}
              x2={bridgeX + bridgeW}    y2={hullY0 - bridgeH + (bridgeH / 5) * (i + 1)}
              stroke="rgba(0,0,0,0.35)" strokeWidth={0.3} />
          ))}
          <rect
            x={bridgeX + bridgeW * 0.1} y={hullY0 - bridgeH + 1.2}
            width={bridgeW * 0.8} height={Math.max(1.2, bridgeH * 0.12)}
            fill="#7dd3fc" opacity={pending ? 0.6 : 1}
            style={{ filter: pending ? "none" : "drop-shadow(0 0 2px #38bdf8)" }}
          />
          {Array.from({ length: 5 }).map((_, i) => (
            <circle key={i}
              cx={bridgeX + (bridgeW / 6) * (i + 1)} cy={hullY0 - bridgeH * 0.45}
              r={Math.max(0.5, bridgeW * 0.04)}
              fill="#fde68a" opacity={pending ? 0.4 : 0.85} />
          ))}
          {/* Funnel */}
          <rect
            x={bridgeX + (bridgeW - funnelW) / 2} y={hullY0 - bridgeH - funnelH}
            width={funnelW} height={funnelH}
            fill="#1f2937" stroke="#0f172a" strokeWidth={0.4} />
          <rect
            x={bridgeX + (bridgeW - funnelW) / 2} y={hullY0 - bridgeH - funnelH * 0.55}
            width={funnelW} height={funnelH * 0.22}
            fill={accent} />
          {/* Mast */}
          <line
            x1={bridgeX + bridgeW / 2} y1={hullY0 - bridgeH - funnelH}
            x2={bridgeX + bridgeW / 2} y2={hullY0 - bridgeH - funnelH - 8}
            stroke="#cbd5e1" strokeWidth={0.4} />
          <circle cx={bridgeX + bridgeW / 2} cy={hullY0 - bridgeH - funnelH - 8} r={0.6} fill="#ef4444">
            {!pending && <animate attributeName="opacity" values="1;0.2;1" dur="2s" repeatCount="indefinite" />}
          </circle>
        </g>

        {/* Hull */}
        <path d={hullPath} fill={`url(#hull-${name.replace(/\s/g,'')})`} stroke="rgba(0,0,0,0.6)" strokeWidth={0.4} />
        <line x1={bowDeck} y1={hullY0 + 0.5} x2={sternBack - 1} y2={hullY0 + 0.5} stroke="rgba(255,255,255,0.25)" strokeWidth={0.5} />
        <path
          d={`M ${bowTip + 4} ${hullY1 - 2.5} Q ${sternBack * 0.5} ${hullY1 - 1}, ${sternBack - 1} ${hullY1 - 2.5}`}
          stroke={pending ? "#94a3b8" : accent} strokeWidth={1} fill="none" opacity={0.7} />
        <circle cx={bowDeck * 0.8} cy={hullY0 + H * 0.45} r={0.8} fill="rgba(255,255,255,0.18)" />

        {/* Ship name on hull */}
        {!pending && size !== "S" && (
          <text x={bowDeck + 4} y={hullY0 + H * 0.7}
            fontSize={H * 0.42} fontWeight={700}
            fill="rgba(255,255,255,0.4)" fontFamily="ui-sans-serif, system-ui" letterSpacing={0.3}>
            {name.toUpperCase()}
          </text>
        )}

        {/* Size badge */}
        <circle cx={bowDeck * 0.6} cy={hullY0 - 4} r={4}
          fill={accent} stroke="rgba(255,255,255,0.4)" strokeWidth={0.4}
          style={{ filter: pending ? "none" : `drop-shadow(0 0 3px ${accent})` }} />
        <text x={bowDeck * 0.6} y={hullY0 - 4}
          textAnchor="middle" dominantBaseline="central"
          fontSize={3.5} fontWeight={800} fill="white" fontFamily="ui-sans-serif, system-ui">
          {size}
        </text>
      </svg>
    </div>
  )
}

export default function HarborView() {
  const { berths, pendingShips, currentDay } = useApp()
  const [arrivedIds, setArrivedIds] = useState(new Set())

  useEffect(() => {
    if (!currentDay) return
    const ids = new Set()
    berths.forEach(b => b.assignments.forEach(a => {
      if (a.status === "Assigned" && a.startDay === currentDay) ids.add(a.id)
    }))
    if (!ids.size) return
    setArrivedIds(ids)
    const t = setTimeout(() => setArrivedIds(new Set()), 2600)
    return () => clearTimeout(t)
  }, [currentDay, berths])

  const isActive = (a) => a.status === "Assigned" && a.startDay <= (currentDay ?? 1) && a.endDay >= (currentDay ?? 1)

  if (berths.length === 0) {
    return (
      <div className="card flex items-center justify-center py-20">
        <p className="text-slate-400 text-sm">Loading harbor data...</p>
      </div>
    )
  }

  return (
    <div className="w-full overflow-hidden rounded-xl border border-white/5 bg-[#03070d] text-slate-100 shadow-[0_30px_80px_-20px_rgba(0,0,0,0.85)]">
      <style>{`
        @keyframes ship-bob { 0%,100%{transform:translateY(0) rotate(0deg)} 50%{transform:translateY(-2.5px) rotate(-0.4deg)} }
        .animate-ship-bob { animation: ship-bob 4.5s ease-in-out infinite; transform-origin: 50% 100%; }
        @keyframes wave-scroll-x { from{transform:translateX(0)} to{transform:translateX(-50%)} }
        @keyframes shimmer { 0%,100%{opacity:0.35} 50%{opacity:0.7} }
        @keyframes twinkle { 0%,100%{opacity:0.3} 50%{opacity:1} }
        @keyframes light-flicker { 0%,100%{opacity:0.9} 47%{opacity:0.85} 50%{opacity:0.4} 53%{opacity:0.9} }
        @keyframes hv-arrive { from{transform:translateY(-70px) scale(0.55);opacity:0.1} to{transform:translateY(0) scale(1);opacity:1} }
      `}</style>

      {/* HEADER */}
      <div className="flex h-12 items-center justify-between border-b border-white/5 bg-gradient-to-r from-[#070f1c] to-[#0c1e34] px-4">
        <div className="flex items-center gap-3">
          <div className="h-2 w-2 rounded-full bg-emerald-400 shadow-[0_0_8px_#10b981]" />
          <h2 className="text-sm font-semibold tracking-wide">
            Harbor View · <span className="text-sky-300">Day {currentDay ?? "—"}</span>
          </h2>
        </div>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-3 text-[10px] text-slate-400">
            {["XL", "L", "M", "S"].map((s) => (
              <div key={s} className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full" style={{ background: SIZE_COLORS[s], boxShadow: `0 0 6px ${SIZE_COLORS[s]}` }} />
                {s}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* WATER / SKY */}
      <div className="relative h-[240px] overflow-hidden">
        <div className="absolute inset-0" style={{ background: "linear-gradient(180deg, #050b18 0%, #07152a 35%, #0a2240 60%, #0c2a4a 75%, #0a1f3a 100%)" }} />

        {/* Moon */}
        <div className="pointer-events-none absolute -left-16 -top-16 h-56 w-56 rounded-full"
          style={{ background: "radial-gradient(circle, rgba(186,230,253,0.25), transparent 70%)" }} />
        <div className="pointer-events-none absolute left-12 top-7 h-10 w-10 rounded-full"
          style={{ background: "radial-gradient(circle, #fff 0%, #e0f2fe 40%, #93c5fd 70%, transparent)", boxShadow: "0 0 50px rgba(186,230,253,0.55), 0 0 20px rgba(186,230,253,0.8)" }} />

        {/* Stars */}
        <div className="absolute inset-x-0 top-0 h-[50%]">
          {Array.from({ length: 70 }).map((_, i) => {
            const x = hash("sx", i) * 100
            const y = hash("sy", i) * 100
            const big = hash("sb", i) > 0.88
            return (
              <span key={i} className="absolute rounded-full bg-white"
                style={{
                  left: `${x}%`, top: `${y}%`,
                  width: big ? 1.6 : 1, height: big ? 1.6 : 1, opacity: 0.6,
                  animation: `twinkle ${2 + hash("sd", i) * 3}s ease-in-out infinite`,
                  animationDelay: `${hash("sdel", i) * 4}s`,
                }} />
            )
          })}
        </div>

        {/* City lights on horizon */}
        <div className="pointer-events-none absolute left-0 right-0" style={{ top: "58%", height: 2 }}>
          {Array.from({ length: 90 }).map((_, i) => {
            const color = hash("c", i) > 0.7 ? "#fde047" : hash("c2", i) > 0.5 ? "#fbbf24" : "#f97316"
            return (
              <span key={i} className="absolute"
                style={{
                  left: `${i * 1.15}%`, top: 0, width: 1, height: 1,
                  background: color, opacity: 0.7 + hash("co", i) * 0.3,
                  boxShadow: `0 0 2px ${color}`,
                  animation: hash("cf", i) > 0.92 ? `light-flicker ${3 + hash("cfd", i) * 2}s infinite` : undefined,
                }} />
            )
          })}
        </div>

        {/* Industrial crane silhouettes */}
        <div className="pointer-events-none absolute bottom-[42%] left-0 right-0 h-[100px]">
          <div className="absolute" style={{ left: "0%", bottom: 0, width: "18%", height: 38, background: "linear-gradient(180deg,#08121f,#040810)", clipPath: "polygon(0 100%, 0 40%, 10% 25%, 25% 35%, 25% 20%, 50% 28%, 75% 20%, 75% 35%, 90% 25%, 100% 40%, 100% 100%)" }} />
          <div className="absolute" style={{ right: "0%", bottom: 0, width: "20%", height: 32, background: "linear-gradient(180deg,#08121f,#040810)", clipPath: "polygon(0 100%, 0 50%, 15% 30%, 35% 45%, 60% 25%, 85% 40%, 100% 30%, 100% 100%)" }} />
          {[{ left: "15%", h: 95, w: 110 }, { left: "38%", h: 78, w: 90 }, { left: "60%", h: 105, w: 130 }, { left: "80%", h: 85, w: 95 }].map((c, i) => {
            const col = "#0a1525"
            const blink = hash("crb", i) > 0.5
            return (
              <div key={i} className="absolute bottom-0" style={{ left: c.left, height: c.h, width: c.w }}>
                <div className="absolute bottom-0 w-[2.5px]" style={{ left: 6, height: c.h * 0.62, background: col }} />
                <div className="absolute bottom-0 w-[2.5px]" style={{ right: 6, height: c.h * 0.62, background: col }} />
                <div className="absolute bottom-0" style={{ left: 8, width: c.w - 16, height: c.h * 0.62, backgroundImage: `linear-gradient(45deg, transparent 48%, ${col} 48%, ${col} 52%, transparent 52%), linear-gradient(-45deg, transparent 48%, ${col} 48%, ${col} 52%, transparent 52%)`, backgroundSize: "20px 20px", opacity: 0.5 }} />
                <div className="absolute h-[3px]" style={{ left: 0, right: 0, bottom: c.h * 0.62, background: col }} />
                <div className="absolute" style={{ left: "30%", bottom: c.h * 0.62, width: 2, height: c.h * 0.3, background: col, transform: "rotate(-12deg)", transformOrigin: "bottom center" }} />
                <div className="absolute" style={{ right: "30%", bottom: c.h * 0.62, width: 2, height: c.h * 0.3, background: col, transform: "rotate(12deg)", transformOrigin: "bottom center" }} />
                <div className="absolute h-[2px]" style={{ left: "30%", right: "30%", bottom: c.h * 0.92, background: col }} />
                <div className="absolute h-[2px]" style={{ left: -20, right: -20, bottom: c.h * 0.95, background: col }} />
                <div className="absolute h-[2px]" style={{ left: 4, width: 36, bottom: c.h * 0.88, background: col }} />
                <div className="absolute" style={{ left: "20%", bottom: c.h * 0.95 - 3, width: 6, height: 3, background: col }} />
                <div className="absolute w-[0.5px]" style={{ left: "21%", bottom: c.h * 0.5, height: c.h * 0.45, background: col, opacity: 0.6 }} />
                <div className="absolute h-[2px] w-[2px] rounded-full" style={{ left: "50%", bottom: c.h - 2, background: "#ef4444", boxShadow: "0 0 3px #ef4444", animation: blink ? "light-flicker 1.6s infinite" : undefined }} />
              </div>
            )
          })}
        </div>

        {/* Water */}
        <div className="absolute inset-x-0" style={{ top: "58%", bottom: 0, background: "linear-gradient(180deg, #061227 0%, #08182f 35%, #0a2040 70%, #0b243f 100%)" }}>
          <div className="pointer-events-none absolute" style={{ left: "5%", top: 0, width: 80, height: "100%", background: "radial-gradient(ellipse at 50% 0%, rgba(186,230,253,0.35), transparent 70%)", filter: "blur(2px)" }} />
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="absolute left-[5%] h-[1px]"
              style={{ top: `${15 + i * 15}%`, width: 60 + hash("rip", i) * 30, background: "linear-gradient(90deg, transparent, rgba(186,230,253,0.7), transparent)", animation: `shimmer ${2 + hash("ri2", i) * 2}s ease-in-out infinite`, animationDelay: `${hash("ri3", i) * 2}s` }} />
          ))}
          <svg className="absolute inset-x-0 top-0 h-full w-[200%]" style={{ animation: "wave-scroll-x 14s linear infinite" }} preserveAspectRatio="none" viewBox="0 0 1200 80">
            <path d="M0 30 Q 50 24, 100 30 T 200 30 T 300 30 T 400 30 T 500 30 T 600 30 T 700 30 T 800 30 T 900 30 T 1000 30 T 1100 30 T 1200 30 L 1200 80 L 0 80 Z" fill="rgba(56,189,248,0.06)" />
          </svg>
          <svg className="absolute inset-x-0 top-2 h-full w-[200%]" style={{ animation: "wave-scroll-x 9s linear infinite" }} preserveAspectRatio="none" viewBox="0 0 1200 80">
            <path d="M0 28 Q 40 22, 80 28 T 160 28 T 240 28 T 320 28 T 400 28 T 480 28 T 560 28 T 640 28 T 720 28 T 800 28 T 880 28 T 960 28 T 1040 28 T 1120 28 T 1200 28 L 1200 80 L 0 80 Z" fill="rgba(56,189,248,0.05)" />
          </svg>
          <svg className="absolute inset-x-0 top-5 h-full w-[200%]" style={{ animation: "wave-scroll-x 6s linear infinite" }} preserveAspectRatio="none" viewBox="0 0 1200 80">
            <path d="M0 20 Q 30 16, 60 20 T 120 20 T 180 20 T 240 20 T 300 20 T 360 20 T 420 20 T 480 20 T 540 20 T 600 20 T 660 20 T 720 20 T 780 20 T 840 20 T 900 20 T 960 20 T 1020 20 T 1080 20 T 1140 20 T 1200 20 L 1200 80 L 0 80 Z" fill="rgba(125,211,252,0.04)" />
          </svg>
        </div>

        {/* Pending ships on horizon */}
        <div className="absolute inset-x-0 flex items-end justify-around px-6" style={{ top: "44%" }}>
          {pendingShips.slice(0, 6).map((p, i) => (
            <div key={p.id} className="flex flex-col items-center gap-1">
              <ContainerShip name={p.name} size={p.size} pending delay={i * 0.5} />
              <div className="text-[9px] font-medium uppercase tracking-wider text-slate-400">
                {p.name} <span className="text-slate-600">· g{p.arrivalDay}</span>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* PIER */}
      <div className="relative h-[130px]">
        {/* Fender bumpers */}
        <div className="pointer-events-none absolute -top-[6px] left-0 right-0 h-[6px] z-10">
          <div className="flex h-full">
            {Array.from({ length: 40 }).map((_, i) => (
              <div key={i} className="flex-1 flex justify-center">
                <div className="w-1.5 h-full" style={{ background: "linear-gradient(180deg,#1a1a1a,#000)", borderRadius: "1px 1px 2px 2px", boxShadow: "0 1px 2px rgba(0,0,0,0.6)" }} />
              </div>
            ))}
          </div>
        </div>

        {/* Concrete surface */}
        <div className="absolute inset-0" style={{ background: "linear-gradient(180deg, #475569 0%, #334155 25%, #1e293b 75%, #0f172a 100%)" }}>
          <div className="absolute inset-0 opacity-[0.15]" style={{ backgroundImage: "radial-gradient(circle at 20% 30%, rgba(255,255,255,0.4) 0.5px, transparent 1px), radial-gradient(circle at 70% 60%, rgba(0,0,0,0.5) 0.5px, transparent 1px), radial-gradient(circle at 40% 80%, rgba(255,255,255,0.3) 0.5px, transparent 1px)", backgroundSize: "8px 8px, 11px 11px, 13px 13px" }} />
          <div className="absolute inset-0 opacity-30" style={{ backgroundImage: "linear-gradient(90deg, transparent 0 calc(20% - 1px), rgba(0,0,0,0.6) calc(20% - 1px) 20%, transparent 20% 40%, rgba(0,0,0,0.6) calc(40% - 1px) 40%, transparent 40% 60%, rgba(0,0,0,0.6) calc(60% - 1px) 60%, transparent 60% 80%, rgba(0,0,0,0.6) calc(80% - 1px) 80%, transparent 80% 100%)" }} />
        </div>

        {/* Yellow safety stripe */}
        <div className="absolute top-0 left-0 right-0 h-[3px]" style={{ background: "repeating-linear-gradient(90deg, #facc15 0 14px, #1a1a1a 14px 22px)", boxShadow: "0 1px 3px rgba(0,0,0,0.6), 0 0 6px rgba(250,204,21,0.3)" }} />

        {/* Berths */}
        <div className="relative flex h-full">
          {berths.map((b) => {
            const active = b.assignments.find(isActive)
            const accent = SIZE_COLORS[b.size] ?? "#64748b"
            const flex = SIZE_WEIGHTS[b.size] ?? 2
            return (
              <div key={b.id} className="relative flex flex-col" style={{ flex }}>
                <div className="absolute top-[4px] bottom-2 right-0 w-[1px]" style={{ background: "rgba(255,255,255,0.1)" }} />

                {/* Bottom label */}
                <div className="absolute bottom-0 left-0 right-0 h-[18px] flex items-center px-2" style={{ background: "linear-gradient(180deg, transparent, rgba(0,0,0,0.5))" }}>
                  <div className="flex items-center gap-1.5 min-w-0">
                    <span className="h-1.5 w-1.5 shrink-0 rounded-full" style={{ background: active ? "#10b981" : "#64748b", boxShadow: active ? "0 0 6px #10b981" : "none" }} />
                    <span className="truncate text-[9px] font-semibold uppercase tracking-wider text-slate-200">{b.name}</span>
                    <span className="text-[8px] uppercase tracking-wider px-1 rounded" style={{ color: active ? "#10b981" : "#94a3b8", background: active ? "rgba(16,185,129,0.1)" : "transparent" }}>
                      {active ? "Occupata" : "Libera"}
                    </span>
                  </div>
                </div>

                {/* Berth code watermark */}
                <div className="pointer-events-none absolute font-black uppercase select-none" style={{ top: 18, left: "50%", transform: "translateX(-50%)", fontSize: b.size === "XL" ? 38 : b.size === "L" ? 32 : b.size === "M" ? 26 : 22, color: "rgba(255,255,255,0.06)", letterSpacing: 2, lineHeight: 1, fontFamily: "ui-sans-serif, system-ui" }}>
                  {b.name.split(" ")[1]}
                </div>

                {/* Bollards */}
                <div className="absolute left-0 right-0 flex justify-around px-3" style={{ top: 8 }}>
                  {[0, 1].map((i) => (
                    <div key={i} className="relative" style={{ width: 8, height: 10 }}>
                      <div className="absolute bottom-0 left-0 right-0 h-[3px] rounded-full" style={{ background: "linear-gradient(180deg,#475569,#1e293b)", boxShadow: "0 1px 2px rgba(0,0,0,0.6)" }} />
                      <div className="absolute bottom-[2px] left-1/2 -translate-x-1/2 w-[4px] h-[6px]" style={{ background: "linear-gradient(90deg,#1f2937,#475569,#1f2937)" }} />
                      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[7px] h-[3.5px] rounded-full" style={{ background: "linear-gradient(180deg,#64748b,#1e293b)", boxShadow: "0 1px 2px rgba(0,0,0,0.7)" }} />
                    </div>
                  ))}
                </div>

                {/* Size badge */}
                <div className="absolute top-1 right-1 flex items-center justify-center rounded text-[8px] font-bold text-white"
                  style={{ width: 14, height: 12, background: accent, boxShadow: `0 0 6px ${accent}80, inset 0 1px 0 rgba(255,255,255,0.3)` }}>
                  {b.size}
                </div>

                {/* Docked ship */}
                {active && (
                  <div className="pointer-events-none absolute left-1/2 z-20 -translate-x-1/2" style={{ bottom: 22 }}>
                    <ContainerShip
                      name={active.shipName}
                      size={b.size}
                      arriving={arrivedIds.has(active.id)}
                    />
                    <svg className="absolute" style={{ left: "-40%", right: "-40%", top: -2, width: "180%", height: 16, overflow: "visible", pointerEvents: "none" }} viewBox="0 0 100 16" preserveAspectRatio="none">
                      <path d="M5 14 Q 30 0, 50 4"  stroke="#cbd5e1" strokeWidth={0.3} fill="none" opacity={0.6} />
                      <path d="M95 14 Q 70 0, 50 4" stroke="#cbd5e1" strokeWidth={0.3} fill="none" opacity={0.6} />
                    </svg>
                  </div>
                )}

                {/* Ship glow on water */}
                {active && (
                  <div className="pointer-events-none absolute left-1/2 -translate-x-1/2"
                    style={{ top: -8, width: "85%", height: 8, background: `radial-gradient(ellipse at center, ${accent}40, transparent 70%)`, filter: "blur(2px)" }} />
                )}
              </div>
            )
          })}
        </div>
      </div>
    </div>
  )
}
