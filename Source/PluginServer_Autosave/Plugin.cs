using GameServer;
using PluginServer;
using PluginServer_Autosave;
using static Shared.CommonEnumerators;

public class Plugin : IPlugin
{
    protected Thread thread;

    public void Init(string pluginPath)
    {
        Autosave.Init(pluginPath);
    }

    public void OnEvent(string name, object[] args)
    {
        switch (name)
        {
            case "pluginsLoaded_post":
                pluginsLoaded_post();
                break;

            case "pluginsUnloaded_pre":
                pluginsUnloaded_pre();
                break;

            case "onUserSave_post":
                if (args[0] is ServerClient serverClient)
                {
                    Autosave.MarkClientSaved(serverClient);
                }
                break;
        }
    }

    public void pluginsLoaded_post()
    {
        Logger.WriteToConsole($"[Plugin:Autosave] > Loaded!", LogMode.Message);

        ThreadStart work = Autosave.ThreadMethod;

        thread = new Thread(work);
        thread.Start();
    }

    public void pluginsUnloaded_pre()
    {
        if (thread.IsAlive)
        {
            thread.Abort();
        }
    }
}