# 🎮 N3MUS Leaderboard API

Studios can push match results (points) directly to the N3MUS leaderboard system.  
This doc explains the endpoint, auth, request/response formats, and provides ready-to-run examples (cURL, JS, Python, **Unity (C#)**, **Unreal (C++)**).  
A complete **OpenAPI/Swagger** spec is included at the bottom.

---

## 🔐 Authentication

Every request **must** include a Bearer token:

```
Authorization: Bearer <your-api-token>
```

You will receive your token from N3MUS.  
If the token is missing or invalid, the API returns **401/403**.

---

## 🌍 Endpoint

**POST** `/postMatchResults`  
Your base URL will be provided (e.g. `https://[gamename].n3mus.com`). For example https://cosmicbomberk.n3mus.com.

**Full example:**  
```
POST https://cosmicbomberk.n3mus.com/postMatchResults
```

---

## 📦 Request

### Headers
- `Authorization: Bearer <API_TOKEN>`
- `Content-Type: application/json`

### Body
```json
{
  "address": "0x1234567890abcdef1234567890abcdef12345678",
  "amount": 42,
  "keys": ["matchNumber"],
  "values": ["1"]
}
```

### Field details
| Field    | Type     | Required | Description                                |
|----------|----------|----------|--------------------------------------------|
| address  | string   | ✅       | Player wallet address (0x + 40 hex chars). |
| amount   | integer  | ✅       | Points to **add** for this player.         |
| keys     | string[] | ✅       | Metadata field names (must align with values). |
| values   | string[] | ✅       | Metadata field values.                     |

**Rules**
- `amount` must be an integer (no decimals/strings).
- `keys.length === values.length`.

---

## ✅ Responses

### 200 OK
```json
{
  "status": "enqueued",
  "jobId": "12345"
}
```

### Error examples

**400 Bad Request**
```json
{ "error": "Invalid EVM address (expected 0x + 40 hex chars)" }
```

**401 Unauthorized**
```json
{ "error": "Missing or invalid Authorization header" }
```

**403 Forbidden**
```json
{ "error": "Invalid API token" }
```

---

## 🚀 Quick Examples

### cURL
```bash
curl -X POST "https://cosmicbomberk.n3mus.com/postMatchResults" \
  -H "Authorization: Bearer <API_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "address": "0x1234567890abcdef1234567890abcdef12345678",
    "amount": 42,
    "keys": ["matchNumber"],
    "values": ["1"]
  }'
```

### JavaScript (fetch)
```js
await fetch("https://cosmicbomberk.n3mus.com/postMatchResults", {
  method: "POST",
  headers: {
    "Authorization": "Bearer " + process.env.N3MUS_API_TOKEN,
    "Content-Type": "application/json"
  },
  body: JSON.stringify({
    address: "0x1234567890abcdef1234567890abcdef12345678",
    amount: 42,
    keys: ["matchNumber"],
    values: ["1"]
  })
}).then(r => r.json()).then(console.log);
```

### Python (requests)
```python
import os, requests

resp = requests.post(
    "https://cosmicbomberk.n3mus.com/postMatchResults",
    headers={
        "Authorization": f"Bearer {os.environ['N3MUS_API_TOKEN']}",
        "Content-Type": "application/json"
    },
    json={
        "address": "0x1234567890abcdef1234567890abcdef12345678",
        "amount": 42,
        "keys": ["matchNumber"],
        "values": ["1"]
    },
    timeout=10
)
print(resp.status_code, resp.json())
```

---

## 🎯 Game Engine Examples

### Unity (C#)
```csharp
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class N3musLeaderboard : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://cosmicbomberk.n3mus.com";
    [SerializeField] private string apiToken = "YOUR_API_TOKEN";

    public IEnumerator PostMatchResults(string address, int amount, string matchNumber)
    {
        var url = $"{baseUrl}/postMatchResults";

        var payload = new
        {
            address = address,
            amount = amount,
            keys = new string[] { "matchNumber" },
            values = new string[] { matchNumber }
        };

        string json = JsonUtility.ToJson(payload);
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiToken}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"N3MUS post failed: {request.responseCode} {request.error} {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"N3MUS post ok: {request.downloadHandler.text}");
        }
    }
}
```

### Unreal Engine (C++)
```cpp
#include "HttpModule.h"
#include "Interfaces/IHttpRequest.h"
#include "Interfaces/IHttpResponse.h"
#include "Json.h"
#include "JsonUtilities.h"

void PostMatchResults(
    const FString& BaseUrl,
    const FString& ApiToken,
    const FString& Address,
    int32 Amount,
    const FString& MatchNumber)
{
    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> Request = FHttpModule::Get().CreateRequest();
    Request->SetURL(BaseUrl + TEXT("/postMatchResults"));
    Request->SetVerb(TEXT("POST"));
    Request->SetHeader(TEXT("Content-Type"), TEXT("application/json"));
    Request->SetHeader(TEXT("Authorization"), FString::Printf(TEXT("Bearer %s"), *ApiToken));

    TSharedPtr<FJsonObject> Root = MakeShareable(new FJsonObject);
    Root->SetStringField(TEXT("address"), Address);
    Root->SetNumberField(TEXT("amount"), Amount);

    TArray<TSharedPtr<FJsonValue>> Keys;
    Keys.Add(MakeShareable(new FJsonValueString(TEXT("matchNumber"))));
    Root->SetArrayField(TEXT("keys"), Keys);

    TArray<TSharedPtr<FJsonValue>> Values;
    Values.Add(MakeShareable(new FJsonValueString(MatchNumber)));
    Root->SetArrayField(TEXT("values"), Values);

    FString Body;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&Body);
    FJsonSerializer::Serialize(Root.ToSharedRef(), Writer);

    Request->SetContentAsString(Body);

    Request->OnProcessRequestComplete().BindLambda(
        [](FHttpRequestPtr Req, FHttpResponsePtr Resp, bool bWasSuccessful)
        {
            if (!bWasSuccessful || !Resp.IsValid())
            {
                UE_LOG(LogTemp, Error, TEXT("N3MUS post failed: no response"));
                return;
            }
            if (Resp->GetResponseCode() >= 200 && Resp->GetResponseCode() < 300)
            {
                UE_LOG(LogTemp, Log, TEXT("N3MUS post ok: %s"), *Resp->GetContentAsString());
            }
            else
            {
                UE_LOG(LogTemp, Error, TEXT("N3MUS post failed: %d %s"),
                       Resp->GetResponseCode(), *Resp->GetContentAsString());
            }
        });

    Request->ProcessRequest();
}
```

---

## 🧾 OpenAPI / Swagger (3.1)
```yaml
openapi: 3.1.0
info:
  title: N3MUS Leaderboard API
  version: "1.0.0"
  description: Studios can push match results (points) to N3MUS leaderboards.
servers:
  - url: https://cosmicbomberk.n3mus.com
paths:
  /postMatchResults:
    post:
      summary: Submit match result points
      description: Adds `amount` points to the player's running total with optional metadata.
      security:
        - bearerAuth: []
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/PostMatchResultsRequest"
            examples:
              default:
                value:
                  address: "0x1234567890abcdef1234567890abcdef12345678"
                  amount: 42
                  keys: ["matchNumber"]
                  values: ["1"]
      responses:
        "200":
          description: Enqueued for processing
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/EnqueueResponse"
              examples:
                ok:
                  value:
                    status: "enqueued"
                    jobId: "12345"
        "400":
          description: Bad Request
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/ErrorResponse"
        "401":
          description: Unauthorized
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/ErrorResponse"
        "403":
          description: Forbidden
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/ErrorResponse"
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
  schemas:
    PostMatchResultsRequest:
      type: object
      required: [address, amount, keys, values]
      properties:
        address:
          type: string
          pattern: "^0x[a-fA-F0-9]{40}$"
          example: "0x1234567890abcdef1234567890abcdef12345678"
        amount:
          type: integer
          example: 42
        keys:
          type: array
          items: { type: string }
          example: ["matchNumber"]
        values:
          type: array
          items: { type: string }
          example: ["1"]
    EnqueueResponse:
      type: object
      properties:
        status:
          type: string
          example: "enqueued"
        jobId:
          type: string
          example: "12345"
    ErrorResponse:
      type: object
      properties:
        error:
          type: string
          example: "Invalid EVM address (expected 0x + 40 hex chars)"
```

---

## 📬 Support

Need help or a token?  
📧 **updates@n3mus.com**
