using UnityEngine;

[RequireComponent(typeof(InputBuffer))]
public class PlayerController : MonoBehaviour, ILevelDependable
{
	[System.Serializable]
	public struct InputSettings
	{
		[SerializeField]
		public string LookX;
		[SerializeField]
		public string LookY;

		[SerializeField]
		public string MoveX;
		[SerializeField]
		public string MoveY;

		[SerializeField]
		public string Dash;

		[SerializeField]
		public string Jump;

        [SerializeField]
        public string Block;

        [SerializeField]
        public string Burst;

        [SerializeField]
		public string Slash;

		[SerializeField]
		public string Kick;

        [SerializeField]
        public string Special;
    }

	public class RawInputData
	{
		public	Vector2		Move;
		public	Vector2		Look;
		public	bool		Dash;
        public  bool        Jump;
        public  bool        Block;
        public  bool        Burst;
        public	bool		Slash;
		public	bool		Kick;
		public	bool		Special;
	}

    [SerializeField]
    public InputSettings ControlsSettings;

    public string SpawnPointTag = "Respawn";

    public CharacterData Character;

	public CameraBase MainCamera;

    public Pawn Pawn { get; private set; }

    public InputBuffer InputBuffer { get; private set; }

    private RawInputData CurrentRawInput;

    private bool isPlayable;

    public void SetPlayable(bool playable)
    {
        isPlayable = playable;
    }

    void Start()
    {
        InputBuffer = GetComponent<InputBuffer>();
        CurrentRawInput = new RawInputData();
    }
    
	void Update()
	{
	    if (!isPlayable)
	        return;
        
		CurrentRawInput = GrabInput();

	    var processedInput = InputBuffer.ProcessInput(CurrentRawInput);
        
	    if (Pawn != null)
        {
            if (Pawn.ProcessInput(processedInput))
            {
                InputBuffer.Clear();
            }
        }
        
	    if (MainCamera != null)
	    {
	        MainCamera.ProcessInput(processedInput);
	    }
	}

	private RawInputData GrabInput()
	{
	    var newInput = new RawInputData();
        
        newInput.Dash	 = Input.GetButton(ControlsSettings.Dash);
        newInput.Jump	 = Input.GetButton(ControlsSettings.Jump);
        newInput.Block	 = Input.GetButton(ControlsSettings.Block);
        newInput.Burst	 = Input.GetButton(ControlsSettings.Burst);
        newInput.Slash	 = Input.GetButton(ControlsSettings.Slash);
        newInput.Kick	 = Input.GetButton(ControlsSettings.Kick);
        newInput.Special = Input.GetButton(ControlsSettings.Special);
        
        newInput.Move	 = new Vector2 {
			x = Input.GetAxis(ControlsSettings.MoveX),
			y = Input.GetAxis(ControlsSettings.MoveY)
		};
        
        newInput.Look	 = new Vector2 {
			x = Input.GetAxis(ControlsSettings.LookX),
			y = Input.GetAxis(ControlsSettings.LookY)
		};

	    return newInput;
	}

    public void SpawnPawn()
    {
        if (Pawn)
            return;

        GameObject spawnPoint = GameObject.FindGameObjectWithTag(SpawnPointTag);

        if (!spawnPoint)
        {
            Debug.LogError("Unable to spawn pawn: No spawn point specified! Check PlayerController inspector!");
        }
        else
        {
            Pawn = Character.SpawnPawn(spawnPoint.transform);
        }
    }

    public void DestroyPawn()
    {
        if (Pawn)
        {
            Pawn.OnLevelCleanUp();
            Destroy(Pawn.gameObject);
        }
    }

    public void OnLevelLoaded()
    {
        SpawnPawn();

        if(Pawn)
            Pawn.OnLevelLoaded();

        MainCamera.OnLevelLoaded();
    }

    public void OnLevelCleanUp()
    {
        DestroyPawn();

        if (MainCamera)
        {
            MainCamera.OnLevelCleanUp();
        }
    }
}
