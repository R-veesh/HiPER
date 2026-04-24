import { motion } from 'framer-motion'
import { Link } from 'react-router-dom'
import { useScrollReveal } from '../hooks/useScrollReveal'
import { GamingLogo } from './GamingLogo'

const bannerImage =
  'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2200&q=80&sat=-20'

export const CarBanner = () => {
  const sectionRef = useScrollReveal({ y: 40 })

  return (
    <section
      ref={sectionRef}
      id="buy"
      className="relative z-10 px-6 py-16 sm:px-8 lg:px-14"
    >
      <div className="max-w-6xl mx-auto overflow-hidden rounded-3xl border border-white/10 bg-gradient-to-r from-black/70 via-black/80 to-black/70 shadow-glow">
        <div className="grid items-center gap-0 lg:grid-cols-[1.1fr_0.9fr]">
          <div className="p-8 sm:p-10 space-y-4">
            <div className="inline-flex items-center gap-3 rounded-full border border-white/10 bg-white/5 px-4 py-2 text-xs uppercase tracking-[0.22em] text-rose-100">
              <GamingLogo className="h-5 w-8" />
              Home // HYPER RACING Garage
            </div>
            <p className="text-sm uppercase tracking-[0.28em] text-rose-100">Signature build</p>
            <h3 className="font-display text-3xl text-white sm:text-4xl">VX-09 Tempest — buy the legend</h3>
            <p className="text-slate-300">
              Carbon weave chassis, plasma boost, adaptive aero, and a glow that melts into the skyline. Secure your
              circuit-ready machine before the next season drops.
            </p>
            <div className="flex flex-wrap items-center gap-3 text-sm text-slate-200">
              <span className="rounded-full border border-white/15 bg-white/5 px-3 py-1">Hyper Edition</span>
              <span className="rounded-full border border-rose-300/60 bg-rose-300/10 px-3 py-1 text-rose-100">
                Immediate Delivery
              </span>
              <span className="rounded-full border border-orange-300/60 bg-orange-300/10 px-3 py-1 text-orange-100">
                Includes circuit pass
              </span>
            </div>
            <div className="flex flex-wrap gap-4 pt-2">
              <motion.div whileHover={{ scale: 1.03 }} whileTap={{ scale: 0.98 }}>
                <Link
                  to="/contact"
                  className="neon-button inline-flex items-center gap-2 rounded-full bg-gradient-to-r from-rose-500/80 via-red-500/80 to-orange-400/80 px-6 py-3 font-semibold text-white shadow-neon ring-1 ring-rose-400/60 transition hover:-translate-y-0.5"
                >
                  Buy this machine
                  <span className="text-base">→</span>
                </Link>
              </motion.div>
              <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                <Link
                  to="/categories"
                  className="neon-button inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/5 px-6 py-3 font-semibold text-slate-100 transition hover:-translate-y-0.5 hover:border-rose-300/60"
                >
                  Preview next lap
                </Link>
              </motion.div>
            </div>
          </div>
          <div className="relative h-full min-h-[280px] lg:min-h-[360px] overflow-hidden">
            <div
              className="absolute inset-0 bg-cover bg-center opacity-90"
              style={{ backgroundImage: `url(${bannerImage})` }}
            />
            <div className="absolute inset-0 bg-gradient-to-l from-black/75 via-black/35 to-transparent" />
            <div className="absolute bottom-6 right-6 flex flex-col items-end gap-2 rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-sm text-white">
              <div className="flex items-center gap-2">
                <GamingLogo className="h-4 w-7" />
                HYPER RACING
              </div>
              <div className="flex items-center gap-2 text-rose-100">
                <span className="h-2 w-2 rounded-full bg-rose-300 animate-pulsefast" />
                Ready to ship
              </div>
              <p className="text-xs uppercase tracking-[0.2em] text-rose-100">Engineered for velocity</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
