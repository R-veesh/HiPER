const express = require('express');
const { body, validationResult } = require('express-validator');
const { databases, storage, DATABASE_ID, COLLECTIONS, STORAGE_BUCKETS, ID } = require('../config/appwrite');
const { authMiddleware } = require('../middleware/auth');

const router = express.Router();

// GET /api/profile/:userId
router.get('/:userId', async (req, res) => {
    try {
        const profile = await databases.getDocument(
            DATABASE_ID, COLLECTIONS.PROFILES, req.params.userId
        );
        res.json({
            userId: profile.userId,
            displayName: profile.displayName,
            age: profile.age,
            bio: profile.bio,
            profilePicUrl: profile.profilePicUrl,
            createdAt: profile.createdAt,
        });
    } catch (err) {
        if (err.code === 404) {
            return res.status(404).json({ error: 'Profile not found' });
        }
        console.error('Get profile error:', err);
        res.status(500).json({ error: 'Failed to get profile' });
    }
});

// PUT /api/profile (auth required)
router.put('/', authMiddleware, [
    body('displayName').optional().trim().isLength({ min: 2, max: 30 }),
    body('age').optional().isInt({ min: 0, max: 150 }),
    body('bio').optional().trim().isLength({ max: 500 }),
], async (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({ errors: errors.array() });
    }

    const updates = {};
    if (req.body.displayName !== undefined) updates.displayName = req.body.displayName;
    if (req.body.age !== undefined) updates.age = req.body.age;
    if (req.body.bio !== undefined) updates.bio = req.body.bio;

    if (Object.keys(updates).length === 0) {
        return res.status(400).json({ error: 'No fields to update' });
    }

    try {
        let profile;
        try {
            profile = await databases.updateDocument(
                DATABASE_ID, COLLECTIONS.PROFILES, req.userId, updates
            );
        } catch (err) {
            if (err.code === 404) {
                // Auto-create profile if it doesn't exist (legacy user)
                profile = await databases.createDocument(DATABASE_ID, COLLECTIONS.PROFILES, req.userId, {
                    userId: req.userId,
                    displayName: updates.displayName || 'Player',
                    age: updates.age || 0,
                    bio: updates.bio || '',
                    profilePicUrl: '',
                });
            } else {
                throw err;
            }
        }
        res.json({
            userId: profile.userId,
            displayName: profile.displayName,
            age: profile.age,
            bio: profile.bio,
            profilePicUrl: profile.profilePicUrl,
        });
    } catch (err) {
        console.error('Update profile error:', err);
        res.status(500).json({ error: 'Failed to update profile' });
    }
});

// POST /api/profile/avatar (auth required, multipart upload)
const multer = require('multer');
const upload = multer({
    storage: multer.memoryStorage(),
    limits: { fileSize: 2 * 1024 * 1024 }, // 2MB max
    fileFilter: (req, file, cb) => {
        const allowed = ['image/jpeg', 'image/png', 'image/webp'];
        cb(null, allowed.includes(file.mimetype));
    },
});

router.post('/avatar', authMiddleware, upload.single('avatar'), async (req, res) => {
    if (!req.file) {
        return res.status(400).json({ error: 'No valid image file provided' });
    }

    try {
        // Upload to Appwrite Storage
        const { InputFile } = require('node-appwrite/file');
        const file = await storage.createFile(
            STORAGE_BUCKETS.AVATARS,
            req.userId, // use userId as file ID for easy overwrite
            InputFile.fromBuffer(req.file.buffer, req.file.originalname)
        );

        const fileUrl = `${process.env.APPWRITE_ENDPOINT}/storage/buckets/${STORAGE_BUCKETS.AVATARS}/files/${file.$id}/view?project=${process.env.APPWRITE_PROJECT_ID}`;

        // Update profile with avatar URL
        await databases.updateDocument(
            DATABASE_ID, COLLECTIONS.PROFILES, req.userId,
            { profilePicUrl: fileUrl }
        );

        res.json({ profilePicUrl: fileUrl });
    } catch (err) {
        // If file already exists, delete and re-upload
        if (err.code === 409) {
            try {
                await storage.deleteFile(STORAGE_BUCKETS.AVATARS, req.userId);
                const { InputFile } = require('node-appwrite/file');
                const file = await storage.createFile(
                    STORAGE_BUCKETS.AVATARS,
                    req.userId,
                    InputFile.fromBuffer(req.file.buffer, req.file.originalname)
                );
                const fileUrl = `${process.env.APPWRITE_ENDPOINT}/storage/buckets/${STORAGE_BUCKETS.AVATARS}/files/${file.$id}/view?project=${process.env.APPWRITE_PROJECT_ID}`;
                await databases.updateDocument(
                    DATABASE_ID, COLLECTIONS.PROFILES, req.userId,
                    { profilePicUrl: fileUrl }
                );
                return res.json({ profilePicUrl: fileUrl });
            } catch (retryErr) {
                console.error('Avatar re-upload error:', retryErr);
                return res.status(500).json({ error: 'Failed to upload avatar' });
            }
        }
        console.error('Avatar upload error:', err);
        res.status(500).json({ error: 'Failed to upload avatar' });
    }
});

module.exports = router;
