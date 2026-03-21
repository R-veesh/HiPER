import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../api';
import { Link } from 'react-router-dom';

const RARITY_COLORS = {
    Common: 'border-gray-500 bg-gray-800',
    Rare: 'border-blue-500 bg-blue-900/30',
    Epic: 'border-purple-500 bg-purple-900/30',
    Legendary: 'border-yellow-500 bg-yellow-900/20',
};

const RARITY_BADGE = {
    Common: 'bg-gray-600 text-gray-200',
    Rare: 'bg-blue-600 text-blue-100',
    Epic: 'bg-purple-600 text-purple-100',
    Legendary: 'bg-yellow-600 text-yellow-100',
};

export default function InventoryPage() {
    const { user } = useAuth();
    const [items, setItems] = useState([]);
    const [filter, setFilter] = useState('all');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        api.get('/inventory')
            .then((res) => setItems(res.data.items))
            .finally(() => setLoading(false));
    }, []);

    const filtered = filter === 'all' ? items : items.filter((i) => i.rarity === filter);

    if (!user) return null;

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <nav className="bg-gray-800 border-b border-gray-700 px-6 py-3 flex items-center justify-between">
                <Link to="/profile" className="text-xl font-bold text-purple-400">HYPER</Link>
                <div className="flex items-center gap-4">
                    <span className="text-yellow-400 font-semibold">{user.coinBalance ?? 0} coins</span>
                    <Link to="/profile" className="text-gray-300 hover:text-white">Profile</Link>
                    <Link to="/shop" className="text-gray-300 hover:text-white">Shop</Link>
                    <Link to="/spin" className="text-gray-300 hover:text-white">Spin</Link>
                </div>
            </nav>

            <div className="max-w-4xl mx-auto p-6">
                <h1 className="text-2xl font-bold mb-6">My Inventory</h1>

                {/* Filter tabs */}
                <div className="flex gap-2 mb-6">
                    {['all', 'Common', 'Rare', 'Epic', 'Legendary'].map((f) => (
                        <button
                            key={f}
                            onClick={() => setFilter(f)}
                            className={`px-4 py-1.5 rounded-full text-sm font-semibold transition ${filter === f
                                    ? 'bg-purple-600 text-white'
                                    : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
                                }`}
                        >
                            {f === 'all' ? 'All' : f}
                        </button>
                    ))}
                </div>

                {loading ? (
                    <p className="text-gray-400">Loading inventory...</p>
                ) : filtered.length === 0 ? (
                    <div className="text-center py-12">
                        <p className="text-gray-400 text-lg mb-4">No items found</p>
                        <Link to="/spin" className="text-purple-400 hover:text-purple-300">
                            Try your luck with a spin!
                        </Link>
                    </div>
                ) : (
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                        {filtered.map((item) => (
                            <div
                                key={item.id}
                                className={`rounded-xl border-2 p-4 ${RARITY_COLORS[item.rarity]} transition hover:scale-105`}
                            >
                                <div className="text-4xl text-center mb-2">🚗</div>
                                <h3 className="font-semibold text-center text-sm">{item.itemName}</h3>
                                <div className="flex justify-center mt-2">
                                    <span className={`text-xs px-2 py-0.5 rounded-full font-semibold ${RARITY_BADGE[item.rarity]}`}>
                                        {item.rarity}
                                    </span>
                                </div>
                                <p className="text-gray-500 text-xs text-center mt-2">
                                    {item.itemType === 'car' ? 'Car' : 'Skin'}
                                </p>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}
