namespace FieldMapperForDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public class FieldMapperConfiguration : FieldMapperConfigurationBase
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly FieldMapperConfigurationOptions options;

        /// <summary>
        /// 
        /// </summary>
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