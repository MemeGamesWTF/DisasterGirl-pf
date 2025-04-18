using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> names;
    [SerializeField] private GameManager gameManager;
    public  string displayName;
    void Start()
    {

        if (!PlayerPrefs.HasKey("Sex") && !PlayerPrefs.HasKey("Email") && !PlayerPrefs.HasKey("Name") && !PlayerPrefs.HasKey("parentName"))
        {
            gameManager.WelcomePageGUI.SetActive(true);
        }
        else
        {

           login();
        }
    }
    //private void Awake()
    //{
    //    PlayerPrefs.DeleteAll();
    //}
    public void login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successful login/account create!");

        // Fetch the player's profile data to get the display name
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(),
            profileResult =>
            {
                displayName = profileResult.PlayerProfile.DisplayName;
                Debug.Log("this is the current name" + profileResult.PlayerProfile.DisplayName);
                if (string.IsNullOrEmpty(displayName))
                {
                    // Set default name from PlayerPrefs if display name is empty or null
                    gameManager.WelcomePageGUI.SetActive(true);
                    Debug.Log("if not am already there this is the current name" + PlayerPrefs.GetString("Name"));
                    // You can adjust the default name logic as needed
                }
                Debug.Log("Display name retrieved: " + displayName);

                // Optionally store the display name locally or use it elsewhere in your game

                // Now update the display name locally or handle it as needed
                UpdateDisplayNameLocally(displayName);
            },
            error =>
            {
                Debug.LogError("Error retrieving player profile: " + error.ErrorMessage);
                // Handle error, such as retrying or informing the player
            });

        // Now fetch the leaderboard
        GetLeaderboard();
    }

    void UpdateDisplayNameLocally(string displayName)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Debug.Log("Display name updated successfully: " + displayName);
                gameManager.leaderBoardName.text = "My name is " + displayName;
                // Optionally handle success, such as updating UI or local storage
            },
            error =>
            {
                Debug.LogError("Error updating display name: " + error.ErrorMessage);
                // Handle error, such as retrying or informing the player
            });
    }

    void OnLoginError(PlayFabError error)
    {
        Debug.Log("Error while Logging in/account create!");
        Debug.Log(error.GenerateErrorReport());
    }

    void SetDisplayName()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = "DefaultName"  // Replace with logic to set display name (e.g., UI input)
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameSet, OnError);
    }

    void OnDisplayNameSet(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Display name set successfully: " + result.DisplayName);
        // Optionally store the display name locally or use it elsewhere in your game
    }


    void OnSuccess(LoginResult result)
    {
        Debug.Log("Successful login/account create!");
    }
    void OnError(PlayFabError error)
    {
        Debug.Log("Error while Logging in/account create!");
        Debug.Log(error.GenerateErrorReport());
    }
    public void SendLeaderBoard(int Score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate {
                    StatisticName = "test1",
                    Value = Score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request,OnLeaderboardUpdate,OnError);
        
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfull leaderboard sent");
      
    }
    
    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "test1",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request,OnLeaderboardGet,OnError);
    }
    void OnLeaderboardGet(GetLeaderboardResult result)
    {
       

        for (int i = 0; i < result.Leaderboard.Count; i++)
        {
            if (i < names.Count)
            {
                string playerName = result.Leaderboard[i].Profile.DisplayName;
                names[i].text = playerName;
                Debug.Log($"Updating UI: {i} - {playerName}");
            }
        }
    }

   
    //void OnLeaderboardGet(GetLeaderboardResult result)
    //{
    //    //foreach(var item in result.Leaderboard)
    //    //{
    //    //    Debug.Log(item.Position + " " + item.PlayFabId + " " + item.StatValue);

    //    //}
    //    for (int i = 0; i < result.Leaderboard.Count; i++)
    //    {
    //        Debug.Log("i am Working");
    //        if (i < names.Count)
    //        {
    //            names[i].text = result.Leaderboard[i].Profile.DisplayName;     
    //        }
    //        //Debug.Log(result.Leaderboard[i].Position + " " + result.Leaderboard[i].Profile.DisplayName+ " " + result.Leaderboard[i].StatValue);
    //    }
    //}
}
