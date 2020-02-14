using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(fileName = "New Input Scheme", menuName = "Data/Input Scheme")]
    [System.Serializable]
    public class InputScheme : BaseScriptableObject
    {
//        [Header("Misc")]
//        public bool InitOnStart;
//        public bool Autopool;
//        
//        [Header("Definitions")]
//        [RequireValue]
//        public GenericInputBuffer Buffer;

        [Header("Movement Axis")]
        [RequireValue]
        [SerializeField]
        public string MoveX;

        [RequireValue]
        [SerializeField]
        public string MoveY;

        [SerializeField]
        [Validate("ValidateButtons")]
        public List<ButtonInfo> Buttons = new List<ButtonInfo>();

//        private Vector2 _currentInput;
//        public Vector2 CurrentInput => _currentInput;

        [System.Serializable]
        public struct ButtonInfo
        {
            [SerializeField]
            public string  Name;
            
            [SerializeField]
            public KeyCode Key;
        }

//        void Start()
//        {
//            if (!InitOnStart)
//                return;
//
//            AdjustCamera();
//        }

//        public void AdjustCamera()
//        {
//            Buffer.DefineButtons(Buttons.ConvertAll(b => b.Name));
//        }
//
//        public void GatherInput()
//        {                            
//            _currentInput.x = Input.GetAxis(MoveX);
//            _currentInput.y = Input.GetAxis(MoveY);
//
//            foreach (var button in Buttons)
//            {
//                Buffer.HandleButton(Input.GetKey(button.Key), button.Name);
//            }
//        }

//        public void Update()
//        {
//            if (!Autopool)
//                return;
//            
//            GatherInput();
//        }

        public ValidationResult ValidateButtons()
        {
            if (Buttons.Any(b => string.IsNullOrWhiteSpace(b.Name)))
                return new ValidationResult(ValidationStatus.Error, "All buttons must have a name!");

            var names = Buttons.ConvertAll(b => b.Name);
            if (names.Count != names.Distinct().Count())
                return new ValidationResult(ValidationStatus.Error, "All buttons must have an unique name!");

            return ValidationResult.Ok;
        }
    }
}