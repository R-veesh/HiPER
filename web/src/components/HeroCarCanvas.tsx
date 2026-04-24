import { Canvas, useFrame } from '@react-three/fiber'
import React, { Component, Suspense, useEffect, useMemo, useRef, useState } from 'react'
import {
  ContactShadows,
  Environment,
  MeshReflectorMaterial,
  OrbitControls,
  Stage,
  useGLTF,
  useProgress,
} from '@react-three/drei'
import * as THREE from 'three'

const LOCAL_MODEL_URL = '/models/hyper-racer.glb'
const REMOTE_MODEL_URL =
  'https://vazxmixjsiawhamofees.supabase.co/storage/v1/object/public/models/f1-car/model.gltf'

const fallbackStill =
  'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1800&q=80&sat=-10'

type CarModelProps = {
  autoRotate: boolean
}

class CanvasErrorBoundary extends Component<{ children: React.ReactNode; fallback: React.ReactNode }, { hasError: boolean }> {
  constructor(props: { children: React.ReactNode; fallback: React.ReactNode }) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  componentDidCatch(error: unknown) {
    console.error('Hero canvas crashed', error)
  }

  componentDidUpdate(prevProps: { children: React.ReactNode }) {
    if (prevProps.children !== this.props.children && this.state.hasError) {
      // reset on new children
      this.setState({ hasError: false })
    }
  }

  render() {
    if (this.state.hasError) return this.props.fallback
    return this.props.children
  }
}

const CarModel = ({ autoRotate }: CarModelProps) => {
  const group = useRef<THREE.Group>(null)
  const [source, setSource] = useState<string>(REMOTE_MODEL_URL)

  useEffect(() => {
    const controller = new AbortController()
    let isMounted = true

    fetch(LOCAL_MODEL_URL, { method: 'HEAD', signal: controller.signal })
      .then((res) => {
        if (res.ok && isMounted) {
          setSource(LOCAL_MODEL_URL)
        }
      })
      .catch(() => {
        // keep remote fallback
      })

    return () => {
      isMounted = false
      controller.abort()
    }
  }, [])

  const { scene } = useGLTF(source) as unknown as { scene: THREE.Group }

  useEffect(() => {
    scene.traverse((node: THREE.Object3D) => {
      if ((node as THREE.Mesh).isMesh) {
        const mesh = node as THREE.Mesh
        mesh.castShadow = true
        mesh.receiveShadow = true
        if (mesh.material) {
          const material = mesh.material as THREE.MeshStandardMaterial
          material.roughness = Math.max(0.12, material.roughness ?? 0.2)
          material.metalness = Math.min(0.9, material.metalness ?? 0.8)
        }
      }
    })
  }, [scene])

  const baseScale = useMemo(() => 1.1, [])

  useFrame((_state: any, delta: number) => {
    if (autoRotate && group.current) {
      group.current.rotation.y += delta * 0.35
    }
  })

  return <primitive ref={group} object={scene} scale={baseScale} position={[0, -0.28, 0]} />
}

const LoadingGradient = () => {
  const { progress } = useProgress()
  return (
    <div className="absolute inset-0 flex flex-col items-center justify-center bg-gradient-to-br from-[#0b0f1a] via-[#0e0d17] to-[#1b0b12]">
      <div className="h-28 w-28 animate-pulse rounded-full bg-gradient-to-br from-rose-500/40 via-red-500/35 to-orange-400/30 blur-xl" />
      <p className="mt-4 text-sm font-semibold tracking-[0.2em] text-rose-100/80">CALIBRATING • {progress.toFixed(0)}%</p>
    </div>
  )
}

const HeroCarFallback = ({ reason }: { reason: string }) => (
  <div className="relative h-[420px] sm:h-[480px] overflow-hidden rounded-3xl border border-white/10 bg-gradient-to-br from-[#0b0f1a] via-[#0e0d17] to-[#1b0b12]">
    <div
      className="absolute inset-0 bg-cover bg-center opacity-80"
      style={{ backgroundImage: `url(${fallbackStill})` }}
    />
    <div className="absolute inset-0 bg-gradient-to-b from-black/30 via-black/60 to-black" />
    <div className="absolute bottom-4 left-4 right-4 rounded-2xl border border-white/10 bg-white/10 px-4 py-3 text-sm text-slate-100 flex items-center justify-between">
      <div>
        <p className="text-xs uppercase tracking-[0.2em] text-rose-100">Garage preview</p>
        <p className="font-semibold text-white">3D temporarily unavailable</p>
        <p className="text-xs text-slate-300">{reason}</p>
      </div>
      <span className="flex h-10 w-10 items-center justify-center rounded-full border border-white/15 bg-white/10 text-xs">Static</span>
    </div>
  </div>
)

