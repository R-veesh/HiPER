import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { PageShell } from '../components/PageShell'
import { useGame } from '../context/GameContext'

const loginBg =
  'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2000&q=80&sat=-16'

export const Login = () => {
  const { login, user } = useGame()
  const navigate = useNavigate()
  const [email, setEmail] = useState(user.email)
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const qrUrl =
    'https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=https%3A%2F%2Fhyperracing.app%2Flogin%2Fqr'

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!email || !password) {
      setError('Enter your credentials to connect to the grid.')
      return
    }
    setError('')
    setSubmitting(true)
    try {
      await login(email, password)
      navigate('/profile')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <PageShell
      eyebrow="Access"
      title="Login"
      subtitle="Sync your racer profile, claim rewards, and jump back into the circuit."
      backgroundImage={loginBg}
      cta={<Link to="/register" className="text-sm text-rose-100 underline underline-offset-4">New here? Register</Link>}
    >
      <div className="grid gap-8 lg:grid-cols-[1.05fr_0.95fr]">
        <form onSubmit={onSubmit} className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <label className="text-sm text-slate-200">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="mt-1 w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
                placeholder="you@hyper.gg"
                required
              />
            </div>
            <div className="sm:col-span-2">
              <label className="text-sm text-slate-200">Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="mt-1 w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
                placeholder="••••••••"
                required
              />
            </div>
          </div>
          {error ? <p className="text-sm text-rose-200">{error}</p> : null}
          <motion.button
            type="submit"
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
            disabled={submitting}
            className="neon-button w-full rounded-xl bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-5 py-3 text-center text-sm font-semibold text-white shadow-neon"
          >
            {submitting ? 'Signing in…' : 'Enter the cockpit'}
          </motion.button>
          <div className="grid gap-3 sm:grid-cols-2">
            <motion.button
              type="button"
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              className="flex items-center justify-center gap-3 rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-sm font-semibold text-white"
            >
              <span className="h-5 w-5 rounded-full bg-white text-black grid place-items-center font-bold">G</span>
              Continue with Google
            </motion.button>
            <motion.button
              type="button"
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              className="flex items-center justify-center gap-3 rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-sm font-semibold text-white"
            >
              <span className="h-5 w-5 rounded-full bg-black text-white grid place-items-center font-bold"></span>
              Continue with Apple
            </motion.button>
          </div>
          <p className="text-sm text-slate-300">Two-factor, QR, and social OAuth are UI-ready.</p>
        </form>

        <div className="space-y-4">
          <div className="glass-panel rounded-2xl border border-white/10 bg-white/5 p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-rose-100">Fast lane</p>
            <h3 className="mt-2 text-xl font-semibold text-white">Session perks</h3>
            <ul className="mt-3 space-y-2 text-sm text-slate-200">
              <li>• Persist your balance, cards, and spins this session</li>
              <li>• Unlock community chat and race briefing room</li>
              <li>• Sync loadouts across store, spins, and profile</li>
            </ul>
          </div>
          <div className="glass-panel rounded-2xl border border-white/10 bg-gradient-to-br from-white/5 via-white/10 to-transparent p-5">
            <h4 className="text-sm uppercase tracking-[0.24em] text-cyan-100">Need an account?</h4>
            <p className="mt-2 text-slate-200">Register in seconds, drop a display name, and tune your racing bio.</p>
            <Link to="/register" className="mt-3 inline-flex items-center gap-2 text-sm font-semibold text-rose-100">
              Go to Register →
            </Link>
          </div>
          <div className="glass-panel rounded-2xl border border-white/10 bg-white/5 p-5">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-xs uppercase tracking-[0.24em] text-rose-100">QR login</p>
                <p className="text-sm text-slate-200">Scan to approve on your device</p>
              </div>
              <div className="rounded-xl border border-white/15 bg-white/10 p-2">
                <img src={qrUrl} alt="QR code" className="h-28 w-28 rounded-lg" />
              </div>
            </div>
            <p className="mt-2 text-xs text-slate-400">Use your mobile app to scan and confirm. This demo QR is static.</p>
          </div>
        </div>
      </div>
    </PageShell>
  )
}
