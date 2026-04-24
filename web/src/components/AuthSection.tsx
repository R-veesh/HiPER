import { motion } from 'framer-motion'
import { useScrollReveal } from '../hooks/useScrollReveal'

const socialButtons = [
  { label: 'Continue with Google', color: 'from-rose-400/60 to-orange-400/40' },
  { label: 'Continue with Apple', color: 'from-slate-200/60 to-slate-500/40' },
]

export const AuthSection = () => {
  const sectionRef = useScrollReveal({ y: 40 })

  return (
    <section ref={sectionRef} id="auth" className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="max-w-6xl mx-auto">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-rose-100">Login & Register</p>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Drop into the pit lane</h2>
            <p className="max-w-xl text-slate-300">
              Claim your seat, sync progress across devices, and unlock the Velocity garage with one click sign-ins.
            </p>
          </div>
          <div className="rounded-full border border-white/10 bg-white/5 px-4 py-2 text-sm text-slate-100 shadow-neon">
            Secure by design · OAuth ready
          </div>
        </div>

        <div className="mt-8 grid gap-6 lg:grid-cols-2">
          <motion.div
            className="glass-panel relative overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-6 shadow-glow"
            whileHover={{ y: -4 }}
          >
            <div className="absolute inset-0 bg-gradient-to-br from-rose-500/12 via-transparent to-orange-400/12" aria-hidden />
            <div className="relative space-y-4">
              <h3 className="font-display text-2xl text-white">Login</h3>
              <div className="grid gap-3 sm:grid-cols-2">
                <label className="space-y-1 text-sm text-slate-200">
                  <span>Email</span>
                  <input
                    type="email"
                    placeholder="you@velocity.gg"
                    className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                  />
                </label>
                <label className="space-y-1 text-sm text-slate-200">
                  <span>Password</span>
                  <input
                    type="password"
                    placeholder="••••••••"
                    className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                  />
                </label>
              </div>
              <motion.button
                className="neon-button w-full rounded-xl bg-gradient-to-r from-rose-500/80 via-red-500/80 to-orange-400/80 px-4 py-3 font-semibold text-white shadow-neon ring-1 ring-rose-400/40 transition"
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
              >
                Enter the cockpit
              </motion.button>
              <div className="grid gap-3 sm:grid-cols-2">
                {socialButtons.map((button) => (
                  <motion.button
                    key={button.label}
                    className={`neon-button w-full rounded-xl border border-white/10 bg-gradient-to-r ${button.color} px-4 py-3 text-sm font-semibold text-white transition`}
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.98 }}
                  >
                    {button.label}
                  </motion.button>
                ))}
              </div>
            </div>
          </motion.div>

          <motion.div
            className="glass-panel relative overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-6 shadow-glow"
            whileHover={{ y: -4 }}
          >
            <div className="absolute inset-0 bg-gradient-to-br from-orange-500/12 via-transparent to-rose-500/12" aria-hidden />
            <div className="relative space-y-4">
              <h3 className="font-display text-2xl text-white">Register</h3>
              <div className="grid gap-3 sm:grid-cols-2">
                <label className="space-y-1 text-sm text-slate-200">
                  <span>Full name</span>
                  <input
                    type="text"
                    placeholder="Ada Velocity"
                    className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                  />
                </label>
                <label className="space-y-1 text-sm text-slate-200">
                  <span>Team</span>
                  <input
                    type="text"
                    placeholder="Phoenix Crew"
                    className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                  />
                </label>
              </div>
              <div className="grid gap-3 sm:grid-cols-2">
                <label className="space-y-1 text-sm text-slate-200">
                  <span>Email</span>
                  <input
                    type="email"
                    placeholder="crew@hyper.gg"
                    className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                  />
                </label>
                <label className="space-y-1 text-sm text-slate-200">
                  <span>Password</span>
                  <input
                    type="password"
                    placeholder="Create a password"
                    className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                  />
                </label>
              </div>
              <motion.button
                className="neon-button w-full rounded-xl bg-gradient-to-r from-rose-500/70 via-red-500/70 to-orange-400/70 px-4 py-3 font-semibold text-white shadow-neon ring-1 ring-rose-400/40 transition"
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
              >
                Create account & buy
              </motion.button>
              <div className="flex items-center justify-between text-xs uppercase tracking-[0.18em] text-slate-400">
                <span>Instant verification</span>
                <span>Secure checkout ready</span>
              </div>
            </div>
          </motion.div>
        </div>
      </div>
    </section>
  )
}
