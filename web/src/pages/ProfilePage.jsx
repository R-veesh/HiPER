import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../api';
import { Link } from 'react-router-dom';

export default function ProfilePage() {
    const { user, logout } = useAuth();
    const [displayName, setDisplayName] = useState('');
    const [age, setAge] = useState(0);
    const [bio, setBio] = useState('');
    const [profilePicUrl, setProfilePicUrl] = useState('');
    const [saving, setSaving] = useState(false);
    const [message, setMessage] = useState('');

    useEffect(() => {
        if (!user) return;
        api.get(`/profile/${user.id}`).then((res) => {
            setDisplayName(res.data.displayName);
            setAge(res.data.age);
            setBio(res.data.bio);
            setProfilePicUrl(res.data.profilePicUrl);
        });
    }, [user]);

    const handleSave = async (e) => {
        e.preventDefault();
        setSaving(true);
        setMessage('');
        try {
            await api.put('/profile', { displayName, age: Number(age), bio });
            setMessage('Profile updated!');
        } catch (err) {
            setMessage(err.response?.data?.error || 'Update failed');
        } finally {
            setSaving(false);
        }
    };

    const handleAvatar = async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        const formData = new FormData();
        formData.append('avatar', file);
        try {
            const res = await api.post('/profile/avatar', formData, {
                headers: { 'Content-Type': 'multipart/form-data' },
            });
            setProfilePicUrl(res.data.profilePicUrl);
            setMessage('Avatar updated!');
        } catch {
            setMessage('Avatar upload failed');
        }
    };

    if (!user) return null;

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <nav className="bg-gray-800 border-b border-gray-700 px-6 py-3 flex items-center justify-between">
                <Link to="/profile" className="text-xl font-bold text-purple-400">HYPER</Link>
                <div className="flex items-center gap-4">
                    <span className="text-yellow-400 font-semibold">{user.coinBalance ?? 0} coins</span>
                    <Link to="/shop" className="text-gray-300 hover:text-white">Shop</Link>
                    <Link to="/spin" className="text-gray-300 hover:text-white">Spin</Link>
                    <Link to="/inventory" className="text-gray-300 hover:text-white">Inventory</Link>
                    <button onClick={logout} className="text-red-400 hover:text-red-300 text-sm">Logout</button>
                </div>
            </nav>

            <div className="max-w-2xl mx-auto p-6">
                <h1 className="text-2xl font-bold mb-6">My Profile</h1>

                {message && (
                    <div className="bg-purple-500/20 border border-purple-500 text-purple-300 px-4 py-2 rounded-lg mb-4">
                        {message}
                    </div>
                )}

                <div className="flex items-center gap-6 mb-6">
                    <div className="relative">
                        {profilePicUrl ? (
                            <img src={profilePicUrl} alt="Avatar" className="w-24 h-24 rounded-full object-cover" />
                        ) : (
                            <div className="w-24 h-24 rounded-full bg-gray-700 flex items-center justify-center text-3xl">
                                {displayName?.[0]?.toUpperCase() || '?'}
                            </div>
                        )}
                        <label className="absolute bottom-0 right-0 bg-purple-600 rounded-full p-1 cursor-pointer hover:bg-purple-700">
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                            </svg>
                            <input type="file" accept="image/*" onChange={handleAvatar} className="hidden" />
                        </label>
                    </div>
                    <div>
                        <h2 className="text-xl font-semibold">{displayName}</h2>
                        <p className="text-gray-400 text-sm">{user.email}</p>
                    </div>
                </div>

                <form onSubmit={handleSave} className="space-y-4">
                    <div>
                        <label className="block text-gray-300 text-sm mb-1">Display Name</label>
                        <input
                            type="text"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                            className="w-full bg-gray-700 text-white px-4 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
                            minLength={2}
                            maxLength={30}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-300 text-sm mb-1">Age</label>
                        <input
                            type="number"
                            value={age}
                            onChange={(e) => setAge(e.target.value)}
                            className="w-full bg-gray-700 text-white px-4 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
                            min={0}
                            max={150}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-300 text-sm mb-1">Bio</label>
                        <textarea
                            value={bio}
                            onChange={(e) => setBio(e.target.value)}
                            className="w-full bg-gray-700 text-white px-4 py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
                            rows={3}
                            maxLength={500}
                        />
                    </div>
                    <button
                        type="submit"
                        disabled={saving}
                        className="bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white font-semibold px-6 py-2 rounded-lg transition"
                    >
                        {saving ? 'Saving...' : 'Save Changes'}
                    </button>
                </form>
            </div>
        </div>
    );
}
