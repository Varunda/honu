using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;


/**
 * Parts of this code are lifted from dotnet runtime:
 * https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Console/src/SimpleConsoleFormatter.cs
 * 
 * That code is licensed as such:
 * The MIT License (MIT)

    Copyright (c) .NET Foundation and Contributors

    All rights reserved.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 */

namespace watchtower.Code {

    public class HonuLogger : ConsoleFormatter, IDisposable {

        private readonly IDisposable _OptionsReloadToken;
        private HonuFormatterOptions _Options;

        public HonuLogger(IOptionsMonitor<HonuFormatterOptions> options)
            : base("HonuLogger") {

            _OptionsReloadToken = options.OnChange((HonuFormatterOptions options) => _Options = options);
            _Options = options.CurrentValue;
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter) {
            string? msg = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && msg == null) {
                return;
            }

            textWriter.Write("[");

            LogLevel logLevel = logEntry.LogLevel;
            string logLevelStr = GetLogLevelString(logLevel);
            ConsoleColors levelColors = GetLogLevelConsoleColors(logLevel);

            textWriter.WriteColoredMessage(logLevelStr, levelColors.Background, levelColors.Foreground);
            textWriter.Write(" ");

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ");
            textWriter.Write(timestamp);
            textWriter.Write("] ");

            textWriter.Write(logEntry.Category);
            textWriter.Write("[");
            textWriter.Write(logEntry.EventId.ToString());
            textWriter.Write("] ");

            textWriter.Write(msg);

            if (logEntry.Exception != null) {
                textWriter.Write(": ");
                textWriter.Write(logEntry.Exception.ToString());
            }

            textWriter.Write(Environment.NewLine);

        }

        public void Dispose() {
            _OptionsReloadToken.Dispose();
        }

        private static string GetLogLevelString(LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => "unknown"
            };
        }

        // Taken from dotnet runtime
        private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel) {
            // We must explicitly set the background color if we are setting the foreground color,
            // since just setting one can look bad on the users console.
            return logLevel switch {
                LogLevel.Trace => new ConsoleColors(ConsoleColor.Magenta, ConsoleColor.Black),
                LogLevel.Debug => new ConsoleColors(ConsoleColor.Blue, ConsoleColor.Black),
                LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
                LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
                LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
                LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
                _ => new ConsoleColors(null, null)
            };
        }

        // Taken from dotnet runtime
        private readonly struct ConsoleColors {
            public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background) {
                Foreground = foreground;
                Background = background;
            }

            public ConsoleColor? Foreground { get; }

            public ConsoleColor? Background { get; }
        }

    }

}
