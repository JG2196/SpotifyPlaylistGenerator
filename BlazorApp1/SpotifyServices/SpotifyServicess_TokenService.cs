using Microsoft.AspNetCore.DataProtection;

namespace BlazorApp1.SpotifyServices
{
    public class SpotifyServicess_TokenService
    {
        private readonly IDataProtector _protector;
        private string _accessToken;
        private DateTime _expiresAt;



        // Simulated DB or secure store
        private string _protectedRefreshToken;

        public SpotifyServicess_TokenService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("SpotifyTokenProtector");
        }

        public string AccessToken => _accessToken;
        public bool IsAccessTokenExpired => DateTime.UtcNow >= _expiresAt;

        public void SetTokens(string accessToken, int expiresInSeconds, string refreshToken)
        {
            _accessToken = accessToken;
            _expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);

            if (!string.IsNullOrEmpty(refreshToken))
            {
                _protectedRefreshToken = _protector.Protect(refreshToken);
            }
        }

        public string? GetRefreshToken()
        {
            return _protectedRefreshToken is null ? null : _protector.Unprotect(_protectedRefreshToken);
        }

        public void ClearTokens()
        {
            _accessToken = null;
            _protectedRefreshToken = null;
        }
    }
}
