using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftEduImportFromGoogle
{
    internal class GoogleAuthenticator
    {
        public static async Task<string> AuthorizeAppAndGetTokenFromGoogle(string clientID, string clientSecret, string authorizationEndpoint)
        {
            // Generates state and PKCE values.
            string state = Utilities.randomDataBase64url(32);
            string code_verifier = Utilities.randomDataBase64url(32);
            string code_challenge = Utilities.base64urlencodeNoPadding(Utilities.sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, Utilities.GetRandomUnusedPort());
            //output("redirect URI: " + redirectURI);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            //output("Listening..");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fclassroom.courses.readonly%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fclassroom.coursework.students.readonly%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fclassroom.courseworkmaterials.readonly&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            // Opens request in the browser.
            Utilities.OpenBrowser(authorizationRequest);


            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings this app back to the foreground.
            //this.Activate();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                // output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return string.Empty;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                //output("Malformed authorization response. " + context.Request.QueryString);
                return string.Empty;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                //output(String.Format("Received request with invalid state ({0})", incoming_state));
                return string.Empty;
            }
            // output("Authorization code: " + code);

            // Starts the code exchange at the Token Endpoint.
            return await GetAccessToken(code, code_verifier, redirectURI, clientID, clientSecret);
        }

        static async Task<string> GetAccessToken(string code, string code_verifier, string redirectURI, string clientID, string clientSecret)
        {
            //output("Exchanging code for tokens...");

            // builds the  request
            string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier,
                clientSecret
                );

            // sends the request

            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    //output(responseText);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    return tokenEndpointDecoded["access_token"];
                    //await userinfoCall(access_token);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        //output("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string errorResponse = await reader.ReadToEndAsync();
                            Console.WriteLine(errorResponse);
                        }
                    }
                }
                return string.Empty;
            }
        }
    }
}
