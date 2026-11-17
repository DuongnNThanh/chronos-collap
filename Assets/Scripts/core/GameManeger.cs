using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Danh sách tên Scene theo đúng thứ tự")]
    public string[] levelSceneNames = { "Man_1", "Man_2", "Man_3", "Man_4", "Man_5" };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Đồng đội chỉ cần gọi hàm này ở sự kiện onPuzzleSolved của câu đố cuối màn
    public void CompleteCurrentLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(levelSceneNames, currentScene);

        if (currentIndex >= 0)
        {
            // Lưu tiến độ màn cao nhất đã mở khóa (mặc định mở sẵn màn 1)
            int nextLevelNumber = currentIndex + 2;
            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (nextLevelNumber > unlockedLevel)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextLevelNumber);
                PlayerPrefs.Save();
            }

            // Chuyển sang màn tiếp theo hoặc quay về MainMenu nếu xong màn 5
            if (currentIndex + 1 < levelSceneNames.Length)
            {
                LoadLevel(levelSceneNames[currentIndex + 1]);
            }
            else
            {
                LoadLevel("MainMenu");
            }
        }
    }

    public void LoadLevel(string sceneName)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAllItems();
        }
        SceneManager.LoadScene(sceneName);
    }
}