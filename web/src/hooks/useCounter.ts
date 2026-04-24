import { useEffect, useRef, useState } from 'react'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'

export const useCounter = (
  target: number,
  triggerRef: React.RefObject<HTMLElement | null>,
  duration = 1.2,
) => {
  const [value, setValue] = useState(0)
  const playedRef = useRef(false)

  useEffect(() => {
    gsap.registerPlugin(ScrollTrigger)
    const trigger = triggerRef.current
    if (!trigger) return

    const number = { val: 0 }

    const st = ScrollTrigger.create({
      trigger,
      start: 'top 80%',
      onEnter: () => {
        if (playedRef.current) return
        playedRef.current = true
        gsap.to(number, {
          val: target,
          duration,
          ease: 'power2.out',
          onUpdate: () => setValue(Math.round(number.val)),
        })
      },
    })

    return () => st.kill()
  }, [duration, target, triggerRef])

  return value
}
