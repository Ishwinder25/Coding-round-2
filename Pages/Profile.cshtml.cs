using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MiniWeb.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(HttpClient httpClient, ILogger<ProfileModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            ErrorMessage = string.Empty;
        }

        [BindProperty(SupportsGet = true)]
        public string? Username { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Domain { get; set; }

        public UserProfile? UserProfile { get; set; }
        public string ErrorMessage { get; set; }
        public string PictureUrl { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            // Retrieve the access token from session
            var accessToken = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                ErrorMessage = "Access token is missing. Please log in again.";
                return Page();
            }

            // Set up the request with the access token
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.mint.devbuildbase.com/api/selfprofile");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response: " + responseBody);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    UserProfile = JsonSerializer.Deserialize<UserProfile>(responseBody, options);
                    _logger.LogInformation("UserProfile: {@UserProfile}", UserProfile);
                    
                    // Store the value of Username from UserProfile to the class variable
                    if (UserProfile != null)
                    {
                        Username = UserProfile.UserName;
                        _logger.LogInformation($"Updated Username: {Username}");
                    }
                }
                else
                {
                    ErrorMessage = "Failed to retrieve user profile. Please try again.";
                    _logger.LogError("API call failed: " + responseBody);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while retrieving the user profile.";
                _logger.LogError(ex, "Exception occurred while calling API");
            }

            return Page();
        }
    }

    public class UserProfile
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("clientName")]
        public string ClientName { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public string Picture { get; set; } = string.Empty;
    }
}
