export type Car = {
  name: string
  class: string
  speed: string
  color: string
  image: string
  blurb: string
}

export const cars: Car[] = [
  {
    name: 'VX-09 Tempest',
    class: 'Hyper',
    speed: '402 km/h',
    color: 'Crimson Veil',
    image:
      'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2400&q=80&sat=-12',
    blurb: 'Carbon weave, twin-turbo V8, active aero tuned for podiums.',
  },
  {
    name: 'Astra Helix',
    class: 'Circuit',
    speed: '376 km/h',
    color: 'Molten Silver',
    image:
      'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2400&q=80&sat=-18',
    blurb: 'Track-attack setup with telemetry-driven aero balance.',
  },
  {
    name: 'Nova Specter',
    class: 'Sprint',
    speed: '428 km/h',
    color: 'Shadow Black',
    image:
      'https://images.unsplash.com/photo-1493238792000-8113da705763?auto=format&fit=crop&w=2400&q=80&sat=-18',
    blurb: 'Ultra-low drag silhouette built for ruthless straight-line pulls.',
  },
  {
    name: 'Mirage Vanta',
    class: 'Time Attack',
    speed: '390 km/h',
    color: 'Scarlet Edge',
    image:
      'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=2400&q=80&sat=-18',
    blurb: 'High-downforce aero kit with razor diffusers for time-attack pace.',
  },
  {
    name: 'Ion Apex',
    class: 'Electric Performance',
    speed: '360 km/h',
    color: 'Liquid Mercury',
    image:
      'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2400&q=80&sat=-24',
    blurb: 'Dual-motor torque vectoring with instant launch authority.',
  },
  {
    name: 'Drift Revenant',
    class: 'Drift',
    speed: '340 km/h',
    color: 'Neon Ember',
    image:
      'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2400&q=80&sat=-28',
    blurb: 'Wide-lock steering, smoky exits, and feather-touch throttle mapping.',
  },
]
