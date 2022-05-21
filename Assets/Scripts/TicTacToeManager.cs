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

	[SerializeField]
	private bool _isPlayerTurn;
	public bool IsPlayerTurn { get => _isPlayerTurn; set => _isPlayerTurn = value; }

	[SerializeField]
	private int _gridSize = 3;
	
	[SerializeField]
	private TicTacToeFigure playerFigure = TicTacToeFigure.cross;
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
		if (IsPlayerTurn == false) { StartCoroutine(TurnAI()); }
	}

	public void Selects(int coordX, int coordY, TicTacToePlayer whoSelects){

		TicTacToeFigure figure = GetCurrentFigure(whoSelects);
		SetVisual(coordX, coordY, figure);

		GameState currentBoardState = CheckBoard(figure);
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
			StartCoroutine(TurnAI());
		}
	}

	public IEnumerator TurnAI()
    {
		UnityEngine.Random.InitState(System.Environment.TickCount);
		yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));	// Let it think a bit :)

        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
				if (boardState[i, j] == TicTacToeFigure.none)
				{

					Selects(i, j, TicTacToePlayer.AI);
					yield break;
				}
            }
        }
		yield return null;
		// Logic
		// Selects(,,TicTacToePlayer.AI);
	}

	private TicTacToeFigure GetCurrentFigure(TicTacToePlayer whoSelects)
	{
		switch (whoSelects)
		{
			case TicTacToePlayer.human: return playerFigure;
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
		VisualObjects.Add(
			Instantiate(
				targetState == TicTacToeFigure.circle ? _oPrefab : _xPrefab,
				_triggers[coordX, coordY].transform.position,
				Quaternion.identity
			)
		);

		boardState[coordX, coordY] = targetState;
    }

	private GameState CheckBoard(TicTacToeFigure figure)
    {
		// Horizontal
		int equals = 0;
        for (int i = 0; i < _gridSize; ++i)
        {
			equals = 0;
            for (int j = 0; j < _gridSize; ++j)
            {
				equals += boardState[j, i] == figure ? 1 : 0;
			}
			if (equals == _gridSize) return playerFigure == figure ? GameState.winsHuman : GameState.winsAi;
		}

		// Vertical
		for (int i = 0; i < _gridSize; ++i)
		{
			equals = 0;
			for (int j = 0; j < _gridSize; ++j)
			{
				equals += boardState[i, j] == figure ? 1 : 0;
			}
			if (equals == _gridSize) return playerFigure == figure ? GameState.winsHuman : GameState.winsAi;
		}

		// Diagonal
		equals = 0;
		for (int i = 0; i < _gridSize; ++i)
		{
			equals += boardState[i, i] == figure ? 1 : 0;
		}
		if (equals == _gridSize) return playerFigure == figure ? GameState.winsHuman : GameState.winsAi;

		equals = 0;
		for (int i = 0; i < _gridSize; ++i)
		{
			equals += boardState[(_gridSize - 1) - i, i] == figure ? 1 : 0;
		}
		if (equals == _gridSize) return playerFigure == figure ? GameState.winsHuman : GameState.winsAi;

		if (VisualObjects.Count == 9) return GameState.tie;
		return GameState.none;	
    }
}