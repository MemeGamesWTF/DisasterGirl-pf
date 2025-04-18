using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
public class GameManager : MonoBehaviour
{

    [SerializeField] private Slider slider; // Assign your slider in the inspector

     public TextMeshProUGUI scoreText; // TMP text component for the score
    private int score = 0;
    private int finalScore = 0;
    [SerializeField] private int foodsTapped = 0;
    [SerializeField] private bool isSpawning = true; // Flag to control spawning

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private int MaximumToFillMyStomach = 30;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float minSpawnInterval;
    [SerializeField] private float spawnSpeedIncreaseRate;

    private GameObject currentPrefabInstance; // Track the current spawned prefab instance
    [SerializeField] private Animator catAnim;
    [SerializeField] private GameObject catHappy;
    //[SerializeField] private GameObject catHunger;
    //[SerializeField] private GameObject catEating;
    [SerializeField] private TextMeshProUGUI emotions;
    [SerializeField] private AudioClip[] catSounds;
    [SerializeField] private AudioSource subAudioSource;
    private bool countdownStarted = false;
    private float countdownDuration = 2 * 60 * 60; // 2 hours in seconds
    private float countdownTimer;
    private float gameStartTime;
    private int currentSegment = -1;
    int GameInCount;
    private GameObject[] currentPrefabInstances = new GameObject[1];
    [SerializeField] private PlayFabManager playFabManager;
    [SerializeField] public TextMeshProUGUI leaderBoardName;
    [Header("Page Variable")]
    [SerializeField] private GameObject leaderBoardGUI;
    [SerializeField] private GameObject BuymeaCoffeeGUI;
    [SerializeField] public GameObject WelcomePageGUI;
    [SerializeField] private GameObject IamFullPage;

    [Header("WelcomePage Variable")]
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private Toggle SexToggle;

    [SerializeField] private TextMeshProUGUI parentname;
    [SerializeField] private TextMeshProUGUI email;

    [Header("Countdown Variable")]
    [SerializeField] private TextMeshProUGUI CountDwonTimerGUI;

    [Header("Email Information")]
    string url = "https://script.google.com/macros/s/AKfycbx3V6rLFu3AafRg3NIA52Iopj8V3kcAqVvI0Teg0tRHXxL_KAMC6mmaH-noqhFSHFzTQg/exec";

    string subject = "Meows HuNgryy....Mmmmzz";
    string body = "Play FeedtheMeow - Meows hunger Pooky PurrrrZ...";
    long timestamp;

    [DllImport("__Internal")]
  private static extern void SendScore(int score, int game);


    //private const string GAS_URL = "https://script.google.com/macros/s/AKfycbylOsKvJ5DYAg3UIu6rX1AHfT9EQvVuW3lSswmlIasWFb7tCgQJ6QqaKf4MysLBWK3s5A/exec";



    private float spawnSpeedIncreaseThreshold = 0.6f;
    private void Awake()
    {



    }



