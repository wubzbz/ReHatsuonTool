using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReHatsuonTool.Services;

public static class UpdateChecker
{
    private const string ApiUrl = "https://api.github.com/repos/wubzbz/ReHatsuonTool/releases/latest";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    private const int MaxRetries = 3;
    private const string UserAgent = "ReHatsuonTool-UpdateCheck/1.0";

    /// <summary>
    /// Fetch the latest release version from GitHub.
    /// Returns the version string without "v" prefix (e.g. "1.1.0"), or null on failure.
    /// </summary>
    public static async Task<string?> GetLatestVersionAsync(
        HttpClient? injectedClient = null,
        CancellationToken ct = default
        )
    {
        HttpClient client;
        bool ownClient = false;

        if (injectedClient != null)
        {
            client = injectedClient;
        }
        else
        {
            client = new HttpClient { Timeout = Timeout };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
            ownClient = true;
        }

        try
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                Debug.WriteLine($"[UpdateChecker] Attempt {attempt}/{MaxRetries}");

                try
                {
                    using var response = await client.GetAsync(ApiUrl, ct);
                    Debug.WriteLine($"[UpdateChecker] HTTP {response.StatusCode}");

                    response.EnsureSuccessStatusCode();

                    using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct);
                    var tag = doc?.RootElement.GetProperty("tag_name").GetString();
                    if (string.IsNullOrEmpty(tag))
                    {
                        Debug.WriteLine("[UpdateChecker] WARNING: tag_name is missing or empty");
                        return null;
                    }

                    var version = tag.StartsWith('v') ? tag[1..] : tag;
                    Debug.WriteLine($"[UpdateChecker] tag_name: {tag} → version: {version}");
                    return version;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"[UpdateChecker] Timeout (attempt {attempt})");
                    if (attempt < MaxRetries)
                        await Task.Delay(1000 * attempt, ct);
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"[UpdateChecker] HTTP error: {ex.Message} (attempt {attempt})");
                    if (attempt < MaxRetries)
                        await Task.Delay(1000 * attempt, ct);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[UpdateChecker] Unexpected error: {ex.GetType().Name}: {ex.Message}");
                    return null;
                }
            }
        }
        finally
        {
            if (ownClient) client.Dispose();
        }

        Debug.WriteLine("[UpdateChecker] All retries exhausted.");
        return null;
    }
}
