using BlazorApp1.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BlazorApp1.SpotifyServices
{
    public partial class SpotifyAppServices
    {
        private readonly HttpClient _httpClient;
        public IConfiguration _Configuration;
        public IJSRuntime _JSRunTime;
        public SpotifyServicess_TokenService _TokenService;

        public SpotifyAppServices(IConfiguration configuration, IJSRuntime jsRuntime, SpotifyServicess_TokenService tokenService)
        {
            _httpClient = new HttpClient();
            _Configuration = configuration;
            _JSRunTime = jsRuntime;
            _TokenService = tokenService;
        }

        public string SpotifySignInAuth(bool ToPlaylists)
        {
            string? spotifyAuthUrl = null;
            string spotifyAuthAddress = "https://accounts.spotify.com/authorize";
            string redirectUri = _Configuration["SpotifyWeb:RedirectUri"];
            
            if (!ToPlaylists) {
                redirectUri = _Configuration["SpotifyWeb:RedirectUriPlaylistGen"];
            }

            Console.WriteLine("clientID: " + _Configuration["SpotifyWeb:ClientId"]);
            try
            {
                spotifyAuthUrl = $"{spotifyAuthAddress}?client_id={_Configuration["SpotifyWeb:ClientId"]}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(_Configuration["SpotifyWeb:Scopes"])}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifySignInAuth Ex: " + ex.Message);
            }

            return spotifyAuthUrl;
        }
        public async Task ExchangeCodeForToken(string code, bool ToPlaylists)
        {
            string redirectUri = _Configuration["SpotifyWeb:RedirectUri"];
            if (!ToPlaylists) { redirectUri = _Configuration["SpotifyWeb:RedirectUriPlaylistGen"]; }
            try
            {

                //HttpClient httpClient = new HttpClient();

                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", _Configuration["SpotifyWeb:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _Configuration["SpotifyWeb:ClientSecret"]),
                });

                var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", requestContent);
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                var accessToken = json.GetProperty("access_token").GetString();
                var expiresIn = json.GetProperty("expires_in").GetInt32();
                var refreshToken = json.GetProperty("refresh_token").GetString();

                _TokenService.SetTokens(accessToken, expiresIn, refreshToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExchangeCodeForToken Ex: " + ex.Message);
            }
        }
        public async Task<bool> TryRefreshAccessTokenAsync()
        {
            var refreshToken = _TokenService.GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            //HttpClient httpClient = new HttpClient();

            var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>( "refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", _Configuration["SpotifyWeb:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _Configuration["SpotifyWeb:ClientSecret"]),
            });

            var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", requestContent);

            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var accessToken = json.GetProperty("access_token").GetString();
            var expiresIn = json.GetProperty("expires_in").GetInt32();

            _TokenService.SetTokens(accessToken, expiresIn, null); // Don't overwrite refresh token
            return true;
        }
        public async Task<HttpResponseMessage> SendWithRateLimitRetryAsync(HttpClient client, string url, HttpMethod method, HttpContent content = null, int maxRetries = 3)
        {
            int currentRetry = 0;
            while (currentRetry <= maxRetries)
            {
                var request = new HttpRequestMessage(method, url);
                if (content != null && method == HttpMethod.Post)
                {
                    request.Content = content;
                }

                var response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (currentRetry <= maxRetries)
                    {
                        throw new Exception("Maximum retry attempts reached due to rate limiting");
                    }
                    int delaySeconds = 1;
                    if (response.Headers.TryGetValues("Retry-After", out var values) &&
                        int.TryParse(values.FirstOrDefault(), out int retryAfter))
                    {
                        delaySeconds = retryAfter;
                    }
                    else
                    {
                        delaySeconds = (int)Math.Pow(2, currentRetry);
                    }
                        Console.WriteLine($"Rate limited. Waiting {delaySeconds}s before retry {currentRetry + 1}/{maxRetries}");
                        await Task.Delay(delaySeconds * 1000);
                        currentRetry++;
                        continue;
                }
                return response;
            }
            throw new Exception("Unexpected end of retry loop");
        }
        public async Task<SpotifyAuthUserData> InitSpotifyFlow(string SpotifyCode)
        {
            SpotifyAuthUserData? spotifyAuthUserData = null;
            try
            {
                if (!string.IsNullOrEmpty(SpotifyCode))
                {
                    if (string.IsNullOrEmpty(_TokenService.AccessToken))
                    {
                        await ExchangeCodeForToken(SpotifyCode, true);
                    }

                    await _JSRunTime.InvokeVoidAsync("spotifyDisplayNavigation");
                    SpotifyAuthUser spotifyAuthUser = await SpotifyGetProfile();
                    List<SpotifyPlaylist>? spotifyListPlaylists = await SpotifyGetPlaylists();

                    if (spotifyAuthUser != null)
                    {
                        spotifyAuthUserData = new SpotifyAuthUserData();

                        spotifyAuthUserData.SpotifyAuthUser = spotifyAuthUser;
                        spotifyAuthUserData.ListSpotifyPlaylists = spotifyListPlaylists;
                    }
                }
            }
            catch (NavigationException navEx)
            {
                Console.WriteLine("OnInitializedAsync navEx: " + navEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnInitializedAsync Ex: " + ex.Message);
            }
            return spotifyAuthUserData;
        }
        public string GetSpotifyCode(Uri uri)
        {
            string? spotifyCode = null;
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                spotifyCode = query["code"] ?? null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetSpotifyCode Ex: " + ex.Message);
            }

            return spotifyCode;
        }
        public async Task<SpotifyAuthUser> SpotifyGetProfile()
        {
            SpotifyAuthUser spotifyAuthUser = new SpotifyAuthUser();
            
            try
            {
                if (_TokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _TokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty. Please authenticate first.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await SendWithRateLimitRetryAsync(_httpClient,
                    "https://api.spotify.com/v1/me",
                    HttpMethod.Get
                    );

                var content = await response.Content.ReadAsStringAsync();
                var profile = JsonDocument.Parse(content).RootElement;

                spotifyAuthUser.DisplayName = profile.GetProperty("display_name").GetString();
                spotifyAuthUser.SpotifyID = profile.GetProperty("id").GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetProfile Ex: " + ex.Message);
            }
            
            return spotifyAuthUser;
        }
        public async Task<List<SpotifyPlaylist>> SpotifyGetPlaylists()
        {

            List<SpotifyPlaylist> listPlaylistItems = new List<SpotifyPlaylist>();

            try
            {
                if (_TokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _TokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty. Please authenticate first.");
                }

                string pageUrl = "https://api.spotify.com/v1/me/playlists";
                bool bPagingComplete = false;

                while (!bPagingComplete)
                {
                    SpotifyPlaylists spotifyPlaylists = await SpotifyPlaylistsNextRequest(accessToken, pageUrl);

                    foreach (SpotifyPlaylist playlist in spotifyPlaylists.Items)
                    {
                        string shortName = playlist.Name;
                        if (shortName.Length > 20)
                        {
                            shortName = SpotifyShortName(playlist.Name);
                        }
                        playlist.ShortName = shortName;
                        listPlaylistItems.Add(playlist);
                    }

                    if (string.IsNullOrEmpty(spotifyPlaylists.Next))
                    {
                        bPagingComplete = true;
                    }
                    else
                    {
                        pageUrl = spotifyPlaylists.Next;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetPlaylists ex: " + ex.Message);
            }
            return listPlaylistItems;
        }
        //Request next page
        public async Task<SpotifyPlaylists> SpotifyPlaylistsNextRequest(string accessToken, string pageUrl)
        {

            SpotifyPlaylists spotifyPlaylists = new SpotifyPlaylists();

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await SendWithRateLimitRetryAsync(
                    _httpClient,
                    pageUrl,
                    HttpMethod.Get
                    );
                
                var content = await response.Content.ReadAsStringAsync();
                spotifyPlaylists = JsonConvert.DeserializeObject<SpotifyPlaylists>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyPlaylistsNextRequest ex: " + ex.Message);
            }

            return spotifyPlaylists;
        }
        public async Task<SpotifyPlaylist> SpotifyGetPlaylist(string playlistId)
        {

            SpotifyPlaylist playlist = null;

            try
            {
                if (_TokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _TokenService.AccessToken;
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty. Please authenticate first.");
                }

                string playlistUrl = "https://api.spotify.com/v1/playlists/" + playlistId;
                string queryFields = "description,id,images(url),tracks(next,total,items(track(album(name),artists(name),duration_ms,id,name)))";

                var builder = new UriBuilder(playlistUrl);
                var query = $"fields={Uri.UnescapeDataString(queryFields)}";
                builder.Query = query;

                string generatedUrl = builder.ToString();

                bool bPagingComplete = false;
                bool nextPage = false;
                while (!bPagingComplete)
                {
                    SpotifyPlaylist playlistResult = null;
                    PlaylistTracks trackItems = null;
                    if (!nextPage)
                    {
                        playlistResult = await SpotifyGetPlaylistsInformation(accessToken, generatedUrl);
                    }
                    else
                    {
                        trackItems = await SpotifyGetTrackInformation(accessToken, generatedUrl);
                    }

                    if (playlist == null)
                    {
                        playlist = playlistResult;
                    }
                    else
                    {
                        if (playlist.Tracks.Next != null)
                        {
                            //playlist.Tracks.Next = playlistResult.Tracks.Next;
                            nextPage = true;
                        }

                        if (!nextPage)
                        {
                            foreach (TrackItem track in playlistResult.Tracks.Items)
                            {
                                string time = SpotifyGenTrackTime(track.Track.Duration_ms);
                                Console.WriteLine(time);
                                //convert ms to seconds
                                playlist.Tracks.Items.Add(track);
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(playlist.Tracks.Next))
                    {
                        bPagingComplete = true;
                    }
                    else
                    {
                        generatedUrl = playlist.Tracks.Next;
                    }
                }

                foreach (TrackItem selectedTrack in playlist.Tracks.Items)
                {
                    if (selectedTrack.Track != null)
                    {
                        selectedTrack.Track.TrackTime = SpotifyGenTrackTime(selectedTrack.Track.Duration_ms);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetPlaylist ex: " + ex.Message);
            }

            return playlist;
        }
        public async Task<SpotifyPlaylist> SpotifyGetPlaylistsInformation(string accessToken, string pageUrl)
        {

            SpotifyPlaylist spotifyPlaylist = new SpotifyPlaylist();

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await SendWithRateLimitRetryAsync(
                    _httpClient,
                    pageUrl,
                    HttpMethod.Get
                    );
                var content = await response.Content.ReadAsStringAsync();
                spotifyPlaylist = JsonConvert.DeserializeObject<SpotifyPlaylist>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetPlaylistsInformation ex: " + ex.Message);
            }

            return spotifyPlaylist;
        }
        public async Task<PlaylistTracks> SpotifyGetTrackInformation(string accessToken, string pageUrl)
        {

            PlaylistTracks trackItem = new PlaylistTracks();

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await SendWithRateLimitRetryAsync(
                    _httpClient,
                    pageUrl,
                    HttpMethod.Get
                );
                var content = await response.Content.ReadAsStringAsync();
                trackItem = JsonConvert.DeserializeObject<PlaylistTracks>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetTrackInformation ex: " + ex.Message);
            }

            return trackItem;
        }
        public async Task<List<OpenAITrack>> SpotifyGetTrackIDs(List<OpenAITrack> listTracks)
        {
            //List<string> listSpotifyTrackIds = new List<string>();
            int successfulSearch = 0;
            List<OpenAITrack> listTracksToRemove = new List<OpenAITrack>();
            try
            {
                if (_TokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _TokenService.AccessToken;
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty. Please authenticate first.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                Console.WriteLine("SpotifyGetTrackIDs number of tracks to find: " + listTracks.Count);

                foreach (OpenAITrack track in listTracks)
                {
                    // Create a more focused search query for better matching
                    var query = $"track:\"{Uri.EscapeDataString(track.Title)}\" artist:\"{Uri.EscapeDataString(track.Artist)}\"";

                    var response = await SendWithRateLimitRetryAsync(
                        _httpClient,
                        $"https://api.spotify.com/v1/search?q={query}&type=track&limit=1",
                        HttpMethod.Get
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var spotifyPlaylist = JsonConvert.DeserializeObject<SpotifyPlaylist>(result);

                        if (spotifyPlaylist?.Tracks?.Items?.Any() == true)
                        {
                            var trackId = spotifyPlaylist.Tracks.Items.FirstOrDefault()?.Id;
                            if (!string.IsNullOrEmpty(trackId))
                            {
                                track.Spotify_Id = trackId;
                                successfulSearch++;
                            }
                        }
                        else
                        {
                            listTracksToRemove.Add(track);
                        }
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"SpotifyGetTrackIDs Search Error: {error}. Title: {track.Title}, Artist: {track.Artist}.");
                    }

                    // Add a small delay between requests to respect rate limits
                    await Task.Delay(100);
                }

                foreach (OpenAITrack track in listTracksToRemove)
                {
                    listTracks.Remove(track);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetTrackIDs ex: " + ex.Message);
            }

            Console.WriteLine($"SpotifyGetTrackIDs: Successfully found {successfulSearch} tracks");
            return listTracks;
        }
        public async Task<string> SpotifyCreatePlaylist(string playlistName)
        {

            SpotifyPlaylist spotifyPlaylist = new SpotifyPlaylist();

            var playlist = new
            {
                name = playlistName,
                description = "AI playlist, " + playlistName
            };

            string requestContent = JsonConvert.SerializeObject(playlist);
            var httpContent = new StringContent(requestContent, Encoding.UTF8, "application/json");

            try
            {
                if (_TokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _TokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty. Please authenticate first.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await SendWithRateLimitRetryAsync(
                    _httpClient,
                    "https://api.spotify.com/v1/users/joval101/playlists",
                    HttpMethod.Post,
                    httpContent
                );

                string result = await response.Content.ReadAsStringAsync();

                spotifyPlaylist = JsonConvert.DeserializeObject<SpotifyPlaylist>(result);

            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyCreatePlaylist ex: " + ex.Message);
            }

            Console.WriteLine($"SpotifyCreatePlaylist: Playlist - {playlistName} created successfully");
            return spotifyPlaylist.Id;
        }
        public async Task SpotifyAddTracksToPlaylist(List<string> tracks, string playlistId)
        {
            const int batchSize = 100;

            try
            {
                if (_TokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _TokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty. Please authenticate first.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                for (int i = 0; i < tracks.Count; i += batchSize)
                {
                    var batchTracks = tracks.Skip(i).Take(batchSize)
                        .Select(t => "spotify:track:" + t)
                        .ToList();

                    var payload = new { uris = batchTracks };
                    string json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await SendWithRateLimitRetryAsync(
                    _httpClient,
                    $"https://api.spotify.com/v1/playlists/{playlistId}/tracks",
                    HttpMethod.Post,
                    httpContent
                );

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to add tracks. Status: {response.StatusCode}");
                    }

                    // Add delay between batches to respect rate limits
                    if (i + batchSize < tracks.Count)
                    {
                        await Task.Delay(1000); // 1 second delay between batches
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyAddTracksToPlaylist ex: " + ex.Message);
                throw;
            }
            Console.WriteLine($"SpotifyAddTracksToPlaylist: Successfully added {tracks.Count} tracks to playlist");
        }
        private string SpotifyShortName(string name)
        {
            string? shortName = null;

            try
            {
                shortName = name.Substring(0, 20) + "...";
                //Console.WriteLine("SpotifyShortName shortName: " + shortName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyShortName ex: " + ex.Message);
            }
            return shortName;
        }
        private string SpotifyGenTrackTime(int ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);

            string time = $"{t.Minutes}:{t.Seconds}";

            return time;

        }
    }
}
