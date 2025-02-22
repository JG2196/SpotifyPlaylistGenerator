using OpenAI.Chat;

namespace BlazorApp1.OpenAIServices
{
    public partial class OpenAIServices
    {
        public ChatClient OpenAIClientService(IConfiguration _Configuration)
        {
            ChatClient? chatClientService = null;

            try
            {
                string chatModel = _Configuration["OpenAI:Model"];
                string chatAPIKey = _Configuration["OpenAI:APIKey"];

                chatClientService = new(model: chatModel, apiKey: chatAPIKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OpenAIClientService Ex: " + ex.Message);
            }

            return chatClientService;
        }
    }
}
