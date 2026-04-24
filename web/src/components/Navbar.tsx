import { Link, NavLink } from 'react-router-dom'
import { motion } from 'framer-motion'
import { GamingLogo } from './GamingLogo'

const navItems = [
  { label: 'Home', to: '/' },
  { label: 'Cars', to: '/cars' },
  { label: 'Categories', to: '/categories' },
  { label: 'Gallery', to: '/gallery' },
  { label: 'Card Store', to: '/card-store' },
  { label: 'Spin', to: '/spin' },
  { label: 'Profile', to: '/profile' },
]

export const Navbar = () => {
  return (
    <header className="fixed left-0 right-0 top-0 z-30 backdrop-blur-xl">
      <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 rounded-full border border-white/10 bg-black/70 px-6 py-3 shadow-glow">
        <Link to="/" className="flex items-center gap-3">
          <div className="h-10 w-10 overflow-hidden rounded-full border border-white/15 bg-white/5 p-1 shadow-neon">
            <GamingLogo className="h-full w-full" />
          </div>
          <div>
            <p className="text-xs uppercase tracking-[0.3em] text-rose-100">HYPER RACING</p>
            <p className="text-sm text-slate-300">Cinematic racing brand</p>
          </div>
        </Link>

        <nav className="hidden items-center gap-5 text-sm text-slate-200 sm:flex">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `transition hover:text-rose-100 ${isActive ? 'text-rose-200' : ''}`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-3">
          <motion.div whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
            <Link
              to="/coin-store"
              className="neon-button inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/10 px-4 py-2 text-sm font-semibold text-rose-100"
            >
              Coin Store <span className="h-2 w-2 rounded-full bg-rose-300 animate-pulsefast" />
            </Link>
          </motion.div>
          <motion.div whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
            <Link
              to="/login"
              className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-2 text-sm font-semibold text-white"
            >
              Login
            </Link>
          </motion.div>
        </div>
      </div>
    </header>
  )
}
