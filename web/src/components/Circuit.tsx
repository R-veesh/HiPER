import { motion } from 'framer-motion'
import { useScrollReveal } from '../hooks/useScrollReveal'
import { categories as categoryData } from '../data/categories'
import { SectionTitle } from './SectionTitle'

type CircuitProps = {
  eyebrow?: string
  title?: string
  description?: string
  limit?: number
}

export const Circuit = ({ eyebrow, title, description, limit }: CircuitProps) => {
  const sectionRef = useScrollReveal({ y: 40 })
  const circuits = limit ? categoryData.slice(0, limit) : categoryData

  return (
    <section
      ref={sectionRef}
      id="categories"
      className="relative z-10 overflow-hidden px-6 py-20 sm:px-8 lg:px-14"
    >
      <div className="absolute inset-x-0 -top-32 h-64 bg-gradient-to-b from-rose-500/10 via-red-500/10 to-transparent blur-3xl" />
      <div className="max-w-6xl mx-auto relative">
        <div className="flex flex-wrap items-center justify-between gap-6">
          <SectionTitle
            eyebrow={eyebrow ?? 'Racing Categories'}
            title={title ?? 'Choose your battleground'}
            description={
              description ??
              'Circuit domination, sprint aggression, or smoke-filled drift arenas—scroll to pick how you rewrite the leaderboard.'
            }
          />
          <div className="inline-flex items-center gap-3 rounded-full border border-white/10 bg-white/5 px-5 py-3 text-sm text-slate-100 shadow-neon">
            <span className="h-2 w-2 rounded-full bg-rose-300 animate-pulsefast" />
            Live session scanning · {circuits.length} formats loaded
          </div>
        </div>

        <div className="mt-10 grid gap-6 md:grid-cols-3">
          {circuits.map((circuit, index) => (
            <motion.div
              key={circuit.name}
              className="group glass-panel relative overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-4 shadow-glow"
              whileHover={{ y: -8, scale: 1.02 }}
              transition={{ type: 'spring', stiffness: 220, damping: 20 }}
            >
              <div className="absolute inset-0 bg-gradient-to-b from-white/5 via-transparent to-black/50" aria-hidden />
              <div
                className="h-40 rounded-xl bg-cover bg-center transition-transform duration-700 group-hover:scale-110"
                style={{ backgroundImage: `url(${circuit.image})` }}
              />
              <div className="relative mt-4 flex items-center justify-between text-xs uppercase tracking-[0.24em] text-rose-100">
                <span>{circuit.mood}</span>
                <span className="rounded-full border border-white/15 bg-white/5 px-2 py-1 text-[11px] text-slate-200">
                  {circuit.distance}
                </span>
              </div>
              <h3 className="relative mt-2 font-display text-xl text-white">{circuit.name}</h3>
              <p className="relative mt-1 text-sm text-slate-300">Tap the lap bar to preview banking and lighting.</p>
              <div className="relative mt-4 flex items-center gap-2 text-sm text-rose-100">
                <span className="h-1 w-8 rounded-full bg-gradient-to-r from-rose-400 to-orange-300" />
                Slot {index + 1} • Ready
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  )
}
