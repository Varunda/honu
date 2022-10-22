
// This code comes from the dotnet runtime:
// https://github.com/dotnet/runtime/blob/3f40c3db794b1ee7100fb427e2bf3fe5f5b51a84/src/libraries/Microsoft.Extensions.Logging.Console/src/TextWriterExtensions.cs

/*
    The MIT License (MIT)

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

using System;
using System.IO;

namespace watchtower.Code.ExtensionMethods {

    public static class TextWriterExtensions {

        public static void WriteColoredMessage(this TextWriter textWriter, string message, ConsoleColor? background, ConsoleColor? foreground) {
            // Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
            if (background.HasValue) {
                textWriter.Write(AnsiParser.GetBackgroundColorEscapeCode(background.Value));
            }
            if (foreground.HasValue) {
                textWriter.Write(AnsiParser.GetForegroundColorEscapeCode(foreground.Value));
            }
            textWriter.Write(message);
            if (foreground.HasValue) {
                textWriter.Write(AnsiParser.DefaultForegroundColor); // reset to default foreground color
            }
            if (background.HasValue) {
                textWriter.Write(AnsiParser.DefaultBackgroundColor); // reset to the background color
            }
        }

    }
}
