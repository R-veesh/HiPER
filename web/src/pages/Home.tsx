import { Hero } from '../components/Hero'
import { Cars } from '../components/Cars'
import { Features } from '../components/Features'
import { Gallery } from '../components/Gallery'
import { AboutSection } from '../components/AboutSection'
import { CarBanner } from '../components/CarBanner'
import { MapPreview } from '../components/MapPreview'

export const Home = () => {
  return (
    <div className="space-y-10">
      <Hero />
      <Cars limit={4} eyebrow="Featured Cars" title="Flagship hypercars" />
      <Features />
      <Gallery />
      <AboutSection />
      <CarBanner />
      <MapPreview />
    </div>
  )
}
