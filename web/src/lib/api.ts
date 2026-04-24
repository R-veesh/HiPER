import type { Card, CoinPackage, MapPack, Message } from '../types/game'

const API_BASE = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'http://localhost:3000'
const TOKEN_KEY = 'hyper_token'

type RequestOptions = RequestInit & { token?: string }

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers = new Headers(options.headers ?? {})
  if (!headers.has('Content-Type') && options.body) headers.set('Content-Type', 'application/json')
  if (options.token) headers.set('Authorization', `Bearer ${options.token}`)

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers })
  const data = await res.json().catch(() => ({}))
  if (!res.ok) {
    const message = (data as { error?: string }).error ?? `Request failed (${res.status})`
    throw new Error(message)
  }
  return data as T
}

export const api = {
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY)
  },
  setToken(token: string) {
    localStorage.setItem(TOKEN_KEY, token)
  },
  clearToken() {
    localStorage.removeItem(TOKEN_KEY)
  },

  getBootstrap() {
    return request<{ cards: Card[]; maps: MapPack[]; channels: string[]; chatSeed: Message[] }>('/api/content/bootstrap')
  },
  getCoinPackages() {
    return request<{ packages: CoinPackage[] }>('/api/coins/packages')
  },
  getSpinConfig() {
    return request<{ cost: number; items: Array<{ itemId: string; itemName: string; rarity: string }> }>('/api/spin/config')
  },

  login(email: string, password: string) {
    return request<{ token: string; user: { id: string; email: string; displayName: string; coinBalance: number } }>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
  },
  register(displayName: string, email: string, password: string) {
    return request<{ token: string; user: { id: string; email: string; displayName: string; coinBalance: number } }>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ displayName, email, password }),
    })
  },
  getMe(token: string) {
    return request<{ id: string; email: string; displayName: string; coinBalance: number }>('/api/auth/me', { token })
  },
  getProfile(userId: string) {
    return request<{ userId: string; displayName: string; age: number; bio: string; profilePicUrl: string }>(`/api/profile/${userId}`)
  },
  updateProfile(token: string, payload: { displayName?: string; age?: number; bio?: string }) {
    return request<{ userId: string; displayName: string; age: number; bio: string; profilePicUrl: string }>('/api/profile', {
      method: 'PUT',
      token,
      body: JSON.stringify(payload),
    })
  },
  getBalance(token: string) {
    return request<{ coinBalance: number }>('/api/coins/balance', { token })
  },
  getInventory(token: string) {
    return request<{
      items: Array<{
        itemType: string
        itemId: string
        itemName: string
        rarity: string
        spriteUrl: string
      }>
    }>('/api/inventory', { token })
  },
  purchaseCoins(token: string, packageId: string) {
    return request<{ newBalance: number; coinsAdded: number }>('/api/coins/purchase', {
      method: 'POST',
      token,
      body: JSON.stringify({
        packageId,
        cardNumber: '4242424242424242',
        expiry: '12/30',
        cvv: '123',
      }),
    })
  },
  buyCard(token: string, cardId: string) {
    return request<{ purchased: Card; newBalance: number }>('/api/content/cards/buy', {
      method: 'POST',
      token,
      body: JSON.stringify({ cardId }),
    })
  },
  buyMap(token: string, mapId: string) {
    return request<{ purchased: MapPack; newBalance: number }>('/api/content/maps/buy', {
      method: 'POST',
      token,
      body: JSON.stringify({ mapId }),
    })
  },
  spin(token: string) {
    return request<{ itemId: string; itemName: string; rarity: string; newBalance: number }>('/api/spin', {
      method: 'POST',
      token,
    })
  },
}
