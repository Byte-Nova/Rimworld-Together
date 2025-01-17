using System;

namespace Shared
{
    // Used for instantiating managers
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RTManager : Attribute { }

    // Used for loading in custom assemblies
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RTStartupAttribute : Attribute { }
}