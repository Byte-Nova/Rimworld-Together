using PluginServer;

/** @NOTE(jrseducate@gmail.com): 
 * 
 *  - The PluginManager looks for a class named "Plugin" without any namespaces applied
 *  
 *  - The IPlugin interface is optional, so long as the methods are available they will be 
 *    used through a reflection wrapper
 */
public class Plugin : IPlugin
{
    /** @NOTE(jrseducate@gmail.com): 
     * 
     * - The Init method is called immediately after the Plugin is loaded
     * 
     * - The pluginPath refers to the folder specific to this plugin. 
     *   In this case it would likely be a folder named "Example" within the Plugins folder
     */
    public void Init(string pluginPath)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] | [Plugin:Example] > Loaded! Path=[{pluginPath}]");
    }

    /** @NOTE(jrseducate@gmail.com): 
     * 
     * - The OnEvent method is called any time an event is emitted from the Server using [Master.plugins.Emit(...)]
     * 
     * - The name argument refers to the name of the event being emitted, it's loosely coupled so adding new events 
     *   is just a matter of emitting a new name
     * 
     * - The args argument is an array of parameters being emitted by the event, it's loose in implementation to allow 
     *   for flexibility in its usage
     */
    public void OnEvent(string name, object[] args)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] | [Plugin:Example] > Event Recieved [{name}]");
    }
}