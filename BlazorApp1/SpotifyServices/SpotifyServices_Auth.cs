using Microsoft.JSInterop;
using BlazorApp1.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorApp1.SpotifyServices
{
    public partial class SpotifyAppServices
    {
        public IConfiguration _Configuration;

        private string clientId;
        private string clientSecret;
        private string redirectUri;

        private readonly IJSRuntime _jsRuntime;

        public ProtectedSessionStorage _ProtectedSessionStorage;

        public SpotifyAppServices(IConfiguration configuration, ProtectedSessionStorage protectedSessionStorage)
        {
            _Configuration = configuration;
            _ProtectedSessionStorage = protectedSessionStorage;

            clientId = _Configuration["SpotifyWeb:ClientId"];
            clientSecret = _Configuration["SpotifyWeb:ClientSecret"]; ;
            redirectUri = _Configuration["SpotifyWeb:RedirectUri"];
        }

        public string SpotifySignInAuth()
        {
            string? spotifyAuthUrl = null;

            string spotifyAuthAddress = "https://accounts.spotify.com/authorize";

            string nUri = "https://localhost:7262/search";
            try
            {
                string scopes = _Configuration["SpotifyWeb:Scopes"];

                spotifyAuthUrl = $"{spotifyAuthAddress}?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(nUri)}&scopes={Uri.EscapeDataString(scopes)}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifySignInAuth Ex: " + ex.Message);
            }

            return spotifyAuthUrl;
        }

        public async Task<string?> ExchangeCodeForToken(string code)
        {
            string? accessToken = null;

            try
            {

                HttpClient httpClient = new HttpClient();

                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                });

                var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", requestContent);
                var responseContent = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();


                accessToken = responseContent.access_token;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExchangeCodeForToken Ex: " + ex.Message);
            }

            return accessToken;
        }
    }
}
