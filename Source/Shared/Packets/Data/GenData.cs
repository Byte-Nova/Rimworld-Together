using static Shared.CommonEnumerators;

namespace Shared
{

    public class GameParameterData
    {
        public GenStepMode _stepMode { get; set; } = GenStepMode.Scenario;

        public ScenarioValuesFile _scenario { get; set; } = null;

        public StorytellerValuesFile _storyteller { get; set; } = null;

        public DifficultyValuesFile _difficulty { get; set; } = null;
    }
}