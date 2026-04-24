import { motion } from 'framer-motion'
import type { Ref } from 'react'
import type { Car } from '../data/cars'

type CarCardProps = {
  car: Car
  tiltRef?: Ref<HTMLDivElement>
}

export const CarCard = ({ car, tiltRef }: CarCardProps) => {
  return (
    <motion.div
      ref={tiltRef}
      className="group glass-panel relative overflow-hidden rounded-2xl border border-white/10 bg-white/5 shadow-glow"
      whileHover={{ y: -6, scale: 1.02 }}
      transition={{ type: 'spring', stiffness: 220, damping: 18 }}
    >
      <div className="absolute inset-0 bg-gradient-to-b from-white/5 via-transparent to-black/50" aria-hidden />
      <div
        className="h-48 w-full bg-cover bg-center transition-transform duration-700 group-hover:scale-110"
        style={{ backgroundImage: `url(${car.image})` }}
      />
      <div className="relative p-4">
        <div className="flex items-center justify-between text-xs uppercase tracking-[0.24em] text-rose-100">
          <span>{car.class}</span>
          <span className="rounded-full border border-white/15 bg-white/5 px-2 py-0.5 text-[11px] text-slate-200">
            {car.color}
          </span>
        </div>
        <h3 className="mt-2 font-display text-xl text-white">{car.name}</h3>
        <p className="text-sm text-slate-300">Top speed {car.speed}</p>
        <p className="mt-2 text-sm text-slate-200/80">{car.blurb}</p>
        <div className="mt-4 flex items-center gap-2 text-sm text-rose-100">
          <span className="h-1 w-8 rounded-full bg-gradient-to-r from-rose-400 to-orange-300" />
          Launch + aero tuned
        </div>
      </div>
    </motion.div>
  )
}
