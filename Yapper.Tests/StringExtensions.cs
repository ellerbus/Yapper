using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yapper.Tests
{
    static class StringExtensions
    {
        public static string TrimAndReduceWhitespace(this string s)
        {
            return Regex.Replace(s, @"\s+", " ").Trim();
        }
    }
}
