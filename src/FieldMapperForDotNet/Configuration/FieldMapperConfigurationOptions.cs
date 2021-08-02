namespace FieldMapperForDotNet
{
    /// <summary>
    /// These are the options used in the configuration for the field mapper. Use them to customize the behavior of the field mapper.
    /// </summary>
    public class FieldMapperConfigurationOptions
    {
        /// <summary>
        /// Return default options.
        /// </summary>
        public static FieldMapperConfigurationOptions Default
        {
            get
            {
                return new FieldMapperConfigurationOptions
                {
                    DeEntitizeContent = true,
                    SeparateByLineBreaks = true
                };
            }
        }

        /// <summary>
        /// This will DeEntitize the content by stripping all html and other relevant characters. If you're pulling plain-text, you can set this to <see langword="false"/> most likely. If you're pulling html strings, you may want it <see langword="true"/>.
        /// </summary>
        public bool DeEntitizeContent { get; set; }

        /// <summary>
        /// This keeps everything clean, recommend leaving as <see langword="true"/>.
        /// </summary>
        public bool SeparateByLineBreaks { get; set; }
    }
}