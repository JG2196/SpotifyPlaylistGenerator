using BlazorApp1.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlazorApp1.SpotifyServices
{
    public partial class SpotifyAppServices
    {
        private readonly HttpClient _httpClient;
        public IConfiguration _config;
        public IJSRuntime _jsRunTime;
        public SpotifyServices_TokenService _tokenService;

        public SpotifyAppServices(IConfiguration configuration, IJSRuntime jsRuntime, SpotifyServices_TokenService tokenService)
        {
            _httpClient = new HttpClient();
            _config = configuration;
            _jsRunTime = jsRuntime;
            _tokenService = tokenService;
        }

        private async Task<bool> TryRefreshAccessTokenAsync()
        {
            bool bSuccessful = false;

            try
            {
                var refreshToken = _tokenService.GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken)) { return bSuccessful; }

                var requestContent = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>( "refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", _config["SpotifyWeb:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _config["SpotifyWeb:ClientSecret"]),
            });

                var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", requestContent);

                if (!response.IsSuccessStatusCode) { return bSuccessful; }

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                var accessToken = json.GetProperty("access_token").GetString();
                var expiresIn = json.GetProperty("expires_in").GetInt32();

                _tokenService.SetTokens(accessToken, expiresIn, null); // Don't overwrite refresh token
                bSuccessful = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - TryRefreshAccessTokenAsync ex: " + ex.Message);
            }
            
            return bSuccessful;
        }
        private async Task<HttpResponseMessage> SendWithRateLimitRetryAsync(HttpClient client, string url, HttpMethod method, HttpContent content = null, int maxRetries = 3)
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
                        throw new Exception("SendWithRateLimitRetryAsync: Maximum retry attempts reached due to rate limiting");
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
                        Console.WriteLine($"SendWithRateLimitRetryAsync: Rate limited. Waiting {delaySeconds}s before retry {currentRetry + 1}/{maxRetries}");
                        await Task.Delay(delaySeconds * 1000);
                        currentRetry++;
                        continue;
                }
                return response;
            }
            throw new Exception("SendWithRateLimitRetryAsync: Unexpected end of retry loop");
        }
        public async Task<SpotifyAuthUserData> InitSpotifyFlow()
        {
            SpotifyAuthUserData? spotifyAuthUserData = null;
            try
            {
                SpotifyAuthUser spotifyAuthUser = await SpotifyGetProfile();

                if (spotifyAuthUser != null)
                {
                    List<SpotifyPlaylist>? spotifyListPlaylists = await SpotifyGetPlaylists();

                    spotifyAuthUserData = new SpotifyAuthUserData()
                    {
                        SpotifyAuthUser = spotifyAuthUser,
                        ListSpotifyPlaylists = spotifyListPlaylists
                    };                    
                }
            
            }
            catch (NavigationException navEx)
            {
                Console.WriteLine("Error - InitSpotifyFlow navEx: " + navEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - InitSpotifyFlow Ex: " + ex.Message);
            }

            return spotifyAuthUserData;
        }
        private async Task<SpotifyAuthUser> SpotifyGetProfile()
        {
            SpotifyAuthUser? spotifyAuthUser = null;
            
            try
            {
                if (_tokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _tokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("SpotifyGetProfile: Access token is null or empty. Please authenticate first.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await SendWithRateLimitRetryAsync(_httpClient,
                    "https://api.spotify.com/v1/me",
                    HttpMethod.Get
                    );

                var content = await response.Content.ReadAsStringAsync();
                var profile = JsonDocument.Parse(content).RootElement;

                spotifyAuthUser = new SpotifyAuthUser()
                {
                    DisplayName = profile.GetProperty("display_name").GetString(),
                    SpotifyID = profile.GetProperty("id").GetString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - SpotifyGetProfile Ex: " + ex.Message);
            }
            
            return spotifyAuthUser;
        }
        public async Task<List<SpotifyPlaylist>> SpotifyGetPlaylists()
        {

            List<SpotifyPlaylist> listPlaylistItems = new List<SpotifyPlaylist>();

            try
            {
                if (_tokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _tokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("SpotifyGetPlaylists: Access token is null or empty. Please authenticate first.");
                }

                string pageUrl = "https://api.spotify.com/v1/me/playlists";
                bool bPagingComplete = false;

                while (!bPagingComplete)
                {
                    SpotifyPlaylists spotifyPlaylists = await SpotifyPlaylistsNextRequest(accessToken, pageUrl);

                    foreach (SpotifyPlaylist playlist in spotifyPlaylists.Items)
                    {
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
                Console.WriteLine("Error - SpotifyGetPlaylists ex: " + ex.Message);
            }
            return listPlaylistItems;
        }
        //Request next page
        private async Task<SpotifyPlaylists> SpotifyPlaylistsNextRequest(string accessToken, string pageUrl)
        {

            SpotifyPlaylists spotifyPlaylists = null;

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
                Console.WriteLine("Error - SpotifyPlaylistsNextRequest ex: " + ex.Message);
            }

            return spotifyPlaylists;
        }
        public async Task<SpotifyPlaylist> SpotifyGetPlaylist(string playlistId)
        {

            SpotifyPlaylist? playlist = null;

            try
            {
                if (_tokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _tokenService.AccessToken;
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("SpotifyGetPlaylist: Access token is null or empty. Please authenticate first.");
                }

                string playlistUrl = $"https://api.spotify.com/v1/playlists/{playlistId}";
                string queryFields = "description,external_urls(spotify),id,images(url),tracks(next,total,items(track(album(name),artists(name),duration_ms,id,name)))";

                var builder = new UriBuilder(playlistUrl);
                var query = $"fields={Uri.UnescapeDataString(queryFields)}";
                builder.Query = query;

                string generatedUrl = builder.ToString();

                bool bPagingComplete = false;
                bool nextPage = false;
                int msPlaylistDuration = 0;

                while (!bPagingComplete)
                {
                    SpotifyPlaylist? playlistResult = null;
                    PlaylistTracks? trackItems = null;
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
                            nextPage = true;
                        }

                        if (!nextPage)
                        {
                            foreach (TrackItem track in playlistResult.Tracks.Items)
                            {
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
                        msPlaylistDuration += selectedTrack.Track.Duration_ms;
                    }
                }

                playlist.PlaylistDuration = SpotifyGenplaylistDuration(msPlaylistDuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine("rror - SpotifyGetPlaylist ex: " + ex.Message);
            }

            return playlist;
        }
        private async Task<SpotifyPlaylist> SpotifyGetPlaylistsInformation(string accessToken, string pageUrl)
        {

            SpotifyPlaylist? spotifyPlaylist = null;

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
                Console.WriteLine("Error - SpotifyGetPlaylistsInformation ex: " + ex.Message);
            }

            return spotifyPlaylist;
        }
        private async Task<PlaylistTracks> SpotifyGetTrackInformation(string accessToken, string pageUrl)
        {

            PlaylistTracks? trackItem = null;

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
                Console.WriteLine("Error - SpotifyGetTrackInformation ex: " + ex.Message);
            }

            return trackItem;
        }
        public async Task<List<OpenAITrack>> SpotifyGetTrackIDs(List<OpenAITrack> listTracks)
        {
            int successfulSearch = 0;
            List<OpenAITrack> listTracksToRemove = new List<OpenAITrack>();
            try
            {
                if (_tokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _tokenService.AccessToken;
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("SpotifyGetTrackIDs: Access token is null or empty. Please authenticate first.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                Console.WriteLine("SpotifyGetTrackIDs: number of tracks to find: " + listTracks.Count);

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
                        Console.WriteLine($"Search Error - SpotifyGetTrackIDs: {error}. Title: {track.Title}, Artist: {track.Artist}.");
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
                Console.WriteLine("Error - SpotifyGetTrackIDs ex: " + ex.Message);
            }

            Console.WriteLine($"SpotifyGetTrackIDs: Successfully found {successfulSearch} tracks");
            return listTracks;
        }
        public async Task<string> SpotifyCreatePlaylist(string playlistName)
        {

            string playlistId = string.Empty;

            var playlist = new
            {
                name = playlistName,
                description = "AI playlist, " + playlistName
            };

            string requestContent = JsonConvert.SerializeObject(playlist);
            var httpContent = new StringContent(requestContent, Encoding.UTF8, "application/json");

            try
            {
                if (_tokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _tokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("SpotifyCreatePlaylist: Access token is null or empty. Please authenticate first.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await SendWithRateLimitRetryAsync(
                    _httpClient,
                    "https://api.spotify.com/v1/users/joval101/playlists",
                    HttpMethod.Post,
                    httpContent
                );

                string result = await response.Content.ReadAsStringAsync();
                SpotifyPlaylist spotifyPlaylist = JsonConvert.DeserializeObject<SpotifyPlaylist>(result);
                
                playlistId = spotifyPlaylist.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - SpotifyCreatePlaylist ex: " + ex.Message);
            }

            Console.WriteLine($"SpotifyCreatePlaylist: Playlist - {playlistName} created successfully");
            return playlistId;
        }
        public async Task SpotifyAddTracksToPlaylist(List<string> tracks, string playlistId)
        {
            try
            {
                if (_tokenService.IsAccessTokenExpired)
                {
                    await TryRefreshAccessTokenAsync();
                }

                var accessToken = _tokenService.AccessToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("SpotifyAddTracksToPlaylist: Access token is null or empty. Please authenticate first.");
                }

                const int batchSize = 100;
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
                Console.WriteLine("Error - SpotifyAddTracksToPlaylist ex: " + ex.Message);
                throw;
            }
            Console.WriteLine($"Error - SpotifyAddTracksToPlaylist: Successfully added {tracks.Count} tracks to playlist");
        }
        private string SpotifyGenplaylistDuration(int playlistDuration)
        {
            string duration = string.Empty;

            try
            {
                TimeSpan ts = TimeSpan.FromMilliseconds(playlistDuration);
                int hours = ts.Hours;
                int minutes = ts.Minutes;

                duration = "0min";

                if (hours > 0 && minutes > 0) { duration = $"{hours}h {minutes}min"; }
                else if (hours > 0 && minutes == 0) { duration = $"{hours}h"; }
                if (hours == 0 && minutes > 0) { duration = $"{minutes}min"; }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - SpotifyGenplaylistDuration ex: " + ex.Message);
            }

            return duration;
        }
    }
}
