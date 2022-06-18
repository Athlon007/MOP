// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MOP
{
    sealed class ModConsole : MSCLoader.ModConsole
    {
        // This class overrides default MSCLoader console messages,
        // in order to catch what MOP "says" and stores it into a list.

        static readonly List<string> messages = new List<string>();

        public new static void Log(string message)
        {
            Add(message);
            MSCLoader.ModConsole.Log(message);
        }

        public new static void LogError(string message)
        {
            Add($"[ERROR] {message}");
            MSCLoader.ModConsole.LogError(message);
        }

        public new static void LogWarning(string message)
        {
            Add($"[WARNING] {message}");
            MSCLoader.ModConsole.LogWarning(message);
        }

        static void Add(string message)
        {
            message = message.Replace("[MOP] ", "");
            message = message.Replace("<color=yellow>", "[WARNING] ");
            message = message.Replace("<color=green>", "[SYSTEM] ");
            message = message.Replace("<color=red>", "[ERROR] ");
            message = message.Trim();
            message = $"{DateTime.Now:HH:mm:ss.fff}: {message}";

            // Remove all things inside of <>.
            string[] messageArray = message.Split('\n');
            for (int i = 0; i < messageArray.Length; ++i)
            {
                if (i >= 1)
                {
                    messageArray[i] = new string(' ', 10) + messageArray[i];
                }
                messageArray[i] = Regex.Replace(messageArray[i], "<(.*?)>", "");
            }

            messages.Add(string.Join("\n", messageArray));
        }

        public static List<string> GetMessages()
        {
            return messages;
        }
    }
}
