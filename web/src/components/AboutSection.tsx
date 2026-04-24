import { motion } from 'framer-motion'
import { useScrollReveal } from '../hooks/useScrollReveal'

const aboutImage =
  'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2200&q=80&sat=-16'

export const AboutSection = () => {
  const sectionRef = useScrollReveal({ y: 36 })

  return (
    <section ref={sectionRef} id="about" className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="section-shell grid items-center gap-10 lg:grid-cols-[1.1fr_0.9fr]">
        <div className="space-y-4">
          <p className="text-sm uppercase tracking-[0.28em] text-rose-100">About</p>
          <h2 className="font-display text-3xl text-white sm:text-4xl">Built for drivers. Honed by engineers.</h2>
          <p className="text-slate-300">
            HYPER RACING blends motorsport obsession with cinema-grade visuals. We obsess over aero maps, tire temps,
            and telemetry as much as lighting, VFX, and motion design—so you feel the punch of a launch, the grip of a
            late apex, and the rush of a photo finish.
          </p>
          <div className="grid gap-3 sm:grid-cols-3 text-sm text-slate-200">
            <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
              <p className="text-xs uppercase tracking-[0.2em] text-rose-100">Studios</p>
              <p className="text-lg font-semibold text-white">3</p>
            </div>
            <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
              <p className="text-xs uppercase tracking-[0.2em] text-rose-100">Countries</p>
              <p className="text-lg font-semibold text-white">14</p>
            </div>
            <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
              <p className="text-xs uppercase tracking-[0.2em] text-rose-100">Podiums</p>
              <p className="text-lg font-semibold text-white">92</p>
            </div>
          </div>
        </div>

        <motion.div
          className="relative glass-panel overflow-hidden rounded-3xl border border-white/10 bg-white/5 shadow-glow"
          whileHover={{ y: -6, scale: 1.01 }}
        >
          <div
            className="h-full min-h-[260px] bg-cover bg-center"
            style={{ backgroundImage: `url(${aboutImage})` }}
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black via-black/50 to-transparent" />
          <div className="absolute bottom-4 left-4 rounded-full border border-white/15 bg-white/10 px-4 py-2 text-xs uppercase tracking-[0.2em] text-rose-100">
            Cinematic engineering lab
          </div>
        </motion.div>
      </div>
    </section>
  )
}
