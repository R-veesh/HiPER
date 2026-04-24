const express = require('express');
const { body, validationResult } = require('express-validator');
const { databases, DATABASE_ID, COLLECTIONS, ID, Query } = require('../config/appwrite');
const { authMiddleware } = require('../middleware/auth');
const { CARD_CATALOG, MAP_CATALOG, CHAT_CHANNELS, CHAT_SEED } = require('../data/catalog');

const router = express.Router();

async function getWallet(userId) {
    try {
        return await databases.getDocument(DATABASE_ID, COLLECTIONS.WALLETS, userId);
    } catch (err) {
        if (err.code === 404) {
            return databases.createDocument(DATABASE_ID, COLLECTIONS.WALLETS, userId, {
                walletId: userId,
                userId,
                coinBalance: 0,
            });
        }
        throw err;
    }
}

async function hasInventoryItem(userId, itemId) {
    const existing = await databases.listDocuments(DATABASE_ID, COLLECTIONS.INVENTORY, [
        Query.equal('userId', userId),
        Query.equal('itemId', itemId),
        Query.limit(1),
    ]);
    return existing.total > 0;
}

async function addInventoryItem({ userId, itemType, itemId, itemName, rarity, spriteUrl }) {
    await databases.createDocument(DATABASE_ID, COLLECTIONS.INVENTORY, ID.unique(), {
        userId,
        itemType,
        itemId,
        itemName,
        rarity,
        spriteUrl: spriteUrl || '',
        obtainedAt: new Date().toISOString(),
    });
}

async function spendCoins({ userId, amount, description }) {
    const wallet = await getWallet(userId);
    if (wallet.coinBalance < amount) {
        return { error: 'Insufficient coins', current: wallet.coinBalance };
    }

    const newBalance = wallet.coinBalance - amount;
    await databases.updateDocument(DATABASE_ID, COLLECTIONS.WALLETS, userId, { coinBalance: newBalance });
    await databases.createDocument(DATABASE_ID, COLLECTIONS.TRANSACTIONS, ID.unique(), {
        userId,
        type: 'store_purchase',
        amount: -amount,
        description,
        timestamp: new Date().toISOString(),
    });
    return { newBalance };
}

// GET /api/content/bootstrap
router.get('/bootstrap', (req, res) => {
    res.json({
        cards: CARD_CATALOG,
        maps: MAP_CATALOG,
        channels: CHAT_CHANNELS,
        chatSeed: CHAT_SEED,
    });
});

// POST /api/content/cards/buy
router.post('/cards/buy', authMiddleware, [
    body('cardId').isString().trim().notEmpty(),
], async (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({ errors: errors.array() });
    }

    const card = CARD_CATALOG.find((item) => item.id === req.body.cardId);
    if (!card) {
        return res.status(404).json({ error: 'Card not found' });
    }

    try {
        if (await hasInventoryItem(req.userId, card.id)) {
            return res.status(409).json({ error: 'Card already owned' });
        }

        const spendResult = await spendCoins({
            userId: req.userId,
            amount: card.price,
            description: `Card purchase: ${card.name}`,
        });
        if (spendResult.error) {
            return res.status(400).json(spendResult);
        }

        await addInventoryItem({
            userId: req.userId,
            itemType: 'car',
            itemId: card.id,
            itemName: card.name,
            rarity: card.rarity,
            spriteUrl: card.image,
        });

        res.json({
            purchased: card,
            newBalance: spendResult.newBalance,
        });
    } catch (err) {
        console.error('Card purchase error:', err);
        res.status(500).json({ error: 'Failed to purchase card' });
    }
});

// POST /api/content/maps/buy
router.post('/maps/buy', authMiddleware, [
    body('mapId').isString().trim().notEmpty(),
], async (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({ errors: errors.array() });
    }

    const mapPack = MAP_CATALOG.find((item) => item.id === req.body.mapId);
    if (!mapPack) {
        return res.status(404).json({ error: 'Map not found' });
    }

    try {
        if (await hasInventoryItem(req.userId, mapPack.id)) {
            return res.status(409).json({ error: 'Map already owned' });
        }

        const spendResult = await spendCoins({
            userId: req.userId,
            amount: mapPack.price,
            description: `Map purchase: ${mapPack.name}`,
        });
        if (spendResult.error) {
            return res.status(400).json(spendResult);
        }

        await addInventoryItem({
            userId: req.userId,
            itemType: 'map',
            itemId: mapPack.id,
            itemName: mapPack.name,
            rarity: mapPack.rarity,
            spriteUrl: mapPack.image,
        });

        res.json({
            purchased: mapPack,
            newBalance: spendResult.newBalance,
        });
    } catch (err) {
        console.error('Map purchase error:', err);
        res.status(500).json({ error: 'Failed to purchase map' });
    }
});

module.exports = router;
