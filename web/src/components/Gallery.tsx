import { motion } from 'framer-motion'
import { useScrollReveal } from '../hooks/useScrollReveal'

const gallery = [
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2200&q=80&sat=-12',
  'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2200&q=80&sat=-18',
  'https://images.unsplash.com/photo-1493238792000-8113da705763?auto=format&fit=crop&w=2200&q=80&sat=-18',
  'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=2200&q=80&sat=-18',
  'https://images.unsplash.com/photo-1478720568477-152d9b164e26?auto=format&fit=crop&w=2200&q=80&sat=-12',
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2200&q=80&sat=-22',
]

export const Gallery = () => {
  const sectionRef = useScrollReveal({ y: 32 })

  return (
    <section ref={sectionRef} id="gallery" className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="section-shell">
        <div className="flex flex-wrap items-baseline justify-between gap-4">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-rose-100">Gallery</p>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Cinematic shots in 4K</h2>
          </div>
          <p className="max-w-xl text-slate-300">
            Every capture is a frame from a racing film—glossy carbon, razor silhouettes, and night circuits drenched in
            speed.
          </p>
        </div>

        <div className="mt-10 grid gap-4 md:grid-cols-3">
          {gallery.map((image, idx) => (
            <motion.div
              key={image + idx}
              className="relative h-56 overflow-hidden rounded-2xl border border-white/10 bg-white/5 shadow-glow md:h-64"
              whileHover={{ scale: 1.01, y: -4 }}
            >
              <div
                className="absolute inset-0 bg-cover bg-center transition-transform duration-700 hover:scale-110"
                style={{ backgroundImage: `url(${image})` }}
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black via-black/30 to-transparent" />
              <div className="absolute bottom-3 left-3 text-xs uppercase tracking-[0.2em] text-rose-100">
                Frame {idx + 1}
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  )
}
