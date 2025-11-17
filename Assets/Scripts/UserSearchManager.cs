using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;   // ✅ Make sure Newtonsoft.Json is installed

// -------------------- DATA CLASSES --------------------

[System.Serializable]
public class Bio
{
    [JsonProperty("Bio")]
    public string BioText { get; set; }

    [JsonProperty("Avatar_Url")]
    public string Avatar_Url { get; set; }

    [JsonProperty("AvatarUrl")]
    public string AvatarUrl { get; set; }

    [JsonProperty("Skills")]
    public List<string> Skills { get; set; }

    [JsonProperty("Onboarding_Status")]
    public string Onboarding_Status { get; set; }

    [JsonProperty("Creator_Level")]
    public string Creator_Level { get; set; }

    [JsonProperty("Creator_Ratings")]
    public int Creator_Ratings { get; set; }
}

[System.Serializable]
public class User
{
    [JsonProperty("id")]
    public string id { get; set; }

    [JsonProperty("username")]
    public string username { get; set; }

    [JsonProperty("email")]
    public string email { get; set; }

    [JsonProperty("github_username")]
    public string github_username { get; set; }

    [JsonProperty("bio")]
    public Bio bio { get; set; }
}

[System.Serializable]
public class ApiResponse
{
    [JsonProperty("results")]
    public List<User> results { get; set; }
}

// -------------------- MAIN SCRIPT --------------------

public class UserSearchManager : MonoBehaviour
{
    [Header("Search Settings")]
    public string query = "nirupam";
    public int limit = 10;

    void Start()
    {
        StartCoroutine(FetchUsers(query, limit));
    }

    IEnumerator FetchUsers(string query, int limit)
    {
        string url = $"https://cbackenddev.guruvrmetaversity.com/users/search-users?query={query}&limit={limit}";
        Debug.Log("🌐 Sending GET request to: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("❌ Network Error: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;

                try
                {
                    ApiResponse response = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                    if (response != null && response.results != null)
                    {
                        Debug.Log($"✅ Found {response.results.Count} users.\n");

                        foreach (var user in response.results)
                        {
                            string avatar = user.bio?.Avatar_Url ?? user.bio?.AvatarUrl ?? "No avatar";
                            string bioInfo = user.bio != null
                                ? $"Level: {user.bio.Creator_Level}, Rating: {user.bio.Creator_Ratings}"
                                : "No bio info";

                            Debug.Log(
                                $"👤 Username: {user.username}\n" +
                                $"📧 Email: {user.email}\n" +
                                $"📸 Avatar: {avatar}\n" +
                                $"{bioInfo}\n" +
                                "----------------------------------------"
                            );
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ No users found in the response.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("❌ JSON Parse Error: " + ex.Message);
                }
            }
        }
    }
}
