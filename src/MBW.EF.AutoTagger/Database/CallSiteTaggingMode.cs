namespace MBW.EF.AutoTagger.Database;

public enum CallSiteTaggingMode
{
    /// <summary>
    /// Include only the method name that called in to EntityFramework
    /// </summary>
    OnlyMethod,

    /// <summary>
    /// Include the full stack, with no filtering
    /// </summary>
    FullStack,

    /// <summary>
    /// Include the full stack, but exclude elements internal to AutoTagger and EntityFramework
    /// </summary>
    FullStackToQuery
}