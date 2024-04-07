using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace MBW.EF.AutoTagger.Database;

internal static class CallSiteTaggerHelpers
{
    private const string ThisAssemblyName = "MBW.EF.AutoTagger";

    static CallSiteTaggerHelpers()
    {
        ObjectPoolProvider objectPool = new DefaultObjectPoolProvider();
        StringBuilderPool = objectPool.CreateStringBuilderPool(1, 1);
    }

    public static ObjectPool<StringBuilder> StringBuilderPool { get; }

    public static TagQueryIncludeFrame DefaultIncludeFrame = (assemblyName, method) =>
    {
        Debug.Assert(assemblyName.Name != null);

        // Skip AutoTagger
        if (assemblyName.Name.Equals(ThisAssemblyName, StringComparison.Ordinal))
            return false;

        // Skip MS
        if (assemblyName.Name.StartsWith("System.", StringComparison.Ordinal) ||
            assemblyName.Name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
            assemblyName.Name.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.Ordinal))
            return false;

        // Skip .NET's enumerators
        if (method.Name.Equals(nameof(IEnumerator.MoveNext), StringComparison.Ordinal))
            return false;

        return true;
    };
}