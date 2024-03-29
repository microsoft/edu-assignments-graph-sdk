﻿using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MicrosoftEduImportFromGoogle
{
    public class FileTypeDetails
    {
        public string FileExtension { get; set; }
        public string FileMimeType { get; set; }
    }
    public class Utilities
    {
        public static FileTypeDetails GetFileDetails(string sourceMimeType) => sourceMimeType switch
        {
            "application/vnd.google-apps.document" => new FileTypeDetails { FileExtension = ".docx", FileMimeType = WebUtility.UrlEncode("application/vnd.openxmlformats-officedocument.wordprocessingml.document") },
            "application/vnd.google-apps.presentation" => new FileTypeDetails { FileExtension = ".pptx", FileMimeType = WebUtility.UrlEncode("application/vnd.openxmlformats-officedocument.presentationml.presentation") },
            "application/vnd.google-apps.spreadsheet" => new FileTypeDetails { FileExtension = ".xlsx", FileMimeType = WebUtility.UrlEncode("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") },
            _ => new FileTypeDetails { FileExtension = "", FileMimeType = sourceMimeType }
        };

        public static async Task<string> MakeHttpGetRequest(string access_token, string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", access_token);
                using (HttpResponseMessage response = await client.SendAsync(httpRequestMessage))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
            }
        }

        public static async Task<Byte[]> MakeHttpGetByteArrayRequest(string access_token, string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", access_token);
                using (HttpResponseMessage response = await client.SendAsync(httpRequestMessage))
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            byte[] bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
                return base64urlencodeNoPadding(bytes);
            }
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static byte[] sha256(string inputStirng)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
                return sha256.ComputeHash(bytes);
            }
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
