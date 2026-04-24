import { useMemo, useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { PageShell } from '../components/PageShell'
import { GlowCard } from '../components/GlowCard'
import { useGame } from '../context/GameContext'
import type { MapPack } from '../types/game'

const mapStoreBg =
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2100&q=80&sat=-14'

const featuredMapImg =
  'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2100&q=80&sat=-12'

type Filter = 'all' | 'owned' | 'locked' | 'premium' | 'urban' | 'night' | 'desert' | 'mountain'

export const MapStore = () => {
  const { coins, maps, buyMapPack, mapCatalog } = useGame()
  const [filter, setFilter] = useState<Filter>('all')
  const [query, setQuery] = useState('')
  const [activeMap, setActiveMap] = useState<MapPack | null>(null)

  const filtered = useMemo(() => {
    return mapCatalog.filter((map) => {
      const owned = maps.some((m) => m.id === map.id)
      if (filter === 'owned' && !owned) return false
      if (filter === 'locked' && owned) return false
      if (filter === 'premium' && map.tier !== 'Premium') return false
      if (filter === 'urban' && map.category !== 'Urban') return false
      if (filter === 'night' && map.region.toLowerCase().includes('night') === false) return false
      if (filter === 'desert' && map.category !== 'Desert') return false
      if (filter === 'mountain' && map.category !== 'Mountain') return false
      if (query && !map.name.toLowerCase().includes(query.toLowerCase()) && !map.region.toLowerCase().includes(query.toLowerCase())) return false
      return true
    })
  }, [filter, query, maps, mapCatalog])

  const ownedCount = maps.length

  const buy = (pack: MapPack) => {
    void buyMapPack(pack)
  }

  return (
    <PageShell
      eyebrow="Marketplace"
      title="Command the Grid"
      subtitle="Secure next-gen circuits, cinematic sprints, and elite locations tuned for Hyper Racing. Unlock, deploy, and own the world’s most advanced tracks."
      backgroundImage={mapStoreBg}
      cta={<span className="rounded-full border border-white/15 bg-white/10 px-4 py-2 text-sm text-white">{coins.toLocaleString()} coins</span>}
    >
      <div className="relative overflow-hidden rounded-3xl border border-white/10 bg-white/5 p-6 shadow-glow">
        <div className="absolute inset-0 bg-gradient-to-r from-rose-500/10 via-transparent to-cyan-400/10" aria-hidden />
        <div className="grid gap-6 lg:grid-cols-[1.1fr_0.9fr] items-center">
          <div className="space-y-3 relative z-10">
            <p className="text-sm uppercase tracking-[0.28em] text-rose-100">Unlock new worlds</p>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Buy premium racing maps</h2>
            <p className="text-slate-200">Claim exclusive circuits engineered for holographic billboards, reactive lighting, and weather-adaptive grip. Each track is curated for cinematic race nights.</p>
            <div className="flex flex-wrap gap-3 text-sm text-slate-100">
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1">Premium circuits</span>
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1">Dynamic FX</span>
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1">Event-ready</span>
            </div>
            <div className="flex items-center gap-3">
              <motion.button
                whileTap={{ scale: 0.97 }}
                onClick={() => setFilter('premium')}
                className="neon-button rounded-full bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-5 py-3 text-sm font-semibold text-white"
              >
                View premium tracks
              </motion.button>
              <span className="rounded-full border border-white/15 bg-white/10 px-3 py-2 text-sm text-white">Balance: {coins.toLocaleString()} c</span>
            </div>
          </div>
          <div className="relative z-10 overflow-hidden rounded-2xl border border-white/10 bg-black/50">
            <div className="absolute inset-0 bg-gradient-to-b from-transparent via-black/20 to-black/70" />
            <img src={featuredMapImg} alt="Featured racing map" className="h-full w-full object-cover" />
            <div className="absolute left-4 bottom-4 flex items-center gap-2 text-xs uppercase tracking-[0.24em] text-white">
              <svg viewBox="0 0 24 24" className="h-4 w-4" fill="currentColor" aria-hidden>
                <path d="M12 2.5c-3.3 0-6 2.6-6 5.9 0 4.3 5.4 10.3 5.6 10.5.2.2.6.2.8 0 .2-.2 5.6-6.2 5.6-10.5 0-3.3-2.7-5.9-6-5.9Zm0 8.2c-1.3 0-2.4-1-2.4-2.3 0-1.3 1-2.3 2.4-2.3s2.4 1 2.4 2.3c0 1.3-1.1 2.3-2.4 2.3Z" />
              </svg>
              <span>Hyper Racing Map</span>
            </div>
          </div>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-[0.9fr_1.1fr]">
        <GlowCard eyebrow="Filters" title="Curate your tracklist" tone="info">
          <div className="flex flex-wrap items-center gap-2">
            {[
              { key: 'all', label: 'All Maps' },
              { key: 'premium', label: 'Premium Tracks' },
              { key: 'owned', label: 'Owned Maps' },
              { key: 'locked', label: 'Locked Maps' },
              { key: 'urban', label: 'Urban Maps' },
              { key: 'night', label: 'Night Tracks' },
              { key: 'desert', label: 'Desert Tracks' },
              { key: 'mountain', label: 'Mountain Routes' },
            ].map((item) => (
              <button
                key={item.key}
                onClick={() => setFilter(item.key as Filter)}
                className={`rounded-full border px-3 py-1 text-sm transition ${
                  filter === item.key
                    ? 'border-rose-300/60 bg-white/10 text-white'
                    : 'border-white/15 bg-white/5 text-slate-200 hover:border-rose-200/50'
                }`}
              >
                {item.label}
              </button>
            ))}
          </div>
          <div className="mt-4">
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search maps (name or region)"
              className="w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
            />
          </div>
        </GlowCard>

          <GlowCard eyebrow="Vault" title="Purchased maps" tone="success">
            <div className="flex flex-wrap gap-2 text-sm text-slate-200">
              {ownedCount ? (
                maps.map((map) => (
                  <span key={map.id} className="rounded-full border border-white/10 bg-white/10 px-3 py-1 text-white">
                    {map.name}
                </span>
              ))
            ) : (
              <span className="text-slate-300">No maps unlocked yet.</span>
            )}
          </div>
        </GlowCard>
      </div>

      <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <AnimatePresence>
          {filtered.map((pack) => {
            const owned = maps.some((m) => m.id === pack.id)
            const affordable = coins >= pack.price
            return (
              <motion.div
                key={pack.id}
                layout
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                whileHover={{ y: -6, scale: 1.01 }}
                className="glass-panel relative overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-4 shadow-glow"
              >
                <div className="absolute inset-0 bg-gradient-to-b from-white/5 via-transparent to-black/60" aria-hidden />
                <div
                  role="button"
                  onClick={() => setActiveMap(pack)}
                  className="relative h-44 cursor-pointer overflow-hidden rounded-xl"
                >
                  <div className="absolute inset-0 bg-gradient-to-t from-black/55 via-black/20 to-transparent" />
                  <img src={pack.image} alt={pack.name} className="h-full w-full object-cover" />
                  <div className="absolute left-3 top-3 flex flex-wrap items-center gap-2">
                    <span className="rounded-full border border-white/20 bg-white/10 px-2 py-1 text-[11px] text-white">{pack.region}</span>
                    {pack.tier === 'Premium' ? (
                      <span className="rounded-full border border-amber-300/60 bg-amber-300/10 px-2 py-1 text-[11px] text-amber-100">Premium</span>
                    ) : null}
                    <span className="rounded-full border border-white/20 bg-white/10 px-2 py-1 text-[11px] text-white">{pack.rarity}</span>
                  </div>
                </div>
                <div className="relative mt-3 flex items-start justify-between gap-3">
                  <div>
                    <h3 className="text-lg font-semibold text-white">{pack.name}</h3>
                    <p className="text-sm text-slate-300">{pack.description}</p>
                    <p className="mt-2 text-sm text-rose-100">{pack.price.toLocaleString()} coins</p>
                  </div>
                  <span className="rounded-full border border-white/15 bg-white/10 px-2 py-1 text-[11px] text-white">
                    {owned ? 'Unlocked' : 'Locked'}
                  </span>
                </div>
                <motion.button
                  whileTap={{ scale: 0.97 }}
                  disabled={owned || !affordable}
                  onClick={() => buy(pack)}
                className="neon-button relative mt-3 w-full rounded-xl bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-4 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-60"
              >
                  {owned ? 'In rotation' : affordable ? 'Unlock track' : 'Insufficient coins'}
                </motion.button>
              </motion.div>
            )
          })}
        </AnimatePresence>
      </div>

      <AnimatePresence>
        {activeMap ? (
          <motion.div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={() => setActiveMap(null)}
          >
            <motion.div
              onClick={(e) => e.stopPropagation()}
              initial={{ scale: 0.96, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.96, opacity: 0 }}
              className="glass-panel w-full max-w-3xl overflow-hidden rounded-3xl border border-white/15 bg-white/5 shadow-neon"
            >
              <div className="relative h-64 w-full">
                <img src={activeMap.image} alt={activeMap.name} className="h-full w-full object-cover" />
                <div className="absolute inset-0 bg-gradient-to-t from-black via-black/40 to-transparent" />
                <div className="absolute left-4 bottom-4 flex items-center gap-3">
                  <span className="rounded-full border border-white/20 bg-white/10 px-3 py-1 text-sm text-white">{activeMap.region}</span>
                  {activeMap.tier === 'Premium' ? (
                    <span className="rounded-full border border-amber-300/60 bg-amber-300/10 px-3 py-1 text-sm text-amber-100">Premium</span>
                  ) : null}
                </div>
              </div>
              <div className="p-6 space-y-3">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <p className="text-xs uppercase tracking-[0.24em] text-rose-100">Map profile</p>
                    <h3 className="text-2xl font-semibold text-white">{activeMap.name}</h3>
                    <p className="text-sm text-slate-300">{activeMap.description}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-slate-300">Price</p>
                    <p className="text-xl font-semibold text-white">{activeMap.price.toLocaleString()} coins</p>
                  </div>
                </div>
                <div className="flex flex-wrap gap-3 text-sm text-slate-200">
                  <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1">Dynamic weather</span>
                  <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1">HDR lighting</span>
                  <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1">Replay-ready</span>
                </div>
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <motion.button
                    whileTap={{ scale: 0.98 }}
                    className="rounded-xl border border-white/20 bg-white/5 px-4 py-2 text-sm text-white"
                    onClick={() => setActiveMap(null)}
                  >
                    Close
                  </motion.button>
                  <motion.button
                    whileTap={{ scale: 0.98 }}
                    disabled={maps.some((m) => m.id === activeMap.id) || coins < activeMap.price}
                    onClick={() => {
                      buy(activeMap)
                      setActiveMap(null)
                    }}
                    className="neon-button rounded-xl bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-5 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    {maps.some((m) => m.id === activeMap.id)
                      ? 'Unlocked'
                      : coins >= activeMap.price
                        ? 'Unlock now'
                        : 'Insufficient coins'}
                  </motion.button>
                </div>
              </div>
            </motion.div>
          </motion.div>
        ) : null}
      </AnimatePresence>
    </PageShell>
  )
}
