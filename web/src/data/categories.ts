export type Category = {
  name: string
  distance: string
  mood: string
  image: string
}

export const categories: Category[] = [
  {
    name: 'Circuit / Grand Prix',
    distance: '5-7 km',
    mood: 'Technical apex mastery',
    image:
      'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=2200&q=80&sat=-12',
  },
  {
    name: 'Sprint / Street',
    distance: '3-5 km',
    mood: 'Tight city night run',
    image:
      'https://images.unsplash.com/photo-1493238792000-8113da705763?auto=format&fit=crop&w=2200&q=80&sat=-18',
  },
  {
    name: 'Drift / Exhibition',
    distance: 'Custom',
    mood: 'Smoke, lights, precision',
    image:
      'https://images.unsplash.com/photo-1503736334956-4c8f8e92946d?auto=format&fit=crop&w=2200&q=80&sat=-18',
  },
  {
    name: 'Hypercar / Endurance',
    distance: '10+ km',
    mood: 'Night endurance dominance',
    image:
      'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=2200&q=80&sat=-22',
  },
  {
    name: 'EV Performance',
    distance: '4-6 km',
    mood: 'Silent torque storm',
    image:
      'https://images.unsplash.com/photo-1478720568477-152d9b164e26?auto=format&fit=crop&w=2200&q=80&sat=-18',
  },
]
