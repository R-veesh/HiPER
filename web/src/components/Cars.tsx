import { useScrollReveal } from '../hooks/useScrollReveal'
import { cars as carsData } from '../data/cars'
import { SectionTitle } from './SectionTitle'
import { CarCard } from './CarCard'
import { useTilt } from '../hooks/useTilt'

type CarsProps = {
  limit?: number
  title?: string
  eyebrow?: string
  description?: string
}

export const Cars = ({ limit, title, eyebrow, description }: CarsProps) => {
  const sectionRef = useScrollReveal({ y: 32 })
  const cars = limit ? carsData.slice(0, limit) : carsData

  return (
    <section ref={sectionRef} id="cars" className="relative z-10 px-6 py-20 sm:px-8 lg:px-14">
      <div className="max-w-6xl mx-auto">
        <div className="flex flex-wrap items-baseline justify-between gap-4">
          <SectionTitle
            eyebrow={eyebrow ?? 'Featured Cars'}
            title={title ?? 'Signature hypercars with glassmorphism sheen'}
            description={description ?? 'Hover to tilt each chassis in 3D and reveal specs across our flagship fleet.'}
          />
        </div>

        <div className="mt-10 grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {cars.map((car) => {
            const tiltRef = useTilt(12)
            return <CarCard key={car.name} car={car} tiltRef={tiltRef} />
          })}
        </div>
      </div>
    </section>
  )
}
