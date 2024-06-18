using Microsoft.AspNetCore.Http; // For session extensions
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniWeb.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Domain { get; set; } = string.Empty;

        private readonly HttpClient _httpClient;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(HttpClient httpClient, ILogger<LoginModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var loginData = new
            {
                Username,
                Password,
                Domain
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.mint.devbuildbase.com/api/auth/login", loginData);

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody); // Log the response

            if (response.IsSuccessStatusCode)
            {
                // Parse the response to extract the access token
                var jsonDocument = JsonDocument.Parse(responseBody);
                var accessToken = jsonDocument.RootElement.GetProperty("accessToken").GetString();

                if (accessToken == null)
                {
                    // Handle the case where accessToken is null
                    _logger.LogError("Access token is null");
                    ModelState.AddModelError(string.Empty, "Failed to retrieve access token. Please try again.");
                    return Page();
                }

                // Store the access token in session
                HttpContext.Session.SetString("AccessToken", accessToken);
                _logger.LogInformation("Access token stored in session");

                // Verify that the access token is stored
                var storedAccessToken = HttpContext.Session.GetString("AccessToken");
                if (storedAccessToken != null)
                {
                    _logger.LogInformation("Stored access token: " + storedAccessToken);
                }
                else
                {
                    _logger.LogError("Failed to store access token in session");
                }

                // Authentication successful
                return RedirectToPage("/Profile");
            }
            else
            {
                // Handle authentication failure
                ModelState.AddModelError(string.Empty, "Login failed. Please check your credentials and try again.");
                return Page();
            }
        }
    }
}