export const HeroCarCanvas = () => {
  const [isInteracting, setIsInteracting] = useState(false)
  const [canRender, setCanRender] = useState(false)

  useEffect(() => {
    try {
      const canvas = document.createElement('canvas')
      const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl')
      setCanRender(Boolean(gl))
    } catch (error) {
      console.warn('WebGL unavailable', error)
      setCanRender(false)
    }
  }, [])

  if (!canRender) {
    return <HeroCarFallback reason="WebGL is disabled or unsupported on this device." />
  }

  return (
    <div className="relative h-[440px] sm:h-[520px] overflow-hidden rounded-[32px] border border-white/10 bg-gradient-to-br from-[#0a0c14] via-[#08070f] to-[#12060d] shadow-neon">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_12%_24%,rgba(255,72,92,0.22),transparent_42%),radial-gradient(circle_at_85%_18%,rgba(120,190,255,0.18),transparent_32%),radial-gradient(circle_at_50%_80%,rgba(255,255,255,0.05),transparent_30%)]" />
      <div className="pointer-events-none absolute inset-0 grid-veil" />

      <CanvasErrorBoundary fallback={<HeroCarFallback reason="Model failed to load. Showing static render." />}>
        <Suspense fallback={<LoadingGradient />}>
          <Canvas
            shadows
            dpr={[1, 1.6]}
            camera={{ position: [4.6, 2.8, 5.4], fov: 36 }}
            className="relative"
          >
            <color attach="background" args={[`#04050a`]} />

            <Stage intensity={1.15} environment={undefined} adjustCamera={false} shadows={false} preset="rembrandt">
              <CarModel autoRotate={!isInteracting} />
            </Stage>

            <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, -0.92, 0]}>
              <planeGeometry args={[24, 24]} />
              <MeshReflectorMaterial
                mirror={0.9}
                blur={[320, 60]}
                mixBlur={0.6}
                mixStrength={12}
                depthScale={1}
                minDepthThreshold={0.25}
                maxDepthThreshold={1.25}
                roughness={0.35}
                metalness={0.9}
                color="#0a0c14"
              />
            </mesh>

            <spotLight
              position={[6, 9, 5]}
              angle={0.55}
              penumbra={0.32}
              intensity={1.55}
              color="#ff4b6b"
              castShadow
              shadow-mapSize-width={2048}
              shadow-mapSize-height={2048}
            />
            <spotLight position={[-7, 7, -3]} angle={0.7} penumbra={0.35} intensity={1.1} color="#7fd7ff" />
            <directionalLight position={[0.6, 4, 4.8]} intensity={0.7} color="#ffffff" />
            <ambientLight intensity={0.38} color="#c4d4ff" />

            <Environment preset="city" />

            <ContactShadows position={[0, -0.9, 0]} opacity={0.45} scale={10} blur={2.6} far={8} />

            <OrbitControls
              enableZoom={false}
              enablePan={false}
              minPolarAngle={(Math.PI / 2) * 0.42}
              maxPolarAngle={(Math.PI / 2) * 0.95}
              autoRotate={false}
              rotateSpeed={0.8}
              target={[0, 0, 0]}
              onStart={() => setIsInteracting(true)}
              onEnd={() => setIsInteracting(false)}
            />
          </Canvas>
        </Suspense>
      </CanvasErrorBoundary>

      <div className="pointer-events-none absolute inset-0 rounded-[30px] ring-1 ring-white/10" />
      <div className="pointer-events-none absolute inset-x-0 bottom-0 h-36 bg-gradient-to-t from-black/70 via-black/10 to-transparent" />
    </div>
  )
}

useGLTF.preload(LOCAL_MODEL_URL)
useGLTF.preload(REMOTE_MODEL_URL)
