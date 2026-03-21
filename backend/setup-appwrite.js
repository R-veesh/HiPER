/**
 * One-time script to create all Appwrite collections, attributes,
 * indexes, and the avatars storage bucket.
 *
 * Usage:  node setup-appwrite.js
 */
require('dotenv').config();
const { Client, Databases, Storage, ID, Permission, Role } = require('node-appwrite');

const client = new Client()
    .setEndpoint(process.env.APPWRITE_ENDPOINT)
    .setProject(process.env.APPWRITE_PROJECT_ID)
    .setKey(process.env.APPWRITE_API_KEY);

const databases = new Databases(client);
const storage = new Storage(client);

const DB = process.env.APPWRITE_DATABASE_ID;
const anyone = [
    Permission.read(Role.any()),
    Permission.create(Role.any()),
    Permission.update(Role.any()),
    Permission.delete(Role.any()),
];

async function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }

// Helper: create collection, skip if already exists
async function ensureCollection(id, name) {
    try {
        await databases.createCollection(DB, id, name, anyone);
        console.log(`  ✔ Created collection "${name}"`);
    } catch (e) {
        if (e.code === 409) console.log(`  – Collection "${name}" already exists`);
        else throw e;
    }
}

// Helper: create attribute, skip if already exists
async function attr(collId, type, key, opts = {}) {
    try {
        switch (type) {
            case 'string':
                await databases.createStringAttribute(collId === 'db' ? DB : DB, collId, key, opts.size || 256, opts.required ?? false, opts.default, opts.array ?? false);
                break;
            case 'integer':
                await databases.createIntegerAttribute(DB, collId, key, opts.required ?? false, opts.min, opts.max, opts.default, opts.array ?? false);
                break;
        }
        console.log(`    + ${collId}.${key} (${type})`);
    } catch (e) {
        if (e.code === 409) console.log(`    – ${collId}.${key} already exists`);
        else throw e;
    }
}

// Helper: create index, skip if already exists
async function idx(collId, key, indexType, attributes) {
    try {
        await databases.createIndex(DB, collId, key, indexType, attributes);
        console.log(`    ⊕ index ${collId}[${key}]`);
    } catch (e) {
        if (e.code === 409) console.log(`    – index ${collId}[${key}] already exists`);
        else throw e;
    }
}

async function main() {
    console.log('\n=== Appwrite Setup ===\n');

    // ── 1. profiles ──
    await ensureCollection('profiles', 'Profiles');
    await attr('profiles', 'string', 'userId', { size: 36, required: true });
    await attr('profiles', 'string', 'displayName', { size: 30, required: true });
    await attr('profiles', 'integer', 'age', { required: false, min: 0, max: 150 });
    await attr('profiles', 'string', 'bio', { size: 500, required: false });
    await attr('profiles', 'string', 'profilePicUrl', { size: 500, required: false });
    await sleep(2000); // let Appwrite process attributes before creating indexes
    await idx('profiles', 'idx_userId', 'key', ['userId']);

    // ── 2. wallets ──
    await ensureCollection('wallets', 'Wallets');
    await attr('wallets', 'string', 'userId', { size: 36, required: true });
    await attr('wallets', 'integer', 'coinBalance', { required: true, min: 0 });
    await sleep(2000);
    await idx('wallets', 'idx_userId', 'key', ['userId']);

    // ── 3. inventory ──
    await ensureCollection('inventory', 'Inventory');
    await attr('inventory', 'string', 'userId', { size: 36, required: true });
    await attr('inventory', 'string', 'itemType', { size: 50, required: false });
    await attr('inventory', 'string', 'itemId', { size: 100, required: true });
    await attr('inventory', 'string', 'itemName', { size: 100, required: true });
    await attr('inventory', 'string', 'rarity', { size: 20, required: true });
    await attr('inventory', 'string', 'spriteUrl', { size: 500, required: false });
    await attr('inventory', 'string', 'obtainedAt', { size: 30, required: false });
    await sleep(2000);
    await idx('inventory', 'idx_userId', 'key', ['userId']);
    await idx('inventory', 'idx_userId_itemId', 'key', ['userId', 'itemId']);

    // ── 4. transactions ──
    await ensureCollection('transactions', 'Transactions');
    await attr('transactions', 'string', 'userId', { size: 36, required: true });
    await attr('transactions', 'string', 'type', { size: 20, required: true });
    await attr('transactions', 'integer', 'amount', { required: true });
    await attr('transactions', 'string', 'description', { size: 200, required: false });
    await attr('transactions', 'string', 'timestamp', { size: 30, required: false });
    await sleep(2000);
    await idx('transactions', 'idx_userId', 'key', ['userId']);

    // ── 5. spin_config ──
    await ensureCollection('spin_config', 'Spin Config');
    await attr('spin_config', 'string', 'itemId', { size: 100, required: true });
    await attr('spin_config', 'string', 'itemName', { size: 100, required: true });
    await attr('spin_config', 'string', 'itemType', { size: 50, required: false });
    await attr('spin_config', 'string', 'rarity', { size: 20, required: true });
    await attr('spin_config', 'integer', 'weight', { required: true, min: 1 });
    await attr('spin_config', 'string', 'spriteUrl', { size: 500, required: false });
    await sleep(2000);
    await idx('spin_config', 'idx_rarity', 'key', ['rarity']);

    // ── 6. avatars storage bucket ──
    try {
        await storage.createBucket('avatars', 'Avatars', anyone, false, true, 2 * 1024 * 1024, ['image/jpeg', 'image/png', 'image/webp']);
        console.log('\n  ✔ Created storage bucket "avatars"');
    } catch (e) {
        if (e.code === 409) console.log('\n  – Storage bucket "avatars" already exists');
        else throw e;
    }

    console.log('\n=== Done! All collections & bucket ready. ===\n');
}

main().catch(err => { console.error('Setup failed:', err); process.exit(1); });
