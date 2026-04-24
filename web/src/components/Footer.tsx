import { motion } from 'framer-motion'

const socials = [
  { name: 'Discord', href: '#', icon: 'M17.472 3.338a13.227 13.227 0 00-3.134-.982.05.05 0 00-.052.024c-.132.234-.278.54-.381.777-1.124-.168-2.24-.168-3.348 0-.103-.246-.256-.543-.389-.777a.05.05 0 00-.052-.024 13.276 13.276 0 00-3.134.982.046.046 0 00-.021.018C2.045 7.073 1.363 10.64 1.5 14.169a.052.052 0 00.019.037 13.38 13.38 0 004.061 2.064.05.05 0 00.054-.017c.313-.426.591-.875.829-1.346a.05.05 0 00-.027-.069 8.802 8.802 0 01-1.248-.595.05.05 0 01-.005-.083c.084-.063.168-.129.248-.195a.05.05 0 01.051-.007c2.619 1.202 5.454 1.202 8.041 0a.05.05 0 01.053.007c.08.066.164.132.248.195a.05.05 0 01-.004.083 8.3 8.3 0 01-1.249.594.05.05 0 00-.026.07c.24.47.518.92.828 1.345a.05.05 0 00.054.018 13.31 13.31 0 004.062-2.064.05.05 0 00.019-.036c.335-3.451-.56-7.004-2.366-10.813a.038.038 0 00-.02-.02zM8.02 12.584c-.789 0-1.438-.724-1.438-1.614 0-.89.637-1.614 1.438-1.614.807 0 1.45.73 1.438 1.614 0 .89-.637 1.614-1.438 1.614zm5.96 0c-.789 0-1.438-.724-1.438-1.614 0-.89.637-1.614 1.438-1.614.807 0 1.45.73 1.438 1.614 0 .89-.63 1.614-1.438 1.614z' },
  { name: 'Twitch', href: '#', icon: 'M3 3h18v13.5l-5.25 5.25H12l-1.5-1.5H6V15H3V3zm3 2.25V15h6.75l1.5 1.5H15l3.75-3.75V5.25H6zm9 1.5v5.25h-2.25V6.75H15zm-4.5 0v5.25H8.25V6.75H10.5z' },
  { name: 'YouTube', href: '#', icon: 'M21.8 8.001s-.2-1.4-.8-2c-.8-.8-1.6-.8-2-1.001C16.6 4.7 12 4.7 12 4.7h-.1s-4.6 0-7 .3c-.4.1-1.3.1-2.1 1-.6.6-.8 2-.8 2S0 9.6 0 11.2v1.5c0 1.6.2 3.2.2 3.2s.2 1.4.8 2c.8.8 1.8.8 2.2.9 1.6.2 6.8.3 6.8.3s4.6 0 7-.3c.4-.1 1.2-.1 2-1 .6-.6.8-2 .8-2s.2-1.6.2-3.2v-1.5c0-1.6-.2-3.2-.2-3.2zM9.6 14.4V8.7l5.4 2.85-5.4 2.85z' },
]

export const Footer = () => {
  return (
    <footer className="relative z-10 overflow-hidden border-t border-white/10 bg-gradient-to-b from-black/70 via-black/80 to-[#050608] px-6 py-12 sm:px-8 lg:px-14">
      <div className="absolute inset-0 bg-animated opacity-60" aria-hidden />
      <div className="pointer-events-none absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-rose-400/60 to-transparent" />
      <div className="max-w-6xl mx-auto relative">
        <div className="flex flex-wrap items-center justify-between gap-6">
          <div>
            <h3 className="font-display text-2xl text-white">HYPER RACING</h3>
            <p className="text-slate-300">Cinematic hypercar culture. Built for speed and spectacle.</p>
          </div>
          <div className="flex items-center gap-3">
            {socials.map((social) => (
              <motion.a
                key={social.name}
                href={social.href}
                className="group relative inline-flex h-12 w-12 items-center justify-center overflow-hidden rounded-full border border-white/15 bg-white/5 text-white shadow-glow"
                whileHover={{ scale: 1.08 }}
                whileTap={{ scale: 0.96 }}
              >
                <div className="absolute inset-0 bg-gradient-to-br from-rose-500/25 to-orange-400/25 opacity-0 transition group-hover:opacity-100" />
                <svg className="relative h-6 w-6 fill-current text-rose-100" viewBox="0 0 24 24" aria-hidden>
                  <path d={social.icon} />
                </svg>
              </motion.a>
            ))}
          </div>
        </div>

        <div className="mt-8 h-px bg-gradient-to-r from-transparent via-white/20 to-transparent" aria-hidden />
        <div className="mt-6 flex flex-wrap items-center justify-between gap-4 text-sm text-slate-400">
          <span>© {new Date().getFullYear()} HYPER RACING. All rights reserved.</span>
          <div className="flex gap-4 text-rose-100">
            <span className="h-px w-12 bg-gradient-to-r from-rose-400 to-orange-300" />
            Racing in every frame.
          </div>
        </div>
      </div>
    </footer>
  )
}
