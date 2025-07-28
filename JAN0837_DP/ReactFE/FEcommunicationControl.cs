using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;

namespace JAN0837_DP.ReactFE
{
    public class FEcommunicationControl
    {
        private readonly string _prefix;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public int refreshInterval = 2000;
        private int parameter1 = 0;
        private string status = "nenastaveno";

        public FEcommunicationControl(string prefix)
        {
            _prefix = prefix;
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _listener.Start();

            _cts = new CancellationTokenSource();
            Task.Run(() => HandleAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }

        public void Update(string key, object value)
        {
            switch (key)
            {
                case "status":
                    status = Convert.ToString(value);
                    break;
                case "parameter1":
                    parameter1 = Convert.ToInt32(value);
                    break;
                case "refreshInterval":
                    refreshInterval = Convert.ToInt32(value);
                    break;
            }
        }

        private async Task HandleAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var ctx = await _listener.GetContextAsync();
                Process(ctx);
            }
        }

        private void Process(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;
            var path = req.Url.AbsolutePath; // e.g. "/api/status"

            if (req.HttpMethod == "GET")
            {
                if (path.EndsWith("/status"))
                    WriteJSON(resp, status);
                else if (path.EndsWith("/parameter1"))
                    WriteJSON(resp, parameter1);
                else if (path.EndsWith("/config"))
                    WriteJSON(resp, refreshInterval);
                else
                    resp.StatusCode = 404;
            }
            else if (req.HttpMethod == "POST")
            {
                using var ms = new MemoryStream();
                req.InputStream.CopyTo(ms);
                var body = Encoding.UTF8.GetString(ms.ToArray());

                if (path.EndsWith("/status"))
                {
                    status = JsonSerializer.Deserialize<string>(body);
                }        
                else if (path.EndsWith("/parameter1"))
                {
                    parameter1 = JsonSerializer.Deserialize<int>(body);
                }
                else if (path.EndsWith("/config"))
                {
                    var doc = JsonSerializer.Deserialize<JsonElement>(body);
                    if (doc.TryGetProperty("refreshInterval", out var jv))
                        refreshInterval = jv.GetInt32();
                }
                else
                {
                    resp.StatusCode = 404;
                }

                resp.StatusCode = 200;
            }
            else
            {
                resp.StatusCode = 405;
            }

            resp.Close();
        }

        private void WriteJSON(HttpListenerResponse resp, object data)
        {
            var buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = buf.Length;
            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.Close();
        }
    }
}
