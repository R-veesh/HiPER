import { useEffect } from 'react'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import { Routes, Route } from 'react-router-dom'
import './App.css'
import { Navbar } from './components/Navbar'
import { Footer } from './components/Footer'
import { Home } from './pages/Home'
import { Cars as CarsPage } from './pages/Cars'
import { Categories } from './pages/Categories'
import { Gallery as GalleryPage } from './pages/Gallery'
import { About } from './pages/About'
import { Contact } from './pages/Contact'
import { Login } from './pages/Login'
import { Register } from './pages/Register'
import { Profile } from './pages/Profile'
import { CoinStore } from './pages/CoinStore'
import { SpinWheel } from './pages/SpinWheel'
import { CommunityChat } from './pages/CommunityChat'
import { CardStore } from './pages/CardStore'
import { MapPage } from './pages/Map'
import { MapStore } from './pages/MapStore'
import { Report } from './pages/Report'

function App() {
  useEffect(() => {
    gsap.registerPlugin(ScrollTrigger)

    const ctx = gsap.context(() => {
      gsap.to('.scroll-progress', {
        scaleX: 1,
        ease: 'none',
        scrollTrigger: {
          start: 'top top',
          end: 'bottom bottom',
          scrub: true,
        },
      })
    })

    return () => ctx.revert()
  }, [])

  return (
    <div className="relative min-h-screen bg-[#050608] text-slate-100">
      <div className="scroll-progress fixed left-0 top-0 z-40 h-1 w-full origin-left scale-x-0 bg-gradient-to-r from-rose-400 via-red-500 to-orange-400" />
      <Navbar />
      <main className="relative pt-24">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/cars" element={<CarsPage />} />
          <Route path="/categories" element={<Categories />} />
          <Route path="/gallery" element={<GalleryPage />} />
          <Route path="/about" element={<About />} />
          <Route path="/contact" element={<Contact />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/coin-store" element={<CoinStore />} />
          <Route path="/spin" element={<SpinWheel />} />
          <Route path="/community" element={<CommunityChat />} />
          <Route path="/card-store" element={<CardStore />} />
          <Route path="/map" element={<MapPage />} />
          <Route path="/map-store" element={<MapStore />} />
          <Route path="/report" element={<Report />} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}

export default App
