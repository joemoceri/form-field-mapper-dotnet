namespace FieldMapperForDotNet
{
    /// <summary>
    /// Use this class to customize the behavior of the field mapper.
    /// </summary>
    public class FieldMapperConfiguration
    {
        /// <summary>
        /// Return default configuration.
        /// </summary>
        public static FieldMapperConfiguration Default
        {
            get
            {
                return new FieldMapperConfiguration();
            }
        }

        /// <summary>
        /// You can modify the options to change how the field mapper works when running <see cref="FieldMapper.Get(string, System.Collections.Generic.IEnumerable{string})"/>.
        /// </summary>
        public readonly FieldMapperConfigurationOptions Options;

        /// <summary>
        /// By default initializes the options with <see cref="FieldMapperConfigurationOptions.DeEntitizeContent"/> as <see langword="true"/> and <see cref="FieldMapperConfigurationOptions.SeparateByLineBreaks"/> as <see langword="true"/>.
        /// </summary>
        public FieldMapperConfiguration()
            : this(FieldMapperConfigurationOptions.Default)
        {

        }
        /// <summary>
        /// You can pass in your own <see cref="FieldMapperConfigurationOptions"/>. By default initializes the options with <see cref="FieldMapperConfigurationOptions.DeEntitizeContent"/> as <see langword="true"/> and <see cref="FieldMapperConfigurationOptions.SeparateByLineBreaks"/> as <see langword="true"/>.
        /// </summary>
        /// /// <param name="options">Your own <see cref="FieldMapperConfigurationOptions"/></param>
        public FieldMapperConfiguration(FieldMapperConfigurationOptions options)
        {
            Options = options ?? FieldMapperConfigurationOptions.Default;
        }
    }
}