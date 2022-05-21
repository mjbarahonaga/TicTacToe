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
    public bool CanClick { get => canClick; set => canClick = value; }

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
		CanClick = val;
	}

	private void AddReference()
	{
		_tttManager.RegisterTransform(_myCoordX, _myCoordY, this);
		CanClick = true;
	}

	private void OnMouseDown()
	{
		if(CanClick && _tttManager.IsPlayerTurn)
		{
			_tttManager.Selects(_myCoordX, _myCoordY,TicTacToePlayer.human);
			CanClick = false;
		}
	}
}
