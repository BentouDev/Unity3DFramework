namespace Framework
{
    public abstract class Condition : BaseScriptableObject
    {
        public static readonly string NO_DESCRIPTION = string.Empty;
        
        public int Priority;

        public abstract bool IsSatisfied();

        public virtual string GetDescription()
        {
            return NO_DESCRIPTION;
        }

        void OnValidate()
        {
            var desc = GetDescription();
            if (desc.Equals(NO_DESCRIPTION))
                return;

            name = desc;
        }
    }
}