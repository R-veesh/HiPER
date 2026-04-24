import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import { PageShell } from '../components/PageShell'
import { GlowCard } from '../components/GlowCard'
import { useGame } from '../context/GameContext'

const mapBg =
  'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=2100&q=80&sat=-14'

const mapMarkers = [
  {
    id: 'neon-city',
    name: 'Neon City Circuit',
    x: '24%',
    y: '36%',
    status: 'Live event',
    type: 'Race',
    description: 'Chromed skyline sprint with rain FX, holo ads, and tight chicanes.',
    image: 'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=1400&q=80&sat=-14',
    color: 'from-rose-400 to-orange-400',
  },
  {
    id: 'glacier-run',
    name: 'Glacier Run',
    x: '62%',
    y: '44%',
    status: 'Night sprint',
    type: 'Race',
    description: 'Icy switchbacks under auroras, slippery apexes, and snow bursts.',
    image: 'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=1600&q=80&sat=-20',
    color: 'from-cyan-300 to-blue-400',
  },
  {
    id: 'desert-apex',
    name: 'Desert Apex',
    x: '48%',
    y: '68%',
    status: 'Drift lobby',
    type: 'Event',
    description: 'Canyon dust plumes, golden-hour sweeps, perfect for drift trains.',
    image: 'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=1600&q=80&sat=-14',
    color: 'from-amber-300 to-rose-300',
  },
  {
    id: 'harbor-loop',
    name: 'Harbor Loop',
    x: '72%',
    y: '28%',
    status: 'Shop',
    type: 'Shop',
    description: 'Coastal endurance loop with container-yard pit shops and wet asphalt.',
    image: 'https://images.unsplash.com/photo-1493236296276-d17357e28875?auto=format&fit=crop&w=1600&q=80&sat=-16',
    color: 'from-emerald-300 to-cyan-300',
  },
  {
    id: 'skyline-ascent',
    name: 'Skyline Ascent',
    x: '16%',
    y: '58%',
    status: 'Garage',
    type: 'Garage',
    description: 'Vertical climb pads, helipad jumps, and tuning bays above the clouds.',
    image: 'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2000&q=80&sat=-16',
    color: 'from-purple-400 to-pink-400',
  },
]

type Filter = 'all' | 'Race' | 'Shop' | 'Garage' | 'Event'

