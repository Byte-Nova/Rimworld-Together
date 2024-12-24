using System;

namespace Shared 
{
    // Used for loaded assemblies to mark their entry point.
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RTStartupAttribute : Attribute {}
}