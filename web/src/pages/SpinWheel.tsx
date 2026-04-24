import { useMemo, useState } from 'react'
import { motion, useAnimation } from 'framer-motion'
import { PageShell } from '../components/PageShell'
import { GlowCard } from '../components/GlowCard'
import { useGame } from '../context/GameContext'

const spinBg =
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2100&q=80&sat=-12'

export const SpinWheel = () => {
  const { coins, spinWheel, spins, spinRewards, spinCost } = useGame()
  const controls = useAnimation()
  const [spinning, setSpinning] = useState(false)
  const [result, setResult] = useState<string>('')
  const [error, setError] = useState('')

  const spin = async () => {
    if (spinning) return
    setSpinning(true)
    setError('')
    await controls.start({ rotate: 1440 + Math.random() * 360, transition: { duration: 2.4, ease: 'easeInOut' } })
    try {
      const outcome = await spinWheel()
      setResult(outcome.reward)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Spin failed')
    }
    setSpinning(false)
  }

  const segments = useMemo(() => spinRewards, [spinRewards])

  return (
    <PageShell
      eyebrow="Rewards"
      title="Spin Wheel"
      subtitle="Spend coins to spin a neon wheel packed with coins, rare wraps, and premium cards."
      backgroundImage={spinBg}
    >
      <div className="grid gap-6 lg:grid-cols-[0.9fr_1.1fr]">
        <GlowCard eyebrow="Balance" title="Coin reserve" tone="info">
          <div className="flex items-center justify-between text-3xl font-semibold text-white">
            <span>{coins.toLocaleString()} coins</span>
            <span className="text-sm text-rose-100">{spinCost} coins per spin</span>
          </div>
        </GlowCard>

        <GlowCard eyebrow="Latest" title="Recent rewards" tone="warning">
          <div className="space-y-2 text-sm text-slate-200">
            {spins.slice(0, 6).map((spin) => (
              <div key={spin.id} className="flex items-center justify-between rounded-xl border border-white/10 bg-white/5 px-3 py-2">
                <div>
                  <p className="font-semibold text-white">{spin.reward}</p>
                  <p className="text-xs uppercase tracking-[0.2em] text-rose-100">{spin.rarity}</p>
                </div>
                <span className={spin.coins >= 0 ? 'text-emerald-200' : 'text-rose-200'}>
                  {spin.coins ? `${spin.coins > 0 ? '+' : ''}${spin.coins} c` : '—'}
                </span>
              </div>
            ))}
          </div>
        </GlowCard>
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-[1.05fr_0.95fr]">
        <div className="flex flex-col items-center justify-center gap-4">
          <div className="relative flex items-center justify-center">
            <div className="absolute inset-[-30px] rounded-full bg-gradient-to-r from-rose-500/30 via-red-500/20 to-orange-400/30 blur-3xl" />
            <motion.div
              animate={controls}
              className="relative grid h-80 w-80 place-items-center rounded-full border border-white/20 bg-white/5 shadow-neon"
            >
              {segments.map((slice, index) => (
                <div
                  key={slice.label}
                  className="absolute flex h-1/2 w-16 origin-bottom -translate-x-1/2 items-center justify-center text-xs font-semibold text-white"
                  style={{ transform: `rotate(${index * (360 / segments.length)}deg) translateY(-10%)` }}
                >
                  <span
                    className={`rounded-full bg-gradient-to-r ${slice.color} px-2 py-1 text-[11px] shadow-lg`}
                    style={{ writingMode: 'vertical-rl' }}
                  >
                    {slice.label}
                  </span>
                </div>
              ))}
              <div className="absolute inset-6 rounded-full border border-white/20 bg-black/60" />
              <div className="absolute inset-0 rounded-full border border-white/10" />
              <div className="absolute -top-4 h-8 w-3 rounded-full bg-gradient-to-b from-rose-400 to-orange-400" />
            </motion.div>
          </div>
          <motion.button
            whileHover={{ scale: 1.04 }}
            whileTap={{ scale: 0.96 }}
            disabled={spinning || coins < spinCost}
            onClick={spin}
            className="neon-button rounded-full bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-6 py-3 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-60"
          >
            {spinning ? 'Spinning…' : 'Spin now'}
          </motion.button>
          {result ? <p className="text-sm text-emerald-200">Result: {result}</p> : null}
          {error ? <p className="text-sm text-rose-200">{error}</p> : null}
        </div>

        <GlowCard eyebrow="Rewards" title="Possible drops" tone="default">
          <div className="grid gap-3 md:grid-cols-2">
            {segments.map((item) => (
              <div
                key={item.label}
                className={`flex items-center justify-between rounded-xl border border-white/10 bg-gradient-to-r ${item.color} px-3 py-2 text-sm text-white/90`}
              >
                <span className="font-semibold text-white">{item.label}</span>
                <span className="text-xs uppercase tracking-[0.2em]">Reward</span>
              </div>
            ))}
          </div>
        </GlowCard>
      </div>
    </PageShell>
  )
}
