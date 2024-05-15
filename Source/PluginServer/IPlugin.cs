namespace PluginServer
{
    public interface IPlugin
    {
        public void Init(string pluginPath);
        public void OnEvent(string name, object[] args);
    }
}