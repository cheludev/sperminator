using UnityEngine;
using TMPro;

/// <summary>
/// Muestra la puntuación en un Canvas de tipo World Space o Screen Space.
/// Añade este script al TextMeshPro de puntuación en el HUD.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("Referencia al texto")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI etsKilledText;

    void Update()
    {
        if (GameManager.Instance == null) return;

        scoreText.text = $"Puntos: {GameManager.Instance.Score}";

        if (etsKilledText != null)
            etsKilledText.text = $"ETS eliminadas: {GameManager.Instance.ETSKilled}";
    }
}
