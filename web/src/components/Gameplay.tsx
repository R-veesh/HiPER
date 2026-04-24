import { useEffect, useRef } from 'react'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'

const scenes = [
  {
    title: 'Neon Overdrive',
    desc: 'Slash through holo billboards while adaptive lighting floods the cockpit.',
    image:
      'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=1600&q=80',
  },
  {
    title: 'Cryo Valley',
    desc: 'Ice tunnels, reflective surfaces, and volumetric snow with motion blur boosts.',
    image:
      'https://images.unsplash.com/photo-1493238792000-8113da705763?auto=format&fit=crop&w=1600&q=80',
  },
  {
    title: 'Ion City Loop',
    desc: 'Vertical drift sections weave across neon megastructures with dynamic traffic.',
    image:
      'https://images.unsplash.com/photo-1472214103451-9374bd1c798e?auto=format&fit=crop&w=1600&q=80',
  },
  {
    title: 'Mirage Circuit',
    desc: 'Heat haze straights, holographic crowd banners, and reactive synth soundtrack.',
    image:
      'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1600&q=80',
  },
]

export const Gameplay = () => {
  const sectionRef = useRef<HTMLDivElement | null>(null)
  const trackRef = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    gsap.registerPlugin(ScrollTrigger)

    const section = sectionRef.current
    const track = trackRef.current
    if (!section || !track) return

    const ctx = gsap.context(() => {
      const totalPanels = scenes.length
      const animation = gsap.to(track, {
        xPercent: -100 * (totalPanels - 1),
        ease: 'none',
        scrollTrigger: {
          trigger: section,
          pin: true,
          scrub: 1.2,
          start: 'top top',
          end: () => `+=${track.scrollWidth - section.offsetWidth}`,
        },
      })

      gsap.utils.toArray<HTMLElement>('.gameplay-panel').forEach((panel) => {
        gsap.fromTo(
          panel,
          { filter: 'blur(6px)', opacity: 0.35 },
            {
              filter: 'blur(0px)',
              opacity: 1,
              duration: 0.8,
              ease: 'power2.out',
              scrollTrigger: {
                trigger: panel,
                containerAnimation: animation,
                start: 'left center',
                toggleActions: 'play none none reverse',
              },
            },
        )
      })
    }, section)

    return () => ctx.revert()
  }, [])

  return (
    <section
      id="gameplay"
      ref={sectionRef}
      className="relative z-10 overflow-hidden bg-gradient-to-b from-night via-carbon to-night py-20"
    >
      <div className="pointer-events-none absolute inset-0 bg-grid opacity-40" aria-hidden />
      <div className="max-w-6xl mx-auto px-6 sm:px-8 lg:px-14">
        <div className="flex items-center justify-between gap-6 pb-10">
          <div>
            <p className="text-sm uppercase tracking-[0.28em] text-cyan-100">Gameplay</p>
            <h2 className="font-display text-3xl text-white sm:text-4xl">Scroll to ride the circuit</h2>
          </div>
          <p className="max-w-md text-slate-300">
            Horizontal scroll is pinned with GSAP ScrollTrigger, revealing cinematic slices of the race world
            with subtle motion blur.
          </p>
        </div>
      </div>

      <div className="relative h-[480px] sm:h-[520px] lg:h-[560px]" aria-label="Horizontal gameplay gallery">
        <div ref={trackRef} className="absolute inset-0 flex">
          {scenes.map((scene, index) => (
            <div
              key={scene.title}
              className="gameplay-panel relative h-full w-screen flex-shrink-0 px-6 sm:px-8 lg:px-14"
            >
              <div className="glass-panel relative h-full overflow-hidden rounded-3xl border border-white/10 shadow-glow">
                <div className="absolute inset-0 bg-gradient-to-r from-black/40 via-black/50 to-transparent" />
                <div
                  className="absolute inset-0 scale-105 bg-cover bg-center"
                  style={{ backgroundImage: `url(${scene.image})` }}
                />
                <div className="absolute right-6 top-6 rounded-full border border-white/15 bg-white/10 px-4 py-2 text-xs uppercase tracking-[0.2em] text-cyan-100">
                  Scene {index + 1}
                </div>
                <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black via-black/60 to-transparent p-6 sm:p-8">
                  <h3 className="font-display text-2xl text-white">{scene.title}</h3>
                  <p className="mt-2 max-w-2xl text-slate-200">{scene.desc}</p>
                  <div className="mt-4 flex items-center gap-3 text-sm text-cyan-100">
                    <span className="h-1 w-8 rounded-full bg-gradient-to-r from-cyan-300 to-fuchsia-300" />
                    Motion blur + parallax layers
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
