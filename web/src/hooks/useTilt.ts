import { useEffect, useRef } from 'react'
import gsap from 'gsap'

export const useTilt = (strength = 10) => {
  const ref = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    const element = ref.current
    if (!element) return

    const handleMove = (event: PointerEvent) => {
      const rect = element.getBoundingClientRect()
      const x = (event.clientX - rect.left - rect.width / 2) / rect.width
      const y = (event.clientY - rect.top - rect.height / 2) / rect.height

      gsap.to(element, {
        rotateX: -y * strength,
        rotateY: x * strength,
        transformPerspective: 800,
        transformOrigin: 'center',
        duration: 0.6,
        ease: 'power3.out',
      })
    }

    const handleLeave = () => {
      gsap.to(element, { rotateX: 0, rotateY: 0, duration: 0.8, ease: 'power3.out' })
    }

    element.addEventListener('pointermove', handleMove)
    element.addEventListener('pointerleave', handleLeave)

    return () => {
      element.removeEventListener('pointermove', handleMove)
      element.removeEventListener('pointerleave', handleLeave)
    }
  }, [strength])

  return ref
}
