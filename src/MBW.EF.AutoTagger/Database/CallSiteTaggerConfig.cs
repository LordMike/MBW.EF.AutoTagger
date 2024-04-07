namespace MBW.EF.AutoTagger.Database;

public class CallSiteTaggerConfig
{
    /// <summary>
    /// A filter function which must return true to apply callsite tagging. If this is not set, all queries are tagged.
    /// </summary>
    public TagQueryFilter? TagFilter { get; set; }

    /// <summary>
    /// In the <see cref="CallSiteTaggingMode.OnlyMethod"/> and <see cref="CallSiteTaggingMode.FullStackToQuery"/> modes, this is an additional
    /// filter to help reduce the frames shown. By default, Microsoft and Entity Framework frames are discarded.
    /// </summary>
    public TagQueryIncludeFrame? FrameFilter { get; set; }

    /// <summary>
    /// The tagging mode to use. The default is <see cref="CallSiteTaggingMode.OnlyMethod"/>, which is a oneliner with a method name. When troubleshooting which frames are discarded with <see cref="FrameFilter"/>, it can help to set this to <see cref="CallSiteTaggingMode.FullStack"/>.
    /// </summary>
    public CallSiteTaggingMode TaggingMode { get; set; } = CallSiteTaggingMode.OnlyMethod;

    /// <summary>
    /// The string to prefix all added tags with. By default, this is "-- "
    /// </summary>
    public string CommentPrefix { get; set; } = "-- ";

    /// <summary>
    /// The string to suffix all added tags with. By default, this is "\n"
    /// </summary>
    public string CommentSuffix { get; set; } = "\n";
}