import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../api';
import { Link } from 'react-router-dom';

const COIN_PACKAGES = [
    { id: 'pack_100', coins: 100, price: 0.99, label: '100 Coins' },
    { id: 'pack_500', coins: 500, price: 3.99, label: '500 Coins' },
    { id: 'pack_1200', coins: 1200, price: 7.99, label: '1,200 Coins' },
    { id: 'pack_3000', coins: 3000, price: 14.99, label: '3,000 Coins' },
];

export default function ShopPage() {
    const { user, refreshBalance } = useAuth();
    const [selectedPack, setSelectedPack] = useState(null);
    const [cardNumber, setCardNumber] = useState('');
    const [expiry, setExpiry] = useState('');
    const [cvv, setCvv] = useState('');
    const [buying, setBuying] = useState(false);
    const [message, setMessage] = useState('');

    useEffect(() => { refreshBalance(); }, []);

    const handlePurchase = async (e) => {
        e.preventDefault();
        if (!selectedPack) return;
        setMessage('');
        setBuying(true);
        try {
            const res = await api.post('/coins/purchase', {
                packageId: selectedPack.id,
                cardNumber: cardNumber.replace(/\s/g, ''),
                expiry,
                cvv,
            });
            setMessage(`Purchased ${res.data.coinsAdded} coins! New balance: ${res.data.newBalance}`);
            await refreshBalance();
            setSelectedPack(null);
            setCardNumber('');
            setExpiry('');
            setCvv('');
        } catch (err) {
            setMessage(err.response?.data?.error || err.response?.data?.errors?.[0]?.msg || 'Purchase failed');
        } finally {
            setBuying(false);
        }
    };

    if (!user) return null;

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <nav className="bg-gray-800 border-b border-gray-700 px-6 py-3 flex items-center justify-between">
                <Link to="/profile" className="text-xl font-bold text-purple-400">HYPER</Link>
                <div className="flex items-center gap-4">
                    <span className="text-yellow-400 font-semibold">{user.coinBalance ?? 0} coins</span>
                    <Link to="/profile" className="text-gray-300 hover:text-white">Profile</Link>
                    <Link to="/spin" className="text-gray-300 hover:text-white">Spin</Link>
                    <Link to="/inventory" className="text-gray-300 hover:text-white">Inventory</Link>
                </div>
            </nav>

            <div className="max-w-4xl mx-auto p-6">
                <h1 className="text-2xl font-bold mb-6">Coin Shop</h1>

                {message && (
                    <div className="bg-green-500/20 border border-green-500 text-green-300 px-4 py-2 rounded-lg mb-4">
                        {message}
                    </div>
                )}

                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
                    {COIN_PACKAGES.map((pack) => (
                        <button
                            key={pack.id}
                            onClick={() => setSelectedPack(pack)}
                            className={`p-6 rounded-xl border-2 transition text-center ${selectedPack?.id === pack.id
                                    ? 'border-purple-500 bg-purple-500/20'
                                    : 'border-gray-700 bg-gray-800 hover:border-gray-500'
                                }`}
                        >
                            <div className="text-3xl mb-2">🪙</div>
                            <div className="text-lg font-bold text-yellow-400">{pack.label}</div>
                            <div className="text-gray-400 mt-1">${pack.price.toFixed(2)}</div>
                        </button>
                    ))}
                </div>

                {selectedPack && (
                    <div className="bg-gray-800 rounded-xl p-6 max-w-md mx-auto">
                        <h2 className="text-lg font-semibold mb-4">
                            Purchase {selectedPack.label} — ${selectedPack.price.toFixed(2)}
                        </h2>
                        <p className="text-gray-400 text-sm mb-4">
                            Test mode: any 16-digit card number is accepted.
                        </p>
                        <form onSubmit={handlePurchase} className="space-y-3">
                            <div>
                                <label className="block text-gray-300 text-sm mb-1">Card Number</label>
                                <input
                                    type="text"
                                    value={cardNumber}
                                    onChange={(e) => setCardNumber(e.target.value)}
                                    placeholder="1234 5678 9012 3456"
                                    className="w-full bg-gray-700 text-white px-4 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
                                    maxLength={19}
                                    required
                                />
                            </div>
                            <div className="flex gap-3">
                                <div className="flex-1">
                                    <label className="block text-gray-300 text-sm mb-1">Expiry</label>
                                    <input
                                        type="text"
                                        value={expiry}
                                        onChange={(e) => setExpiry(e.target.value)}
                                        placeholder="MM/YY"
                                        className="w-full bg-gray-700 text-white px-4 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
                                        maxLength={5}
                                        required
                                    />
                                </div>
                                <div className="flex-1">
                                    <label className="block text-gray-300 text-sm mb-1">CVV</label>
                                    <input
                                        type="text"
                                        value={cvv}
                                        onChange={(e) => setCvv(e.target.value)}
                                        placeholder="123"
                                        className="w-full bg-gray-700 text-white px-4 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
                                        maxLength={4}
                                        required
                                    />
                                </div>
                            </div>
                            <button
                                type="submit"
                                disabled={buying}
                                className="w-full bg-green-600 hover:bg-green-700 disabled:opacity-50 text-white font-semibold py-2 rounded-lg transition mt-2"
                            >
                                {buying ? 'Processing...' : `Pay $${selectedPack.price.toFixed(2)}`}
                            </button>
                        </form>
                    </div>
                )}
            </div>
        </div>
    );
}
