# 💳 Payment Processing API

A .NET 8 Web API for ingesting, normalizing, and querying payment transactions from various providers (e.g., PayPal, Trustly), with a clean domain-driven architecture and in-memory EF Core database.

---

## 🚀 Getting Started

### ✅ Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A REST client (Postman)

### 🛠️ Running the API

```bash
git clone https://github.com/your-org/payment-processing-api.git
cd payment-processing-api
dotnet build
dotnet run --project PaymentProcessing.Api

### 🧪 Testing the API
The API can found at: 

https://localhost:7080

<img width="1081" alt="image" src="https://github.com/user-attachments/assets/051ca068-72d1-4132-b467-2b573e02a170" />

The following endpoints are available to test
POST /ingest/{providerName}: Ingests a raw transaction and maps it to the internal normalized format.

GET /transactions: Returns a list of all normalized transactions, with optional filters.

GET /summary: Returns a summary of total number of transactions, volume per provider, and status breakdown


