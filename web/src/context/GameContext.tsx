import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { api } from '../lib/api'
import type { Card, CoinPackage, MapPack, Message, SpinDisplayReward, SpinResult, Transaction, UserProfile } from '../types/game'

type GameContextShape = {
  user: UserProfile
  coins: number
  inventory: Card[]
  maps: MapPack[]
  messages: Message[]
  transactions: Transaction[]
  spins: SpinResult[]
  channels: string[]
  cardCatalog: Card[]
  mapCatalog: MapPack[]
  coinPackages: CoinPackage[]
  spinRewards: SpinDisplayReward[]
  spinCost: number
  loading: boolean
  error: string
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (data: { displayName: string; age: number; bio: string; email: string; password: string }) => Promise<void>
  updateProfile: (payload: Partial<UserProfile>) => Promise<void>
  purchaseCoins: (packageId: string) => Promise<void>
  buyCard: (card: Card) => Promise<void>
  buyMapPack: (pack: MapPack) => Promise<void>
  sendMessage: (content: string) => void
  spinWheel: () => Promise<SpinResult>
}

const defaultUser: UserProfile = {
  displayName: 'Guest Racer',
  age: 0,
  bio: 'Login to sync your profile and inventory.',
  email: '',
  avatar: 'https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=400&q=80',
  stats: {
    races: 0,
    wins: 0,
    spins: 0,
    cards: 0,
  },
}

const rarityTone: Record<string, SpinDisplayReward['color']> = {
  Common: 'from-slate-400 to-slate-500',
  Rare: 'from-cyan-400 to-blue-500',
  Epic: 'from-purple-400 to-pink-400',
  Legendary: 'from-amber-400 to-orange-400',
}

const GameContext = createContext<GameContextShape | null>(null)

const now = () => new Date().toISOString()

