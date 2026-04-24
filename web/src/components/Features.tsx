import { motion } from 'framer-motion'
import { useScrollReveal } from '../hooks/useScrollReveal'

const features = [
  {
    title: '1,200 HP Powertrain',
    description: 'Twin-turbo hybrid drive with active torque vectoring tuned for apex exits.',
    icon: '🏁',
    accent: 'from-rose-500/70 to-orange-400/60',
  },
  {
    title: 'Aero Intelligence',
    description: 'Adaptive rear wing + DRS synced to telemetry for max downforce on demand.',
    icon: '🛩️',
    accent: 'from-slate-200/70 to-rose-400/60',
  },
  {
    title: 'Thermal Control',
    description: 'Carbon ceramic brakes with smart cooling channels to hold pace lap after lap.',
    icon: '🔥',
    accent: 'from-orange-500/70 to-amber-300/60',
  },
  {
    title: 'Telemetry Suite',
    description: 'Live delta, race craft analytics, and cinematic replays for every overtake.',
    icon: '📊',
    accent: 'from-red-500/70 to-slate-400/60',
  },
]

export const Features = () => {
  const sectionRef = useScrollReveal({ y: 40 })

  return (
    <section ref={sectionRef} id="performance" className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="max-w-6xl mx-auto">
        <div className="flex flex-wrap items-baseline justify-between gap-4">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-rose-100">Performance Highlights</p>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Engineered for ruthless pace</h2>
          </div>
          <p className="max-w-xl text-slate-300">
            Precision-tuned aerodynamics, telemetry-first power delivery, and cinema-grade visuals make every
            overtake feel like a highlight reel.
          </p>
        </div>

        <div className="mt-10 grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {features.map((feature) => (
            <motion.div
              key={feature.title}
              className="group glass-panel relative h-full overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-5 shadow-glow"
              whileHover={{ y: -6, scale: 1.02 }}
              transition={{ type: 'spring', stiffness: 220, damping: 20 }}
            >
              <div
                className={`absolute inset-0 opacity-0 transition-opacity duration-500 group-hover:opacity-60 bg-gradient-to-br ${feature.accent}`}
                aria-hidden
              />
              <div className="relative flex h-12 w-12 items-center justify-center rounded-xl border border-white/15 bg-white/10 text-xl shadow-neon">
                {feature.icon}
              </div>
              <h3 className="relative mt-4 font-display text-xl text-white">{feature.title}</h3>
              <p className="relative mt-2 text-slate-300">{feature.description}</p>
              <div className="relative mt-4 flex items-center gap-2 text-sm text-rose-100">
                <span className="h-1 w-6 rounded-full bg-gradient-to-r from-rose-400 to-orange-300" />
                Learn More
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  )
}
