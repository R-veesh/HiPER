import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import mapCarImg from '../assets/hero.png'

const mapBg =
  'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=2000&q=80&sat=-14'

const markers = [
  {
    id: 'neon-city',
    name: 'Neon City',
    x: '22%',
    y: '34%',
    status: 'Live event',
    color: 'from-rose-400 to-orange-400',
  },
  {
    id: 'glacier-run',
    name: 'Glacier Run',
    x: '64%',
    y: '42%',
    status: 'Night sprint',
    color: 'from-cyan-300 to-blue-400',
  },
  {
    id: 'desert-apex',
    name: 'Desert Apex',
    x: '48%',
    y: '70%',
    status: 'Drift lobby',
    color: 'from-amber-300 to-rose-300',
  },
  {
    id: 'harbor-loop',
    name: 'Harbor Loop',
    x: '74%',
    y: '28%',
    status: 'Shop',
    color: 'from-emerald-300 to-cyan-300',
  },
]

export const MapPreview = () => {
  return (
    <section className="px-6 py-12 sm:px-8 lg:px-14">
      <div className="mx-auto flex max-w-6xl flex-col gap-6 rounded-3xl border border-white/10 bg-white/5 p-6 shadow-glow">
        <div className="flex flex-wrap items-end justify-between gap-3">
          <div className="space-y-2">
            <div className="flex items-center gap-2 text-sm uppercase tracking-[0.28em] text-rose-100">
              <svg viewBox="0 0 24 24" className="h-4 w-4" fill="currentColor" aria-hidden>
                <path d="M12 2.5c-3.3 0-6 2.6-6 5.9 0 4.3 5.4 10.3 5.6 10.5.2.2.6.2.8 0 .2-.2 5.6-6.2 5.6-10.5 0-3.3-2.7-5.9-6-5.9Zm0 8.2c-1.3 0-2.4-1-2.4-2.3 0-1.3 1-2.3 2.4-2.3s2.4 1 2.4 2.3c0 1.3-1.1 2.3-2.4 2.3Z" />
              </svg>
              <svg viewBox="0 0 24 24" className="h-4 w-4 text-rose-100" fill="currentColor" aria-hidden>
                <path d="M5.3 5.1 10 3v5.2l4 2V3l4.7 2.1c.2.1.3.3.3.5v11.8c0 .2-.1.4-.3.5L14 20v-5.2l-4-2V19L5.3 16.9c-.2-.1-.3-.3-.3-.5V5.6c0-.2.1-.4.3-.5Z" />
              </svg>
              <span>Track map</span>
            </div>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Explore the grid</h2>
            <p className="max-w-2xl text-slate-300">Preview live hotspots, events, and garages before diving into the full map experience.</p>
          </div>
          <div className="flex items-center gap-3">
            <Link
              to="/map"
              className="group overflow-hidden rounded-2xl border border-white/15 bg-white/5 shadow-glow transition hover:-translate-y-0.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-rose-400/60"
              aria-label="Open full Hyper Racing map"
            >
              <div
                className="h-20 w-32 bg-cover bg-center transition duration-500 group-hover:scale-[1.05]"
                style={{ backgroundImage: `url(${mapCarImg})` }}
              />
              <div className="flex items-center justify-between px-3 py-1 text-[11px] uppercase tracking-[0.24em] text-rose-100">
                <span>Hyper Racing Map</span>
                <span className="text-white/80">↗</span>
              </div>
            </Link>
            <Link
              to="/map"
              className="neon-button inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/10 px-4 py-2 text-sm font-semibold text-white"
            >
              Open full map →
            </Link>
          </div>
        </div>

        <div className="relative overflow-hidden rounded-2xl border border-white/10 bg-black/70 p-4">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_center,rgba(255,255,255,0.06),transparent_60%)]" aria-hidden />
          <div className="relative h-[340px] overflow-hidden rounded-xl border border-white/10 bg-gradient-to-br from-slate-900 via-slate-800 to-black">
            <div className="absolute inset-0 opacity-60" aria-hidden>
              <div className="absolute -left-16 top-10 h-56 w-56 rounded-full bg-rose-500/20 blur-3xl" />
              <div className="absolute right-0 bottom-8 h-60 w-60 rounded-full bg-cyan-400/20 blur-3xl" />
            </div>
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_30%,rgba(255,255,255,0.08),transparent_38%),radial-gradient(circle_at_80%_70%,rgba(255,255,255,0.08),transparent_38%)]" />
            <img src={mapBg} alt="Map" className="h-full w-full object-cover opacity-35" />

            {markers.map((marker) => (
              <motion.div
                key={marker.id}
                className="group absolute flex -translate-x-1/2 -translate-y-1/2 flex-col items-center"
                style={{ left: marker.x, top: marker.y }}
                initial={{ scale: 0.9, opacity: 0.85 }}
                whileHover={{ scale: 1.08 }}
                transition={{ type: 'spring', stiffness: 260, damping: 18 }}
              >
                <div
                  className={`relative grid h-10 w-10 place-items-center rounded-full bg-gradient-to-br ${marker.color} shadow-neon`}
                >
                  <svg viewBox="0 0 24 24" className="h-5 w-5 text-white" fill="currentColor" aria-hidden>
                    <path d="M12 2.5c-3.3 0-6 2.6-6 5.9 0 4.3 5.4 10.3 5.6 10.5.2.2.6.2.8 0 .2-.2 5.6-6.2 5.6-10.5 0-3.3-2.7-5.9-6-5.9Zm0 8.2c-1.3 0-2.4-1-2.4-2.3 0-1.3 1-2.3 2.4-2.3s2.4 1 2.4 2.3c0 1.3-1.1 2.3-2.4 2.3Z" />
                  </svg>
                </div>
                <div className="mt-2 rounded-xl border border-white/10 bg-white/10 px-3 py-2 text-xs text-white opacity-0 shadow-lg backdrop-blur group-hover:opacity-100">
                  <p className="font-semibold">{marker.name}</p>
                  <p className="text-[11px] uppercase tracking-[0.2em] text-rose-100">{marker.status}</p>
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
