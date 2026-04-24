import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { PageShell } from '../components/PageShell'
import { useGame } from '../context/GameContext'

const registerBg =
  'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=2000&q=80&sat=-10'

export const Register = () => {
  const { register } = useGame()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    displayName: '',
    age: '',
    bio: '',
    email: '',
    password: '',
    confirm: '',
  })
  const [error, setError] = useState('')

  const onSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.displayName || !form.email || !form.password || !form.confirm || !form.age) {
      setError('Complete all fields to register your driver profile.')
      return
    }
    if (form.password !== form.confirm) {
      setError('Passwords must match.')
      return
    }
    setError('')
    register({
      displayName: form.displayName,
      age: Number(form.age),
      bio: form.bio || 'New racer joining the Hyper grid.',
      email: form.email,
      password: form.password,
    })
    navigate('/profile')
  }

  const update = (key: keyof typeof form, value: string) => setForm((f) => ({ ...f, [key]: value }))

  return (
    <PageShell
      eyebrow="Onboard"
      title="Register"
      subtitle="Secure your callsign, set your bio, and sync with the Hyper Racing universe."
      backgroundImage={registerBg}
      cta={<Link to="/login" className="text-sm text-rose-100 underline underline-offset-4">Have an account? Login</Link>}
    >
      <div className="grid gap-8 lg:grid-cols-[1.1fr_0.9fr]">
        <form onSubmit={onSubmit} className="grid gap-4 sm:grid-cols-2">
          <div className="sm:col-span-1">
            <label className="text-sm text-slate-200">Display Name</label>
            <input
              value={form.displayName}
              onChange={(e) => update('displayName', e.target.value)}
              className="mt-1 w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
              placeholder="Ava “Driftline”"
              required
            />
          </div>
          <div className="sm:col-span-1">
            <label className="text-sm text-slate-200">Age</label>
            <input
              type="number"
              value={form.age}
              onChange={(e) => update('age', e.target.value)}
              className="mt-1 w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
              placeholder="27"
              required
            />
          </div>
          <div className="sm:col-span-2">
            <label className="text-sm text-slate-200">Bio</label>
            <textarea
              value={form.bio}
              onChange={(e) => update('bio', e.target.value)}
              className="mt-1 min-h-[110px] w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
              placeholder="Night sprint addict, data nerd, aero whisperer."
            />
          </div>
          <div className="sm:col-span-2">
            <label className="text-sm text-slate-200">Email</label>
            <input
              type="email"
              value={form.email}
              onChange={(e) => update('email', e.target.value)}
              className="mt-1 w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
              placeholder="you@hyper.gg"
              required
            />
          </div>
          <div className="sm:col-span-1">
            <label className="text-sm text-slate-200">Password</label>
            <input
              type="password"
              value={form.password}
              onChange={(e) => update('password', e.target.value)}
              className="mt-1 w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
              placeholder="••••••••"
              required
            />
          </div>
          <div className="sm:col-span-1">
            <label className="text-sm text-slate-200">Confirm Password</label>
            <input
              type="password"
              value={form.confirm}
              onChange={(e) => update('confirm', e.target.value)}
              className="mt-1 w-full rounded-xl border border-white/15 bg-white/5 px-4 py-3 text-white outline-none ring-rose-400/40 focus:ring"
              placeholder="••••••••"
              required
            />
          </div>
          {error ? <p className="sm:col-span-2 text-sm text-rose-200">{error}</p> : null}
          <motion.button
            type="submit"
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
            className="neon-button sm:col-span-2 w-full rounded-xl bg-gradient-to-r from-rose-500 via-red-500 to-orange-400 px-5 py-3 text-center text-sm font-semibold text-white shadow-neon"
          >
            Create racer profile
          </motion.button>
        </form>

        <div className="space-y-4">
          <div className="glass-panel rounded-2xl border border-white/10 bg-white/5 p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-rose-100">Premium onboarding</p>
            <h3 className="mt-2 text-xl font-semibold text-white">Included perks</h3>
            <ul className="mt-3 space-y-2 text-sm text-slate-200">
              <li>• 2,000 bonus coins for first spins</li>
              <li>• Access to community chat + squad channels</li>
              <li>• Marketplace-ready loadout &amp; profile editor</li>
            </ul>
          </div>
          <div className="glass-panel rounded-2xl border border-white/10 bg-gradient-to-br from-white/5 via-white/10 to-transparent p-5">
            <h4 className="text-sm uppercase tracking-[0.24em] text-cyan-100">Security</h4>
            <p className="mt-2 text-slate-200">All flows are frontend-only for this demo. Swap in real auth later without breaking UI.</p>
          </div>
        </div>
      </div>
    </PageShell>
  )
}
