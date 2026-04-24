import { useRef } from 'react'
import { motion } from 'framer-motion'
import { useScrollReveal } from '../hooks/useScrollReveal'
import { useCounter } from '../hooks/useCounter'

const stats = [
  { label: 'Active Racers', value: 128430 },
  { label: 'Live Matches', value: 842 },
  { label: 'Tracks Uploaded', value: 240 },
]

const leaderboard = [
  { name: 'AeroFlux', score: 98210, country: 'JP', streak: '18x', delta: '+210' },
  { name: 'NovaDrift', score: 95120, country: 'US', streak: '11x', delta: '+130' },
  { name: 'Vanta', score: 93440, country: 'DE', streak: '9x', delta: '+88' },
  { name: 'HelixZero', score: 92110, country: 'BR', streak: '7x', delta: '+64' },
]

export const Leaderboard = () => {
  const sectionRef = useRef<HTMLElement | null>(null)
  const headingRef = useScrollReveal<HTMLDivElement>({ y: 32 })

  const counters = stats.map((stat) => useCounter(stat.value, sectionRef, 1.4))

  return (
    <section ref={sectionRef} id="leaderboard" className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="max-w-6xl mx-auto">
        <div ref={headingRef} className="flex flex-wrap items-baseline justify-between gap-4">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-cyan-100">Leaderboard</p>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Dynamic leaderboards + counters</h2>
          </div>
          <p className="max-w-xl text-slate-300">
            Numbers animate on scroll using GSAP timelines while cards slide in with hover glow.
          </p>
        </div>

        <div className="mt-10 grid gap-6 lg:grid-cols-[1.2fr_0.9fr]">
          <div className="glass-panel overflow-hidden rounded-2xl border border-white/10 bg-white/5 shadow-glow">
            <div className="flex items-center justify-between border-b border-white/10 px-6 py-4 text-sm text-slate-300">
              <span>Rank</span>
              <span className="flex-1 text-center">Pilot</span>
              <span>Score</span>
              <span className="w-16 text-right">Δ</span>
            </div>
            <div className="divide-y divide-white/5">
              {leaderboard.map((entry, index) => (
                <motion.div
                  key={entry.name}
                  className="relative flex items-center gap-4 px-6 py-4"
                  initial={{ opacity: 0, x: 40 }}
                  whileInView={{ opacity: 1, x: 0 }}
                  viewport={{ once: true, margin: '-10%' }}
                  transition={{ duration: 0.6, delay: index * 0.05 }}
                  whileHover={{ scale: 1.01 }}
                >
                  <div className="text-lg font-semibold text-cyan-100">#{index + 1}</div>
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      <span className="font-display text-lg text-white">{entry.name}</span>
                      <span className="rounded-full border border-white/15 bg-white/5 px-2 py-0.5 text-xs uppercase tracking-[0.16em] text-slate-300">
                        {entry.country}
                      </span>
                      <span className="text-xs text-fuchsia-200">Streak {entry.streak}</span>
                    </div>
                  </div>
                  <div className="font-semibold text-white">{entry.score.toLocaleString()}</div>
                  <div className="w-16 text-right text-cyan-100">{entry.delta}</div>
                  <div className="pointer-events-none absolute inset-0 opacity-0 transition-opacity duration-500 hover:opacity-30 bg-gradient-to-r from-cyan-400/30 via-transparent to-fuchsia-400/30" />
                </motion.div>
              ))}
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-3 lg:grid-cols-1">
            {stats.map((stat, index) => (
              <div key={stat.label} className="glass-panel rounded-2xl border border-white/10 bg-white/5 p-5 shadow-glow">
                <p className="text-xs uppercase tracking-[0.24em] text-cyan-100">{stat.label}</p>
                <p className="mt-2 text-3xl font-semibold text-white">
                  {counters[index].toLocaleString()}
                  {stat.label.includes('Live') ? '' : '+'}
                </p>
                <p className="text-sm text-slate-300">Live counter powered by GSAP ScrollTrigger.</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
