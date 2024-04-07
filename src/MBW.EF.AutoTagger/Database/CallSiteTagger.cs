using System;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.ObjectPool;

namespace MBW.EF.AutoTagger.Database;

internal class CallSiteTagger : DbCommandInterceptor, ISingletonInterceptor
{
    private readonly CallSiteTaggerConfig _config;

    public CallSiteTagger(CallSiteTaggerConfig config)
    {
        _config = config;
    }

    public override DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
    {
        // Allow user to filter which queries will be tagged
        if (_config.TagFilter != null && !_config.TagFilter(eventData.Context, result))
            return result;

        ManipulateCommand(result);

        return result;
    }

    private void ManipulateCommand(DbCommand command)
    {
        StackTrace trace = new StackTrace();

        if (_config.TaggingMode is CallSiteTaggingMode.OnlyMethod or CallSiteTaggingMode.FullStackToQuery)
        {
            // Skip until we hit userland code
            int frameIdx = 0;
            for (; frameIdx < trace.FrameCount; frameIdx++)
            {
                StackFrame? frame = trace.GetFrame(frameIdx);
                MethodBase? frameMethod = frame?.GetMethod();

                if (frameMethod == null)
                    continue;

                Assembly assembly = frameMethod.Module.Assembly;
                AssemblyName assemblyName = assembly.GetName();

                if (assemblyName.Name == null)
                    continue;

                if (!CallSiteTaggerHelpers.DefaultIncludeFrame(assemblyName, frameMethod))
                    continue;
                if (_config.FrameFilter != null && !_config.FrameFilter(assemblyName, frameMethod))
                    continue;

                if (_config.TaggingMode == CallSiteTaggingMode.OnlyMethod)
                {
                    // Use this frame. Do not include assembly name in output
                    string thisFrameStr = GetStringForFrame(frame, frameMethod, false);
                    command.CommandText = $"{_config.CommentPrefix}{thisFrameStr}{_config.CommentSuffix}{command.CommandText}";

                    return;
                }

                // Stop here
                break;
            }

            // TaggingMethod is all stack from this point
            StringBuilder sb = CallSiteTaggerHelpers.StringBuilderPool.Get();

            for (; frameIdx < trace.FrameCount; frameIdx++)
            {
                StackFrame? frame = trace.GetFrame(frameIdx);
                MethodBase? method = frame?.GetMethod();
                if (method == null)
                    continue;

                // Use assembly name in output as the stack may span multiple assemblies
                sb.Append(_config.CommentPrefix);
                sb.Append(GetStringForFrame(frame!, method, true));
                sb.Append(_config.CommentSuffix);
            }

            sb.Append(command.CommandText);

            command.CommandText = sb.ToString();
            
            CallSiteTaggerHelpers.StringBuilderPool.Return(sb);
        }
        else if (_config.TaggingMode == CallSiteTaggingMode.FullStack)
        {
            StringBuilder sb = CallSiteTaggerHelpers.StringBuilderPool.Get();

            foreach (StackFrame frame in trace.GetFrames())
            {
                MethodBase? method = frame.GetMethod();
                if (method == null)
                    continue;

                // Use assembly name in output as the stack may span multiple assemblies
                sb.Append(_config.CommentPrefix);
                sb.Append(GetStringForFrame(frame, method, true));
                sb.Append(_config.CommentSuffix);
            }

            sb.Append(command.CommandText);

            command.CommandText = sb.ToString();
            
            CallSiteTaggerHelpers.StringBuilderPool.Return(sb);
        }
        else
            throw new InvalidOperationException("Unsupported method");
    }

    private static string GetStringForFrame(StackFrame frame, MethodBase frameMethod, bool includeAssembly)
    {
        string? fileName = frame.GetFileName();
        if (includeAssembly)
        {
            if (fileName != null)
                return $"{frameMethod.Module.Assembly.GetName().Name} / {frameMethod.DeclaringType?.FullName}.{frameMethod.Name}, File:{fileName}, Line:{frame.GetFileLineNumber()}:{frame.GetFileColumnNumber()}";
            return $"{frameMethod.Module.Assembly.GetName().Name} / {frameMethod.DeclaringType?.FullName}.{frameMethod.Name}";
        }
        else
        {
            if (fileName != null)
                return $" {frameMethod.DeclaringType?.FullName}.{frameMethod.Name}, File:{fileName}, Line:{frame.GetFileLineNumber()}:{frame.GetFileColumnNumber()}";
            return $"{frameMethod.DeclaringType?.FullName}.{frameMethod.Name}";
        }
    }
}