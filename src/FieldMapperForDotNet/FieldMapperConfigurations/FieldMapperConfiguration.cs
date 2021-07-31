namespace FieldMapperForDotNet
{
    public class FieldMapperConfiguration : FieldMapperConfigurationBase
    {
        public readonly FieldMapperConfigurationOptions options;

        public FieldMapperConfiguration()
        {
            options = new FieldMapperConfigurationOptions
            {
                DeEntitizeContent = true,
                SeparateByLineBreaks = true
            };
        }
    }
}