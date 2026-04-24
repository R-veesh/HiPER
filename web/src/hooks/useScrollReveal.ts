import { useEffect, useRef } from 'react'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'

type ScrollRevealOptions = {
  duration?: number
  y?: number
  stagger?: number
  delay?: number
  once?: boolean
}

export const useScrollReveal = <T extends HTMLElement = HTMLElement>(options: ScrollRevealOptions = {}) => {
  const targetRef = useRef<T | null>(null)

  useEffect(() => {
    gsap.registerPlugin(ScrollTrigger)

    const element = targetRef.current
    if (!element) return

    const ctx = gsap.context(() => {
      gsap.fromTo(
        element,
        { autoAlpha: 0, y: options.y ?? 60 },
        {
          autoAlpha: 1,
          y: 0,
          duration: options.duration ?? 1,
          ease: 'power3.out',
          delay: options.delay ?? 0,
          stagger: options.stagger,
          scrollTrigger: {
            trigger: element,
            start: 'top 80%',
            toggleActions: 'play none none reverse',
            once: options.once ?? false,
          },
        },
      )
    }, element)

    return () => ctx.revert()
  }, [options.delay, options.duration, options.once, options.stagger, options.y])

  return targetRef
}
