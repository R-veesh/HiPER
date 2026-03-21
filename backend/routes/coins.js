const express = require('express');
const { body, validationResult } = require('express-validator');
const { databases, DATABASE_ID, COLLECTIONS, ID } = require('../config/appwrite');
const { authMiddleware } = require('../middleware/auth');

const router = express.Router();

// GET /api/coins/balance
router.get('/balance', authMiddleware, async (req, res) => {
    try {
        const wallet = await databases.getDocument(DATABASE_ID, COLLECTIONS.WALLETS, req.userId);
        res.json({ coinBalance: wallet.coinBalance });
    } catch (err) {
        if (err.code === 404) {
            return res.json({ coinBalance: 0 });
        }
        console.error('Get balance error:', err);
        res.status(500).json({ error: 'Failed to get balance' });
    }
});

// Coin packages available for purchase
const COIN_PACKAGES = [
    { id: 'pack_100', coins: 100, price: 0.99, label: '100 Coins' },
    { id: 'pack_500', coins: 500, price: 3.99, label: '500 Coins' },
    { id: 'pack_1200', coins: 1200, price: 7.99, label: '1,200 Coins' },
    { id: 'pack_3000', coins: 3000, price: 14.99, label: '3,000 Coins' },
];

// GET /api/coins/packages
router.get('/packages', (req, res) => {
    res.json({ packages: COIN_PACKAGES });
});

// POST /api/coins/purchase (fake card payment)
router.post('/purchase', authMiddleware, [
    body('packageId').notEmpty(),
    body('cardNumber').isLength({ min: 16, max: 16 }).isNumeric()
        .withMessage('Card number must be 16 digits'),
    body('expiry').matches(/^\d{2}\/\d{2}$/).withMessage('Expiry must be MM/YY format'),
    body('cvv').isLength({ min: 3, max: 4 }).isNumeric()
        .withMessage('CVV must be 3-4 digits'),
], async (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({ errors: errors.array() });
    }

    const { packageId } = req.body;
    const coinPack = COIN_PACKAGES.find(p => p.id === packageId);
    if (!coinPack) {
        return res.status(400).json({ error: 'Invalid package' });
    }

    try {
        // Get current wallet (auto-create if missing)
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
        const newBalance = wallet.coinBalance + coinPack.coins;

        // Update balance
        await databases.updateDocument(DATABASE_ID, COLLECTIONS.WALLETS, req.userId, {
            coinBalance: newBalance,
        });

        // Log transaction
        await databases.createDocument(DATABASE_ID, COLLECTIONS.TRANSACTIONS, ID.unique(), {
            userId: req.userId,
            type: 'purchase',
            amount: coinPack.coins,
            description: `Purchased ${coinPack.label} for $${coinPack.price}`,
            timestamp: new Date().toISOString(),
        });

        res.json({
            message: 'Purchase successful',
            coinsAdded: coinPack.coins,
            newBalance,
        });
    } catch (err) {
        console.error('Purchase error:', err);
        res.status(500).json({ error: 'Purchase failed' });
    }
});

// POST /api/coins/spend (deduct coins — used internally by spin, etc.)
router.post('/spend', authMiddleware, [
    body('amount').isInt({ min: 1 }),
    body('reason').optional().trim(),
], async (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({ errors: errors.array() });
    }

    const { amount, reason } = req.body;

    try {
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
        if (wallet.coinBalance < amount) {
            return res.status(400).json({ error: 'Insufficient coins' });
        }

        const newBalance = wallet.coinBalance - amount;
        await databases.updateDocument(DATABASE_ID, COLLECTIONS.WALLETS, req.userId, {
            coinBalance: newBalance,
        });

        await databases.createDocument(DATABASE_ID, COLLECTIONS.TRANSACTIONS, ID.unique(), {
            userId: req.userId,
            type: 'spend',
            amount: -amount,
            description: reason || 'Coin spend',
            timestamp: new Date().toISOString(),
        });

        res.json({ newBalance });
    } catch (err) {
        console.error('Spend error:', err);
        res.status(500).json({ error: 'Spend failed' });
    }
});

module.exports = router;
