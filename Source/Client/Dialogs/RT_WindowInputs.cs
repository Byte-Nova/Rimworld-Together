using System.Collections.Generic;


namespace GameClient
{
    public interface RT_WindowInputs
    {
        public abstract void CacheInputs();

        public abstract void SubstituteInputs(List<object> newInputs);
    }
}
