using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlSprinkles
{
    public class ParameterManipulator
    {
        private IDictionary<Regex, string> Replacements { get; } = new Dictionary<Regex, string>();
        private string Pattern { get; } = @"(?<VariablePart>[ \t]*SET[ \t]+@?{0}[ \t]*=[ \t]*)(?<ValuePart>'[^'\r\n]+'|\S+)[ \t]*(?<CommentPart>--.*)?";

        public ParameterManipulator(IDictionary<string, string> replacements)
        {
            foreach (var item in replacements) {
                var re = new Regex(string.Format(Pattern, item.Key));
                var value = MakeSqlValue(item.Value);
                Replacements.Add(re, value);
            }
        }

        public string Replace(string sqlText)
        {
            foreach (var item in Replacements)
            {
                var re = item.Key;
                sqlText = re.Replace(sqlText, @"${VariablePart}" + item.Value + "${CommentPart} -- ${ValuePart} replaced by " + item.Value);
            }
            return sqlText;
        }

        private static string MakeSqlValue(string value)
        {
            if (value is null)
                return "NULL";
            else if (value.StartsWith("'") && value.EndsWith("'"))
                return value;
            else if (value.ToUpper() == "TRUE" || value.ToUpper() == "FALSE")
                return value;
            else if (decimal.TryParse(value, out decimal n))
                return n.ToString();
            return $"'{value}'";
        }
    }
}
