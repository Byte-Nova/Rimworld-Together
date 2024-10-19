namespace Shared 
{
    /// <summary>
    /// This class is how modded packets can contact modded assemblies./>.
    /// </summary>
    public class ModdedData 
    {
        public string _assemblyName;
        /// <summary>
        /// This constructor is there to make sure all data types are filled, as they are all required.>.
        /// </summary>
        /// <param name="_assemblyName">The name of the assembly, without the extension.</param>
        /// <param name="_type">The type of your class. Should include the namespacer. Should start with either "GameClient." or "GameServer." for consistency.</param>
        public ModdedData(string _assemblyName)
        {  
            this._assemblyName = _assemblyName;
        }
    }
}