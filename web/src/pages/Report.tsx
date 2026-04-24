import { PageShell } from '../components/PageShell'
import { GlowCard } from '../components/GlowCard'
import { useGame } from '../context/GameContext'

const reportBg =
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2100&q=80&sat=-12'

export const Report = () => {
  const { coins, transactions, spins, inventory } = useGame()

  const totals = {
    spins: spins.length,
    purchases: transactions.filter((t) => t.type === 'card').length,
    coinTopUps: transactions.filter((t) => t.type === 'coins').length,
  }

  return (
    <PageShell
      eyebrow="Analytics"
      title="Report"
      subtitle="A clean dashboard summarizing your activity, coin flow, and marketplace moves."
      backgroundImage={reportBg}
    >
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <GlowCard eyebrow="Coins" title="Balance" tone="info">
          <p className="text-3xl font-semibold text-white">{coins.toLocaleString()}</p>
          <p className="text-sm text-slate-300">Live wallet</p>
        </GlowCard>
        <GlowCard eyebrow="Spins" title="Wheel runs" tone="warning">
          <p className="text-3xl font-semibold text-white">{totals.spins}</p>
          <p className="text-sm text-slate-300">Reward spins executed</p>
        </GlowCard>
        <GlowCard eyebrow="Cards" title="Purchased" tone="success">
          <p className="text-3xl font-semibold text-white">{inventory.length}</p>
          <p className="text-sm text-slate-300">Collectibles secured</p>
        </GlowCard>
        <GlowCard eyebrow="Top-ups" title="Coin buys" tone="default">
          <p className="text-3xl font-semibold text-white">{totals.coinTopUps}</p>
          <p className="text-sm text-slate-300">Purchases completed</p>
        </GlowCard>
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-[1.05fr_0.95fr]">
        <GlowCard eyebrow="Transactions" title="Coin flow" tone="default">
          <div className="space-y-2 text-sm text-slate-200">
            {transactions.slice(0, 8).map((tx) => (
              <div key={tx.id} className="flex items-center justify-between rounded-xl border border-white/10 bg-white/5 px-3 py-2">
                <div>
                  <p className="font-semibold text-white">{tx.title}</p>
                  <p className="text-[11px] uppercase tracking-[0.2em] text-rose-100">{tx.type}</p>
                </div>
                <span className={tx.coinsDelta >= 0 ? 'text-emerald-200' : 'text-rose-200'}>
                  {tx.coinsDelta >= 0 ? '+' : ''}{tx.coinsDelta} c
                </span>
              </div>
            ))}
          </div>
        </GlowCard>

        <GlowCard eyebrow="Activity" title="Highlights" tone="info">
          <div className="grid gap-3 sm:grid-cols-2">
            <div className="rounded-xl border border-white/10 bg-gradient-to-br from-emerald-400/10 via-emerald-400/5 to-transparent p-3 text-sm text-white">
              <p className="text-xs uppercase tracking-[0.2em] text-emerald-200">Coins</p>
              <p className="mt-1 text-lg font-semibold">Net +{coins - 2000} coins</p>
              <p className="text-slate-200">Since starter bonus</p>
            </div>
            <div className="rounded-xl border border-white/10 bg-gradient-to-br from-rose-400/10 via-rose-400/5 to-transparent p-3 text-sm text-white">
              <p className="text-xs uppercase tracking-[0.2em] text-rose-200">Spins</p>
              <p className="mt-1 text-lg font-semibold">{totals.spins} wheel pulls</p>
              <p className="text-slate-200">Rewards stacked in vault</p>
            </div>
          </div>
          <div className="mt-4">
            <div className="h-2 rounded-full bg-gradient-to-r from-rose-400 via-orange-400 to-emerald-300" />
            <p className="mt-2 text-sm text-slate-300">Momentum meter across coins, spins, and purchases.</p>
          </div>
        </GlowCard>
      </div>
    </PageShell>
  )
}
