using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlSprinkles
{
    public class ParameterManipulator
    {
        private IDictionary<Regex, string> Replacements { get; } = new Dictionary<Regex, string>();
        private string Pattern { get; } = @"(?<VariablePart>[ \t]*SET[ \t]+@?{0}[ \t]*=[ \t]*)(?<ValuePart>N?'[^'\r\n]+'|\S+)[ \t]*(?<CommentPart>--.*)?";

        [FlagsAttribute]
        public enum ParameterOptions
        {
            AlwaysUseNvarchar = 1
        }

        public ParameterOptions Options { get; } = new ParameterOptions();

        public ParameterManipulator(IDictionary<string, string> replacements, ParameterOptions options) 
        {
            Options |= options;

            foreach (var item in replacements)
            {
                var re = new Regex(string.Format(Pattern, item.Key));
                var value = MakeSqlValue(item.Value);
                Replacements.Add(re, value);
            }
        }

        public ParameterManipulator(IDictionary<string, string> replacements) :
            this (replacements, new ParameterOptions())
        { }

        public string Replace(string sqlText)
        {
            foreach (var item in Replacements)
            {
                var re = item.Key;
                sqlText = re.Replace(sqlText, @"${VariablePart}" + item.Value + "${CommentPart} -- ${ValuePart} replaced by " + item.Value);
            }
            return sqlText;
        }

        private string MakeSqlValue(string value)
        {
            if (value is null || value == "NULL")
                return "NULL";
            
            if (value.StartsWith("'") && value.EndsWith("'"))
                return CreateVarCharString(value.Substring(1, value.Length -2));

            if (value.StartsWith("N'") && value.EndsWith("'"))
                return CreateNvarCharString(value.Substring(2, value.Length - 3));

            if (value == "TRUE" || value == "FALSE")
                return value;
            
            if (decimal.TryParse(value, out decimal n))
                return n.ToString();
            
            return CreateVarCharString(value);
        }

        private string CreateVarCharString(string value)
        { 
            if ((Options & ParameterOptions.AlwaysUseNvarchar) == ParameterOptions.AlwaysUseNvarchar)
                return CreateNvarCharString(value);
            return "'" + value.Replace("'", "''") + "'";
        }
        private string CreateNvarCharString(string value)
            => "N'" + value.Replace("'", "''") + "'";
    }
}
