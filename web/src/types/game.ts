export type UserProfile = {
  id?: string
  displayName: string
  age: number
  bio: string
  email: string
  avatar: string
  stats: {
    races: number
    wins: number
    spins: number
    cards: number
  }
}

export type Transaction = {
  id: string
  type: 'coins' | 'spin' | 'card' | 'map'
  title: string
  amount: number
  coinsDelta: number
  timestamp: string
}

export type Card = {
  id: string
  name: string
  rarity: 'Common' | 'Rare' | 'Epic' | 'Legendary'
  price: number
  image: string
}

export type MapPack = {
  id: string
  name: string
  region: string
  description: string
  tier: 'Standard' | 'Premium'
  category: 'Urban' | 'Night' | 'Desert' | 'Mountain' | 'Coastal' | 'Volcanic'
  rarity: 'Rare' | 'Epic' | 'Legendary' | 'Exclusive'
  price: number
  image: string
}

export type Message = {
  id: string
  user: string
  avatar: string
  time: string
  content: string
}

export type SpinResult = {
  id: string
  reward: string
  coins: number
  rarity: 'Bonus' | 'Rare' | 'Premium' | 'Special'
  timestamp: string
}

export type CoinPackage = {
  id: string
  coins: number
  price: number
  label: string
}

export type SpinDisplayReward = {
  id: string
  label: string
  color: string
}
