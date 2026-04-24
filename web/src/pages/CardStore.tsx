import { motion } from 'framer-motion'
import { PageShell } from '../components/PageShell'
import { GlowCard } from '../components/GlowCard'
import { useGame } from '../context/GameContext'

const cardBg =
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2100&q=80&sat=-14'

export const CardStore = () => {
  const { buyCard, inventory, coins, cardCatalog } = useGame()

  return (
    <PageShell
      eyebrow="Marketplace"
      title="Card Store"
      subtitle="Collect cinematic car cards, level up rarities, and expand your garage loadout."
      backgroundImage={cardBg}
    >
      <div className="grid gap-6 lg:grid-cols-[0.9fr_1.1fr]">
        <GlowCard eyebrow="Reserve" title="Wallet" tone="info">
          <div className="flex items-center justify-between text-3xl font-semibold text-white">
            <span>{coins.toLocaleString()} coins</span>
            <span className="text-sm text-rose-100">Ready to deploy</span>
          </div>
        </GlowCard>

        <GlowCard eyebrow="Collection" title="Owned" tone="success">
          <div className="flex flex-wrap gap-2 text-sm text-slate-200">
            {inventory.map((card) => (
              <span key={card.id} className="rounded-full border border-white/10 bg-white/10 px-3 py-1 text-white">
                {card.name}
              </span>
            ))}
            {inventory.length === 0 ? <span className="text-slate-300">No cards yet—buy one to start.</span> : null}
          </div>
        </GlowCard>
      </div>

      <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {cardCatalog.map((card) => {
          const owned = inventory.some((c) => c.id === card.id)
          return (
            <motion.div
              key={card.id}
              whileHover={{ y: -4, scale: 1.01 }}
              className="glass-panel relative overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-4 shadow-glow"
            >
              <div className="absolute inset-0 bg-gradient-to-b from-white/5 via-transparent to-black/60" aria-hidden />
              <div className="relative h-40 rounded-xl bg-cover bg-center" style={{ backgroundImage: `url(${card.image})` }} />
              <div className="relative mt-3 flex items-center justify-between">
                <div>
                  <p className="text-xs uppercase tracking-[0.24em] text-rose-100">{card.rarity}</p>
                  <h3 className="text-lg font-semibold text-white">{card.name}</h3>
                  <p className="text-sm text-slate-300">{card.price.toLocaleString()} coins</p>
                </div>
                <span className="rounded-full border border-white/15 bg-white/10 px-2 py-1 text-[11px] text-white">{owned ? 'Owned' : 'New'}</span>
              </div>
              <motion.button
                whileTap={{ scale: 0.97 }}
                disabled={owned || coins < card.price}
                onClick={() => void buyCard(card)}
                className="neon-button relative mt-3 w-full rounded-xl bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-4 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-60"
              >
                {owned ? 'In garage' : 'Buy card'}
              </motion.button>
            </motion.div>
          )
        })}
      </div>
    </PageShell>
  )
}
