import { useMemo } from 'react'

type Particle = {
  size: number
  top: string
  left: string
  blur: number
  delay: number
  duration: number
}

const createParticle = (): Particle => ({
  size: Math.random() * 4 + 2,
  top: `${Math.random() * 100}%`,
  left: `${Math.random() * 100}%`,
  blur: Math.random() * 2,
  delay: Math.random() * 6,
  duration: Math.random() * 6 + 6,
})

export const ParticleField = ({ count = 22 }: { count?: number }) => {
  const particles = useMemo(() => Array.from({ length: count }, createParticle), [count])

  return (
    <div className="pointer-events-none absolute inset-0 overflow-hidden">
      {particles.map((particle, index) => (
        <span
          key={index}
          className="particle"
          style={{
            width: particle.size,
            height: particle.size,
            top: particle.top,
            left: particle.left,
            filter: `blur(${particle.blur}px)`,
            animation: `float ${particle.duration}s ease-in-out infinite`,
            animationDelay: `${particle.delay}s`,
          }}
        />
      ))}
    </div>
  )
}
