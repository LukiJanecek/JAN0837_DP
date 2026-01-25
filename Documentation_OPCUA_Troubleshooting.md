# OPC UA Server - Troubleshooting Guide

## Common Errors and Solutions

### ? Error: "Failed to establish tcp listener sockets for Ipv4 and IPv6"

**What it means:** The server cannot bind to the specified IP address and port.

**Common Causes:**

#### 1. **Port Already in Use** ?? Most Common

Another application is using port 4840.

**Check if port is in use:**
```powershell
# PowerShell
netstat -ano | findstr :4840

# If you see output, the port is in use
# Example:
# TCP    0.0.0.0:4840    0.0.0.0:0    LISTENING    1234
```

**Solutions:**
- **Use a different port:** Change port to 4841, 4842, etc.
- **Stop the other application** using the port
- **Kill the process:**
  ```powershell
  # Find the PID (Process ID) from netstat output
  taskkill /PID 1234 /F
  ```

#### 2. **Invalid IP Address**

You're trying to bind to an IP that doesn't exist on your machine.

**Check your IP addresses:**
```powershell
ipconfig
```

**Solutions:**
- ? **Use `localhost`** (recommended for testing)
- ? **Use `0.0.0.0`** (binds to all network interfaces)
- ? **Use your actual IP** from `ipconfig` (e.g., `192.168.1.100`)
- ? **Don't use** `127.0.0.1` for network access (use `localhost` instead)

#### 3. **Firewall Blocking**

Windows Firewall is blocking the port.

**Solution:**
```powershell
# Run as Administrator
New-NetFirewallRule -DisplayName "OPC UA Server" -Direction Inbound -Protocol TCP -LocalPort 4840 -Action Allow
```

Or manually:
1. Open Windows Firewall
2. Advanced Settings ? Inbound Rules ? New Rule
3. Port ? TCP ? Specific local ports: 4840
4. Allow the connection

#### 4. **Permission Issues**

Application doesn't have permission to bind to the port.

**Solution:**
- Run Visual Studio/application as **Administrator**
- Or use a port > 1024 (ports 1-1024 require admin rights)

---

## Configuration Examples

### For Local Testing (Same PC)

```csharp
IP: localhost
Port: 4840
```

**Server URL:** `opc.tcp://localhost:4840`

**Clients connect to:** `opc.tcp://localhost:4840`

### For Network Access (Other Devices)

```csharp
IP: 0.0.0.0  // Binds to all network interfaces
Port: 4840
```

**Server URL:** `opc.tcp://0.0.0.0:4840`

**Clients connect to:** `opc.tcp://YOUR-PC-IP:4840`
- Find YOUR-PC-IP with `ipconfig` (e.g., `192.168.1.100`)

### For Specific Network Interface

```csharp
IP: 192.168.1.100  // Your PC's IP from ipconfig
Port: 4840
```

**Server URL:** `opc.tcp://192.168.1.100:4840`

**Clients connect to:** `opc.tcp://192.168.1.100:4840`

---

## Testing the Server

### 1. Check if Server is Running

```powershell
netstat -ano | findstr :4840
```

Expected output:
```
TCP    0.0.0.0:4840    0.0.0.0:0    LISTENING    5678
```

### 2. Test with UAExpert

1. Download UAExpert: https://www.unified-automation.com/products/development-tools/uaexpert.html
2. Start your server
3. In UAExpert:
   - Custom Discovery ? Add Server
   - URL: `opc.tcp://localhost:4840`
   - Connect
   - Should see your variables under Objects ? Crossroad

### 3. Test with Command Line (netcat)

```bash
# Install netcat (nc) if needed
# Try to connect to the port
nc -v localhost 4840
```

---

## Quick Fixes

### Problem: Port 4840 in use

**Quick Fix:**
```csharp
// In your UI, change port to:
txtBoxPara2.Text = "4841";  // Or 4842, 4843, etc.
```

### Problem: Can't connect from another PC

**Quick Fix:**
1. Use `0.0.0.0` as IP
2. Add firewall rule (see above)
3. Connect from other PC using: `opc.tcp://YOUR-PC-IP:4840`

### Problem: "Access Denied" or "Permission Denied"

**Quick Fix:**
- Run Visual Studio as Administrator
- Or use port > 1024 (e.g., 48400)

---

## Understanding IP Bindings

| IP Address | Binds To | Accessible From | Use Case |
|------------|----------|-----------------|----------|
| `localhost` | Loopback | Same PC only | Testing |
| `127.0.0.1` | Loopback | Same PC only | Testing (prefer localhost) |
| `0.0.0.0` | All interfaces | Same PC + Network | Production |
| `192.168.x.x` | Specific interface | Same PC + Network | Specific network |

---

## Error Messages Reference

### "Failed to establish tcp listener sockets"
? Port binding issue (see solutions above)

### "Server does not have an instance certificate"
? Certificate not created (fixed in current code)

### "TrustedIssuerCertificates StorePath must be specified"
? Certificate paths not configured (fixed in current code)

### "Address already in use"
? Port is in use, change port number

### "Cannot assign requested address"
? Invalid IP address, use `localhost` or `0.0.0.0`

---

## Advanced Troubleshooting

### Check Windows Event Viewer

1. Open Event Viewer
2. Windows Logs ? Application
3. Look for errors from your application

### Enable OPC UA Logging

Check the log file:
```
%TEMP%\JAN0837_Server.log
```

Location: `C:\Users\[YourName]\AppData\Local\Temp\JAN0837_Server.log`

### Test Different Ports

Try these ports in order:
1. `4840` (standard OPC UA)
2. `4841`
3. `4842`
4. `48400` (way above standard range)

### Disable Antivirus Temporarily

Sometimes antivirus blocks network bindings.

**Test:**
1. Temporarily disable antivirus
2. Try starting server
3. If it works, add exception for your application
4. Re-enable antivirus

---

## Prevention

### Best Practices

1. ? **Always use `localhost` for testing**
2. ? **Use `0.0.0.0` for production**
3. ? **Check port availability before starting**
4. ? **Handle exceptions gracefully**
5. ? **Log errors for debugging**

### Check Port Before Starting

```csharp
// Add this helper method
public static bool IsPortInUse(int port)
{
    try
    {
        using (var socket = new System.Net.Sockets.Socket(
            System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp))
        {
            socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));
            return false;
        }
    }
    catch
    {
        return true;
    }
}

// Before starting server:
if (IsPortInUse(4840))
{
    MessageBox.Show("Port 4840 is already in use. Please use a different port.");
    return;
}
```

---

## Summary

? **Quick Checklist:**

1. [ ] Using `localhost` or `0.0.0.0` as IP?
2. [ ] Port not in use? (check with `netstat`)
3. [ ] Firewall allows port?
4. [ ] Running as Administrator (if needed)?
5. [ ] Certificate directories exist?

? **Most Common Fix:**

Change IP to `localhost` and try again!

? **If Still Not Working:**

1. Try port `4841` instead of `4840`
2. Run as Administrator
3. Check firewall settings
4. Check `%TEMP%\JAN0837_Server.log` for details
