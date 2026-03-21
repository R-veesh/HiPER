const express = require('express');
const { databases, DATABASE_ID, COLLECTIONS, ID, Query } = require('../config/appwrite');
const { authMiddleware } = require('../middleware/auth');

const router = express.Router();

// GET /api/inventory
router.get('/', authMiddleware, async (req, res) => {
    try {
        const docs = await databases.listDocuments(DATABASE_ID, COLLECTIONS.INVENTORY, [
            Query.equal('userId', req.userId),
            Query.limit(100),
        ]);
        res.json({
            items: docs.documents.map(d => ({
                id: d.$id,
                itemType: d.itemType,
                itemId: d.itemId,
                itemName: d.itemName,
                rarity: d.rarity,
                spriteUrl: d.spriteUrl,
                obtainedAt: d.obtainedAt,
            })),
        });
    } catch (err) {
        console.error('Get inventory error:', err);
        res.status(500).json({ error: 'Failed to get inventory' });
    }
});

// GET /api/inventory/check/:itemId (check if user owns an item)
router.get('/check/:itemId', authMiddleware, async (req, res) => {
    try {
        const docs = await databases.listDocuments(DATABASE_ID, COLLECTIONS.INVENTORY, [
            Query.equal('userId', req.userId),
            Query.equal('itemId', req.params.itemId),
            Query.limit(1),
        ]);
        res.json({ owned: docs.total > 0 });
    } catch (err) {
        console.error('Check inventory error:', err);
        res.status(500).json({ error: 'Failed to check inventory' });
    }
});

module.exports = router;
