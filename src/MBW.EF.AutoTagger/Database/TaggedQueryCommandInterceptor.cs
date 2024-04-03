using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MBW.EF.AutoTagger.Database;

public class TaggedQueryCommandInterceptor : DbCommandInterceptor
{
    public override DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
    {
        ManipulateCommand(eventData.Command);

        return eventData.Command;
    }

    private static void ManipulateCommand(DbCommand command)
    {
        StackTrace trace = new StackTrace();

        // Skip until we hit userland code
        int frameIdx = 0;
        for (; frameIdx < trace.FrameCount; frameIdx++)
        {
            var frame = trace.GetFrame(frameIdx);
            var assembly = frame.GetMethod()?.Module.Assembly;

            if (assembly == null)
                continue;

            var assemblyName = assembly.GetName();
            
            // Skip AutoTagger
            if (assemblyName.Name.Equals("MBW.EF.AutoTagger", StringComparison.Ordinal))
                continue;

            // Skip MS
            if (assemblyName.Name.StartsWith("System.", StringComparison.Ordinal) ||
                assemblyName.Name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)||
                assemblyName.Name.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.Ordinal))
                continue;

            // Stop here
            break;
        }

        string toStr(StackFrame frame) => $"-- {frame.GetMethod().Module.Assembly.GetName().Name} / {frame.GetMethod().Name}, File:{frame.GetFileName()}, Line:{frame.GetFileLineNumber()}:{frame.GetFileColumnNumber()}";

        var thisFrame = trace.GetFrame(frameIdx);
        var thisFrameStr = toStr(thisFrame);

        var strDebug = string.Join("\n", trace.GetFrames().Skip(frameIdx).Select(toStr));

        // command.CommandText = $"{thisFrameStr}\n\n{strDebug}\n{command.CommandText}";
        command.CommandText = $"{thisFrameStr}\n\n{command.CommandText}";
    }
}