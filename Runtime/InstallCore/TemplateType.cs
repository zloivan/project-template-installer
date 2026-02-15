namespace IKhom.TemplateInstaller
{
    /// <summary>
    /// Available template types for project generation
    /// </summary>
    public enum TemplateType
    {
        /// <summary>
        /// Single-scene hypercasual prototype (1-3 weeks dev cycle)
        /// </summary>
        SingleScene,

        /// <summary>
        /// Multi-scene modular architecture (3-12 months lifecycle)
        /// </summary>
        Modular,

        /// <summary>
        /// Clean architecture with domain/application/infrastructure separation (1+ years)
        /// </summary>
        CleanArchitecture
    }
}
