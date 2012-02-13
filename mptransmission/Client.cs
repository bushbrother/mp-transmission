using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Jayrock;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace mptransmission
{
    class TransmissionClient
    {
        public Uri Url { get; private set; }
        public WebClient WebClient { get; set; }
        public string SessionId { get; private set; }

        public TransmissionClient(Uri url) : this(url, null) { }

        public TransmissionClient(Uri url, WebClient wc)
        {
            if (url == null) throw new ArgumentNullException("url");
            Url = url;
            WebClient = wc;
        }

        public object Invoke(string method, object args)
        {
            return Invoke(method, args, null);
        }

        public virtual object Invoke(string method, object args, JsonNumber? tag)
        {
            if (method == null) throw new ArgumentNullException("method");

            var sessionId = SessionId;
            var wc = WebClient ?? new WebClient();
            var url = Url;

            // 2.1.  Requests
            //  
            //     Requests support three keys:
            //  
            //     (1) A required "method" string telling the name of the method to invoke
            //     (2) An optional "arguments" object of key/value pairs
            //     (3) An optional "tag" number used by clients to track responses.
            //         If provided by a request, the response MUST include the same tag.

            var request = new
            {
                method,
                arguments = args,
                tag,
            };

            while (true)
            {
                try
                {
                    if (!string.IsNullOrEmpty(sessionId))
                        wc.Headers.Add("X-Transmission-Session-Id", sessionId);
                    var requestJson = JsonConvert.ExportToString(request);
                    var responseJson = wc.UploadString(url, requestJson);

                    // 2.2.  Responses
                    //  
                    //     Reponses support three keys:
                    //  
                    //     (1) A required "result" string whose value MUST be "success" on success,
                    //         or an error string on failure.
                    //     (2) An optional "arguments" object of key/value pairs
                    //     (3) An optional "tag" number as described in 2.1.

                    var responseObject = JsonConvert.Import<JsonObject>(responseJson);

                    var result = (responseObject["result"] ?? string.Empty).ToString();
                    if ("error".Equals(result, StringComparison.OrdinalIgnoreCase))
                        throw new Exception("Method failed.");
                    if (!"success".Equals(result, StringComparison.OrdinalIgnoreCase))
                        throw new Exception("Unexpected response result.");

                    if (tag != null && tag.Value.LogicallyEquals(responseObject["tag"]))
                        throw new Exception("Missing or unexpected tag in response.");

                    return responseObject["arguments"];
                }
                catch (WebException e)
                {
                    // 2.3.1.  CSRF Protection
                    //
                    //     Most Transmission RPC servers require a X-Transmission-Session-Id
                    //     header to be sent with requests, to prevent CSRF attacks.
                    //  
                    //     When your request has the wrong id -- such as when you send your first
                    //     request, or when the server expires the CSRF token -- the
                    //     Transmission RPC server will return an HTTP 409 error with the
                    //     right X-Transmission-Session-Id in its own headers.
                    //
                    //     So, the correct way to handle a 409 response is to update your
                    //     X-Transmission-Session-Id and to resend the previous request.

                    HttpWebResponse response;
                    if (e.Status == WebExceptionStatus.ProtocolError
                        && (response = e.Response as HttpWebResponse) != null
                        && response.StatusCode == /* 409 */ HttpStatusCode.Conflict)
                    {
                        sessionId = response.GetResponseHeader("X-Transmission-Session-Id");
                        if (!string.IsNullOrEmpty(sessionId))
                        {
                            SessionId = sessionId;
                            continue;
                        }
                    }

                    throw;
                }
            }
        }
    }
}
