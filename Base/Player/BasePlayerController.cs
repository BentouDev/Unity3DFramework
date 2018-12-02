using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class BasePlayerController : Controller
    {
        [Header("Camera")]
        public bool FindCameraByTag;
        [Validate("ValidateCameraTag")]
        public string CameraTag;
        [Validate("ValidateCamera")]
        public ActionCamera PawnCamera;

        protected override void OnInit()
        {
            if (!PawnCamera && FindCameraByTag)
            {
                var go = GameObject.FindGameObjectWithTag(CameraTag);
                PawnCamera = go ? go.GetComponent<ActionCamera>() : null;
            }
    
            if (PawnCamera != null) PawnCamera.Init();
        }

    #if UNITY_EDITOR
        public ValidationResult ValidateCamera()
        {
            if (!PawnCamera && !FindCameraByTag)
                return new ValidationResult(ValidationStatus.Warning, "Camera is not set and finding by tag is disabled.");
            return ValidationResult.Ok;
        }
    
        public ValidationResult ValidateCameraTag()
        {
            bool noTag = FindCameraByTag && string.IsNullOrWhiteSpace(CameraTag);
            if (noTag)
            {
                if (PawnCamera)
                    return new ValidationResult(ValidationStatus.Warning, "Camera is present, but Tag shouldn't be empty if FindByTag is enabled");
    
                return new ValidationResult(ValidationStatus.Error, "Tag cannot be empty if FindByTag is enabled");
            }
    
            return ValidationResult.Ok;
        }
    #endif
    }
}