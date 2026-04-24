import { useState } from 'react'
import { PageShell } from '../components/PageShell'
import { GlowCard } from '../components/GlowCard'
import { useGame } from '../context/GameContext'

const chatBg =
  'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2100&q=80&sat=-14'

export const CommunityChat = () => {
  const { messages, sendMessage, channels } = useGame()
  const [value, setValue] = useState('')
  const [active, setActive] = useState('#race-room')

  const onSend = (e: React.FormEvent) => {
    e.preventDefault()
    if (!value.trim()) return
    sendMessage(value)
    setValue('')
  }

  return (
    <PageShell
      eyebrow="Social"
      title="Community Chat"
      subtitle="Coordinate races, trade cards, and drop race notes with your crew in a premium lounge."
      backgroundImage={chatBg}
    >
      <div className="grid gap-6 lg:grid-cols-[0.38fr_1.62fr]">
        <GlowCard eyebrow="Channels" title="Rooms" tone="info">
          <div className="space-y-2 text-sm text-slate-200">
            {channels.map((ch) => (
              <button
                key={ch}
                onClick={() => setActive(ch)}
                className={`flex w-full items-center justify-between rounded-xl border border-white/10 px-3 py-2 text-left transition ${active === ch ? 'bg-white/10 text-white' : 'bg-white/5 hover:bg-white/10'}`}
              >
                <span>{ch}</span>
                <span className="h-2 w-2 rounded-full bg-emerald-300" />
              </button>
            ))}
          </div>
        </GlowCard>

        <GlowCard eyebrow={active} title="Group chat" tone="default">
          <div className="flex flex-col gap-3">
            <div className="max-h-[320px] min-h-[320px] space-y-3 overflow-y-auto rounded-xl border border-white/10 bg-black/30 p-3">
              {messages.map((m) => (
                <div key={m.id} className="flex gap-3 rounded-xl border border-white/10 bg-white/5 p-3">
                  <div
                    className="h-10 w-10 flex-none rounded-full border border-white/15 bg-cover bg-center"
                    style={{ backgroundImage: `url(${m.avatar})` }}
                  />
                  <div className="space-y-1 text-sm text-slate-200">
                    <div className="flex items-center gap-2">
                      <p className="font-semibold text-white">{m.user}</p>
                      <span className="text-[11px] uppercase tracking-[0.2em] text-rose-100">{m.time}</span>
                    </div>
                    <p className="leading-snug text-slate-100">{m.content}</p>
                  </div>
                </div>
              ))}
            </div>
            <form onSubmit={onSend} className="flex items-center gap-3">
              <input
                value={value}
                onChange={(e) => setValue(e.target.value)}
                placeholder="Share strategy, drop telemetry notes, or hype your crew"
                className="flex-1 rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
              />
              <button
                type="submit"
                className="neon-button rounded-xl bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-4 py-3 text-sm font-semibold text-white"
              >
                Send
              </button>
            </form>
          </div>
        </GlowCard>
      </div>
    </PageShell>
  )
}
