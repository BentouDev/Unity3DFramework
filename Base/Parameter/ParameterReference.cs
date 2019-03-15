using System.Linq;

namespace Framework
{
    [System.Serializable]
    public class ParameterReference
    {
        public System.Guid ParameterId;

        public Parameter Get(IDataSetProvider provider)
        {
            // ToDo: guid based dictionary lookup!
            return provider.GetParameters().FirstOrDefault(p => p.Id == ParameterId);
        }

        public bool IsValid()
        {
            return ParameterId != System.Guid.Empty;
        }
    }
}