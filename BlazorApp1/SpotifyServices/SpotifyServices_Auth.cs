using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;
using BlazorApp1.Data;
using System;
using System.Threading.Tasks;

namespace BlazorApp1.SpotifyServices
{
    public partial class SpotifyAppServices
    {
        public IConfiguration _Configuration;

        private string clientId;
        private string clientSecret;
        private string redirectUri;

        private readonly IJSRuntime _jsRuntime;

        public SpotifyAppServices(IConfiguration configuration)
        {
            _Configuration = configuration;

            clientId = _Configuration["SpotifyWeb:ClientId"];
            clientSecret = _Configuration["SpotifyWeb:ClientSecret"]; ;
            redirectUri = _Configuration["SpotifyWeb:RedirectUri"];
        }

        public string SpotifySignInAuth()
        {
            string? spotifyAuthUrl = null;

            string spotifyAuthAddress = "https://accounts.spotify.com/authorize";
            string nUrl = "https://localhost:7262/auth/spotifycallback";
            try
            {
                string scopes = _Configuration["SpotifyWeb:Scopes"];

                spotifyAuthUrl = $"{spotifyAuthAddress}?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(nUrl)}&scopes={Uri.EscapeDataString(scopes)}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifySignInAuth Ex: " + ex.Message);
            }

            return spotifyAuthUrl;
        }

        //public async Task<SpotifyTokenResponse> ExchangeCodeForToken(string code)
        //{
        //    HttpClient httpClient = new HttpClient();

        //    var requestContent = new FormUrlEncodedContent(new[]
        //    {
        //    new KeyValuePair<string, string>("grant_type", "authorization_code"),
        //    new KeyValuePair<string, string>("code", code),
        //    new KeyValuePair<string, string>("redirect_uri", redirectUri),
        //    new KeyValuePair<string, string>("client_id", clientId),
        //    new KeyValuePair<string, string>("client_secret", clientSecret),
        //});

        //    var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", requestContent);
        //    var responseContent = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();

        //    if (responseContent != null)
        //    {

        //        // Store the token securely (e.g., local storage or session)
        //        //*/*/*/*/*/*/Navigation.NavigateTo("/");*/*/*/*/*/*/
        //        return responseContent;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    }
}
