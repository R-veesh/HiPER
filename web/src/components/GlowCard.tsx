import type { ReactNode } from 'react'

type GlowCardProps = {
  title?: string
  eyebrow?: string
  action?: ReactNode
  children?: ReactNode
  tone?: 'default' | 'info' | 'success' | 'warning'
  footer?: ReactNode
}

const toneRing: Record<NonNullable<GlowCardProps['tone']>, string> = {
  default: 'from-rose-400 to-orange-300',
  info: 'from-cyan-300 to-blue-400',
  success: 'from-emerald-300 to-cyan-300',
  warning: 'from-amber-300 to-rose-300',
}

export const GlowCard = ({ title, eyebrow, action, children, tone = 'default', footer }: GlowCardProps) => {
  return (
    <div className="glass-panel relative flex h-full flex-col gap-3 rounded-2xl border border-white/10 bg-white/5 p-5 shadow-glow">
      <div className={`absolute inset-x-0 top-0 h-[1px] bg-gradient-to-r ${toneRing[tone]} opacity-70`} aria-hidden />
      {(eyebrow || title || action) && (
        <div className="flex items-start justify-between gap-3">
          <div>
            {eyebrow ? <p className="text-[11px] uppercase tracking-[0.24em] text-rose-100">{eyebrow}</p> : null}
            {title ? <h3 className="mt-1 font-semibold text-white">{title}</h3> : null}
          </div>
          {action}
        </div>
      )}
      {children ? <div className="text-slate-200">{children}</div> : null}
      {footer ? <div className="mt-auto pt-2 text-sm text-slate-300">{footer}</div> : null}
    </div>
  )
}
