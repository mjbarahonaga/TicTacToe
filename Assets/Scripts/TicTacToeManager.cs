using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState 
{ 
	none = -2,
	tie = -1,
	winsHuman = 0,
	winsAi = 1
}
public enum TicTacToeFigure{none, cross, circle}
public enum TicTacToePlayer { none, human, AI}
[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeManager : MonoBehaviour
{

	int _aiLevel;

	TicTacToeFigure[,] boardState = new TicTacToeFigure[3,3];

	private bool _isPlayerTurn;
	public bool IsPlayerTurn { get => _isPlayerTurn; set => _isPlayerTurn = value; }

	[SerializeField]
	private int _gridSize = 3;
	
	[SerializeField]
	private TicTacToeFigure humanFigure = TicTacToeFigure.cross;
	TicTacToeFigure aiFigure = TicTacToeFigure.circle;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	ClickTrigger[,] _triggers;
	
	private List<GameObject> VisualObjects = new List<GameObject>();

	private void OnValidate()
	{
		Utils.ValidationUtility.SafeOnValidate(() =>
		{
			if (this == null) return;

			if(humanFigure == TicTacToeFigure.cross)
            {
				_isPlayerTurn = true;
				aiFigure = TicTacToeFigure.circle;
            }
            else
            {
				_isPlayerTurn = false;
				aiFigure = TicTacToeFigure.cross;
			}

		});
	}

	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

	public void StartAI(int AILevel){
		
		ClearListVisualObjects();
		ResetBoardState();

		_aiLevel = AILevel;
		StartGame();
		
	}

    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		_triggers = new ClickTrigger[3,3];
		onGameStarted.Invoke();
		if (IsPlayerTurn == false) { StartCoroutine(TurnIA()); }
	}

	public void Selects(int coordX, int coordY, TicTacToePlayer whoSelects){

		TicTacToeFigure figure = GetCurrentFigure(whoSelects);
		SetVisual(coordX, coordY, figure);

		GameState currentBoardState = CheckBoard();
		if (currentBoardState != GameState.none) 
		{ 
			onPlayerWin?.Invoke((int)currentBoardState);
			return;
		}

		if (whoSelects == TicTacToePlayer.AI)
		{
			IsPlayerTurn = true;
		}
		else
		{
			IsPlayerTurn = false;
			StartCoroutine(TurnIA());
			
		}
	}

	public IEnumerator TurnIA()
    {
		UnityEngine.Random.InitState(System.Environment.TickCount);
		yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));	// Let it think a bit :)


		Vector2Int move = MoveAi();
		Selects(move.x, move.y, TicTacToePlayer.AI);
	}

	private TicTacToeFigure GetCurrentFigure(TicTacToePlayer whoSelects)
	{
		switch (whoSelects)
		{
			case TicTacToePlayer.human: return humanFigure;
			case TicTacToePlayer.AI:	return aiFigure;
		}
		return TicTacToeFigure.none;
	}

	private void ClearListVisualObjects()
	{
		if (VisualObjects.Count == 0) return;

		int length = VisualObjects.Count;
		GameObject currentGO = VisualObjects[0];
		for (int i = 0; i < length; ++i)
		{
			currentGO = VisualObjects[i];
			VisualObjects.RemoveAt(i);
			--i;
			Destroy(currentGO);
		}
	}

	private void ResetBoardState()
	{
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
				boardState[i, j] = TicTacToeFigure.none;

			}
        }
	}

	private void SetVisual(int coordX, int coordY, TicTacToeFigure targetState)
	{
		_triggers[coordX, coordY].CanClick = false;
		boardState[coordX, coordY] = targetState;

		VisualObjects.Add(
			Instantiate(
				targetState == TicTacToeFigure.circle ? _oPrefab : _xPrefab,
				_triggers[coordX, coordY].transform.position,
				Quaternion.identity
			)
		);
    }

	private GameState CheckBoard()
    {
		// Horizontal
		int equals = 0;
		TicTacToeFigure currentFigure = TicTacToeFigure.none;
		TicTacToeFigure winner = TicTacToeFigure.none;
		for (int i = 0; i < _gridSize; ++i)
        {
			equals = 0;
			currentFigure = boardState[i, 0];

			for (int j = 0; j < _gridSize; ++j)
            {
				if(boardState[i, j] == currentFigure)
                {
					++equals;
					currentFigure = boardState[i, j];
				}
			}
			if (equals == _gridSize && currentFigure != TicTacToeFigure.none) winner = currentFigure;
		}

		// Vertical
		for (int i = 0; i < _gridSize; ++i)
		{
			equals = 0;
			currentFigure = boardState[0, i];
			for (int j = 0; j < _gridSize; ++j)
			{
				if (boardState[j, i] == currentFigure)
				{
					++equals;
					currentFigure = boardState[j, i];
				}
			}
			if (equals == _gridSize && currentFigure != TicTacToeFigure.none) winner = currentFigure;
		}

		// Diagonal
		equals = 0;
		currentFigure = boardState[0, 0];
		for (int i = 0; i < _gridSize; ++i)
		{
			if(currentFigure == boardState[i, i])
            {
				++equals;
				currentFigure = boardState[i, i];
			}
		}
		if (equals == _gridSize && currentFigure != TicTacToeFigure.none) winner = currentFigure;

		equals = 0;
		currentFigure = boardState[_gridSize - 1, 0];
		for (int i = 0; i < _gridSize; ++i)
		{
			if (currentFigure == boardState[(_gridSize - 1) - i, i])
			{
				++equals;
				currentFigure = boardState[(_gridSize - 1) - i, i];
			}
		}
		if (equals == _gridSize && currentFigure != TicTacToeFigure.none) winner = currentFigure;

		if (GetEmptyPlacesBoard() == 0 && winner == TicTacToeFigure.none) return GameState.tie;
		return humanFigure == winner ? GameState.winsHuman : aiFigure == winner ? GameState.winsAi : GameState.none ;	
    }

	private int GetEmptyPlacesBoard()
    {
		int empties = 0;
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
				empties += boardState[i, j] == TicTacToeFigure.none ? 1 : 0;
            }
        }
		return empties;
    }

	private Vector2Int MoveAi()
	{
		Vector2Int move = new Vector2Int(0, 0);
		// _aiLevel == 0 ? easy : hard
		int depth = _aiLevel == 0 ? Mathf.Clamp(GetEmptyPlacesBoard() - 3, 0, _gridSize * _gridSize): GetEmptyPlacesBoard();

		int bestScore = int.MinValue;
		for (int i = 0; i < _gridSize; i++)
		{
			for (int j = 0; j < _gridSize; j++)
			{
				if (boardState[i, j] == TicTacToeFigure.none)
				{
					boardState[i, j] = aiFigure;
					int score = MiniMax(depth - 1, false);
					boardState[i, j] = TicTacToeFigure.none;
					if(score > bestScore)
                    {
						bestScore = score;
						move.Set(i, j);
                    }
				}
			}
		}

		return move;
    }


	private int MiniMax(int depth, bool isMaximizing)
    {
		GameState state = CheckBoard();
		if (state == GameState.winsAi || state == GameState.winsHuman) return state == GameState.winsAi ? 1 : -1;
		if (depth <= 0 ) {
			
			return state == GameState.winsAi ? 1 : state == GameState.winsHuman ? -1 : 0;
		}
		int bestScore;

		if (isMaximizing)
        {
			bestScore = int.MinValue;
			for (int i = 0; i < _gridSize; i++)
			{
				for (int j = 0; j < _gridSize; j++)
				{
					if(boardState[i,j] == TicTacToeFigure.none)
                    {
						boardState[i, j] = aiFigure;
						int score = MiniMax(depth - 1, false);
						boardState[i, j] = TicTacToeFigure.none;
						bestScore = Mathf.Max(bestScore, score);
					}
				}
			}
			return bestScore;
        }
        else
        {
			bestScore = int.MaxValue;

			for (int i = 0; i < _gridSize; i++)
			{
				for (int j = 0; j < _gridSize; j++)
				{
					if (boardState[i, j] == TicTacToeFigure.none)
					{
						boardState[i, j] = humanFigure;
						int score = MiniMax(depth - 1, true);
						boardState[i, j] = TicTacToeFigure.none;
						bestScore = Mathf.Min(bestScore, score);
					}
				}
			}
			return bestScore;
		}
       
    }
}