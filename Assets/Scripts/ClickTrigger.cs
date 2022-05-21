using UnityEngine;

public class ClickTrigger : MonoBehaviour
{
	[SerializeField] TicTacToeManager _tttManager;

	[SerializeField]
	private int _myCoordX = 0;
	[SerializeField]
	private int _myCoordY = 0;

	[SerializeField]
	private bool canClick;

    private void OnValidate()
    {
		Utils.ValidationUtility.SafeOnValidate(() =>
		{
			if (this == null) return;

			if(_tttManager == null) _tttManager = FindObjectOfType<TicTacToeManager>();

		});
    }

	private void Awake(){

		_tttManager.onGameStarted.AddListener(AddReference);
		_tttManager.onGameStarted.AddListener(() => SetInputEndabled(true));
	}

	private void SetInputEndabled(bool val){
		canClick = val;
	}

	private void AddReference()
	{
		_tttManager.RegisterTransform(_myCoordX, _myCoordY, this);
		canClick = true;
	}

	private void OnMouseDown()
	{
		if(canClick && _tttManager.IsPlayerTurn)
		{
			_tttManager.Selects(_myCoordX, _myCoordY,TicTacToePlayer.human);
			canClick = false;
		}
	}
}
