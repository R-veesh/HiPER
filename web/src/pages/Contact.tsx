import { SectionTitle } from '../components/SectionTitle'
import { motion } from 'framer-motion'

const contactImage =
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2200&q=80&sat=-18'

export const Contact = () => {
  return (
    <section className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="section-shell grid gap-10 lg:grid-cols-[1.05fr_0.95fr] items-center">
        <div className="space-y-6">
          <SectionTitle
            eyebrow="Contact"
            title="Book your build and join the grid"
            description="Tell us your racing goals. Our engineers will spec a hypercar setup and prepare you for the next season."
          />

          <div className="grid gap-3 sm:grid-cols-2 text-sm text-slate-200">
            <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
              <p className="text-xs uppercase tracking-[0.2em] text-rose-100">Pit wall</p>
              <p className="text-lg font-semibold text-white">+1 (800) HYPER-01</p>
            </div>
            <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
              <p className="text-xs uppercase tracking-[0.2em] text-rose-100">Garage HQ</p>
              <p className="text-lg font-semibold text-white">garage@hyperracing.gg</p>
            </div>
          </div>

          <motion.form
            className="glass-panel relative overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-6 shadow-glow"
            whileHover={{ y: -2 }}
          >
            <div className="absolute inset-0 bg-gradient-to-br from-rose-500/10 via-transparent to-orange-400/10" aria-hidden />
            <div className="relative grid gap-4 sm:grid-cols-2">
              <label className="space-y-1 text-sm text-slate-200">
                <span>Name</span>
                <input
                  type="text"
                  placeholder="Ava Velocity"
                  className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                />
              </label>
              <label className="space-y-1 text-sm text-slate-200">
                <span>Email</span>
                <input
                  type="email"
                  placeholder="driver@hyper.gg"
                  className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                />
              </label>
              <label className="space-y-1 text-sm text-slate-200 sm:col-span-2">
                <span>What do you want to build?</span>
                <input
                  type="text"
                  placeholder="Track-focused VX-09 with endurance aero"
                  className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                />
              </label>
              <label className="space-y-1 text-sm text-slate-200 sm:col-span-2">
                <span>Message</span>
                <textarea
                  rows={4}
                  placeholder="Share your racing goals, circuits, and timeline."
                  className="w-full rounded-xl border border-white/10 bg-black/40 px-3 py-3 text-white placeholder:text-slate-500 focus:border-rose-300/70 focus:outline-none"
                />
              </label>
              <div className="sm:col-span-2 flex items-center justify-between gap-3">
                <div className="text-xs uppercase tracking-[0.2em] text-slate-400">We reply within 24h</div>
                <motion.button
                  type="submit"
                  className="neon-button rounded-full bg-gradient-to-r from-rose-500/80 via-red-500/80 to-orange-400/80 px-6 py-3 text-sm font-semibold text-white shadow-neon ring-1 ring-rose-400/60 transition"
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                >
                  Send request
                </motion.button>
              </div>
            </div>
          </motion.form>
        </div>

        <motion.div
          className="glass-panel relative h-full min-h-[320px] overflow-hidden rounded-3xl border border-white/10 bg-white/5 shadow-glow"
          whileHover={{ y: -6, scale: 1.01 }}
        >
          <div
            className="absolute inset-0 bg-cover bg-center"
            style={{ backgroundImage: `url(${contactImage})` }}
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black via-black/60 to-transparent" />
          <div className="absolute bottom-4 left-4 right-4 flex items-center justify-between text-sm text-white">
            <div className="rounded-full border border-white/15 bg-white/10 px-4 py-2 uppercase tracking-[0.2em] text-rose-100">
              Dedicated race concierge
            </div>
            <div className="flex items-center gap-2 text-rose-100">
              <span className="h-2 w-2 rounded-full bg-rose-300 animate-pulsefast" />
              Online now
            </div>
          </div>
        </motion.div>
      </div>
    </section>
  )
}
