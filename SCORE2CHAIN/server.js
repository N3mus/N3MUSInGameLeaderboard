// server.js
const express = require("express");
const { ethers } = require("ethers");
const dotenv = require("dotenv");
const ScoresJSON = require("./Scores.json");

dotenv.config();

const app = express();
const port = process.env.PORT || 8000; // Load PORT from .env, default to 8000
const provider = new ethers.JsonRpcProvider(process.env.ETH_NODE_URL);
const privateKey = process.env.STUDIO_PRIVATE_KEY; // Private key for signing transactions
const contractAddress = process.env.MATCH_SCORES_CONTRACT; // Load contract address from .env

if (!privateKey || !contractAddress || !process.env.ETH_NODE_URL) {
  console.error("❌ Missing environment variables. Please check your .env file.");
  process.exit(1);
}

const wallet = new ethers.Wallet(privateKey, provider);
const scoresContract = new ethers.Contract(contractAddress, ScoresJSON.abi, wallet);

// Middleware to parse JSON (raise limit if your payloads can be big)
app.use(express.json({ limit: "1mb" }));

app.post("/postMatchResults", async (req, res) => {
  console.log("Received request from Unity:", req.body);

  const { address, amount, keys, values } = req.body;

  // Basic presence checks
  if (!address || amount === undefined || amount === null || !keys || !values) {
    return res.status(400).send({ error: "Missing parameters" });
  }
  if (!Array.isArray(keys) || !Array.isArray(values)) {
    return res.status(400).send({ error: "Keys and values must be arrays" });
  }
  if (keys.length !== values.length) {
    return res.status(400).send({ error: "Keys and values arrays must be the same length" });
  }

  // Validate address
  if (!ethers.isAddress(address)) {
    return res.status(400).send({ error: "Invalid player address" });
  }

  // Treat amount as a plain integer score (no decimals)
  // Accept strings or numbers but **require an integer** (e.g., "3171", 3171)
  const amountStr = String(amount).trim();

  if (!/^\d+$/.test(amountStr)) {
    return res.status(400).send({ error: "Score 'amount' must be a non-negative integer (no decimals)" });
  }

  // Convert to BigInt safely
  let score;
  try {
    score = ethers.toBigInt(amountStr);
  } catch {
    return res.status(400).send({ error: "Invalid score value" });
  }

  try {
    console.log(`Sending transaction: Player=${address}, Score=${score.toString()}, DataKeys=${JSON.stringify(keys)}`);

    // Send the transaction directly to the blockchain
    const tx = await scoresContract.setScore(address, score, keys, values);
    console.log(`⛓️  Submitted TX: ${tx.hash}`);

    const receipt = await tx.wait();

    if (receipt.status === 1n || receipt.status === 1) {
      console.log(`✅ Transaction successful! TX Hash: ${tx.hash}`);
      return res.send({ message: "success", txHash: tx.hash });
    } else {
      console.log("❌ Transaction failed!", receipt);
      return res.status(500).send({ message: "fail", receipt });
    }
  } catch (error) {
    console.error("⚠️ Error submitting score:", error);
    return res.status(500).send({ error: "Failed to submit score", details: error.message });
  }
});

// Start server
app.listen(port, () => {
  console.log(`🚀 Studio's API is running on http://localhost:${port}`);
});
