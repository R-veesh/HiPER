import { useState } from 'react'
import { PageShell } from '../components/PageShell'
import { GlowCard } from '../components/GlowCard'
import { useGame } from '../context/GameContext'

const profileBg =
  'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=2000&q=80&sat=-9'

export const Profile = () => {
  const { user, coins, updateProfile, inventory, spins, transactions } = useGame()
  const [editing, setEditing] = useState(false)
  const [bio, setBio] = useState(user.bio)

  const saveBio = () => {
    updateProfile({ bio })
    setEditing(false)
  }

  return (
    <PageShell
      eyebrow="Pilot"
      title="Profile"
      subtitle="Tune your identity, review stats, and manage your racing assets."
      backgroundImage={profileBg}
      cta={
        <button
          onClick={() => setEditing((e) => !e)}
          className="neon-button rounded-full border border-white/20 bg-white/10 px-4 py-2 text-sm font-semibold text-white"
        >
          {editing ? 'Cancel' : 'Edit profile'}
        </button>
      }
    >
      <div className="grid gap-6 lg:grid-cols-[0.9fr_1.1fr]">
        <div className="space-y-4">
          <div className="glass-panel grid grid-cols-[90px_1fr] items-center gap-4 rounded-2xl border border-white/10 bg-white/5 p-5">
            <div
              className="h-24 w-24 rounded-2xl border border-white/15 bg-cover bg-center"
              style={{ backgroundImage: `url(${user.avatar})` }}
            />
            <div>
              <p className="text-xs uppercase tracking-[0.24em] text-rose-100">{user.email}</p>
              <h3 className="text-2xl font-semibold text-white">{user.displayName}</h3>
              <p className="text-sm text-slate-300">Age {user.age} • {user.stats.races} races • {user.stats.wins} wins</p>
            </div>
          </div>

          <GlowCard title="Bio" eyebrow="Identity" action={editing ? <button onClick={saveBio} className="text-sm text-rose-100">Save</button> : null}>
            {editing ? (
              <textarea
                value={bio}
                onChange={(e) => setBio(e.target.value)}
                className="min-h-[120px] w-full rounded-xl border border-white/15 bg-white/5 px-3 py-2 text-white outline-none ring-rose-400/40 focus:ring"
              />
            ) : (
              <p className="text-slate-200">{bio}</p>
            )}
          </GlowCard>

          <GlowCard eyebrow="Balance" title="Coin reserve" tone="info">
            <div className="flex items-center justify-between text-3xl font-semibold text-white">
              <span>{coins.toLocaleString()} coins</span>
              <span className="text-sm text-rose-100">Spin-ready</span>
            </div>
          </GlowCard>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <GlowCard eyebrow="Stats" title="Driver KPIs" tone="success">
            <div className="grid grid-cols-2 gap-3 text-sm text-slate-200">
              <div>
                <p className="text-xs uppercase tracking-[0.22em] text-rose-100">Races</p>
                <p className="text-2xl font-semibold text-white">{user.stats.races}</p>
              </div>
              <div>
                <p className="text-xs uppercase tracking-[0.22em] text-rose-100">Wins</p>
                <p className="text-2xl font-semibold text-white">{user.stats.wins}</p>
              </div>
              <div>
                <p className="text-xs uppercase tracking-[0.22em] text-rose-100">Spins</p>
                <p className="text-2xl font-semibold text-white">{user.stats.spins}</p>
              </div>
              <div>
                <p className="text-xs uppercase tracking-[0.22em] text-rose-100">Cards</p>
                <p className="text-2xl font-semibold text-white">{inventory.length}</p>
              </div>
            </div>
          </GlowCard>

          <GlowCard eyebrow="Recent spins" title="Rewards pulled" tone="warning">
            <div className="space-y-3 text-sm text-slate-200">
              {spins.slice(0, 4).map((spin) => (
                <div key={spin.id} className="flex items-center justify-between rounded-xl border border-white/10 bg-white/5 px-3 py-2">
                  <div>
                    <p className="font-semibold text-white">{spin.reward}</p>
                    <p className="text-xs uppercase tracking-[0.2em] text-rose-100">{spin.rarity}</p>
                  </div>
                  <span className="text-sm text-emerald-200">{spin.coins ? `+${spin.coins} c` : '—'}</span>
                </div>
              ))}
            </div>
          </GlowCard>

          <GlowCard eyebrow="Garage" title="Owned cards" tone="default">
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {inventory.slice(0, 4).map((card) => (
                <div key={card.id} className="rounded-xl border border-white/10 bg-white/5 p-3 text-sm text-slate-200">
                  <div className="flex items-center justify-between gap-2">
                    <p className="font-semibold text-white">{card.name}</p>
                    <span className="rounded-full border border-white/15 bg-white/10 px-2 py-0.5 text-[11px] text-rose-100">
                      {card.rarity}
                    </span>
                  </div>
                  <div className="mt-2 h-24 rounded-lg bg-cover bg-center" style={{ backgroundImage: `url(${card.image})` }} />
                </div>
              ))}
            </div>
          </GlowCard>

          <GlowCard eyebrow="Activity" title="Latest moves" tone="info">
            <div className="space-y-2 text-sm text-slate-200">
              {transactions.slice(0, 5).map((tx) => (
                <div key={tx.id} className="flex items-center justify-between rounded-xl border border-white/10 bg-white/5 px-3 py-2">
                  <div>
                    <p className="font-semibold text-white">{tx.title}</p>
                    <p className="text-xs uppercase tracking-[0.2em] text-rose-100">{tx.type}</p>
                  </div>
                  <span className={tx.coinsDelta >= 0 ? 'text-emerald-200' : 'text-rose-200'}>
                    {tx.coinsDelta >= 0 ? '+' : ''}{tx.coinsDelta} c
                  </span>
                </div>
              ))}
            </div>
          </GlowCard>
        </div>
      </div>
    </PageShell>
  )
}
