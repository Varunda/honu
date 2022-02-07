using System;
using System.Runtime.CompilerServices;

namespace DaybreakGames.Census
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class UriQueryPropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public UriQueryPropertyAttribute([CallerMemberName] string name = null)
        {
            Name = name.ToLower();
        }
    }
}
