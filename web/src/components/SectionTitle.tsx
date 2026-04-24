type SectionTitleProps = {
  eyebrow: string
  title: string
  description?: string
  align?: 'left' | 'center'
}

export const SectionTitle = ({ eyebrow, title, description, align = 'left' }: SectionTitleProps) => {
  const alignment = align === 'center' ? 'text-center items-center' : 'text-left items-start'
  return (
    <div className={`flex flex-col gap-2 ${alignment}`}>
      <p className="text-sm uppercase tracking-[0.28em] text-rose-100">{eyebrow}</p>
      <h2 className="font-display text-3xl text-white sm:text-4xl">{title}</h2>
      {description ? <p className="max-w-2xl text-slate-300">{description}</p> : null}
    </div>
  )
}
