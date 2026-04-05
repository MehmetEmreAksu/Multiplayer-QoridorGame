using UnityEngine;
using TMPro; // TextMeshPro kullanmak için þart
using UnityEngine.SceneManagement; // Sahneler arasý geçiþ ve Restart için
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Oyun Ýçi UI (HUD)")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI p1WallText;
    [SerializeField] private TextMeshProUGUI p2WallText;

    [Header("Oyun Sonu (Game Over)")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI winnerText;

    private Coroutine turnFadeCoroutine;
    // Oyun baþlar baþlamaz Game Over panelini gizle
    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void UpdateTurnText(int currentPlayer){ 

        turnText.text = $"Player {currentPlayer}'s Turn";
        if (turnFadeCoroutine != null)
        {
            StopCoroutine(turnFadeCoroutine);
        }
        turnFadeCoroutine = StartCoroutine(FadeOutTurnText());
    }

    IEnumerator FadeOutTurnText()
    {
        CanvasGroup cg = turnText.GetComponent<CanvasGroup>();

        // 1. Önce yazýyý anýnda görünür yap (Alpha = 1)
        cg.alpha = 1f;

        // 2. Oyuncu okusun diye 1.5 saniye ekranda tam net kalsýn
        yield return new WaitForSeconds(0.75f);

        // 3. Yavaþça silinme (Fade Out) animasyonu - 1 saniye sürecek
        float fadeDuration = 1f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Lerp ile 1'den 0'a yumuþak geçiþ
            cg.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null; // Bir sonraki frame'i bekle
        }

        // 4. Emin olmak için tamamen görünmez yap
        cg.alpha = 0f;
    }

    public void UpdateWallCount(int p1Count, int p2Count)
    {
        p1WallText.text = "Duvar: " + p1Count.ToString();
        p2WallText.text = "Duvar: " + p2Count.ToString();
    }
    public void ShowGameOver(int winnerPlayer)
    {
        if (gameOverPanel != null && winnerText != null)
        {
            gameOverPanel.SetActive(true);
            winnerText.text = $"Player {winnerPlayer} Wins!";
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f; // Oyun hýzýný normal seviyeye getir
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}
