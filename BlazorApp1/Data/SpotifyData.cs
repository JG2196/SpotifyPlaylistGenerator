namespace BlazorApp1.Data
{
    public class SpotifyAuthUserData
    {
        public SpotifyAuthUser SpotifyAuthUser { get; set; }
        public List<SpotifyPlaylist> ListSpotifyPlaylists { get; set; }
    }
    public class SpotifyAuthUser
    {
        public string DisplayName { get; set; }
        public string SpotifyID { get; set; }
    }
    public class SpotifyPlaylists
    {
        public string Next { get; set; }
        public List<SpotifyPlaylist> Items { get; set; } = new List<SpotifyPlaylist>();
    }
    public class SpotifyPlaylist
    {
        public string Description { get; set; }
        public string Id { get; set; }
        public List<PlaylistImage> Images { get; set; }
        public string Name { get; set; }
        public PlaylistTracks Tracks { get; set; }
        public Urls External_Urls { get; set; }
        public string PlaylistDuration { get; set; } = string.Empty;
    }
    public class Urls { 
        public string Spotify { get; set; }
    }
    public class PlaylistImage
    {
        public string Url { get; set; }
    }
    public class PlaylistTracks
    {
        public string Next { get; set; }
        public List<TrackItem> Items { get; set; } = new List<TrackItem>();
    }
    public class TrackItem
    {
        public Track Track { get; set; } = new Track();
        public string Id { get; set; }
    }
    public class Track
    {
        public Album Album { get; set; }
        public List<Artists> Artists { get; set; }
        public int Duration_ms { get; set; }
        public string Name { get; set; }
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
