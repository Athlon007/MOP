// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

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

namespace MOP.Common
{
    class WittyComments
    {
        static readonly string[] loadingWittyComments =
        {
            "Please wait...",
            //"Donate on Patreon, if you want your message to appear here!"
            "Refilling beer cases...",
            "Adding meat to sausages...",
            "Pumping up tires...",
            "Painting the vehicles...",
            "Adding rust to Satsuma..."
        };

        static readonly string[] errorComments =
        {
            "Well that's awkward",
            "Uh-oh!",
            "A specialised monkey has been dispatched to deal with this error",
            "My bad",
            "Oops",
            "It was all according to plan, I swear!",
            "D'oh!",
            "Fission Mailed",
            "Frick",
            "uwu compuwutew made a oopsy woopsy",
            "Slow down there buckaroo",
            "So you have chosen... crash",
            "Sorry :(",
            "Ouch!",
            "Bollocks!"
        };

        public static string GetWittyText()
        {
            Random rnd = new Random();
            return loadingWittyComments[rnd.Next(0, loadingWittyComments.Length - 1)];
        }

        public static string GetErrorWittyText()
        {
            Random rnd = new Random();
            return errorComments[rnd.Next(0, errorComments.Length - 1)];
        }

        public static string GetLoadingMessage()
        {
            Random rnd = new Random();
            return rnd.Next(0, 100) == 0 ? "HAVE A NICE DAY :)" : "LOADING MODERN OPTIMIZATION PLUGIN";
        }
    }
}
