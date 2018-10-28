using UnityEngine;

namespace Framework
{
    public interface IValueChecker
    {
        bool HasValue();
    }
    
    public class RequireValue : BaseEditorAttribute
    {
        
    }
}