export const MapPage = () => {
  const { maps } = useGame()
  const [selectedId, setSelectedId] = useState<string>(mapMarkers[0].id)
  const [filter, setFilter] = useState<Filter>('all')
  const [zoom, setZoom] = useState(1.05)

  const selected = useMemo(() => mapMarkers.find((m) => m.id === selectedId) ?? mapMarkers[0], [selectedId])
  const visibleMarkers = useMemo(
    () => mapMarkers.filter((m) => (filter === 'all' ? true : m.type === filter)),
    [filter],
  )

  return (
    <PageShell
      eyebrow="Command"
      title="Hyper Racing Command Grid"
      subtitle="Navigate an elite control center built for live race intel—scan hotspots, jump into active lobbies, and dive into premium zones across the illuminated grid."
      backgroundImage={mapBg}
      cta={
        <Link
          to="/map-store"
          className="neon-button rounded-full border border-white/15 bg-white/10 px-4 py-2 text-sm font-semibold text-white"
        >
          Buy maps
        </Link>
      }
    >
      <div className="grid gap-6 xl:grid-cols-[1.35fr_0.85fr]">
        <div className="space-y-4">
          <GlowCard eyebrow="Filters" title="Recon channels" tone="info">
            <div className="flex flex-wrap gap-2 text-sm text-slate-200">
              {['all', 'Race', 'Shop', 'Garage', 'Event'].map((f) => (
                <button
                  key={f}
                  onClick={() => setFilter(f as Filter)}
                  className={`rounded-full border px-3 py-1 transition ${
                    filter === f
                      ? 'border-rose-300/60 bg-white/10 text-white'
                      : 'border-white/15 bg-white/5 text-slate-200 hover:border-rose-200/50'
                  }`}
                >
                  {f === 'all' ? 'All' : f}
                </button>
              ))}
            </div>
          </GlowCard>

          <div className="relative overflow-hidden rounded-3xl border border-white/10 bg-black/70 p-4 shadow-glow">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_center,rgba(255,255,255,0.06),transparent_60%)]" aria-hidden />
            <div className="relative h-[520px] overflow-hidden rounded-2xl border border-white/10 bg-gradient-to-br from-slate-900 via-slate-800 to-black">
              <div className="absolute inset-0 opacity-60" aria-hidden>
                <div className="absolute -left-20 top-10 h-64 w-64 rounded-full bg-rose-500/20 blur-3xl" />
                <div className="absolute right-0 bottom-10 h-72 w-72 rounded-full bg-cyan-400/20 blur-3xl" />
              </div>
              <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_30%,rgba(255,255,255,0.08),transparent_38%),radial-gradient(circle_at_80%_70%,rgba(255,255,255,0.08),transparent_38%)]" />

              <div className="absolute left-4 top-4 z-20 flex items-center gap-2">
                <button
                  onClick={() => setZoom((z) => Math.min(1.35, z + 0.1))}
                  className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-white"
                >
                  +
                </button>
                <button
                  onClick={() => setZoom((z) => Math.max(0.9, z - 0.1))}
                  className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-white"
                >
                  −
                </button>
              </div>

              <div className="absolute inset-0" aria-hidden>
                <img
                  src={mapBg}
                  alt="Map texture"
                  className="h-full w-full object-cover opacity-35"
                  style={{ transform: `scale(${zoom})`, transformOrigin: 'center' }}
                />
              </div>

              {visibleMarkers.map((marker) => {
                const isActive = marker.id === selected.id
                return (
                  <motion.div
                    key={marker.id}
                    className="group absolute flex -translate-x-1/2 -translate-y-1/2 flex-col items-center"
                    style={{ left: marker.x, top: marker.y }}
                    initial={{ scale: 0.9, opacity: 0.8 }}
                    animate={{ scale: isActive ? 1.1 : 1, opacity: 1 }}
                    transition={{ type: 'spring', stiffness: 260, damping: 18 }}
                    onClick={() => setSelectedId(marker.id)}
                  >
                    <div
                      className={`relative grid h-10 w-10 place-items-center rounded-full bg-gradient-to-br ${marker.color} shadow-neon`}
                    >
                      <svg
                        viewBox="0 0 24 24"
                        className="h-5 w-5 text-white"
                        fill="currentColor"
                        aria-hidden
                      >
                        <path d="M12 2.5c-3.3 0-6 2.6-6 5.9 0 4.3 5.4 10.3 5.6 10.5.2.2.6.2.8 0 .2-.2 5.6-6.2 5.6-10.5 0-3.3-2.7-5.9-6-5.9Zm0 8.2c-1.3 0-2.4-1-2.4-2.3 0-1.3 1-2.3 2.4-2.3s2.4 1 2.4 2.3c0 1.3-1.1 2.3-2.4 2.3Z" />
                      </svg>
                      {isActive ? (
                        <span className="absolute inset-[-6px] rounded-full border border-white/50 opacity-80 animate-ping" />
                      ) : null}
                    </div>
                    <div className="mt-2 rounded-xl border border-white/10 bg-white/10 px-3 py-2 text-xs text-white opacity-0 shadow-lg backdrop-blur group-hover:opacity-100">
                      <p className="font-semibold">{marker.name}</p>
                      <p className="text-[11px] uppercase tracking-[0.2em] text-rose-100">{marker.status}</p>
                    </div>
                  </motion.div>
                )
              })}

              <AnimatePresence>
                {selected ? (
                  <motion.div
                    key={selected.id}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: 10 }}
                    className="pointer-events-none absolute left-6 bottom-6 w-[300px] rounded-2xl border border-white/15 bg-black/70 p-3 shadow-neon"
                  >
                    <div className="flex gap-3">
                      <div
                        className="h-14 w-14 flex-none rounded-xl bg-cover bg-center"
                        style={{ backgroundImage: `url(${selected.image})` }}
                      />
                      <div className="space-y-1 text-sm text-slate-200">
                        <p className="text-xs uppercase tracking-[0.2em] text-rose-100">{selected.status}</p>
                        <p className="font-semibold text-white">{selected.name}</p>
                        <p className="text-[13px] text-slate-300 line-clamp-2">{selected.description}</p>
                      </div>
                    </div>
                  </motion.div>
                ) : null}
              </AnimatePresence>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <GlowCard eyebrow="Locations" title="Selectable list" tone="default">
            <div className="max-h-[360px] space-y-3 overflow-y-auto pr-1">
              {visibleMarkers.map((loc) => {
                const active = loc.id === selected.id
                return (
                  <button
                    key={loc.id}
                    onClick={() => setSelectedId(loc.id)}
                    className={`w-full rounded-2xl border px-4 py-3 text-left transition ${
                      active
                        ? 'border-rose-300/60 bg-white/10 text-white shadow-neon'
                        : 'border-white/10 bg-white/5 text-slate-200 hover:border-rose-200/40'
                    }`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <div className="flex items-center gap-2">
                        <svg viewBox="0 0 24 24" className="h-4 w-4 text-rose-100" fill="currentColor" aria-hidden>
                          <path d="M12 2.5c-3.3 0-6 2.6-6 5.9 0 4.3 5.4 10.3 5.6 10.5.2.2.6.2.8 0 .2-.2 5.6-6.2 5.6-10.5 0-3.3-2.7-5.9-6-5.9Zm0 8.2c-1.3 0-2.4-1-2.4-2.3 0-1.3 1-2.3 2.4-2.3s2.4 1 2.4 2.3c0 1.3-1.1 2.3-2.4 2.3Z" />
                        </svg>
                        <p className="font-semibold text-white">{loc.name}</p>
                      </div>
                      <span className="rounded-full border border-white/15 bg-white/10 px-2 py-1 text-[11px] text-white">
                        {loc.type}
                      </span>
                    </div>
                    <p className="text-sm text-slate-300">{loc.description}</p>
                    <p className="text-xs uppercase tracking-[0.2em] text-rose-100">{loc.status}</p>
                  </button>
                )
              })}
            </div>
          </GlowCard>

          <GlowCard eyebrow="Details" title="Selected location" tone="info">
            <div className="flex gap-3">
              <div
                className="h-24 w-24 flex-none rounded-xl bg-cover bg-center"
                style={{ backgroundImage: `url(${selected.image})` }}
              />
              <div className="space-y-1 text-sm text-slate-200">
                <div className="flex items-center gap-2 text-xs uppercase tracking-[0.2em] text-rose-100">
                  <svg viewBox="0 0 24 24" className="h-4 w-4 text-rose-100" fill="currentColor" aria-hidden>
                    <path d="M12 2.5c-3.3 0-6 2.6-6 5.9 0 4.3 5.4 10.3 5.6 10.5.2.2.6.2.8 0 .2-.2 5.6-6.2 5.6-10.5 0-3.3-2.7-5.9-6-5.9Zm0 8.2c-1.3 0-2.4-1-2.4-2.3 0-1.3 1-2.3 2.4-2.3s2.4 1 2.4 2.3c0 1.3-1.1 2.3-2.4 2.3Z" />
                  </svg>
                  <span>{selected.status}</span>
                </div>
                <p className="text-lg font-semibold text-white">{selected.name}</p>
                <p className="text-sm text-slate-300">{selected.description}</p>
                <div className="flex flex-wrap gap-2 text-[12px] text-slate-200">
                  <span className="rounded-full border border-white/15 bg-white/10 px-2 py-1">HDR lighting</span>
                  <span className="rounded-full border border-white/15 bg-white/10 px-2 py-1">Replay cams</span>
                  <span className="rounded-full border border-white/15 bg-white/10 px-2 py-1">Dynamic weather</span>
                </div>
              </div>
            </div>
          </GlowCard>

          <GlowCard eyebrow="Unlocked" title="Owned maps" tone="success">
            <div className="flex flex-wrap gap-2 text-sm text-slate-200">
              {maps.length ? (
                maps.map((m) => (
                  <span key={m.id} className="rounded-full border border-white/10 bg-white/10 px-3 py-1 text-white">
                    {m.name}
                  </span>
                ))
              ) : (
                <span className="text-slate-300">No maps unlocked yet.</span>
              )}
            </div>
          </GlowCard>
        </div>
      </div>
    </PageShell>
  )
}
