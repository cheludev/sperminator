using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Pantalla final que muestra la puntuación al llegar al óvulo.
/// Ponlo en el Canvas de la escena "EndScreen" (o actívalo en la misma escena).
/// </summary>
public class EndScreenUI : MonoBehaviour
{
    [Header("Textos de la pantalla final")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI etsKilledText;
    public TextMeshProUGUI rankText;

    void Start()
    {
        if (GameManager.Instance == null) return;

        int score = GameManager.Instance.Score;
        int killed = GameManager.Instance.ETSKilled;

        titleText.text = "¡Has llegado al óvulo!";
        finalScoreText.text = $"Puntuación final: {score}";
        etsKilledText.text = $"ETS eliminadas: {killed}";
        rankText.text = GetRank(score);
    }

    string GetRank(int score)
    {
        if (score >= 1000) return "Rango: S - Esperma Legendario 🏆";
        if (score >= 700)  return "Rango: A - Esperma Élite 💪";
        if (score >= 400)  return "Rango: B - Buen Trabajo";
        if (score >= 200)  return "Rango: C - Podría ser peor";
        return                    "Rango: D - Suerte la próxima";
    }

    // Llama a este método desde el botón "Jugar de nuevo"
    public void PlayAgain()
    {
        GameManager.Instance.ResetScore();
        SceneManager.LoadScene("Game"); // Nombre correcto de la escena principal
    }

    // Llama a este método desde el botón "Salir"
    public void QuitGame()
    {
        Application.Quit();
    }
}
