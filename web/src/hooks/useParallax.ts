import { useEffect, useRef } from 'react'
import gsap from 'gsap'

export const useParallax = (strength = 24) => {
  const ref = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    const element = ref.current
    if (!element) return

    const handleMove = (event: PointerEvent) => {
      const rect = element.getBoundingClientRect()
      const x = (event.clientX - rect.left - rect.width / 2) / rect.width
      const y = (event.clientY - rect.top - rect.height / 2) / rect.height

      gsap.to(element, {
        x: x * strength,
        y: y * strength,
        duration: 0.8,
        ease: 'power3.out',
      })
    }

    const handleLeave = () => {
      gsap.to(element, { x: 0, y: 0, duration: 1, ease: 'power3.out' })
    }

    window.addEventListener('pointermove', handleMove)
    window.addEventListener('pointerleave', handleLeave)

    return () => {
      window.removeEventListener('pointermove', handleMove)
      window.removeEventListener('pointerleave', handleLeave)
    }
  }, [strength])

  return ref
}
