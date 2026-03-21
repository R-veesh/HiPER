import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../api';
import { Link } from 'react-router-dom';

const RARITY_COLORS = {
    Common: 'text-gray-300 border-gray-500',
    Rare: 'text-blue-400 border-blue-500',
    Epic: 'text-purple-400 border-purple-500',
    Legendary: 'text-yellow-400 border-yellow-500',
};

const RARITY_GLOW = {
    Common: '',
    Rare: 'shadow-blue-500/30 shadow-lg',
    Epic: 'shadow-purple-500/40 shadow-xl',
    Legendary: 'shadow-yellow-500/50 shadow-2xl animate-pulse',
};

export default function SpinPage() {
    const { user, refreshBalance } = useAuth();
    const [spinning, setSpinning] = useState(false);
    const [result, setResult] = useState(null);
    const [spinCost, setSpinCost] = useState(100);
    const [animating, setAnimating] = useState(false);

    useEffect(() => {
        refreshBalance();
        api.get('/spin/config').then((res) => {
            setSpinCost(res.data.cost);
        });
    }, []);

    const handleSpin = async () => {
        setResult(null);
        setSpinning(true);
        setAnimating(true);

        try {
            const res = await api.post('/spin');
            // Delay showing result for animation effect
            setTimeout(() => {
                setResult(res.data);
                setAnimating(false);
                refreshBalance();
            }, 2000);
        } catch (err) {
            setAnimating(false);
            setResult({ error: err.response?.data?.error || 'Spin failed' });
        } finally {
            setSpinning(false);
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
                    <Link to="/shop" className="text-gray-300 hover:text-white">Shop</Link>
                    <Link to="/inventory" className="text-gray-300 hover:text-white">Inventory</Link>
                </div>
            </nav>

            <div className="max-w-2xl mx-auto p-6 text-center">
                <h1 className="text-3xl font-bold mb-2">Car Gacha Spin</h1>
                <p className="text-gray-400 mb-8">Cost: {spinCost} coins per spin</p>

                {/* Spin area */}
                <div className="mb-8">
                    {animating ? (
                        <div className="w-48 h-48 mx-auto rounded-2xl bg-gray-700 flex items-center justify-center border-4 border-purple-500 animate-spin">
                            <span className="text-6xl">🎰</span>
                        </div>
                    ) : result && !result.error ? (
                        <div className={`w-64 mx-auto rounded-2xl bg-gray-800 p-6 border-4 ${RARITY_COLORS[result.rarity]} ${RARITY_GLOW[result.rarity]}`}>
                            <div className="text-6xl mb-3">🚗</div>
                            <h2 className="text-xl font-bold">{result.itemName}</h2>
                            <span className={`text-sm font-semibold ${RARITY_COLORS[result.rarity]}`}>
                                {result.rarity}
                            </span>
                            {result.isNew ? (
                                <div className="text-green-400 text-sm mt-2 font-semibold">✨ NEW!</div>
                            ) : (
                                <div className="text-gray-500 text-sm mt-2">Already owned</div>
                            )}
                        </div>
                    ) : result?.error ? (
                        <div className="bg-red-500/20 border border-red-500 text-red-300 px-6 py-4 rounded-xl max-w-sm mx-auto">
                            {result.error}
                        </div>
                    ) : (
                        <div className="w-48 h-48 mx-auto rounded-2xl bg-gray-800 border-2 border-gray-600 flex items-center justify-center">
                            <span className="text-6xl">🎰</span>
                        </div>
                    )}
                </div>

                <div className="flex gap-4 justify-center">
                    <button
                        onClick={handleSpin}
                        disabled={spinning || animating || (user.coinBalance ?? 0) < spinCost}
                        className="bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white font-bold px-8 py-3 rounded-xl text-lg transition"
                    >
                        {animating ? 'Spinning...' : `Spin (${spinCost} coins)`}
                    </button>
                    {result && !result.error && (
                        <Link
                            to="/inventory"
                            className="bg-gray-700 hover:bg-gray-600 text-white font-semibold px-6 py-3 rounded-xl transition"
                        >
                            View Inventory
                        </Link>
                    )}
                </div>

                {(user.coinBalance ?? 0) < spinCost && (
                    <p className="text-red-400 mt-4 text-sm">
                        Not enough coins!{' '}
                        <Link to="/shop" className="text-purple-400 hover:text-purple-300">Buy more</Link>
                    </p>
                )}

                {/* Rarity guide */}
                <div className="mt-12 bg-gray-800 rounded-xl p-6 text-left">
                    <h3 className="font-semibold mb-3">Drop Rates</h3>
                    <div className="grid grid-cols-2 gap-2 text-sm">
                        <div className="text-gray-300">⬜ Common</div><div className="text-gray-400">60%</div>
                        <div className="text-blue-400">🟦 Rare</div><div className="text-gray-400">25%</div>
                        <div className="text-purple-400">🟪 Epic</div><div className="text-gray-400">12%</div>
                        <div className="text-yellow-400">🟨 Legendary</div><div className="text-gray-400">3%</div>
                    </div>
                </div>
            </div>
        </div>
    );
}
