using Microsoft.JSInterop;
using BlazorApp1.Data;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text;

namespace BlazorApp1.SpotifyServices
{
    public partial class SpotifyAppServices
    {
        public IConfiguration _Configuration;

        public SpotifyAppServices(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public string SpotifySignInAuth()
        {
            string? spotifyAuthUrl = null;

            string spotifyAuthAddress = "https://accounts.spotify.com/authorize";

            string nUri = "https://localhost:7262/search";
            Console.WriteLine("SpotifySignInAuth: " + _Configuration["SpotifyWeb:Scopes"]);
            try
            {
                spotifyAuthUrl = $"{spotifyAuthAddress}?client_id={_Configuration["SpotifyWeb:ClientId"]}&response_type=code&redirect_uri={Uri.EscapeDataString(nUri)}&scope={Uri.EscapeDataString(_Configuration["SpotifyWeb:Scopes"])}";
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
                    new KeyValuePair<string, string>("redirect_uri", _Configuration["SpotifyWeb:RedirectUri"]),
                    new KeyValuePair<string, string>("client_id", _Configuration["SpotifyWeb:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _Configuration["SpotifyWeb:ClientSecret"]),
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
        public async Task<SpotifyAuthUser> SpotifyGetProfile(string spotifyAccessToken)
        {

            SpotifyAuthUser spotifyAuthUser = new SpotifyAuthUser();

            HttpClient httpClient = new HttpClient();

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", spotifyAccessToken);
                var response = await httpClient.GetAsync("https://api.spotify.com/v1/me");
                var content = await response.Content.ReadAsStringAsync();

                var profile = JsonDocument.Parse(content).RootElement;

                var displayName = profile.GetProperty("display_name").GetString();
                var spotifyId = profile.GetProperty("id").GetString();

                spotifyAuthUser.DisplayName = displayName;
                spotifyAuthUser.SpotifyID = spotifyId;

                //Console.WriteLine($"Name: {displayName}, ID: {spotifyId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetProfile Ex: " + ex.Message);
            }

            return spotifyAuthUser;
        }
        public async Task<List<SpotifyPlaylist>> SpotifyGetPlaylists(string spotifyAccessToken)
        {

            List<SpotifyPlaylist> listPlaylistItems = new List<SpotifyPlaylist>();

            try
            {

                string pageUrl = "https://api.spotify.com/v1/me/playlists";
                bool bPagingComplete = false;

                while (!bPagingComplete)
                {
                    SpotifyPlaylists spotifyPlaylists = await SpotifyPlaylistsNextRequest(spotifyAccessToken, pageUrl);

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

            HttpClient httpClient = new HttpClient();
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.GetAsync(pageUrl);
                var content = await response.Content.ReadAsStringAsync();
                spotifyPlaylists = JsonConvert.DeserializeObject<SpotifyPlaylists>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyPlaylistsNextRequest ex: " + ex.Message);
            }

            return spotifyPlaylists;
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
        public async Task<SpotifyPlaylist> SpotifyGetPlaylist(string playlistId, string spotifyAccessToken)
        {

            SpotifyPlaylist playlist = null;

            try
            {
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
                        playlistResult = await SpotifyGetPlaylistsInformation(spotifyAccessToken, generatedUrl);
                    }
                    else
                    {
                        trackItems = await SpotifyGetTrackInformation(spotifyAccessToken, generatedUrl);
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
                    selectedTrack.Track.TrackTime = SpotifyGenTrackTime(selectedTrack.Track.Duration_ms);
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

            HttpClient httpClient = new HttpClient();

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.GetAsync(pageUrl);
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

            HttpClient httpClient = new HttpClient();

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.GetAsync(pageUrl);
                var content = await response.Content.ReadAsStringAsync();
                trackItem = JsonConvert.DeserializeObject<PlaylistTracks>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetTrackInformation ex: " + ex.Message);
            }

            return trackItem;
        }
        private string SpotifyGenTrackTime(int ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);

            string time = $"{t.Minutes}:{t.Seconds}";

            return time;

        }
        public class PlaylistOBJ
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Public { get; set; }
        }
        public async Task<string> SpotifyCreatePlaylist(string accessToken, string playlistName)
        {

            SpotifyPlaylist spotifyPlaylist = new SpotifyPlaylist();

            HttpClient httpClient = new HttpClient();

            var playlist = new
            {
                name = playlistName,
            };

            string requestContent = JsonConvert.SerializeObject(playlist);
            var httpContent = new StringContent(requestContent, Encoding.UTF8, "application/json");

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.PostAsync("https://api.spotify.com/v1/users/joval101/playlists", httpContent);

                string result = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(result);

                spotifyPlaylist = JsonConvert.DeserializeObject<SpotifyPlaylist>(result);

            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyCreatePlaylist ex: " + ex.Message);
            }

            return spotifyPlaylist.Id;
        }
        public async Task SpotifyAddTracksToPlaylist(string accessToken, List<string> tracks, string playlistId)
        {
            HttpClient httpClient = new HttpClient();

            List<string> trackUris = new List<string>();

            foreach (string track in tracks)
            {
                trackUris.Add("spotify:track:" + track);
            }

            // Build JSON payload
            var payload = new { uris = trackUris };
            string json = System.Text.Json.JsonSerializer.Serialize(payload);

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks", httpContent);
                //Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyAddTracksToPlaylist ex: " + ex.Message);
            }
        }


        public async Task<List<string>> SpotifyGetTrackIDs(string accessToken, List<CreateTrack> listTracks)
        {
            List<string> listSpotifyTrackIds = new List<string>(); 
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                foreach(CreateTrack track in listTracks) 
                {
                    string query = $"track:\"{track.Name}\" artist:${track.Artist}";
                    var encodedQuery = Uri.EscapeDataString(query);

                    var response = await httpClient.GetAsync($"https://api.spotify.com/v1/search?q={encodedQuery}&type=track&limit=1");

                    var result = await response.Content.ReadAsStringAsync();
                    SpotifyPlaylist spotifyPlaylist = JsonConvert.DeserializeObject<SpotifyPlaylist>(result);

                    if (spotifyPlaylist.Tracks.Items.Count > 0)
                    {
                        listSpotifyTrackIds.Add(spotifyPlaylist.Tracks.Items[0].Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SpotifyGetTrackIDs ex: " + ex.Message);
            }
            return listSpotifyTrackIds;
        }
    }
}
