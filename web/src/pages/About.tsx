import { AboutSection } from '../components/AboutSection'
import { Features } from '../components/Features'
import { CarBanner } from '../components/CarBanner'

export const About = () => {
  return (
    <div className="space-y-12 pt-6">
      <AboutSection />
      <Features />
      <CarBanner />
    </div>
  )
}
