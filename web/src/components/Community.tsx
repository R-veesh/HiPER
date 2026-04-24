import { motion } from 'framer-motion'
import { useScrollReveal } from '../hooks/useScrollReveal'

const testimonials = [
  {
    name: 'Lena “Nitro” Park',
    role: 'Esports Pilot',
    quote: 'HyperRacing nails the low-latency feel. Drifts snap instantly and replays look unreal.',
  },
  {
    name: 'Diego Cruz',
    role: 'Track Architect',
    quote: 'The custom track builder is crazy intuitive. Weather + light kits make every circuit pop.',
  },
  {
    name: 'Kai Nakamura',
    role: 'Clan Captain',
    quote: 'Squad lobbies, shared ghosts, and seasonal resets keep our crew grinding every night.',
  },
]

export const Community = () => {
  const sectionRef = useScrollReveal<HTMLDivElement>({ y: 30 })

  return (
    <section ref={sectionRef} id="community" className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="max-w-6xl mx-auto">
        <div className="flex flex-wrap items-baseline justify-between gap-4">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-cyan-100">Community</p>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Testimonials + sliding carousel</h2>
          </div>
          <p className="max-w-xl text-slate-300">
            Framer Motion drives a continuous marquee so player stories glide across the viewport.
          </p>
        </div>

        <div className="mt-8 overflow-hidden rounded-2xl border border-white/10 bg-white/5 shadow-glow">
          <motion.div
            className="flex gap-6"
            animate={{ x: ['0%', '-50%'] }}
            transition={{ repeat: Infinity, duration: 16, ease: 'linear' }}
          >
            {[...testimonials, ...testimonials].map((item, index) => (
              <div
                key={`${item.name}-${index}`}
                className="min-w-[320px] max-w-sm flex-1 rounded-2xl border border-white/10 bg-white/5 p-6"
              >
                <div className="flex items-center justify-between text-xs uppercase tracking-[0.18em] text-cyan-100">
                  <span>{item.role}</span>
                  <span className="rounded-full bg-cyan-400/10 px-3 py-1 text-[11px] text-cyan-100">Verified</span>
                </div>
                <p className="mt-3 text-lg font-semibold text-white">{item.name}</p>
                <p className="mt-2 text-slate-200">“{item.quote}”</p>
              </div>
            ))}
          </motion.div>
        </div>
      </div>
    </section>
  )
}
