namespace BlazorApp1.Data
{
    //    //// OpenAI Classes
    public class OpenAIPlaylist
    {
        public string Description { get; set; }
        public List<OpenAITrack> Playlist { get; set; }
    }
    public class OpenAITrack
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Spotify_Id { get; set; }
    }
}
