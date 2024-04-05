using System.Reflection;

namespace MBW.EF.AutoTagger.Database;

public delegate bool TagQueryIncludeFrame(AssemblyName assembly, MethodBase method);