export const GameProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserProfile>(defaultUser)
  const [coins, setCoins] = useState(0)
  const [inventory, setInventory] = useState<Card[]>([])
  const [maps, setMaps] = useState<MapPack[]>([])
  const [messages, setMessages] = useState<Message[]>([])
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [spins, setSpins] = useState<SpinResult[]>([])
  const [channels, setChannels] = useState<string[]>([])
  const [cardCatalog, setCardCatalog] = useState<Card[]>([])
  const [mapCatalog, setMapCatalog] = useState<MapPack[]>([])
  const [coinPackages, setCoinPackages] = useState<CoinPackage[]>([])
  const [spinRewards, setSpinRewards] = useState<SpinDisplayReward[]>([])
  const [spinCost, setSpinCost] = useState(100)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [isAuthenticated, setIsAuthenticated] = useState(false)

  const hydrateAuthState = async (token: string, cards: Card[], mapPacks: MapPack[]) => {
    const [me, inventoryResponse] = await Promise.all([api.getMe(token), api.getInventory(token)])
    setCoins(me.coinBalance)

    try {
      const profile = await api.getProfile(me.id)
      setUser((prev) => ({
        ...prev,
        id: me.id,
        email: me.email,
        displayName: profile.displayName || me.displayName || prev.displayName,
        age: profile.age ?? 0,
        bio: profile.bio ?? '',
        avatar: profile.profilePicUrl || prev.avatar,
      }))
    } catch {
      setUser((prev) => ({
        ...prev,
        id: me.id,
        email: me.email,
        displayName: me.displayName || prev.displayName,
      }))
    }

    const cardById = new Map(cards.map((item) => [item.id, item]))
    const mapById = new Map(mapPacks.map((item) => [item.id, item]))

    const ownedCards = inventoryResponse.items
      .filter((item) => item.itemType === 'car')
      .map((item) => {
        const fromCatalog = cardById.get(item.itemId)
        if (fromCatalog) return fromCatalog
        return {
          id: item.itemId,
          name: item.itemName,
          rarity: (item.rarity as Card['rarity']) || 'Common',
          price: 0,
          image: item.spriteUrl,
        }
      })

    const ownedMaps = inventoryResponse.items
      .filter((item) => item.itemType === 'map')
      .map((item) => {
        const fromCatalog = mapById.get(item.itemId)
        if (fromCatalog) return fromCatalog
        return {
          id: item.itemId,
          name: item.itemName,
          region: 'Unknown',
          description: '',
          tier: 'Standard' as const,
          category: 'Urban' as const,
          rarity: (item.rarity as MapPack['rarity']) || 'Rare',
          price: 0,
          image: item.spriteUrl,
        }
      })

    setInventory(ownedCards)
    setMaps(ownedMaps)
    setUser((prev) => ({
      ...prev,
      stats: {
        ...prev.stats,
        cards: ownedCards.length,
      },
    }))
  }

  useEffect(() => {
    const load = async () => {
      setLoading(true)
      setError('')
      try {
        // Load coin packages
        let coinPkgs: CoinPackage[] = []
        try {
          const packRes = await api.getCoinPackages()
          coinPkgs = packRes.packages
          setCoinPackages(packRes.packages)
        } catch (err) {
          console.warn('Failed to load coin packages:', err)
          // Fallback: provide default packages
          coinPkgs = [
            { id: 'pack_100', coins: 100, price: 0.99, label: '100 Coins' },
            { id: 'pack_500', coins: 500, price: 3.99, label: '500 Coins' },
            { id: 'pack_1200', coins: 1200, price: 7.99, label: '1,200 Coins' },
            { id: 'pack_3000', coins: 3000, price: 14.99, label: '3,000 Coins' },
          ]
          setCoinPackages(coinPkgs)
        }

        // Load spin config to populate reward catalog
        let spinItems: SpinDisplayReward[] = []
        let cardCatalogFromSpin: Card[] = []
        try {
          const spinRes = await api.getSpinConfig()
          setSpinCost(spinRes.cost)
          spinItems = spinRes.items.map((item) => ({
            id: item.itemId,
            label: item.itemName,
            color: rarityTone[item.rarity] ?? 'from-rose-400 to-orange-400',
          }))
          setSpinRewards(spinItems)
          
          // Also populate card catalog from spin config (cars)
          cardCatalogFromSpin = spinRes.items
            .map((item) => ({
              id: item.itemId,
              name: item.itemName,
              rarity: (item.rarity as Card['rarity']) || 'Common',
              price: 0,
              image: '',
            }))
          setCardCatalog(cardCatalogFromSpin)
        } catch (err) {
          console.warn('Failed to load spin config:', err)
          setSpinCost(100)
          setSpinRewards([])
        }

        // Try to load bootstrap data (catalog + chat seed)
        let cards = cardCatalogFromSpin
        let maps: MapPack[] = []
        let channels = ['general', 'races', 'trading', 'off-topic']
        let chatSeed: Message[] = []
        
        try {
          const bootstrap = await api.getBootstrap()
          cards = bootstrap.cards
          maps = bootstrap.maps
          channels = bootstrap.channels
          chatSeed = bootstrap.chatSeed
          setCardCatalog(bootstrap.cards)
          setMapCatalog(bootstrap.maps)
          setChannels(bootstrap.channels)
          setMessages(bootstrap.chatSeed)
        } catch (err) {
          console.warn('Bootstrap endpoint not available, using defaults:', err)
          // Keep defaults set above
          setChannels(channels)
          setMessages(chatSeed)
        }

        // If user has a token, hydrate their auth state
        const token = api.getToken()
        if (token) {
          setIsAuthenticated(true)
          try {
            await hydrateAuthState(token, cards, maps)
          } catch (err) {
            // Token might be expired
            console.warn('Failed to hydrate auth state:', err)
            api.clearToken()
            setIsAuthenticated(false)
          }
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load game data')
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [])

  const login = async (email: string, password: string) => {
    const response = await api.login(email, password)
    api.setToken(response.token)
    setIsAuthenticated(true)
    await hydrateAuthState(response.token, cardCatalog, mapCatalog)
  }

  const register = async (data: { displayName: string; age: number; bio: string; email: string; password: string }) => {
    const response = await api.register(data.displayName, data.email, data.password)
    api.setToken(response.token)
    setIsAuthenticated(true)
    await api.updateProfile(response.token, { age: data.age, bio: data.bio, displayName: data.displayName })
    await hydrateAuthState(response.token, cardCatalog, mapCatalog)
  }

  const updateProfile = async (payload: Partial<UserProfile>) => {
    const token = api.getToken()
    if (!token) {
      throw new Error('Login is required to update your profile')
    }

    const updates: { displayName?: string; age?: number; bio?: string } = {}
    if (payload.displayName !== undefined) updates.displayName = payload.displayName
    if (payload.age !== undefined) updates.age = payload.age
    if (payload.bio !== undefined) updates.bio = payload.bio
    const updated = await api.updateProfile(token, updates)

    setUser((prev) => ({
      ...prev,
      displayName: updated.displayName,
      age: updated.age,
      bio: updated.bio,
      avatar: updated.profilePicUrl || prev.avatar,
    }))
  }

  const purchaseCoins = async (packageId: string) => {
    const token = api.getToken()
    if (!token) {
      throw new Error('Login is required to purchase coins')
    }

    const coinPackage = coinPackages.find((item) => item.id === packageId)
    if (!coinPackage) {
      throw new Error('Coin package not found')
    }

    const result = await api.purchaseCoins(token, packageId)
    setCoins(result.newBalance)
    setTransactions((prev) => [
      {
        id: crypto.randomUUID(),
        type: 'coins',
        title: coinPackage.label,
        amount: coinPackage.coins,
        coinsDelta: coinPackage.coins,
        timestamp: now(),
      },
      ...prev,
    ])
  }

  const buyCard = async (card: Card) => {
    const token = api.getToken()
    if (!token) {
      throw new Error('Login is required to buy cards')
    }

    const result = await api.buyCard(token, card.id)
    setCoins(result.newBalance)
    setInventory((prev) => (prev.some((item) => item.id === card.id) ? prev : [...prev, result.purchased]))
    setTransactions((prev) => [
      { id: crypto.randomUUID(), type: 'card', title: card.name, amount: card.price, coinsDelta: -card.price, timestamp: now() },
      ...prev,
    ])
    setUser((prev) => ({ ...prev, stats: { ...prev.stats, cards: prev.stats.cards + 1 } }))
  }

  const buyMapPack = async (pack: MapPack) => {
    const token = api.getToken()
    if (!token) {
      throw new Error('Login is required to buy map packs')
    }

    const result = await api.buyMap(token, pack.id)
    setCoins(result.newBalance)
    setMaps((prev) => (prev.some((item) => item.id === pack.id) ? prev : [...prev, result.purchased]))
    setTransactions((prev) => [
      { id: crypto.randomUUID(), type: 'map', title: pack.name, amount: pack.price, coinsDelta: -pack.price, timestamp: now() },
      ...prev,
    ])
  }

  const sendMessage = (content: string) => {
    if (!content.trim()) return
    const message: Message = {
      id: crypto.randomUUID(),
      user: user.displayName,
      avatar: user.avatar,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      content,
    }
    setMessages((msgs) => [...msgs, message])
  }

  const spinWheel = async () => {
    const token = api.getToken()
    if (!token) {
      throw new Error('Login is required to spin the wheel')
    }

    const result = await api.spin(token)
    const spin: SpinResult = {
      id: crypto.randomUUID(),
      reward: result.itemName,
      coins: 0,
      rarity:
        result.rarity === 'Legendary'
          ? 'Special'
          : result.rarity === 'Epic'
            ? 'Premium'
            : result.rarity === 'Rare'
              ? 'Rare'
              : 'Bonus',
      timestamp: now(),
    }
    setCoins(result.newBalance)
    setSpins((prev) => [spin, ...prev])
    setTransactions((prev) => [
      {
        id: crypto.randomUUID(),
        type: 'spin',
        title: spin.reward,
        amount: spinCost,
        coinsDelta: -spinCost,
        timestamp: now(),
      },
      ...prev,
    ])
    setUser((prev) => ({ ...prev, stats: { ...prev.stats, spins: prev.stats.spins + 1 } }))
    return spin
  }

  const value = useMemo(
    () => ({
      user,
      coins,
      inventory,
      maps,
      messages,
      transactions,
      spins,
      channels,
      cardCatalog,
      mapCatalog,
      coinPackages,
      spinRewards,
      spinCost,
      loading,
      error,
      isAuthenticated,
      login,
      register,
      updateProfile,
      purchaseCoins,
      buyCard,
      buyMapPack,
      sendMessage,
      spinWheel,
    }),
    [
      user,
      coins,
      inventory,
      maps,
      messages,
      transactions,
      spins,
      channels,
      cardCatalog,
      mapCatalog,
      coinPackages,
      spinRewards,
      spinCost,
      loading,
      error,
      isAuthenticated,
    ],
  )

  return <GameContext.Provider value={value}>{children}</GameContext.Provider>
}

export const useGame = () => {
  const ctx = useContext(GameContext)
  if (!ctx) throw new Error('useGame must be used within GameProvider')
  return ctx
}
