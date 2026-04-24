import type { ReactNode } from 'react'

type PageShellProps = {
  title: string
  subtitle?: string
  eyebrow?: string
  cta?: ReactNode
  backgroundImage?: string
  children: ReactNode
}

export const PageShell = ({ title, subtitle, eyebrow, cta, backgroundImage, children }: PageShellProps) => {
  return (
    <section className="relative min-h-[80vh] overflow-hidden px-6 py-16 sm:px-8 lg:px-14">
      <div className="absolute inset-0 opacity-60" aria-hidden>
        {backgroundImage ? (
          <div className="absolute inset-0 bg-cover bg-center" style={{ backgroundImage: `url(${backgroundImage})` }} />
        ) : null}
        <div className="speed-lines" />
        <div className="hyper-grid absolute inset-0" />
        <div className="absolute inset-0 bg-gradient-to-b from-black/50 via-black/70 to-night" />
      </div>

      <div className="relative z-10 mx-auto flex max-w-6xl flex-col gap-10">
        <div className="flex flex-wrap items-end justify-between gap-4">
          <div className="space-y-2">
            {eyebrow ? <p className="text-sm uppercase tracking-[0.28em] text-rose-100">{eyebrow}</p> : null}
            <h1 className="font-display text-3xl text-white sm:text-4xl lg:text-5xl">{title}</h1>
            {subtitle ? <p className="max-w-2xl text-slate-300">{subtitle}</p> : null}
          </div>
          {cta}
        </div>

        <div className="glass-panel relative rounded-3xl border border-white/10 bg-white/5 p-6 shadow-glow sm:p-8">
          <div className="absolute -right-16 -top-24 h-64 w-64 rounded-full bg-rose-500/10 blur-3xl" aria-hidden />
          <div className="relative z-10">{children}</div>
        </div>
      </div>
    </section>
  )
}