    IEnumerator SendEmailRequest()
    {
        string requestUrl = $"{url}?recipient={PlayerPrefs.GetString("Email")}&subject={subject}&body={body}&timestamp={timestamp}";

        using (UnityWebRequest www = UnityWebRequest.Get(requestUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                if (response.Contains("Sheet Updated"))
                {
                    Debug.Log("Email sent successfully");
                }
                else
                {
                    Debug.Log("Failed to send email");
                }
            }
        }
    }
    void Start()
    {
        StartCoroutine(SpawnRoutine());

        //StartCoroutine(SendEmailRequest());
    }
    void Update()
    {
        OnSliderValueChanged();
        // Check if slider value passes the spawn speed increase threshold
        if (isSpawning && slider.value > spawnSpeedIncreaseThreshold)
        {
            // Increase spawn speed by reducing spawn interval
            if (spawnInterval > minSpawnInterval)
            {
                spawnInterval -= spawnSpeedIncreaseRate * Time.deltaTime; // Adjust spawn interval over time
            }
        }
        // Check if spawning is false and countdown has not started
        if (!isSpawning && !countdownStarted)
        {
            // Start the countdown
            countdownStarted = true;
            gameStartTime = Time.time;
        }

        // Check if countdown has started
        if (countdownStarted)
        {
            // Calculate remaining time
            float timeRemaining = countdownDuration - (Time.time - gameStartTime);
            int hours = Mathf.FloorToInt(timeRemaining / 3600);
            int minutes = Mathf.FloorToInt((timeRemaining - hours * 3600) / 60);
            int seconds = Mathf.FloorToInt(timeRemaining - hours * 3600 - minutes * 60);

            // Format the time into a string in the format "hh:mm:ss"
            string timerText = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");

            // Update GUI text
            CountDwonTimerGUI.text = timerText;

            // Update slider value based on countdown progress
            slider.value = timeRemaining / countdownDuration;

            // Check if countdown is complete
            if (timeRemaining <= 0)
            {
                countdownStarted = false;
                ResetGame();
            }
        }
        if (isSpawning && Input.GetMouseButtonDown(0)) // Assuming left mouse button or touch input
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Item"))
            {
                foodsTapped++;
                // Update slider value based on foodsTapped
                slider.value = (float)foodsTapped / MaximumToFillMyStomach;
                catAnim.SetBool("isEating", true);
                 score += 5; // Increase score by 5
                UpdateScoreText();
               
                //catHunger.SetActive(false);
                //catEating.SetActive(true);
                emotions.text = "Eating";
                if (foodsTapped > MaximumToFillMyStomach)
                {
                    // Slider is full, stop spawning
                    isSpawning = false;

                    //catHappy.SetActive(true);
                    //catHunger.SetActive(false);
                    //catEating.SetActive(false);
                    GameInCount = 30;
                    SendScore();
                    DestroyAllItems();


                    StopCoroutine(SpawnRoutine()); // Stop spawning coroutine
                }

                Destroy(hit.collider.gameObject);
            }// Handle clicks on poison
            else if (hit.collider.gameObject.CompareTag("Poison"))
            {
                float reduceAmount = 0.1f; // Define how much the slider should decrease
                slider.value = Mathf.Max(0, slider.value - reduceAmount); // Ensure slider value does not go below 0
                emotions.text = "Poisoned!";
                score -= 5; // Decrease score by 5
                UpdateScoreText();
                 
                catAnim.SetBool("isEating", false);

                Destroy(hit.collider.gameObject);
            }
        }
    }

     private void UpdateScoreText()
    {
        scoreText.text = " " + score; // Update the text with the current score
    }

    void OnSliderValueChanged()
    {

        // Calculate which segment the slider value falls into
        int newSegment = Mathf.FloorToInt(slider.value / (1f / 6f));

        // If the segment has changed, print the corresponding message
        if (newSegment != currentSegment)
        {
            currentSegment = newSegment;
            PrintSegmentMessage(currentSegment);
        }
    }

    void PrintSegmentMessage(int segment)
    {

        switch (segment)
        {
            case 0:

                subAudioSource.clip = catSounds[0];
                // subAudioSource.Play();
                break;
            case 1:

                subAudioSource.clip = catSounds[1];
                // subAudioSource.Play();

                break;
            case 2:

                subAudioSource.clip = catSounds[2];
                // subAudioSource.Play();

                break;
            case 3:

                subAudioSource.clip = catSounds[3];
                // subAudioSource.Play();

                break;
            case 4:

                subAudioSource.clip = catSounds[4];
                // subAudioSource.Play();

                break;
            case 5:

                subAudioSource.clip = catSounds[5];
                // subAudioSource.Play();

                break;
            default:

                subAudioSource.clip = catSounds[5];
                // subAudioSource.Play();

                break;
        }
    }
    IEnumerator DelayEndMenue()
    {
        yield return new WaitForSeconds(1);
        IamFullPage.SetActive(true);
    }
    public void WelcomePageSubmit()
    {
        // Assuming username, parentname, and email are your input fields
        string playerName = username.text;
        string parentName = parentname.text;
        string emailValue = email.text;

        // Set PlayerPrefs for PlayerData
        PlayerPrefs.SetString("Name", playerName);
        PlayerPrefs.SetInt("Sex", SexToggle.isOn ? 0 : 1); // 0 for Male, 1 for Female
        PlayerPrefs.SetString("Parent", parentName);
        PlayerPrefs.SetString("Email", emailValue);
        playFabManager.displayName = playerName;
        // Hide the welcome page GUI
        WelcomePageGUI.SetActive(false);



        playFabManager.login();
    }
    private void SendScore()
    {
        StartCoroutine(DelayEndMenue());
        if (GameInCount == 30)
        {
            if (!PlayerPrefs.HasKey("Name") || PlayerPrefs.GetString("Name") == null)
            {
                PlayerPrefs.SetString("Name", SystemInfo.deviceUniqueIdentifier);
            }
            long epochTime = (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
            timestamp = epochTime;
            int saveScore = PlayerPrefs.GetInt("Score") + GameInCount;
            PlayerPrefs.SetInt("Score", saveScore);
            Debug.Log("Game done");
            Debug.Log("Score: " + score);
            SendScore(score, 21);
            playFabManager.SendLeaderBoard(GameInCount);
            GameInCount = 0;

            // Get current epoch time in seconds
            StartCoroutine(SendEmailRequest());
        }
    }

    void ResetGame()
    {
        isSpawning = true;
        slider.value = 0f;
        StartCoroutine(SpawnRoutine());
        emotions.text = "Hungry";
        IamFullPage.SetActive(false);
        catHappy.SetActive(false);
        catAnim.SetBool("isEating", false);

        //catHunger.SetActive(true);
        //catEating.SetActive(false);
        // Additional reset logic if needed
    }
public GameObject instructionPanel;
    public void InstructionActive(bool value)
    {
        instructionPanel.SetActive(value);
    }
    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (EmptyItems())
            {
                SpawnPrefabs();
            }

            yield return new WaitForSeconds(spawnInterval);

            if (spawnInterval > minSpawnInterval)
            {
                spawnInterval -= spawnSpeedIncreaseRate;
            }
        }
    }

    void SpawnPrefabs()
    {
        int spawnCount = Mathf.Min(prefabs.Length, spawnPoints.Length); // Limit to the smaller of prefabs or spawn points
        List<Transform> randomSpawnPoints = GetRandomSpawnPoints(spawnCount); // Get random spawn points

        emotions.text = "Hunger";
        catAnim.SetBool("isEating", false);

        DestroyAllItems(); // Clear existing items if necessary

        // Start coroutine to spawn items one by one
        StartCoroutine(SpawnItemsIndividually(randomSpawnPoints));
    }

    IEnumerator SpawnItemsIndividually(List<Transform> spawnPoints)
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject randomPrefab = GetRandomPrefab();
            if (randomPrefab != null)
            {
                GameObject instance = Instantiate(randomPrefab, spawnPoint.position, Quaternion.identity);
                StartCoroutine(DestroymeAftersometime(instance)); // Automatically destroy after some time
            }

            // Delay between spawning items
            yield return new WaitForSeconds(spawnInterval / spawnPoints.Count);
        }
    }


    IEnumerator DestroymeAftersometime(GameObject gameObject)
    {
        yield return new WaitForSeconds(spawnInterval);
        Destroy(gameObject);
    }
    void DestroyAllItems()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
    }

    bool EmptyItems()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        return items.Length == 0;
    }

    List<Transform> GetRandomSpawnPoints(int count)
    {
        List<Transform> randomPoints = new List<Transform>();
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, availablePoints.Count);
            randomPoints.Add(availablePoints[index]);
            availablePoints.RemoveAt(index);
        }

        return randomPoints;
    }

    GameObject GetRandomPrefab()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("No prefabs assigned.");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);
        return prefabs[randomIndex];
    }
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OpenLeaderBoard()
    {
        leaderBoardGUI.SetActive(true);
    }
    public void CloseLeaderBoard()
    {
        leaderBoardGUI.SetActive(false);
    }

    public void OpenBuyMeCoffee()
    {
        BuymeaCoffeeGUI.SetActive(true);
    }
    public void CloseBuyMeCoffee()
    {

    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int sex;
    public string parentName;
    public string email;
}