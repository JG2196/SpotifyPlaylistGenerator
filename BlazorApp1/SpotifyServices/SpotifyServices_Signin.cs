using System.Net.Http;
using System.Text.Json;

namespace BlazorApp1.SpotifyServices
{
    public class SpotifyServices_Signin
    {
        public IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly SpotifyServices_TokenService _TokenService;

        public SpotifyServices_Signin(IConfiguration configuration, HttpClient httpClient, SpotifyServices_TokenService tokenService)
        {
            _config = configuration;
            _httpClient = httpClient;
            _TokenService = tokenService;
        }

        public string SpotifySigninAuth()
        {
            string? spotifyAuthUrl = null;
            string spotifyAuthAddress = _config["SpotifyWeb:AuthAddress"];
            string redirectUri = _config["SpotifyWeb:RedirectUri"];
            try
            {
                spotifyAuthUrl = $"{spotifyAuthAddress}?client_id={_config["SpotifyWeb:ClientId"]}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(_config["SpotifyWeb:Scopes"])}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - SpotifySigninAuth Ex: " + ex.Message);
            }

            return spotifyAuthUrl;
        }
        public string SpotifyGetSpotifyCode(Uri uri)
        {
            string? spotifyCode = null;
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                spotifyCode = query["code"] ?? null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - SpotifyGetSpotifyCode Ex: " + ex.Message);
            }

            return spotifyCode;
        }
        public async Task<bool> SpotifyExchangeCodeForToken(string code)
        {
            bool bSucessful = false;
            try
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", _config["SpotifyWeb:RedirectUri"]),
                    new KeyValuePair<string, string>("client_id", _config["SpotifyWeb:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _config["SpotifyWeb:ClientSecret"]),
                });

                var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", requestContent);
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                var accessToken = json.GetProperty("access_token").GetString();
                var expiresIn = json.GetProperty("expires_in").GetInt32();
                var refreshToken = json.GetProperty("refresh_token").GetString();

                _TokenService.SetTokens(accessToken, expiresIn, refreshToken);
                bSucessful = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - SpotifyExchangeCodeForToken Ex: " + ex.Message);
            }
            return bSucessful;
        }
    }
}
