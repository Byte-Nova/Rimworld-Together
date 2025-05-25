namespace Shared
{
    public class ScenarioValuesFile
    {
        public bool EnforceScenario;

        public string ScenarioName;

        public override string ToString()
        {
            return $"ScenarioValuesFile:|{EnforceScenario}|{ScenarioName}";
        }
    }
}