const express = require('express');
const { body, validationResult } = require('express-validator');
const { Client, Account } = require('node-appwrite');
const { users, databases, DATABASE_ID, COLLECTIONS, ID, Query } = require('../config/appwrite');
const { generateToken } = require('../middleware/auth');

const router = express.Router();

// POST /api/auth/register
router.post('/register', [
    body('email').isEmail().normalizeEmail(),
    body('password').isLength({ min: 8 }).withMessage('Password must be at least 8 characters'),
    body('displayName').trim().isLength({ min: 2, max: 30 }),
], async (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({ errors: errors.array() });
    }

    const { email, password, displayName } = req.body;

    try {
        // Create Appwrite user
        const user = await users.create(ID.unique(), email, undefined, password, displayName);

        // Create profile document
        await databases.createDocument(DATABASE_ID, COLLECTIONS.PROFILES, user.$id, {
            userId: user.$id,
            displayName,
            age: 0,
            bio: '',
            profilePicUrl: '',
        });

        // Create wallet with 500 starter coins
        await databases.createDocument(DATABASE_ID, COLLECTIONS.WALLETS, user.$id, {
            walletId: user.$id,
            userId: user.$id,
            coinBalance: 500,
        });

        const token = generateToken(user.$id, email);

        res.status(201).json({
            token,
            user: {
                id: user.$id,
                email,
                displayName,
                coinBalance: 500,
            },
        });
    } catch (err) {
        if (err.code === 409) {
            return res.status(409).json({ error: 'A user with this email already exists' });
        }
        console.error('Register error:', err);
        res.status(500).json({ error: 'Registration failed' });
    }
});

// POST /api/auth/login
router.post('/login', [
    body('email').isEmail().normalizeEmail(),
    body('password').notEmpty(),
], async (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({ errors: errors.array() });
    }

    const { email, password } = req.body;

    try {
        // Verify credentials using a throwaway Appwrite client (no API key).
        // Account.createEmailPasswordSession is a CLIENT endpoint —
        // it only works on a Client that has NO API key set.
        const verifyClient = new Client()
            .setEndpoint(process.env.APPWRITE_ENDPOINT)
            .setProject(process.env.APPWRITE_PROJECT_ID);

        const account = new Account(verifyClient);

        let session;
        try {
            session = await account.createEmailPasswordSession(email, password);
        } catch {
            return res.status(401).json({ error: 'Invalid email or password' });
        }

        // Delete the session right away (we only needed it to verify the password)
        try {
            verifyClient.setSession(session.$id);
            await account.deleteSession(session.$id);
        } catch {
            // Not critical if cleanup fails
        }

        // Look up the user via the Server SDK to get their userId
        const userList = await users.list([Query.equal('email', [email])]);
        if (userList.total === 0) {
            return res.status(401).json({ error: 'Invalid email or password' });
        }

        const user = userList.users[0];

        // Get wallet balance
        let coinBalance = 0;
        try {
            const wallet = await databases.getDocument(DATABASE_ID, COLLECTIONS.WALLETS, user.$id);
            coinBalance = wallet.coinBalance;
        } catch {
            // Wallet might not exist yet for legacy users
        }

        const token = generateToken(user.$id, email);

        res.json({
            token,
            user: {
                id: user.$id,
                email: user.email,
                displayName: user.name,
                coinBalance,
            },
        });
    } catch (err) {
        console.error('Login error:', err);
        res.status(500).json({ error: 'Login failed' });
    }
});

// GET /api/auth/me (requires auth)
const { authMiddleware } = require('../middleware/auth');
router.get('/me', authMiddleware, async (req, res) => {
    try {
        const user = await users.get(req.userId);
        let coinBalance = 0;
        try {
            const wallet = await databases.getDocument(DATABASE_ID, COLLECTIONS.WALLETS, req.userId);
            coinBalance = wallet.coinBalance;
        } catch {
            // No wallet yet
        }

        res.json({
            id: user.$id,
            email: user.email,
            displayName: user.name,
            coinBalance,
        });
    } catch (err) {
        console.error('Get user error:', err);
        res.status(500).json({ error: 'Failed to get user info' });
    }
});

// POST /api/auth/logout (client-side token discard — stateless JWT)
router.post('/logout', (req, res) => {
    res.json({ message: 'Logged out successfully' });
});

module.exports = router;
