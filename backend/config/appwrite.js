const { Client, Databases, Users, Storage, ID, Query } = require('node-appwrite');
require('dotenv').config();

const client = new Client()
    .setEndpoint(process.env.APPWRITE_ENDPOINT)
    .setProject(process.env.APPWRITE_PROJECT_ID)
    .setKey(process.env.APPWRITE_API_KEY);

const databases = new Databases(client);
const users = new Users(client);
const storage = new Storage(client);

const DATABASE_ID = process.env.APPWRITE_DATABASE_ID;

const COLLECTIONS = {
    PROFILES: 'profiles',
    WALLETS: 'wallets',
    INVENTORY: 'inventory',
    TRANSACTIONS: 'transactions',
    SPIN_CONFIG: 'spin_config',
};

const STORAGE_BUCKETS = {
    AVATARS: 'avatars',
};

module.exports = {
    client,
    databases,
    users,
    storage,
    DATABASE_ID,
    COLLECTIONS,
    STORAGE_BUCKETS,
    ID,
    Query,
};
