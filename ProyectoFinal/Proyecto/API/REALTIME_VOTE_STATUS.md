# Real-time Vote Status WebSocket

This feature provides real-time updates of scrutiny voting status using SignalR WebSocket.

**⚠️ AUTHENTICATION REQUIRED**: This WebSocket endpoint requires JWT authentication with ADMIN role. Only authenticated administrators can connect and receive vote status updates.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [How to Connect](#how-to-connect)
  - [Step 1: Obtain Authentication Token](#step-1-obtain-authentication-token)
  - [Step 2: Configure WebSocket Connection](#step-2-configure-websocket-connection)
  - [Step 3: Handle Connection Events](#step-3-handle-connection-events)
  - [Step 4: Subscribe to Updates](#step-4-subscribe-to-updates)
- [Testing with Postman](#testing-with-postman)
  - [Method 1: Using Postman WebSocket (Recommended for Testing)](#method-1-using-postman-websocket-recommended-for-testing)
  - [Method 2: Using REST Client with SignalR Negotiate](#method-2-using-rest-client-with-signalr-negotiate)
  - [Postman Limitations](#postman-limitations)
  - [Troubleshooting Postman Connection](#troubleshooting-postman-connection)
  - [Alternative: SignalR Client Testing Tools](#alternative-signalr-client-testing-tools)
- [Client-Side Integration](#client-side-integration)
  - [JavaScript Example](#javascript-example-using-microsoftsignalr)
  - [React Example](#react-example)
- [Connection Details](#connection-details)
- [Hub Methods](#hub-methods)
  - [SubscribeToVoteUpdates()](#subscribetovoteupdates)
  - [UnsubscribeFromVoteUpdates()](#unsubscribefromvoteupdates)
- [Events](#events)
  - [ReceiveVoteStatus](#receivevotestatus)
- [Connection Requirements](#connection-requirements)
  - [Authentication](#authentication)
  - [Network Requirements](#network-requirements)
  - [Browser Support](#browser-support)
- [Error Handling](#error-handling)
  - [Common Errors](#common-errors)
- [Troubleshooting](#troubleshooting)
  - [Connection fails immediately](#connection-fails-immediately)
  - [Connection drops frequently](#connection-drops-frequently)
  - [Not receiving updates](#not-receiving-updates)
  - [Updates stop after some time](#updates-stop-after-some-time)
- [Security Considerations](#security-considerations)
- [Notes](#notes)

## Overview

The WebSocket broadcasts the status of all currently open scrutinies (that haven't ended yet) every 5 seconds. Each scrutiny includes:
- Basic scrutiny information
- Total vote count
- List of slates with:
  - Vote count per slate
  - First candidacy member (with position #1)

## Prerequisites

Before connecting to the WebSocket, ensure you have:

1. **SignalR Client Library**: Install the appropriate SignalR client for your platform
   - JavaScript/TypeScript: `npm install @microsoft/signalr`
   - .NET: `Microsoft.AspNetCore.SignalR.Client` NuGet package
   
2. **Valid JWT Token**: Obtain a valid JWT authentication token with ADMIN role
   - Login through `/api/auth/login` endpoint with admin credentials
   - Token must be included in the connection configuration
   
3. **Network Access**: Ensure your client can reach the WebSocket endpoint
   - Server URL: `http://localhost:5000` (or your configured server URL)
   - CORS is enabled for `localhost:5000` and `localhost:5500`

## How to Connect

### Step 1: Obtain Authentication Token

First, authenticate with admin credentials to get a JWT token:

```javascript
// Login to get JWT token
const loginResponse = await fetch('http://localhost:5000/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        email: 'admin@example.com',
        password: 'your-password'
    })
});

const { token } = await loginResponse.json();
// Store token for WebSocket connection
localStorage.setItem('userToken', token);
```

### Step 2: Configure WebSocket Connection

Create a SignalR connection with the JWT token:

```javascript
import * as signalR from "@microsoft/signalr";

const jwtToken = localStorage.getItem('userToken');

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/vote-status", {
        accessTokenFactory: () => jwtToken  // Include JWT token
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])  // Retry intervals
    .configureLogging(signalR.LogLevel.Information)
    .build();
```

### Step 3: Handle Connection Events

```javascript
// Connection closed handler
connection.onclose((error) => {
    if (error) {
        console.error('Connection closed with error:', error.message);
        // Handle authentication errors
        if (error.message.includes('No autorizado')) {
            console.error('Authentication failed. Please login again.');
            // Redirect to login or refresh token
        }
    } else {
        console.log('Connection closed');
    }
});

// Reconnecting handler
connection.onreconnecting((error) => {
    console.log('Reconnecting...', error);
});

// Reconnected handler
connection.onreconnected((connectionId) => {
    console.log('Reconnected with ID:', connectionId);
    // Re-subscribe after reconnection
    connection.invoke("SubscribeToVoteUpdates");
});
```

### Step 4: Subscribe to Updates

```javascript
// Start connection and subscribe
connection.start()
    .then(() => {
        console.log('Connected to vote status hub');
        return connection.invoke("SubscribeToVoteUpdates");
    })
    .then(() => {
        console.log('Subscribed to vote updates');
    })
    .catch(err => {
        console.error('Connection error:', err.message);
        // Handle specific errors
        if (err.message.includes('401')) {
            console.error('Unauthorized: Invalid or expired token');
        }
    });
```

## Testing with Postman

Postman supports WebSocket connections and can be used to test the SignalR hub. However, note that **SignalR uses a specific protocol** that requires proper handshake and message formatting.

### Method 1: Using Postman WebSocket (Recommended for Testing)

#### Step 1: Get Authentication Token

First, create a POST request to get your JWT token:

**Request:**
- Method: `POST`
- URL: `http://localhost:5000/api/auth/login`
- Headers: `Content-Type: application/json`
- Body (raw JSON):
```json
{
  "email": "admin@example.com",
  "password": "your-password"
}
```

**Response:** Copy the `token` value from the response.

#### Step 2: Connect to WebSocket

1. **Create New WebSocket Request:**
   - In Postman, click "New" → "WebSocket Request"
   - Or use the URL bar and select "WebSocket" protocol

2. **Enter WebSocket URL:**
   ```
   ws://localhost:5000/hubs/vote-status
   ```
   
   **Add Authorization Header:**
   - Key: `Authorization`
   - Value: `Bearer YOUR_JWT_TOKEN_HERE`
   
   Replace `YOUR_JWT_TOKEN_HERE` with the token from Step 1.

3. **Connect:**
   - Click the "Connect" button
   - If authentication is successful, connection will be established
   - If you see "401 Unauthorized" or "Connection closed", verify your token is valid and user has ADMIN role

#### Step 3: Send SignalR Protocol Messages

SignalR uses a specific message protocol. You need to send messages in the correct format:

**A. Send Handshake (automatic in some versions):**
```json
{"protocol":"json","version":1}
```
Press Send. You should receive a response like:
```json
{}
```

**B. Subscribe to Vote Updates:**

After successful handshake, send the invocation message:
```json
{"type":1,"target":"SubscribeToVoteUpdates","arguments":[]}
```

The message format explained:
- `type: 1` = Invocation message
- `target` = Method name on the hub
- `arguments` = Array of parameters (empty for this method)
- Add `` (record separator) at the end

**Note:** SignalR protocol requires messages to end with the record separator character (ASCII 0x1E). In Postman, you may need to manually add this or use the proper SignalR client.

#### Step 4: Receive Updates

Once subscribed, you'll receive messages every 5 seconds in this format:

```json
{
  "type": 1,
  "target": "ReceiveVoteStatus",
  "arguments": [{
    "scrutinies": [
      {
        "id": "guid-here",
        "title": "Scrutiny Title",
        "description": "Description",
        "startDate": "2026-03-29T10:00:00",
        "endDate": "2026-03-29T18:00:00",
        "imageUrl": "http://...",
        "totalVotes": 150,
        "slates": [
          {
            "id": "slate-guid",
            "position": 1,
            "voteCount": 75,
            "firstCandidacy": {
              "id": "candidacy-guid",
              "name": "John",
              "lastName": "Doe",
              "imageUrl": "http://..."
            }
          }
        ]
      }
    ]
  }]
}
```

#### Step 5: Unsubscribe (Optional)

To stop receiving updates:
```json
{"type":1,"target":"UnsubscribeFromVoteUpdates","arguments":[]}
```

### Method 2: Using REST Client with SignalR Negotiate

SignalR connections start with a negotiate handshake:

1. **Negotiate Connection:**
   - Method: `POST`
   - URL: `http://localhost:5000/hubs/vote-status/negotiate`
   - Headers: 
     - `Authorization: Bearer YOUR_JWT_TOKEN`
     - `Content-Type: application/json`

   This returns connection information including the WebSocket URL.

2. **Use returned connection ID** to establish WebSocket connection with the same Authorization header.

### Postman Limitations

⚠️ **Important Notes:**
- Postman's WebSocket support may not fully implement the SignalR protocol
- The record separator character (0x1E) is required between messages but may be hard to input in Postman
- For full testing, use a proper SignalR client (JavaScript, .NET, etc.)
- Postman is best for basic connectivity testing, not full SignalR protocol testing

### Troubleshooting Postman Connection

**"Connection failed" or "401 Unauthorized":**
- Verify JWT token is valid (decode at jwt.io)
- Check token hasn't expired
- Ensure user has ADMIN role
- Make sure Authorization header is set: `Authorization: Bearer YOUR_TOKEN`
- Verify the Bearer prefix is included in the header value

**Connected but no messages received:**
- Verify you sent the handshake message first
- Check the subscribe invocation message format
- Ensure there are OPEN scrutinies in the database that haven't ended
- Look at server logs for errors

**"No autorizado" error:**
- User doesn't have ADMIN role
- Token validation failed on the server
- Authorization header not properly set or Bearer token missing
- Connection attempt will be aborted

### Alternative: SignalR Client Testing Tools

For better SignalR protocol support, consider these alternatives:

1. **Browser Console** (with SignalR JavaScript client)
2. **SignalR Client CLI** (.NET tool)
3. **Postman Newman** with custom scripts
4. **WebSocket online tools** that support SignalR protocol

## Client-Side Integration

### JavaScript Example (using @microsoft/signalr)

```javascript
// Install: npm install @microsoft/signalr

import * as signalR from "@microsoft/signalr";
    const [error, setError] = useState(null);
    const [connectionStatus, setConnectionStatus] = useState('disconnected');

    useEffect(() => {
        // Get JWT token from your auth context/storage
        const jwtToken = localStorage.getItem('userToken');

        if (!jwtToken) {
            setError('Se requiere autenticación de administrador');
            return;
        }
onclose((error) => {
            setConnectionStatus('disconnected');
            if (error) {
                setError('Conexión cerrada: ' + error.message);
            }
        });

        connection.onreconnecting(() => {
            setConnectionStatus('reconnecting');
        });

        connection.onreconnected(() => {
            setConnectionStatus('connected');
            connection.invoke("SubscribeToVoteUpdates");
        });

        setConnectionStatus('connecting');
        connection.start()
            .then(() => {
                setConnectionStatus('connected');
                return connection.invoke("SubscribeToVoteUpdates");
            })
            .catch(err => {
                setConnectionStatus('disconnected');
                setError('Error de conexión: ' + err.message);
                console.error(err);
            }alhost:5000/hubs/vote-status", {
                accessTokenFactory: () => jwtToken
            }
if (!jwtToken) {
    console.error('No authentication token found. Please login first.');
    // Redirect to login page
}

// Create connection with authentication
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/vote-status", {
        accessTokenFactory: () => jwtToken
    }
## Connection Details

**WebSocket Endpoint:** `/hubs/vote-status`

**Full URL Example:** `http://localhost:5000/hubs/vote-status`

**Authentication Method:** JWT Bearer Token

**Required Role:** ADMIN

**Protocol:** WebSocket (SignalR)

## Client-Side Integration

### JavaScript Example (using @microsoft/signalr)

```javascript
// Install: npm install @microsoft/signalr

import * as signalR from "@microsoft/signalr";

// Create connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/vote-status")
    .withAutomaticReconnect()
    .build();

// Subscribe to vote status updates
connection.on("ReceiveVoteStatus", (data) => {
    console.log("Vote Status Update:", data);
    
    // data structure:
    // {
    //   scrutinies: [
    //     {
    //       id: "guid",
    //       title: "string",
    //       description: "string",
    //       startDate: "datetime",
    //       endDate: "datetime",
    //       imageUrl: "string",
    //       totalVotes: number,
    //       slates: [
    //         {
    //           id: "guid",
    //           position: number,
    //           voteCount: number,
    //           firstCandidacy: {
    //             id: "guid",
    //             name: "string",
    //             lastName: "string",
    //             imageUrl: "string"
    //           }
    //         }
    //       ]
    //     }
    //   ]
    // }
    
    // Update your UI with the data
    updateVoteDisplay(data.scrutinies);
});

// Start connection
connection.start()
    .then(() => {
        console.log("Connected to vote status hub");
        // Subscribe to updates
                .catch(console.error);
            connection.stop();
        };
    }, []);

    if (error) {
        return <div className="error">Error: {error}</div>;
    }

    return (
        <div>
            <div className="connection-status">
                Status: {connectionStatus}
            </dEventListener("beforeunload", () => {
    connection.invoke("UnsubscribeFromVoteUpdates");
    connection.stop();
});
```

### React Example

```jsx
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

function VoteStatusComponent() {
    const [voteData, setVoteData] = useState({ scrutinies: [] });

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
### SubscribeToVoteUpdates()
Subscribe to receive real-time vote status updates.

**Requirements:**
- Must be called after successful connection
- Requires ADMIN role authentication
- Returns a promise

**Example:**
```javascript
await connection.invoke("SubscribeToVoteUpdates");
```

**Errors:**
- Throws `HubException` if user is not authenticated or doesn't have ADMIN role
- Error message: "No autorizado. Solo administradores pueden acceder a las actualizaciones de votos."

### UnsubscribeFromVoteUpdates()
Unsubscribe from vote status updates.

**Example:**
```javascript
await connection.invoke("UnsubscribeFromVoteUpdates");
```

## Events

### ReceiveVoteStatus
Emitted every 5 seconds with current vote status data.

**Event Data Structure:**
```typescript
{
  scrutinies: Array<{
    id: string;              // GUID
    title: string;
    description: string;
    startDate: string;       // ISO 8601 datetime
    endDate: string;         // ISO 8601 datetime
    imageUrl: string;
    totalVotes: number;      // Sum of all votes in this scrutiny
    slates: Array<{
      id: string;            // GUID
      position: number;      // Slate position/number
      voteCount: number;     // Votes for this slate
      firstCandidacy: {      // Candidacy with position #1
        id: string;
        name: string;
        lastName: string;
        imageUrl: string;
      } | null
    }>
  }>
}
```

## Connection Requirements

### Authentication
- **Required:** Valid JWT Bearer token
- **Role:** ADMIN only
- **Token Validation:** Performed on connection and on each method invocation
- **Token Location:** Passed via `accessTokenFactory` in connection options

### Network Requirements
- **Protocol:** WebSocket (ws:// or wss://)
- **CORS:** Must be from allowed origin (localhost:5000 or localhost:5500)
- **Firewall:** Ensure WebSocket traffic is not blocked

### Browser Support
The SignalR client supports:
- Chrome 16+
- Firefox 11+
- Safari 7+
- Edge (all versions)
- Internet Explorer 10+ (with polyfills)

## Error Handling

### Common Errors

#### 401 Unauthorized
**Cause:** Missing or invalid JWT token
**Solution:** 
- Ensure token is obtained from login endpoint
- Check token hasn't expired
- Verify token is passed in `accessTokenFactory`

```javascript
connection.onclose((error) => {
    if (error?.message.includes('401') || error?.message.includes('No autorizado')) {
        // Token is invalid or expired
        // Redirect to login or refresh token
        window.location.href = '/login';
    }
});
```

#### 403 Forbidden
**Cause:** User doesn't have ADMIN role
**Solution:** Ensure logged-in user has ADMIN privileges

#### Connection Timeout
**Cause:** Network issues or server unavailable
**Solution:** Implement retry logic with backoff

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl(url, options)
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

#### HubException
**Cause:** Server-side validation failed
**Common Messages:**
- "No autorizado. Solo administradores pueden conectarse."
- "No autorizado. Solo administradores pueden acceder a las actualizaciones de votos."

**Solution:** Verify user authentication and role

## Troubleshooting

### Connection fails immediately
1. Verify server is running on expected URL
2. Check JWT token is valid (decode at jwt.io)
3. Confirm user has ADMIN role
4. Check browser console for CORS errors

### Connection drops frequently
1. Check network stability
2. Verify token hasn't expired
3. Implement automatic reconnection (already included in examples)
4. Check server logs for errors

### Not receiving updates
1. Ensure `SubscribeToVoteUpdates()` was called successfully
2. Verify there are OPEN scrutinies that haven't ended
3. Check event listener is registered before calling `connection.start()`
4. Look for errors in browser console

### Updates stop after some time
1. JWT token may have expired - implement token refresh
2. Check connection status: `connection.state`
3. Manually reconnect if needed

```javascript
if (connection.state === signalR.HubConnectionState.Disconnected) {
    await connection.start();
    await connection.invoke("SubscribeToVoteUpdates");
}
```

## Security Considerations

1. **Token Storage:** Store JWT tokens securely
   - Use `httpOnly` cookies when possible
   - Avoid storing in localStorage for production (XSS risk)
   - Consider using sessionStorage for better security

2. **HTTPS:** Always use WSS (WebSocket Secure) in production
   ```javascript
   .withUrl("wss://yourdomain.com/hubs/vote-status", options)
   ```

3. **Token Refresh:** Implement token refresh before expiration
   ```javascript
   // Refresh token every 50 minutes (if token expires in 60 minutes)
   setInterval(async () => {
       const newToken = await refreshAuthToken();
       localStorage.setItem('userToken', newToken);
       // Reconnect with new token
       await connection.stop();
       await connection.start();
       await connection.invoke("SubscribeToVoteUpdates");
   }, 50 * 60 * 1000);
   ```

4. **Role Verification:** Server validates ADMIN role on:
   - Initial connection
   - Every method invocation
   - Connection will be aborted if validation fails

## Hub Methods

- `SubscribeToVoteUpdates()` - Subscribe to receive real-time updates
- `UnsubscribeFromVoteUpdates()` - Unsubscribe from updates

## Events

- `ReceiveVoteStatus` - Emitted every 5 seconds with current vote status data

## Notes

- Updates are sent every 5 seconds
- Only includes scrutinies that are:
  - In OPEN status
  - Have not reached their end date yet
- Connection automatically reconnects on disconnect
- CORS is configured to allow WebSocket connections from localhost:5000 and localhost:5500
