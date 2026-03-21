const express = require('express');
const { databases, DATABASE_ID, COLLECTIONS, ID, Query } = require('../config/appwrite');
const { authMiddleware } = require('../middleware/auth');
const crypto = require('crypto');

const router = express.Router();

const SPIN_COST = 100;

// Rarity tiers with cumulative probability thresholds
const RARITY_TIERS = [
    { rarity: 'Common', threshold: 0.60 },
    { rarity: 'Rare', threshold: 0.85 },     // 0.60 + 0.25
    { rarity: 'Epic', threshold: 0.97 },      // 0.85 + 0.12
    { rarity: 'Legendary', threshold: 1.00 }, // 0.97 + 0.03
];

function pickRarity() {
    const roll = crypto.randomInt(10000) / 10000; // server-side secure random
    for (const tier of RARITY_TIERS) {
        if (roll < tier.threshold) return tier.rarity;
    }
    return 'Common'; // fallback
}

// POST /api/spin
router.post('/', authMiddleware, async (req, res) => {
    try {
        // 1. Check balance (auto-create wallet if missing)
        let wallet;
        try {
            wallet = await databases.getDocument(DATABASE_ID, COLLECTIONS.WALLETS, req.userId);
        } catch (err) {
            if (err.code === 404) {
                wallet = await databases.createDocument(DATABASE_ID, COLLECTIONS.WALLETS, req.userId, {
                    walletId: req.userId,
                    userId: req.userId,
                    coinBalance: 0,
                });
            } else {
                throw err;
            }
        }
        if (wallet.coinBalance < SPIN_COST) {
            return res.status(400).json({
                error: 'Insufficient coins',
                required: SPIN_COST,
                current: wallet.coinBalance,
            });
        }

        // 2. Pick rarity
        const rarity = pickRarity();

        // 3. Get available items for this rarity from spin_config
        const configDocs = await databases.listDocuments(DATABASE_ID, COLLECTIONS.SPIN_CONFIG, [
            Query.equal('rarity', rarity),
            Query.limit(100),
        ]);

        if (configDocs.total === 0) {
            return res.status(500).json({ error: 'No items configured for this rarity' });
        }

        // 4. Pick a random item from the rarity pool
        const randomIndex = crypto.randomInt(configDocs.total);
        const selectedItem = configDocs.documents[randomIndex];

        // 5. Check if user already owns this item
        const existingItems = await databases.listDocuments(DATABASE_ID, COLLECTIONS.INVENTORY, [
            Query.equal('userId', req.userId),
            Query.equal('itemId', selectedItem.itemId),
            Query.limit(1),
        ]);
        const isNew = existingItems.total === 0;

        // 6. Deduct coins
        const newBalance = wallet.coinBalance - SPIN_COST;
        await databases.updateDocument(DATABASE_ID, COLLECTIONS.WALLETS, req.userId, {
            coinBalance: newBalance,
        });

        // 7. Add to inventory if new
        if (isNew) {
            await databases.createDocument(DATABASE_ID, COLLECTIONS.INVENTORY, ID.unique(), {
                userId: req.userId,
                itemType: selectedItem.itemType || 'car',
                itemId: selectedItem.itemId,
                itemName: selectedItem.itemName,
                rarity: selectedItem.rarity,
                spriteUrl: selectedItem.spriteUrl || '',
                obtainedAt: new Date().toISOString(),
            });
        }

        // 8. Log transaction
        await databases.createDocument(DATABASE_ID, COLLECTIONS.TRANSACTIONS, ID.unique(), {
            userId: req.userId,
            type: 'spin',
            amount: -SPIN_COST,
            description: `Spin result: ${selectedItem.itemName} (${rarity})`,
            timestamp: new Date().toISOString(),
        });

        res.json({
            itemId: selectedItem.itemId,
            itemName: selectedItem.itemName,
            rarity: selectedItem.rarity,
            spriteUrl: selectedItem.spriteUrl || '',
            isNew,
            newBalance,
        });
    } catch (err) {
        console.error('Spin error:', err);
        res.status(500).json({ error: 'Spin failed' });
    }
});

// GET /api/spin/config (public — returns available items for display)
router.get('/config', async (req, res) => {
    try {
        const configDocs = await databases.listDocuments(DATABASE_ID, COLLECTIONS.SPIN_CONFIG, [
            Query.limit(100),
        ]);
        res.json({
            cost: SPIN_COST,
            items: configDocs.documents.map(d => ({
                itemId: d.itemId,
                itemName: d.itemName,
                rarity: d.rarity,
                spriteUrl: d.spriteUrl || '',
            })),
        });
    } catch (err) {
        console.error('Get spin config error:', err);
        res.status(500).json({ error: 'Failed to get spin config' });
    }
});

module.exports = router;
