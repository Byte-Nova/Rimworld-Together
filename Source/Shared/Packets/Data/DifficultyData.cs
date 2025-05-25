namespace Shared
{

    public class DifficultyData
    {
        public DifficultyValuesFile _values { get; set; } = new DifficultyValuesFile();

        public override string ToString()
        {
            return $"DifficultyData:|{_values}";
        }
    }
}