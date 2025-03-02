using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;
using BlazorApp1.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;

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

            //string nUri = "https://localhost:7262/search";
            string nUri = "https://localhost:7262/auth/spotifycallback";
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

        public async Task ExchangeCodeForToken(string code)
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

            if (responseContent != null)
            {
                //ProtectedSessionStorage protectedSessionStorage = new ProtectedSessionStorage();
                _ProtectedSessionStorage.SetAsync("spotify_token", responseContent.access_token);
                Console.WriteLine("ExchangeCodeForToken accessToken: " + responseContent.access_token);
                // Store the token securely (e.g., local storage or session)
                //*/*/*/*/*/*/Navigation.NavigateTo("/");*/*/*/*/*/*/
                //return responseContent;
            }
            //else
            //{
            //    return null;
            //}
        }

        public async Task<string> GetTokenAsync()
        {
            var result = await _ProtectedSessionStorage.GetAsync<string>("spotify_token");
            if (result.Success)
            {
                Console.WriteLine($"Stored Value: {result.Value}");
                return result.Value;
            }
            return "";
        }
}
}
