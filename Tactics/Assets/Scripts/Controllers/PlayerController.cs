using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _turnText;

    public PlayerColor CurrentTurn { get; private set; } = PlayerColor.White;
    public bool IsBusy { get; private set; }

    private void Start()
    {
        UpdateTurnUI(false);
    }

    public void MoveUnit(Unit unit, Vector3 targetPosition, bool keepTurn = false)
    {
        StartCoroutine(MoveRoutine(unit, targetPosition, keepTurn));
    }

    private IEnumerator MoveRoutine(Unit unit, Vector3 targetPosition, bool keepTurn)
    {
        IsBusy = true;
        Vector3 startPos = unit.transform.position;
        float elapsed = 0f;
        float duration = 0.25f;

        while (!Mathf.Approximately(elapsed, duration))
        {
            elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
            float t = elapsed / duration;
            unit.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        unit.transform.position = targetPosition;
        IsBusy = false;

        if (keepTurn)
        {
            UpdateTurnUI(true);
            Debug.Log("Серия атак! Продолжайте рубить этой же шашкой.");
        }
        else
        {
            SwitchTurn();
        }
    }

    private void SwitchTurn()
    {
        PlayerColor nextPlayer = (CurrentTurn == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;

        if (!HasRemainingUnits(nextPlayer))
        {
            DeclareWinner(CurrentTurn);
            return;
        }

        CurrentTurn = nextPlayer;
        UpdateTurnUI(false);
        Debug.Log($"Ход переходит к игроку: {CurrentTurn}");
    }

    private bool HasRemainingUnits(PlayerColor color)
    {
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit unit in allUnits)
        {
            if (unit.ColorType == color)
            {
                return true;
            }
        }
        return false;
    }

    private void DeclareWinner(PlayerColor winner)
    {
        IsBusy = true;
        string winnerName = (winner == PlayerColor.White) ? "БЕЛЫЕ" : "ЧЕРНЫЕ";
        if (_turnText != null)
        {
            _turnText.text = $"ИГРА ОКОНЧЕНА! ПОБЕДИЛИ {winnerName}!";
        }
        Debug.Log($"Игра окончена! Победили: {winnerName}");
    }

    private void UpdateTurnUI(bool isCombo)
    {
        if (_turnText != null)
        {
            string colorName = (CurrentTurn == PlayerColor.White) ? "БЕЛЫХ" : "ЧЕРНЫХ";
            if (isCombo)
            {
                _turnText.text = $"СЕРИЯ АТАК ({colorName})!\nВыберите следующую цель и нажмите SPACE";
            }
            else
            {
                _turnText.text = $"Сейчас ход: {colorName}\n(ЛКМ - выбор, SPACE - ход, ESC - отмена)";
            }
        }
    }
}
