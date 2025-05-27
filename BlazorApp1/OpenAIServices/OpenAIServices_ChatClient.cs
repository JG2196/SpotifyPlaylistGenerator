using Microsoft.JSInterop;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorApp1.OpenAIServices
{
    public partial class OpenAIServices
    {
        public IConfiguration _Configuration;

        // Try using a Verbatim String "@"
        private readonly string AssistantContent = "Create a playlist that aligns with the user's needs based on their input. The assistant should be friendly with a laid-back, 80's music connoisseur vibe. "
        //+ "Understand the user's preferences, such as genre, mood, occasion, and any specific artists or songs mentioned. Use this information to curate a list of songs that best match their requirements. Generate a playlist in JSON format with 'title', 'artist', and 'spotify_id' fields. 'spotify_id' must be an existing Spotify URI using Spotify’s website."
        + "Understand the user's preferences, such as genre, mood, occasion, and any specific artists or songs mentioned. Use this information to curate a list of songs that best match their requirements. Generate a playlist in JSON format with 'title' and 'artist' fields."
        + "Each track must be available on Spotify."
        + "Add the playlist description 'description' to the JSON"
        + "JSON format"
        + "{'description': 'abc',"
        + "'playlist': ["
        + "{"
        + "'title': 'abc',"
        + "'artist': 'abc',"
        //+ "'spotify_id': 'abc'"
        + "}, ]"
        + "}"
        + "# Steps"
        + "1. **Extract Preferences**: Identify key elements from the user's input, such as preferred genres, mood, occasion, tone, and any specific artists or songs."
        + "2. **Curate Playlist**: Use the extracted preferences to select a range of songs that align with the user's needs."
        + "3. **Ensure Diversity**: Include a variety of songs within the specified genre or mood to maintain the playlist's interest."
        + "# Output Format"
        //+ "Provide the playlist as a numbered list of song titles along with their respective artists, there should also be a short description of the playlist for the user. Each entry should be formatted as: 'Song Title - Artist Name - Spotify ID'."
        + "Provide the playlist as a numbered list of song titles along with their respective artists, there should also be a short description of the playlist for the user. Each entry should be formatted as: 'Song Title - Artist Name'."
        + "# Examples"
        + "**User Input**: 'I need a relaxing playlist for studying, with some jazz and acoustic tracks.'"
        + "**Playlist Output**:"
        + "1. 'Autumn Leaves - Chet Baker'"
        + "2. 'Norwegian Wood - The Beatles'"
        + "3. 'Take Five - Dave Brubeck'"
        + "4. 'Blackbird - The Beatles'"
        + "(Actual playlists created should include a minimum of 20 songs to ensure sufficient variety, and a maximum of 25 songs.)"
        + "# Notes"
        + "- Ensure the playlist is tailored as closely as possible to the provided preferences."
        + "- Handle vague or broad inputs by providing a balanced playlist across suggested elements."
        + "- Consider mentioning popular or critically acclaimed tracks within the specified genres."
        + "- If the user's request is unsuccessful, politely ask them to build on their request.";

        public OpenAIServices(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public async Task<string> OpenAISubmitQuery(string prompt)
        {
            string? resultString = string.Empty;
            try
            {
                string endpoint = "https://api.openai.com/v1/chat/completions";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_Configuration["OpenAI:APIKey"]}");

                var requestBody = new
                {
                    model = "gpt-4o-mini",  // or "gpt-3.5-turbo"
                    messages = new[]
                    {
                new { role = "system", content = AssistantContent },
                new { role = "user", content = prompt }
            }
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseString);

                resultString = OpenAIExtractJsonFromResponse(result.choices[0].message.content.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("OpenAISubmitQuery ex: " + ex.Message);
            }
            return resultString;
        }

        // OpenAi JSON response contains ```
        // Convert to readable string
        public static string OpenAIExtractJsonFromResponse(string response)
        {
            // Try to find a JSON block inside triple backticks (```json ... ```)
            var match = Regex.Match(response, @"```(?:json)?\s*(\{.*?\})\s*```", RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Fallback: Try to match any JSON object in the string
            match = Regex.Match(response, @"(\{.*\})", RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // If no match is found, return original string (may cause deserialize errors)
            return response;
        }
    }
}
