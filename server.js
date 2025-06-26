const express = require('express');
const cors = require('cors');
const { MongoClient } = require('mongodb');
require('dotenv').config();

const app = express();
const port = process.env.PORT || 3000;

// Allow CORS so Unity WebGL builds can communicate with the server
app.use(cors());

// Accept JSON bodies up to 1MB
app.use(express.json({ limit: '1mb' }));

let collection;
async function init() {
  if (!process.env.MONGO_URI) {
    throw new Error('MONGO_URI is not defined in environment');
  }
  const client = new MongoClient(process.env.MONGO_URI, {
    useNewUrlParser: true,
    useUnifiedTopology: true
  });
  await client.connect();
  collection = client.db().collection('messhall_sessions');
  console.log('Connected to MongoDB');
}

init().catch(err => {
  console.error('Failed to initialize server:', err);
  process.exit(1);
});

app.post('/api/messhall/upload', async (req, res) => {
  try {
    await collection.insertOne(req.body);
    res.status(200).json({ status: 'ok' });
  } catch (err) {
    console.error('Insert failed:', err);
    res.status(500).json({ error: err.message });
  }
});

app.listen(port, () => {
  console.log(`Server listening on port ${port}`);
});
