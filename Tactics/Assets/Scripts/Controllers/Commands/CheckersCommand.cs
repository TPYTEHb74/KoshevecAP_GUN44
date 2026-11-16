using System.Collections.Generic;
using UnityEngine;

public class CheckersCommand : IGameplayCommand
{
    private readonly Battlefield _battlefield;
    private readonly PlayerController _playerController;

    private Unit _selectedUnit;
    private Cell _targetCell;
    private Unit _enemyToDestroy;
    private Dictionary<Cell, Unit> _validMoves = new Dictionary<Cell, Unit>();
    private bool _isMultiJumpActive;

    public CheckersCommand(Battlefield battlefield, PlayerController playerController)
    {
        _battlefield = battlefield;
        _playerController = playerController;
    }

    public void Interact(Cell cell)
    {
        if (cell == null) return;

        if (!_isMultiJumpActive && cell.CurrentUnit != null && cell.CurrentUnit.ColorType == _playerController.CurrentTurn)
        {
            bool mustAttack = AnyUnitCanAttack(_playerController.CurrentTurn);
            Dictionary<Cell, Unit> moves = GetMovesForUnit(cell.CurrentUnit, mustAttack);

            if (mustAttack && moves.Count == 0)
            {
                Debug.Log("Обязательное взятие! Выберите шашку, которая может срубить врага.");
                return;
            }

            SelectUnit(cell.CurrentUnit, moves);
            return;
        }

        if (_selectedUnit != null && _validMoves.ContainsKey(cell))
        {
            _targetCell = cell;
            _enemyToDestroy = _validMoves[cell];
            Debug.Log($"Выбрана клетка ({cell.X}, {cell.Y}). Нажмите SPACE для подтверждения!");
        }
    }

    private void SelectUnit(Unit unit, Dictionary<Cell, Unit> moves)
    {
        ClearHighlights();
        _selectedUnit = unit;
        _selectedUnit.CurrentCell.SetSelected(true);
        _targetCell = null;
        _enemyToDestroy = null;
        _validMoves = moves;

        foreach (Cell moveCell in _validMoves.Keys)
        {
            moveCell.SetAvailableForMove(true);
        }

        Debug.Log($"Выбрана шашка на ({unit.CurrentCell.X}, {unit.CurrentCell.Y}). Доступно ходов: {_validMoves.Count}");
    }

    private bool AnyUnitCanAttack(PlayerColor color)
    {
        for (int x = 0; x != 8; x++)
        {
            for (int y = 0; y != 8; y++)
            {
                Cell c = _battlefield.GetCell(x, y);
                if (c != null && c.CurrentUnit != null && c.CurrentUnit.ColorType == color)
                {
                    if (GetMovesForUnit(c.CurrentUnit, true).Count != 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private Dictionary<Cell, Unit> GetMovesForUnit(Unit unit, bool onlyAttacks)
    {
        Dictionary<Cell, Unit> attacks = new Dictionary<Cell, Unit>();
        Dictionary<Cell, Unit> regularMoves = new Dictionary<Cell, Unit>();

        int startX = unit.CurrentCell.X;
        int startY = unit.CurrentCell.Y;
        int forwardDir = (unit.ColorType == PlayerColor.White) ? 1 : -1;

        int[] dirX = { -1, -1, 1, 1 };
        int[] dirY = { -1, 1, -1, 1 };

        for (int i = 0; i != 4; i++)
        {
            int dx = dirX[i];
            int dy = dirY[i];

            if (unit.Type == UnitType.Man)
            {
                Cell stepCell = _battlefield.GetCell(startX + dx, startY + dy);
                if (stepCell != null && stepCell.CurrentUnit == null && dx == forwardDir)
                {
                    regularMoves[stepCell] = null;
                }

                if (stepCell != null && stepCell.CurrentUnit != null && stepCell.CurrentUnit.ColorType != unit.ColorType)
                {
                    Cell jumpCell = _battlefield.GetCell(startX + dx * 2, startY + dy * 2);
                    if (jumpCell != null && jumpCell.CurrentUnit == null)
                    {
                        attacks[jumpCell] = stepCell.CurrentUnit;
                    }
                }
            }
            else
            {
                Unit metEnemy = null;
                for (int dist = 1; dist != 8; dist++)
                {
                    Cell checkCell = _battlefield.GetCell(startX + dx * dist, startY + dy * dist);
                    if (checkCell == null) break;

                    if (checkCell.CurrentUnit == null)
                    {
                        if (metEnemy != null)
                            attacks[checkCell] = metEnemy;
                        else
                            regularMoves[checkCell] = null;
                    }
                    else if (checkCell.CurrentUnit.ColorType == unit.ColorType)
                    {
                        break;
                    }
                    else
                    {
                        if (metEnemy != null) break;
                        metEnemy = checkCell.CurrentUnit;
                    }
                }
            }
        }

        if (onlyAttacks || attacks.Count != 0)
        {
            return attacks;
        }

        return regularMoves;
    }

    public bool Confirm()
    {
        if (_selectedUnit == null || _targetCell == null)
        {
            Debug.Log("Сначала выберите шашку и зеленую клетку для хода!");
            return false;
        }

        ClearHighlights();

        bool wasAttack = (_enemyToDestroy != null);
        if (wasAttack)
        {
            _enemyToDestroy.CurrentCell.CurrentUnit = null;
            Object.Destroy(_enemyToDestroy.gameObject);
            Debug.Log("Вражеская шашка сбита!");
        }

        _selectedUnit.CurrentCell.CurrentUnit = null;
        _targetCell.CurrentUnit = _selectedUnit;
        _selectedUnit.CurrentCell = _targetCell;

        if (_selectedUnit.Type == UnitType.Man)
        {
            if ((_selectedUnit.ColorType == PlayerColor.White && _targetCell.X == 7) ||
                (_selectedUnit.ColorType == PlayerColor.Black && _targetCell.X == 0))
            {
                _selectedUnit.PromoteToKing();
                Debug.Log("Шашка стала дамкой!");
            }
        }

        Vector3 endPos = _targetCell.transform.position + Vector3.up * 0.1f;

        if (wasAttack)
        {
            Dictionary<Cell, Unit> nextAttacks = GetMovesForUnit(_selectedUnit, true);
            if (nextAttacks.Count != 0)
            {
                _isMultiJumpActive = true;
                _playerController.MoveUnit(_selectedUnit, endPos, true);
                SelectUnit(_selectedUnit, nextAttacks);
                return true;
            }
        }

        _isMultiJumpActive = false;
        _playerController.MoveUnit(_selectedUnit, endPos, false);
        ResetCommand();
        return true;
    }

    public void Cancel()
    {
        if (_isMultiJumpActive)
        {
            Debug.Log("Нельзя отменить ход в середине серии атак!");
            return;
        }

        ResetCommand();
        Debug.Log("Выбор отменен.");
    }

    public void ResetCommand()
    {
        ClearHighlights();
        _selectedUnit = null;
        _targetCell = null;
        _enemyToDestroy = null;
        _validMoves.Clear();
    }

    private void ClearHighlights()
    {
        if (_selectedUnit != null && _selectedUnit.CurrentCell != null)
        {
            _selectedUnit.CurrentCell.SetSelected(false);
        }

        foreach (Cell cell in _validMoves.Keys)
        {
            cell.SetAvailableForMove(false);
        }
    }
}
