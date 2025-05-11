namespace BlazorApp1.Data
{
    public class SpotifyTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
    public class SpotifyAuthUser
    {
        public string DisplayName { get; set; }
        public string SpotifyID { get; set; }
    }
    public class SpotifyPlaylists
    {
        public string Next { get; set; }
        public int Total { get; set; }
        public List<SpotifyPlaylist> Items = new List<SpotifyPlaylist>();
    }
    public class SpotifyPlaylist
    {
        public string Description { get; set; }
        public string Href { get; set; }
        public string Id { get; set; }
        public List<PlaylistImage> Images { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public PlaylistOwner Owner { get; set; }
        public PlaylistTracks Tracks { get; set; }
    }
    public class PlaylistImage
    {
        public string Url { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }
    public class PlaylistOwner
    {
        public string DisplayName { get; set; }
    }
    public class PlaylistTracks
    {
        public string Next { get; set; }
        public int Total { get; set; }
        public List<TrackItem> Items = new List<TrackItem>();
    }
    public class TrackItem
    {
        public Track Track = new Track();
    }
    public class Track
    {
        public Album Album { get; set; }
        public List<Artists> Artists { get; set; }
        public int Duration_ms { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string TrackTime { get; set; }
    }
    public class Album
    {
        public string Name { get; set; }
    }
    public class Artists
    {
        public string Name { get; set; }
    }
}
