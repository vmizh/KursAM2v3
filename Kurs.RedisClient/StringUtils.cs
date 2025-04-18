﻿

using ServiceStack.Script;
#if !NET6_0_OR_GREATER
using ServiceStack.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Text;
using System;
#endif

namespace ServiceStack;

public class TextNode
{
    public TextNode()
    {
        Children = new List<TextNode>();
    }

    public string Text { get; set; }

    public List<TextNode> Children { get; set; }
}

public static class StringUtils
{
    public static string Join(string delimiter, IEnumerable<string> values, int lineBreak=120, string linePrefix="")
    {
        var sb = StringBuilderCache.Allocate();
        var lastLine = 0;
        foreach (var value in values)
        {
            if (sb.Length > 0)
            {
                sb.Append(delimiter);
                if (sb.Length + value.Length > lastLine + lineBreak)
                {
                    sb.AppendLine();
                    lastLine = sb.Length;
                    sb.Append(linePrefix);
                }
            }
            sb.Append(value);
        }
        var text = StringBuilderCache.ReturnAndFree(sb);
        return text;
    }

    public static List<Command> ParseCommands(this string commandsString)
    {
        return commandsString.AsMemory().ParseCommands(',');
    }
    
    public static List<Command> ParseCommands(this ReadOnlyMemory<char> commandsString, 
        char separator = ',')
    {
        var to = new List<Command>();
        List<ReadOnlyMemory<char>> args = null;
        var pos = 0;

        if (commandsString.IsNullOrEmpty())
            return to;

        var inDoubleQuotes = false;
        var inSingleQuotes = false;
        var inBackTickQuotes = false;
        var inPrimeQuotes = false;
        var inBrackets = false;

        var endBlockPos = commandsString.Length;
        var cmd = new Command();
        var segmentStartPos = 0;

        try
        {
            for (var i = 0; i < commandsString.Length; i++)
            {
                var c = commandsString.Span[i];
                if (c.IsWhiteSpace())
                    continue;

                if (inDoubleQuotes)
                {
                    if (c == '"')
                        inDoubleQuotes = false;
                    continue;
                }
                if (inSingleQuotes)
                {
                    if (c == '\'')
                        inSingleQuotes = false;
                    continue;
                }
                if (inBackTickQuotes)
                {
                    if (c == '`')
                        inBackTickQuotes = false;
                    continue;
                }
                if (inPrimeQuotes)
                {
                    if (c == '′')
                        inPrimeQuotes = false;
                    continue;
                }
                switch (c)
                {
                    case '"':
                        inDoubleQuotes = true;
                        continue;
                    case '\'':
                        inSingleQuotes = true;
                        continue;
                    case '`':
                        inBackTickQuotes = true;
                        continue;
                    case '′':
                        inPrimeQuotes = true;
                        continue;
                }

                if (c == '(')
                {
                    inBrackets = true;
                    cmd.Name = commandsString.Slice(pos, i - pos).Trim().ToString();
                    pos = i + 1;

                    var literal = commandsString.Slice(pos);
                    var literalRemaining = ParseArguments(literal, out args);
                    cmd.Args = args;
                    var endPos = literal.Length - literalRemaining.Length;
                    
                    i += endPos;
                    pos = i + 1;
                    continue;
                }
                if (c == ')')
                {
                    inBrackets = false;
                    pos = i + 1;

                    pos = cmd.IndexOfMethodEnd(commandsString, pos);
                    continue;
                }

                if (inBrackets && c == ',')
                {
                    var arg = commandsString.Slice(pos, i - pos).Trim();
                    cmd.Args.Add(arg);
                    pos = i + 1;
                }
                else if (c == separator)
                {
                    if (string.IsNullOrEmpty(cmd.Name))
                        cmd.Name = commandsString.Slice(pos, i - pos).Trim().ToString();
                    else
                        cmd.Suffix = commandsString.Slice(pos - cmd.Suffix.Length, i - pos + cmd.Suffix.Length);

                    cmd.Original = commandsString.Slice(segmentStartPos, i - segmentStartPos).Trim();

                    to.Add(cmd);
                    cmd = new Command();
                    segmentStartPos = pos = i + 1;
                }
            }

            var remaining = commandsString.Slice(pos, endBlockPos - pos);
            if (!remaining.Trim().IsNullOrEmpty())
            {
                pos += remaining.Length;
                cmd.Name = remaining.Trim().ToString();
            }

            if (!cmd.Name.IsNullOrEmpty())
            {
                cmd.Original = commandsString.Slice(segmentStartPos, commandsString.Length - segmentStartPos).Trim();
                to.Add(cmd);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Illegal syntax near '{commandsString.SafeSlice(pos - 10, 50)}...'", e);
        }

        return to;
    }
    
    // ( {args} , {args} )
    //   ^
    public static ReadOnlyMemory<char> ParseArguments(ReadOnlyMemory<char> argsString, out List<ReadOnlyMemory<char>> args)
    {
        var to = new List<ReadOnlyMemory<char>>();

        var inDoubleQuotes = false;
        var inSingleQuotes = false;
        var inBackTickQuotes = false;
        var inPrimeQuotes = false;
        var inBrackets = 0;
        var inParens = 0;
        var inBraces = 0;
        var lastPos = 0;

        for (var i = 0; i < argsString.Length; i++)
        {
            var c = argsString.Span[i];
            if (inDoubleQuotes)
            {
                if (c == '"')
                    inDoubleQuotes = false;
                continue;
            }
            if (inSingleQuotes)
            {
                if (c == '\'')
                    inSingleQuotes = false;
                continue;
            }
            if (inBackTickQuotes)
            {
                if (c == '`')
                    inBackTickQuotes = false;
                continue;
            }
            if (inPrimeQuotes)
            {
                if (c == '′')
                    inPrimeQuotes = false;
                continue;
            }
            if (inBrackets > 0)
            {
                if (c == '[')
                    ++inBrackets;
                else if (c == ']')
                    --inBrackets;
                continue;
            }
            if (inBraces > 0)
            {
                if (c == '{')
                    ++inBraces;
                else if (c == '}')
                    --inBraces;
                continue;
            }
            if (inParens > 0)
            {
                if (c == '(')
                    ++inParens;
                else if (c == ')')
                    --inParens;
                continue;
            }

            switch (c)
            {
                case '"':
                    inDoubleQuotes = true;
                    continue;
                case '\'':
                    inSingleQuotes = true;
                    continue;
                case '`':
                    inBackTickQuotes = true;
                    continue;
                case '′':
                    inPrimeQuotes = true;
                    continue;
                case '[':
                    inBrackets++;
                    continue;
                case '{':
                    inBraces++;
                    continue;
                case '(':
                    inParens++;
                    continue;
                case ',':
                {
                    var arg = argsString.Slice(lastPos, i - lastPos).Trim();
                    to.Add(arg);
                    lastPos = i + 1;
                    continue;
                }
                case ')':
                {
                    var arg = argsString.Slice(lastPos, i - lastPos).Trim();
                    if (!arg.IsNullOrEmpty())
                    {
                        to.Add(arg);
                    }

                    args = to;
                    return argsString.Advance(i);
                }
            }
        }
        
        args = to;
        return TypeConstants.EmptyStringMemory;
    }

    /// <summary>
    /// Multiple string replacements
    /// </summary>
    /// <param name="str"></param>
    /// <param name="replaceStringsPairs">Even number of old and new value pairs</param>
    public static string ReplacePairs(string str, string[] replaceStringsPairs)
    {
        if (replaceStringsPairs.Length < 2 || replaceStringsPairs.Length % 2 != 0)
            throw new ArgumentException("Replacement pairs must be an even number of old and new value pairs", nameof(replaceStringsPairs));
        
        for (var i = 0; i < replaceStringsPairs.Length; i+=2)
        {
            str = str.Replace(replaceStringsPairs[i], replaceStringsPairs[i + 1]);
        }
        return str;
    }
    
    /// <summary>
    /// Replace string contents outside of string quotes 
    /// </summary>
    public static string ReplaceOutsideOfQuotes(this string str, params string[] replaceStringsPairs)
    {
        var inDoubleQuotes = false;
        var inSingleQuotes = false;
        var inBackTickQuotes = false;
        var inPrimeQuotes = false;
        var quoteStartPos = 0;
        var chunkLastPos = 0;

        var sb = StringBuilderCache.Allocate();

        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (i > 0 && c == '\\')
            {
                switch (str[i-1]) 
                {
                    case '"':
                    case '\'':
                    case '`':
                    case '′':
                        continue;
                }
            }

            if (inDoubleQuotes)
            {
                if (c == '"')
                {
                    sb.Append(str.Substring(quoteStartPos, (chunkLastPos = i) - quoteStartPos));
                    inDoubleQuotes = false;
                }
                continue;
            }
            if (inSingleQuotes)
            {
                if (c == '\'')
                {
                    sb.Append(str.Substring(quoteStartPos, (chunkLastPos = i) - quoteStartPos));
                    inSingleQuotes = false;
                }
                continue;
            }
            if (inBackTickQuotes)
            {
                if (c == '`')
                {
                    sb.Append(str.Substring(quoteStartPos, (chunkLastPos = i) - quoteStartPos));
                    inBackTickQuotes = false;
                }
                continue;
            }
            if (inPrimeQuotes)
            {
                if (c == '′')
                {
                    sb.Append(str.Substring(quoteStartPos, (chunkLastPos = i) - quoteStartPos));
                    inPrimeQuotes = false;
                }
                continue;
            }
            
            switch (c) 
            {
                case '"':
                case '\'':
                case '`':
                case '′':
                    var prevChunk = str.Substring(chunkLastPos, i-chunkLastPos);
                    sb.Append(ReplacePairs(prevChunk, replaceStringsPairs));
                    chunkLastPos = i;
                    quoteStartPos = i;
                    switch (c)
                    {
                        case '"':
                            inDoubleQuotes = true;
                            continue;
                        case '\'':
                            inSingleQuotes = true;
                            continue;
                        case '`':
                            inBackTickQuotes = true;
                            continue;
                        case '′':
                            inPrimeQuotes = true;
                            continue;
                    }
                    continue;
            }
        }

        var lastChunk = str.Substring(chunkLastPos);
        sb.Append(ReplacePairs(lastChunk, replaceStringsPairs));

        var ret = StringBuilderCache.ReturnAndFree(sb);
        return ret;
    }

    /// <summary>
    /// Protect against XSS by cleaning non-standard User Input
    /// </summary>
    public static string SafeInput(this string text)
    {
        return string.IsNullOrEmpty(text)
            ? text
            : SafeInputRegEx.Replace(text, "");
    }

    public static readonly Dictionary<char, string> EscapedCharMap = new Dictionary<char, string> {
        // {'\'', @"\'"},
        {'\"', "\\\""},
        {'\\', @"\\"},
        {'\0', @"\0"},
        {'\a', @"\a"},
        {'\b', @"\b"},
        {'\f', @"\f"},
        {'\n', @"\n"},
        {'\r', @"\r"},
        {'\t', @"\t"},
        {'\v', @"\v"},
    };

    public static string ToEscapedString(this string input)
    {
        var sb = new StringBuilder(input.Length + 2);
        sb.Append('"');
        foreach (var c in input)
        {
            if (EscapedCharMap.TryGetValue(c, out var escapedChar))
            {
                sb.Append(escapedChar);
            }
            else
            {
                if (char.GetUnicodeCategory(c) != UnicodeCategory.Control)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(@"\u");
                    sb.Append(((ushort) c).ToString("x4"));
                }
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    public static string SnakeCaseToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;
        
        var safeVarName = snakeCase.SafeVarName();
        if (safeVarName.IndexOf('_') >= 0)
        {
            var parts = safeVarName.Split('_').Where(x => !string.IsNullOrEmpty(x));
            var pascalName = "";
            foreach (var part in parts)
            {
                pascalName += char.ToUpper(part[0]) + part.Substring(1);
            }
            return pascalName;
        }
        return char.IsLower(safeVarName[0])
            ? char.ToUpper(safeVarName[0]) + safeVarName.Substring(1)
            : safeVarName;
    }

    public static string RemoveSuffix(string name, string suffix) => name == null ? null :
        name.EndsWith(suffix)
            ? name.Substring(0, name.Length - suffix.Length)
            : name;

    static readonly Regex StripHtmlUnicodeRegEx =
        new Regex(@"&(#)?([xX])?([^ \f\n\r\t\v;]+);", RegexOptions.Compiled);

    static readonly Regex SafeInputRegEx = new Regex(@"[^\w\s\.,@-\\+\\/]", RegexOptions.Compiled);

    public static string HtmlEncodeLite(this string html)
    {
        return html.Replace("<", "&lt;").Replace(">", "&gt;");
    }

    public static string HtmlEncode(this string html)
    {
        return System.Net.WebUtility.HtmlEncode(html).Replace("′", "&prime;");
    }

    public static string HtmlDecode(this string html)
    {
        return System.Net.WebUtility.HtmlDecode(html);
    }

    public static string ConvertHtmlCodes(this string html)
    {
        return StripHtmlUnicodeRegEx.Replace(html, ConvertHtmlCodeToCharacter);
    }

    static string ConvertHtmlCodeToCharacter(Match match)
    {
        // http://www.w3.org/TR/html5/syntax.html#character-references
        // match.Groups[0] is the entire match, the sub groups start at index one
        if (!match.Groups[1].Success)
        {
            if (HtmlCharacterCodes.TryGetValue(match.Value, out var convertedValue))
            {
                return convertedValue;
            }
            return match.Value; // ambiguous ampersand
        }
        string decimalString = match.Groups[3].Value;
        ushort decimalValue;
        if (match.Groups[2].Success)
        {
            bool parseWasSuccessful = ushort.TryParse(decimalString, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out decimalValue);
            if (!parseWasSuccessful)
            {
                return match.Value; // ambiguous ampersand
            }
        }
        else
        {
            bool parseWasSuccessful = ushort.TryParse(decimalString, out decimalValue);
            if (!parseWasSuccessful)
            {
                return match.Value; // ambiguous ampersand
            }
        }

        return Convert.ToString((char) decimalValue, CultureInfo.InvariantCulture);
    }

    public static string ToChar(this int codePoint)
    {
        return Convert.ToString(Convert.ToChar(codePoint), CultureInfo.InvariantCulture);
    }

    private static readonly char[] FieldSeparators = {',', ';'};
    public static string[] SplitVarNames(string fields)
    {
        if (string.IsNullOrEmpty(fields))
            return TypeConstants.EmptyStringArray;

        var sanitizedFields = fields.Trim().TrimEnd(FieldSeparators);
        if (string.IsNullOrEmpty(sanitizedFields))
            return TypeConstants.EmptyStringArray;
        
        return sanitizedFields
            .Split(FieldSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).ToArray();
    }

    public static List<string> SplitGenericArgs(string argList)
    {
        var to = new List<string>();
        if (string.IsNullOrEmpty(argList))
            return to;

        var lastPos = 0;
        var blockCount = 0;
        for (var i = 0; i < argList.Length; i++)
        {
            var argChar = argList[i];
            switch (argChar)
            {
                case ',':
                    if (blockCount == 0)
                    {
                        var arg = argList.Substring(lastPos, i - lastPos);
                        to.Add(arg);
                        lastPos = i + 1;
                    }
                    break;
                case '<':
                    blockCount++;
                    break;
                case '>':
                    blockCount--;
                    break;
            }
        }

        if (lastPos > 0)
        {
            var arg = argList.Substring(lastPos);
            to.Add(arg);
        }
        else
        {
            to.Add(argList);
        }

        return to;
    }

    static readonly char[] csharpGenericDelimChars = { '<', '>' };
    public static TextNode ParseTypeIntoNodes(this string typeDef) => typeDef.ParseTypeIntoNodes(csharpGenericDelimChars);
    public static TextNode ParseTypeIntoNodes(this string typeDef, char[] genericDelimChars)
    {
        if (string.IsNullOrEmpty(typeDef))
            return null;

        var openDelim = genericDelimChars[0];
        var openDelimStr = openDelim.ToString();

        var node = new TextNode();
        var lastBlockPos = typeDef.IndexOf(openDelim);

        if (lastBlockPos >= 0)
        {
            node.Text = typeDef.Substring(0, lastBlockPos).Trim();

            var blockStartingPos = new Stack<int>();
            blockStartingPos.Push(lastBlockPos);

            while (true)
            {
                var nextPos = typeDef.IndexOfAny(genericDelimChars, lastBlockPos + 1);
                if (nextPos == -1)
                    break;

                var blockChar = typeDef.Substring(nextPos, 1);

                if (blockChar == openDelimStr)
                {
                    blockStartingPos.Push(nextPos);
                }
                else
                {
                    var startPos = blockStartingPos.Pop();
                    if (blockStartingPos.Count == 0)
                    {
                        var endPos = nextPos;
                        var childBlock = typeDef.Substring(startPos + 1, endPos - startPos - 1);

                        var args = SplitGenericArgs(childBlock);
                        foreach (var arg in args)
                        {
                            if (arg.IndexOfAny(genericDelimChars) >= 0)
                            {
                                var childNode = ParseTypeIntoNodes(arg, genericDelimChars);
                                if (childNode != null)
                                {
                                    node.Children.Add(childNode);
                                }
                            }
                            else
                            {
                                node.Children.Add(new TextNode { Text = arg.Trim() });
                            }
                        }

                    }
                }

                lastBlockPos = nextPos;
            }
        }
        else
        {
            node.Text = typeDef.Trim();
        }

        return node;
    }

    public static ReadOnlyMemory<char> NewLineMemory = Environment.NewLine.AsMemory();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendLine(this StringBuilder sb, ReadOnlyMemory<char> line)
    {
#if NET6_0_OR_GREATER
        sb.Append(line).Append(NewLineMemory);
#else
        sb.AppendLine(line.ToString());
#endif
    }
    
    // http://www.w3.org/TR/html5/entities.json
    // TODO: conditional compilation for NET45 that uses ReadOnlyDictionary
    public static readonly IDictionary<string, string> HtmlCharacterCodes = new SortedDictionary<string, string> {
        {@"&Aacute;", 193.ToChar()},
        {@"&aacute;", 225.ToChar()},
        {@"&Abreve;", 258.ToChar()},
        {@"&abreve;", 259.ToChar()},
        {@"&ac;", 8766.ToChar()},
        {@"&acd;", 8767.ToChar()},
        {@"&acE;", 8766.ToChar()},
        {@"&Acirc;", 194.ToChar()},
        {@"&acirc;", 226.ToChar()},
        {@"&acute;", 180.ToChar()},
        {@"&Acy;", 1040.ToChar()},
        {@"&acy;", 1072.ToChar()},
        {@"&AElig;", 198.ToChar()},
        {@"&aelig;", 230.ToChar()},
        {@"&af;", 8289.ToChar()},
//{ @"&Afr;", 120068.ToChar() },
//{ @"&afr;", 120094.ToChar() },
        {@"&Agrave;", 192.ToChar()},
        {@"&agrave;", 224.ToChar()},
        {@"&alefsym;", 8501.ToChar()},
        {@"&aleph;", 8501.ToChar()},
        {@"&Alpha;", 913.ToChar()},
        {@"&alpha;", 945.ToChar()},
        {@"&Amacr;", 256.ToChar()},
        {@"&amacr;", 257.ToChar()},
        {@"&amalg;", 10815.ToChar()},
        {@"&AMP;", 38.ToChar()},
        {@"&amp;", 38.ToChar()},
        {@"&And;", 10835.ToChar()},
        {@"&and;", 8743.ToChar()},
        {@"&andand;", 10837.ToChar()},
        {@"&andd;", 10844.ToChar()},
        {@"&andslope;", 10840.ToChar()},
        {@"&andv;", 10842.ToChar()},
        {@"&ang;", 8736.ToChar()},
        {@"&ange;", 10660.ToChar()},
        {@"&angle;", 8736.ToChar()},
        {@"&angmsd;", 8737.ToChar()},
        {@"&angmsdaa;", 10664.ToChar()},
        {@"&angmsdab;", 10665.ToChar()},
        {@"&angmsdac;", 10666.ToChar()},
        {@"&angmsdad;", 10667.ToChar()},
        {@"&angmsdae;", 10668.ToChar()},
        {@"&angmsdaf;", 10669.ToChar()},
        {@"&angmsdag;", 10670.ToChar()},
        {@"&angmsdah;", 10671.ToChar()},
        {@"&angrt;", 8735.ToChar()},
        {@"&angrtvb;", 8894.ToChar()},
        {@"&angrtvbd;", 10653.ToChar()},
        {@"&angsph;", 8738.ToChar()},
        {@"&angst;", 197.ToChar()},
        {@"&angzarr;", 9084.ToChar()},
        {@"&Aogon;", 260.ToChar()},
        {@"&aogon;", 261.ToChar()},
//{ @"&Aopf;", 120120.ToChar() },
//{ @"&aopf;", 120146.ToChar() },
        {@"&ap;", 8776.ToChar()},
        {@"&apacir;", 10863.ToChar()},
        {@"&apE;", 10864.ToChar()},
        {@"&ape;", 8778.ToChar()},
        {@"&apid;", 8779.ToChar()},
        {@"&apos;", 39.ToChar()},
        {@"&ApplyFunction;", 8289.ToChar()},
        {@"&approx;", 8776.ToChar()},
        {@"&approxeq;", 8778.ToChar()},
        {@"&Aring;", 197.ToChar()},
        {@"&aring;", 229.ToChar()},
//{ @"&Ascr;", 119964.ToChar() },
//{ @"&ascr;", 119990.ToChar() },
        {@"&Assign;", 8788.ToChar()},
        {@"&ast;", 42.ToChar()},
        {@"&asymp;", 8776.ToChar()},
        {@"&asympeq;", 8781.ToChar()},
        {@"&Atilde;", 195.ToChar()},
        {@"&atilde;", 227.ToChar()},
        {@"&Auml;", 196.ToChar()},
        {@"&auml;", 228.ToChar()},
        {@"&awconint;", 8755.ToChar()},
        {@"&awint;", 10769.ToChar()},
        {@"&backcong;", 8780.ToChar()},
        {@"&backepsilon;", 1014.ToChar()},
        {@"&backprime;", 8245.ToChar()},
        {@"&backsim;", 8765.ToChar()},
        {@"&backsimeq;", 8909.ToChar()},
        {@"&Backslash;", 8726.ToChar()},
        {@"&Barv;", 10983.ToChar()},
        {@"&barvee;", 8893.ToChar()},
        {@"&Barwed;", 8966.ToChar()},
        {@"&barwed;", 8965.ToChar()},
        {@"&barwedge;", 8965.ToChar()},
        {@"&bbrk;", 9141.ToChar()},
        {@"&bbrktbrk;", 9142.ToChar()},
        {@"&bcong;", 8780.ToChar()},
        {@"&Bcy;", 1041.ToChar()},
        {@"&bcy;", 1073.ToChar()},
        {@"&bdquo;", 8222.ToChar()},
        {@"&becaus;", 8757.ToChar()},
        {@"&Because;", 8757.ToChar()},
        {@"&because;", 8757.ToChar()},
        {@"&bemptyv;", 10672.ToChar()},
        {@"&bepsi;", 1014.ToChar()},
        {@"&bernou;", 8492.ToChar()},
        {@"&Bernoullis;", 8492.ToChar()},
        {@"&Beta;", 914.ToChar()},
        {@"&beta;", 946.ToChar()},
        {@"&beth;", 8502.ToChar()},
        {@"&between;", 8812.ToChar()},
//{ @"&Bfr;", 120069.ToChar() },
//{ @"&bfr;", 120095.ToChar() },
        {@"&bigcap;", 8898.ToChar()},
        {@"&bigcirc;", 9711.ToChar()},
        {@"&bigcup;", 8899.ToChar()},
        {@"&bigodot;", 10752.ToChar()},
        {@"&bigoplus;", 10753.ToChar()},
        {@"&bigotimes;", 10754.ToChar()},
        {@"&bigsqcup;", 10758.ToChar()},
        {@"&bigstar;", 9733.ToChar()},
        {@"&bigtriangledown;", 9661.ToChar()},
        {@"&bigtriangleup;", 9651.ToChar()},
        {@"&biguplus;", 10756.ToChar()},
        {@"&bigvee;", 8897.ToChar()},
        {@"&bigwedge;", 8896.ToChar()},
        {@"&bkarow;", 10509.ToChar()},
        {@"&blacklozenge;", 10731.ToChar()},
        {@"&blacksquare;", 9642.ToChar()},
        {@"&blacktriangle;", 9652.ToChar()},
        {@"&blacktriangledown;", 9662.ToChar()},
        {@"&blacktriangleleft;", 9666.ToChar()},
        {@"&blacktriangleright;", 9656.ToChar()},
        {@"&blank;", 9251.ToChar()},
        {@"&blk12;", 9618.ToChar()},
        {@"&blk14;", 9617.ToChar()},
        {@"&blk34;", 9619.ToChar()},
        {@"&block;", 9608.ToChar()},
        {@"&bne;", 61.ToChar()},
        {@"&bnequiv;", 8801.ToChar()},
        {@"&bNot;", 10989.ToChar()},
        {@"&bnot;", 8976.ToChar()},
//{ @"&Bopf;", 120121.ToChar() },
//{ @"&bopf;", 120147.ToChar() },
        {@"&bot;", 8869.ToChar()},
        {@"&bottom;", 8869.ToChar()},
        {@"&bowtie;", 8904.ToChar()},
        {@"&boxbox;", 10697.ToChar()},
        {@"&boxDL;", 9559.ToChar()},
        {@"&boxDl;", 9558.ToChar()},
        {@"&boxdL;", 9557.ToChar()},
        {@"&boxdl;", 9488.ToChar()},
        {@"&boxDR;", 9556.ToChar()},
        {@"&boxDr;", 9555.ToChar()},
        {@"&boxdR;", 9554.ToChar()},
        {@"&boxdr;", 9484.ToChar()},
        {@"&boxH;", 9552.ToChar()},
        {@"&boxh;", 9472.ToChar()},
        {@"&boxHD;", 9574.ToChar()},
        {@"&boxHd;", 9572.ToChar()},
        {@"&boxhD;", 9573.ToChar()},
        {@"&boxhd;", 9516.ToChar()},
        {@"&boxHU;", 9577.ToChar()},
        {@"&boxHu;", 9575.ToChar()},
        {@"&boxhU;", 9576.ToChar()},
        {@"&boxhu;", 9524.ToChar()},
        {@"&boxminus;", 8863.ToChar()},
        {@"&boxplus;", 8862.ToChar()},
        {@"&boxtimes;", 8864.ToChar()},
        {@"&boxUL;", 9565.ToChar()},
        {@"&boxUl;", 9564.ToChar()},
        {@"&boxuL;", 9563.ToChar()},
        {@"&boxul;", 9496.ToChar()},
        {@"&boxUR;", 9562.ToChar()},
        {@"&boxUr;", 9561.ToChar()},
        {@"&boxuR;", 9560.ToChar()},
        {@"&boxur;", 9492.ToChar()},
        {@"&boxV;", 9553.ToChar()},
        {@"&boxv;", 9474.ToChar()},
        {@"&boxVH;", 9580.ToChar()},
        {@"&boxVh;", 9579.ToChar()},
        {@"&boxvH;", 9578.ToChar()},
        {@"&boxvh;", 9532.ToChar()},
        {@"&boxVL;", 9571.ToChar()},
        {@"&boxVl;", 9570.ToChar()},
        {@"&boxvL;", 9569.ToChar()},
        {@"&boxvl;", 9508.ToChar()},
        {@"&boxVR;", 9568.ToChar()},
        {@"&boxVr;", 9567.ToChar()},
        {@"&boxvR;", 9566.ToChar()},
        {@"&boxvr;", 9500.ToChar()},
        {@"&bprime;", 8245.ToChar()},
        {@"&Breve;", 728.ToChar()},
        {@"&breve;", 728.ToChar()},
        {@"&brvbar;", 166.ToChar()},
        {@"&Bscr;", 8492.ToChar()},
//{ @"&bscr;", 119991.ToChar() },
        {@"&bsemi;", 8271.ToChar()},
        {@"&bsim;", 8765.ToChar()},
        {@"&bsime;", 8909.ToChar()},
        {@"&bsol;", 92.ToChar()},
        {@"&bsolb;", 10693.ToChar()},
        {@"&bsolhsub;", 10184.ToChar()},
        {@"&bull;", 8226.ToChar()},
        {@"&bullet;", 8226.ToChar()},
        {@"&bump;", 8782.ToChar()},
        {@"&bumpE;", 10926.ToChar()},
        {@"&bumpe;", 8783.ToChar()},
        {@"&Bumpeq;", 8782.ToChar()},
        {@"&bumpeq;", 8783.ToChar()},
        {@"&Cacute;", 262.ToChar()},
        {@"&cacute;", 263.ToChar()},
        {@"&Cap;", 8914.ToChar()},
        {@"&cap;", 8745.ToChar()},
        {@"&capand;", 10820.ToChar()},
        {@"&capbrcup;", 10825.ToChar()},
        {@"&capcap;", 10827.ToChar()},
        {@"&capcup;", 10823.ToChar()},
        {@"&capdot;", 10816.ToChar()},
        {@"&CapitalDifferentialD;", 8517.ToChar()},
        {@"&caps;", 8745.ToChar()},
        {@"&caret;", 8257.ToChar()},
        {@"&caron;", 711.ToChar()},
        {@"&Cayleys;", 8493.ToChar()},
        {@"&ccaps;", 10829.ToChar()},
        {@"&Ccaron;", 268.ToChar()},
        {@"&ccaron;", 269.ToChar()},
        {@"&Ccedil;", 199.ToChar()},
        {@"&ccedil;", 231.ToChar()},
        {@"&Ccirc;", 264.ToChar()},
        {@"&ccirc;", 265.ToChar()},
        {@"&Cconint;", 8752.ToChar()},
        {@"&ccups;", 10828.ToChar()},
        {@"&ccupssm;", 10832.ToChar()},
        {@"&Cdot;", 266.ToChar()},
        {@"&cdot;", 267.ToChar()},
        {@"&cedil;", 184.ToChar()},
        {@"&Cedilla;", 184.ToChar()},
        {@"&cemptyv;", 10674.ToChar()},
        {@"&cent;", 162.ToChar()},
        {@"&CenterDot;", 183.ToChar()},
        {@"&centerdot;", 183.ToChar()},
        {@"&Cfr;", 8493.ToChar()},
//{ @"&cfr;", 120096.ToChar() },
        {@"&CHcy;", 1063.ToChar()},
        {@"&chcy;", 1095.ToChar()},
        {@"&check;", 10003.ToChar()},
        {@"&checkmark;", 10003.ToChar()},
        {@"&Chi;", 935.ToChar()},
        {@"&chi;", 967.ToChar()},
        {@"&cir;", 9675.ToChar()},
        {@"&circ;", 710.ToChar()},
        {@"&circeq;", 8791.ToChar()},
        {@"&circlearrowleft;", 8634.ToChar()},
        {@"&circlearrowright;", 8635.ToChar()},
        {@"&circledast;", 8859.ToChar()},
        {@"&circledcirc;", 8858.ToChar()},
        {@"&circleddash;", 8861.ToChar()},
        {@"&CircleDot;", 8857.ToChar()},
        {@"&circledR;", 174.ToChar()},
        {@"&circledS;", 9416.ToChar()},
        {@"&CircleMinus;", 8854.ToChar()},
        {@"&CirclePlus;", 8853.ToChar()},
        {@"&CircleTimes;", 8855.ToChar()},
        {@"&cirE;", 10691.ToChar()},
        {@"&cire;", 8791.ToChar()},
        {@"&cirfnint;", 10768.ToChar()},
        {@"&cirmid;", 10991.ToChar()},
        {@"&cirscir;", 10690.ToChar()},
        {@"&ClockwiseContourIntegral;", 8754.ToChar()},
        {@"&CloseCurlyDoubleQuote;", 8221.ToChar()},
        {@"&CloseCurlyQuote;", 8217.ToChar()},
        {@"&clubs;", 9827.ToChar()},
        {@"&clubsuit;", 9827.ToChar()},
        {@"&Colon;", 8759.ToChar()},
        {@"&colon;", 58.ToChar()},
        {@"&Colone;", 10868.ToChar()},
        {@"&colone;", 8788.ToChar()},
        {@"&coloneq;", 8788.ToChar()},
        {@"&comma;", 44.ToChar()},
        {@"&commat;", 64.ToChar()},
        {@"&comp;", 8705.ToChar()},
        {@"&compfn;", 8728.ToChar()},
        {@"&complement;", 8705.ToChar()},
        {@"&complexes;", 8450.ToChar()},
        {@"&cong;", 8773.ToChar()},
        {@"&congdot;", 10861.ToChar()},
        {@"&Congruent;", 8801.ToChar()},
        {@"&Conint;", 8751.ToChar()},
        {@"&conint;", 8750.ToChar()},
        {@"&ContourIntegral;", 8750.ToChar()},
        {@"&Copf;", 8450.ToChar()},
//{ @"&copf;", 120148.ToChar() },
        {@"&coprod;", 8720.ToChar()},
        {@"&Coproduct;", 8720.ToChar()},
        {@"&COPY;", 169.ToChar()},
        {@"&copy;", 169.ToChar()},
        {@"&copysr;", 8471.ToChar()},
        {@"&CounterClockwiseContourIntegral;", 8755.ToChar()},
        {@"&crarr;", 8629.ToChar()},
        {@"&Cross;", 10799.ToChar()},
        {@"&cross;", 10007.ToChar()},
//{ @"&Cscr;", 119966.ToChar() },
//{ @"&cscr;", 119992.ToChar() },
        {@"&csub;", 10959.ToChar()},
        {@"&csube;", 10961.ToChar()},
        {@"&csup;", 10960.ToChar()},
        {@"&csupe;", 10962.ToChar()},
        {@"&ctdot;", 8943.ToChar()},
        {@"&cudarrl;", 10552.ToChar()},
        {@"&cudarrr;", 10549.ToChar()},
        {@"&cuepr;", 8926.ToChar()},
        {@"&cuesc;", 8927.ToChar()},
        {@"&cularr;", 8630.ToChar()},
        {@"&cularrp;", 10557.ToChar()},
        {@"&Cup;", 8915.ToChar()},
        {@"&cup;", 8746.ToChar()},
        {@"&cupbrcap;", 10824.ToChar()},
        {@"&CupCap;", 8781.ToChar()},
        {@"&cupcap;", 10822.ToChar()},
        {@"&cupcup;", 10826.ToChar()},
        {@"&cupdot;", 8845.ToChar()},
        {@"&cupor;", 10821.ToChar()},
        {@"&cups;", 8746.ToChar()},
        {@"&curarr;", 8631.ToChar()},
        {@"&curarrm;", 10556.ToChar()},
        {@"&curlyeqprec;", 8926.ToChar()},
        {@"&curlyeqsucc;", 8927.ToChar()},
        {@"&curlyvee;", 8910.ToChar()},
        {@"&curlywedge;", 8911.ToChar()},
        {@"&curren;", 164.ToChar()},
        {@"&curvearrowleft;", 8630.ToChar()},
        {@"&curvearrowright;", 8631.ToChar()},
        {@"&cuvee;", 8910.ToChar()},
        {@"&cuwed;", 8911.ToChar()},
        {@"&cwconint;", 8754.ToChar()},
        {@"&cwint;", 8753.ToChar()},
        {@"&cylcty;", 9005.ToChar()},
        {@"&Dagger;", 8225.ToChar()},
        {@"&dagger;", 8224.ToChar()},
        {@"&daleth;", 8504.ToChar()},
        {@"&Darr;", 8609.ToChar()},
        {@"&dArr;", 8659.ToChar()},
        {@"&darr;", 8595.ToChar()},
        {@"&dash;", 8208.ToChar()},
        {@"&Dashv;", 10980.ToChar()},
        {@"&dashv;", 8867.ToChar()},
        {@"&dbkarow;", 10511.ToChar()},
        {@"&dblac;", 733.ToChar()},
        {@"&Dcaron;", 270.ToChar()},
        {@"&dcaron;", 271.ToChar()},
        {@"&Dcy;", 1044.ToChar()},
        {@"&dcy;", 1076.ToChar()},
        {@"&DD;", 8517.ToChar()},
        {@"&dd;", 8518.ToChar()},
        {@"&ddagger;", 8225.ToChar()},
        {@"&ddarr;", 8650.ToChar()},
        {@"&DDotrahd;", 10513.ToChar()},
        {@"&ddotseq;", 10871.ToChar()},
        {@"&deg;", 176.ToChar()},
        {@"&Del;", 8711.ToChar()},
        {@"&Delta;", 916.ToChar()},
        {@"&delta;", 948.ToChar()},
        {@"&demptyv;", 10673.ToChar()},
        {@"&dfisht;", 10623.ToChar()},
//{ @"&Dfr;", 120071.ToChar() },
//{ @"&dfr;", 120097.ToChar() },
        {@"&dHar;", 10597.ToChar()},
        {@"&dharl;", 8643.ToChar()},
        {@"&dharr;", 8642.ToChar()},
        {@"&DiacriticalAcute;", 180.ToChar()},
        {@"&DiacriticalDot;", 729.ToChar()},
        {@"&DiacriticalDoubleAcute;", 733.ToChar()},
        {@"&DiacriticalGrave;", 96.ToChar()},
        {@"&DiacriticalTilde;", 732.ToChar()},
        {@"&diam;", 8900.ToChar()},
        {@"&Diamond;", 8900.ToChar()},
        {@"&diamond;", 8900.ToChar()},
        {@"&diamondsuit;", 9830.ToChar()},
        {@"&diams;", 9830.ToChar()},
        {@"&die;", 168.ToChar()},
        {@"&DifferentialD;", 8518.ToChar()},
        {@"&digamma;", 989.ToChar()},
        {@"&disin;", 8946.ToChar()},
        {@"&div;", 247.ToChar()},
        {@"&divide;", 247.ToChar()},
        {@"&divideontimes;", 8903.ToChar()},
        {@"&divonx;", 8903.ToChar()},
        {@"&DJcy;", 1026.ToChar()},
        {@"&djcy;", 1106.ToChar()},
        {@"&dlcorn;", 8990.ToChar()},
        {@"&dlcrop;", 8973.ToChar()},
        {@"&dollar;", 36.ToChar()},
//{ @"&Dopf;", 120123.ToChar() },
//{ @"&dopf;", 120149.ToChar() },
        {@"&Dot;", 168.ToChar()},
        {@"&dot;", 729.ToChar()},
        {@"&DotDot;", 8412.ToChar()},
        {@"&doteq;", 8784.ToChar()},
        {@"&doteqdot;", 8785.ToChar()},
        {@"&DotEqual;", 8784.ToChar()},
        {@"&dotminus;", 8760.ToChar()},
        {@"&dotplus;", 8724.ToChar()},
        {@"&dotsquare;", 8865.ToChar()},
        {@"&doublebarwedge;", 8966.ToChar()},
        {@"&DoubleContourIntegral;", 8751.ToChar()},
        {@"&DoubleDot;", 168.ToChar()},
        {@"&DoubleDownArrow;", 8659.ToChar()},
        {@"&DoubleLeftArrow;", 8656.ToChar()},
        {@"&DoubleLeftRightArrow;", 8660.ToChar()},
        {@"&DoubleLeftTee;", 10980.ToChar()},
        {@"&DoubleLongLeftArrow;", 10232.ToChar()},
        {@"&DoubleLongLeftRightArrow;", 10234.ToChar()},
        {@"&DoubleLongRightArrow;", 10233.ToChar()},
        {@"&DoubleRightArrow;", 8658.ToChar()},
        {@"&DoubleRightTee;", 8872.ToChar()},
        {@"&DoubleUpArrow;", 8657.ToChar()},
        {@"&DoubleUpDownArrow;", 8661.ToChar()},
        {@"&DoubleVerticalBar;", 8741.ToChar()},
        {@"&DownArrow;", 8595.ToChar()},
        {@"&Downarrow;", 8659.ToChar()},
        {@"&downarrow;", 8595.ToChar()},
        {@"&DownArrowBar;", 10515.ToChar()},
        {@"&DownArrowUpArrow;", 8693.ToChar()},
        {@"&DownBreve;", 785.ToChar()},
        {@"&downdownarrows;", 8650.ToChar()},
        {@"&downharpoonleft;", 8643.ToChar()},
        {@"&downharpoonright;", 8642.ToChar()},
        {@"&DownLeftRightVector;", 10576.ToChar()},
        {@"&DownLeftTeeVector;", 10590.ToChar()},
        {@"&DownLeftVector;", 8637.ToChar()},
        {@"&DownLeftVectorBar;", 10582.ToChar()},
        {@"&DownRightTeeVector;", 10591.ToChar()},
        {@"&DownRightVector;", 8641.ToChar()},
        {@"&DownRightVectorBar;", 10583.ToChar()},
        {@"&DownTee;", 8868.ToChar()},
        {@"&DownTeeArrow;", 8615.ToChar()},
        {@"&drbkarow;", 10512.ToChar()},
        {@"&drcorn;", 8991.ToChar()},
        {@"&drcrop;", 8972.ToChar()},
//{ @"&Dscr;", 119967.ToChar() },
//{ @"&dscr;", 119993.ToChar() },
        {@"&DScy;", 1029.ToChar()},
        {@"&dscy;", 1109.ToChar()},
        {@"&dsol;", 10742.ToChar()},
        {@"&Dstrok;", 272.ToChar()},
        {@"&dstrok;", 273.ToChar()},
        {@"&dtdot;", 8945.ToChar()},
        {@"&dtri;", 9663.ToChar()},
        {@"&dtrif;", 9662.ToChar()},
        {@"&duarr;", 8693.ToChar()},
        {@"&duhar;", 10607.ToChar()},
        {@"&dwangle;", 10662.ToChar()},
        {@"&DZcy;", 1039.ToChar()},
        {@"&dzcy;", 1119.ToChar()},
        {@"&dzigrarr;", 10239.ToChar()},
        {@"&Eacute;", 201.ToChar()},
        {@"&eacute;", 233.ToChar()},
        {@"&easter;", 10862.ToChar()},
        {@"&Ecaron;", 282.ToChar()},
        {@"&ecaron;", 283.ToChar()},
        {@"&ecir;", 8790.ToChar()},
        {@"&Ecirc;", 202.ToChar()},
        {@"&ecirc;", 234.ToChar()},
        {@"&ecolon;", 8789.ToChar()},
        {@"&Ecy;", 1069.ToChar()},
        {@"&ecy;", 1101.ToChar()},
        {@"&eDDot;", 10871.ToChar()},
        {@"&Edot;", 278.ToChar()},
        {@"&eDot;", 8785.ToChar()},
        {@"&edot;", 279.ToChar()},
        {@"&ee;", 8519.ToChar()},
        {@"&efDot;", 8786.ToChar()},
//{ @"&Efr;", 120072.ToChar() },
//{ @"&efr;", 120098.ToChar() },
        {@"&eg;", 10906.ToChar()},
        {@"&Egrave;", 200.ToChar()},
        {@"&egrave;", 232.ToChar()},
        {@"&egs;", 10902.ToChar()},
        {@"&egsdot;", 10904.ToChar()},
        {@"&el;", 10905.ToChar()},
        {@"&Element;", 8712.ToChar()},
        {@"&elinters;", 9191.ToChar()},
        {@"&ell;", 8467.ToChar()},
        {@"&els;", 10901.ToChar()},
        {@"&elsdot;", 10903.ToChar()},
        {@"&Emacr;", 274.ToChar()},
        {@"&emacr;", 275.ToChar()},
        {@"&empty;", 8709.ToChar()},
        {@"&emptyset;", 8709.ToChar()},
        {@"&EmptySmallSquare;", 9723.ToChar()},
        {@"&emptyv;", 8709.ToChar()},
        {@"&EmptyVerySmallSquare;", 9643.ToChar()},
        {@"&emsp;", 8195.ToChar()},
        {@"&emsp13;", 8196.ToChar()},
        {@"&emsp14;", 8197.ToChar()},
        {@"&ENG;", 330.ToChar()},
        {@"&eng;", 331.ToChar()},
        {@"&ensp;", 8194.ToChar()},
        {@"&Eogon;", 280.ToChar()},
        {@"&eogon;", 281.ToChar()},
//{ @"&Eopf;", 120124.ToChar() },
//{ @"&eopf;", 120150.ToChar() },
        {@"&epar;", 8917.ToChar()},
        {@"&eparsl;", 10723.ToChar()},
        {@"&eplus;", 10865.ToChar()},
        {@"&epsi;", 949.ToChar()},
        {@"&Epsilon;", 917.ToChar()},
        {@"&epsilon;", 949.ToChar()},
        {@"&epsiv;", 1013.ToChar()},
        {@"&eqcirc;", 8790.ToChar()},
        {@"&eqcolon;", 8789.ToChar()},
        {@"&eqsim;", 8770.ToChar()},
        {@"&eqslantgtr;", 10902.ToChar()},
        {@"&eqslantless;", 10901.ToChar()},
        {@"&Equal;", 10869.ToChar()},
        {@"&equals;", 61.ToChar()},
        {@"&EqualTilde;", 8770.ToChar()},
        {@"&equest;", 8799.ToChar()},
        {@"&Equilibrium;", 8652.ToChar()},
        {@"&equiv;", 8801.ToChar()},
        {@"&equivDD;", 10872.ToChar()},
        {@"&eqvparsl;", 10725.ToChar()},
        {@"&erarr;", 10609.ToChar()},
        {@"&erDot;", 8787.ToChar()},
        {@"&Escr;", 8496.ToChar()},
        {@"&escr;", 8495.ToChar()},
        {@"&esdot;", 8784.ToChar()},
        {@"&Esim;", 10867.ToChar()},
        {@"&esim;", 8770.ToChar()},
        {@"&Eta;", 919.ToChar()},
        {@"&eta;", 951.ToChar()},
        {@"&ETH;", 208.ToChar()},
        {@"&eth;", 240.ToChar()},
        {@"&Euml;", 203.ToChar()},
        {@"&euml;", 235.ToChar()},
        {@"&euro;", 8364.ToChar()},
        {@"&excl;", 33.ToChar()},
        {@"&exist;", 8707.ToChar()},
        {@"&Exists;", 8707.ToChar()},
        {@"&expectation;", 8496.ToChar()},
        {@"&ExponentialE;", 8519.ToChar()},
        {@"&exponentiale;", 8519.ToChar()},
        {@"&fallingdotseq;", 8786.ToChar()},
        {@"&Fcy;", 1060.ToChar()},
        {@"&fcy;", 1092.ToChar()},
        {@"&female;", 9792.ToChar()},
        {@"&ffilig;", 64259.ToChar()},
        {@"&fflig;", 64256.ToChar()},
        {@"&ffllig;", 64260.ToChar()},
//{ @"&Ffr;", 120073.ToChar() },
//{ @"&ffr;", 120099.ToChar() },
        {@"&filig;", 64257.ToChar()},
        {@"&FilledSmallSquare;", 9724.ToChar()},
        {@"&FilledVerySmallSquare;", 9642.ToChar()},
        {@"&fjlig;", 102.ToChar()},
        {@"&flat;", 9837.ToChar()},
        {@"&fllig;", 64258.ToChar()},
        {@"&fltns;", 9649.ToChar()},
        {@"&fnof;", 402.ToChar()},
//{ @"&Fopf;", 120125.ToChar() },
//{ @"&fopf;", 120151.ToChar() },
        {@"&ForAll;", 8704.ToChar()},
        {@"&forall;", 8704.ToChar()},
        {@"&fork;", 8916.ToChar()},
        {@"&forkv;", 10969.ToChar()},
        {@"&Fouriertrf;", 8497.ToChar()},
        {@"&fpartint;", 10765.ToChar()},
        {@"&frac12;", 189.ToChar()},
        {@"&frac13;", 8531.ToChar()},
        {@"&frac14;", 188.ToChar()},
        {@"&frac15;", 8533.ToChar()},
        {@"&frac16;", 8537.ToChar()},
        {@"&frac18;", 8539.ToChar()},
        {@"&frac23;", 8532.ToChar()},
        {@"&frac25;", 8534.ToChar()},
        {@"&frac34;", 190.ToChar()},
        {@"&frac35;", 8535.ToChar()},
        {@"&frac38;", 8540.ToChar()},
        {@"&frac45;", 8536.ToChar()},
        {@"&frac56;", 8538.ToChar()},
        {@"&frac58;", 8541.ToChar()},
        {@"&frac78;", 8542.ToChar()},
        {@"&frasl;", 8260.ToChar()},
        {@"&frown;", 8994.ToChar()},
        {@"&Fscr;", 8497.ToChar()},
//{ @"&fscr;", 119995.ToChar() },
        {@"&gacute;", 501.ToChar()},
        {@"&Gamma;", 915.ToChar()},
        {@"&gamma;", 947.ToChar()},
        {@"&Gammad;", 988.ToChar()},
        {@"&gammad;", 989.ToChar()},
        {@"&gap;", 10886.ToChar()},
        {@"&Gbreve;", 286.ToChar()},
        {@"&gbreve;", 287.ToChar()},
        {@"&Gcedil;", 290.ToChar()},
        {@"&Gcirc;", 284.ToChar()},
        {@"&gcirc;", 285.ToChar()},
        {@"&Gcy;", 1043.ToChar()},
        {@"&gcy;", 1075.ToChar()},
        {@"&Gdot;", 288.ToChar()},
        {@"&gdot;", 289.ToChar()},
        {@"&gE;", 8807.ToChar()},
        {@"&ge;", 8805.ToChar()},
        {@"&gEl;", 10892.ToChar()},
        {@"&gel;", 8923.ToChar()},
        {@"&geq;", 8805.ToChar()},
        {@"&geqq;", 8807.ToChar()},
        {@"&geqslant;", 10878.ToChar()},
        {@"&ges;", 10878.ToChar()},
        {@"&gescc;", 10921.ToChar()},
        {@"&gesdot;", 10880.ToChar()},
        {@"&gesdoto;", 10882.ToChar()},
        {@"&gesdotol;", 10884.ToChar()},
        {@"&gesl;", 8923.ToChar()},
        {@"&gesles;", 10900.ToChar()},
//{ @"&Gfr;", 120074.ToChar() },
//{ @"&gfr;", 120100.ToChar() },
        {@"&Gg;", 8921.ToChar()},
        {@"&gg;", 8811.ToChar()},
        {@"&ggg;", 8921.ToChar()},
        {@"&gimel;", 8503.ToChar()},
        {@"&GJcy;", 1027.ToChar()},
        {@"&gjcy;", 1107.ToChar()},
        {@"&gl;", 8823.ToChar()},
        {@"&gla;", 10917.ToChar()},
        {@"&glE;", 10898.ToChar()},
        {@"&glj;", 10916.ToChar()},
        {@"&gnap;", 10890.ToChar()},
        {@"&gnapprox;", 10890.ToChar()},
        {@"&gnE;", 8809.ToChar()},
        {@"&gne;", 10888.ToChar()},
        {@"&gneq;", 10888.ToChar()},
        {@"&gneqq;", 8809.ToChar()},
        {@"&gnsim;", 8935.ToChar()},
//{ @"&Gopf;", 120126.ToChar() },
//{ @"&gopf;", 120152.ToChar() },
        {@"&grave;", 96.ToChar()},
        {@"&GreaterEqual;", 8805.ToChar()},
        {@"&GreaterEqualLess;", 8923.ToChar()},
        {@"&GreaterFullEqual;", 8807.ToChar()},
        {@"&GreaterGreater;", 10914.ToChar()},
        {@"&GreaterLess;", 8823.ToChar()},
        {@"&GreaterSlantEqual;", 10878.ToChar()},
        {@"&GreaterTilde;", 8819.ToChar()},
//{ @"&Gscr;", 119970.ToChar() },
        {@"&gscr;", 8458.ToChar()},
        {@"&gsim;", 8819.ToChar()},
        {@"&gsime;", 10894.ToChar()},
        {@"&gsiml;", 10896.ToChar()},
        {@"&GT;", 62.ToChar()},
        {@"&Gt;", 8811.ToChar()},
        {@"&gt;", 62.ToChar()},
        {@"&gtcc;", 10919.ToChar()},
        {@"&gtcir;", 10874.ToChar()},
        {@"&gtdot;", 8919.ToChar()},
        {@"&gtlPar;", 10645.ToChar()},
        {@"&gtquest;", 10876.ToChar()},
        {@"&gtrapprox;", 10886.ToChar()},
        {@"&gtrarr;", 10616.ToChar()},
        {@"&gtrdot;", 8919.ToChar()},
        {@"&gtreqless;", 8923.ToChar()},
        {@"&gtreqqless;", 10892.ToChar()},
        {@"&gtrless;", 8823.ToChar()},
        {@"&gtrsim;", 8819.ToChar()},
        {@"&gvertneqq;", 8809.ToChar()},
        {@"&gvnE;", 8809.ToChar()},
        {@"&Hacek;", 711.ToChar()},
        {@"&hairsp;", 8202.ToChar()},
        {@"&half;", 189.ToChar()},
        {@"&hamilt;", 8459.ToChar()},
        {@"&HARDcy;", 1066.ToChar()},
        {@"&hardcy;", 1098.ToChar()},
        {@"&hArr;", 8660.ToChar()},
        {@"&harr;", 8596.ToChar()},
        {@"&harrcir;", 10568.ToChar()},
        {@"&harrw;", 8621.ToChar()},
        {@"&Hat;", 94.ToChar()},
        {@"&hbar;", 8463.ToChar()},
        {@"&Hcirc;", 292.ToChar()},
        {@"&hcirc;", 293.ToChar()},
        {@"&hearts;", 9829.ToChar()},
        {@"&heartsuit;", 9829.ToChar()},
        {@"&hellip;", 8230.ToChar()},
        {@"&hercon;", 8889.ToChar()},
        {@"&Hfr;", 8460.ToChar()},
//{ @"&hfr;", 120101.ToChar() },
        {@"&HilbertSpace;", 8459.ToChar()},
        {@"&hksearow;", 10533.ToChar()},
        {@"&hkswarow;", 10534.ToChar()},
        {@"&hoarr;", 8703.ToChar()},
        {@"&homtht;", 8763.ToChar()},
        {@"&hookleftarrow;", 8617.ToChar()},
        {@"&hookrightarrow;", 8618.ToChar()},
        {@"&Hopf;", 8461.ToChar()},
//{ @"&hopf;", 120153.ToChar() },
        {@"&horbar;", 8213.ToChar()},
        {@"&HorizontalLine;", 9472.ToChar()},
        {@"&Hscr;", 8459.ToChar()},
//{ @"&hscr;", 119997.ToChar() },
        {@"&hslash;", 8463.ToChar()},
        {@"&Hstrok;", 294.ToChar()},
        {@"&hstrok;", 295.ToChar()},
        {@"&HumpDownHump;", 8782.ToChar()},
        {@"&HumpEqual;", 8783.ToChar()},
        {@"&hybull;", 8259.ToChar()},
        {@"&hyphen;", 8208.ToChar()},
        {@"&Iacute;", 205.ToChar()},
        {@"&iacute;", 237.ToChar()},
        {@"&ic;", 8291.ToChar()},
        {@"&Icirc;", 206.ToChar()},
        {@"&icirc;", 238.ToChar()},
        {@"&Icy;", 1048.ToChar()},
        {@"&icy;", 1080.ToChar()},
        {@"&Idot;", 304.ToChar()},
        {@"&IEcy;", 1045.ToChar()},
        {@"&iecy;", 1077.ToChar()},
        {@"&iexcl;", 161.ToChar()},
        {@"&iff;", 8660.ToChar()},
        {@"&Ifr;", 8465.ToChar()},
//{ @"&ifr;", 120102.ToChar() },
        {@"&Igrave;", 204.ToChar()},
        {@"&igrave;", 236.ToChar()},
        {@"&ii;", 8520.ToChar()},
        {@"&iiiint;", 10764.ToChar()},
        {@"&iiint;", 8749.ToChar()},
        {@"&iinfin;", 10716.ToChar()},
        {@"&iiota;", 8489.ToChar()},
        {@"&IJlig;", 306.ToChar()},
        {@"&ijlig;", 307.ToChar()},
        {@"&Im;", 8465.ToChar()},
        {@"&Imacr;", 298.ToChar()},
        {@"&imacr;", 299.ToChar()},
        {@"&image;", 8465.ToChar()},
        {@"&ImaginaryI;", 8520.ToChar()},
        {@"&imagline;", 8464.ToChar()},
        {@"&imagpart;", 8465.ToChar()},
        {@"&imath;", 305.ToChar()},
        {@"&imof;", 8887.ToChar()},
        {@"&imped;", 437.ToChar()},
        {@"&Implies;", 8658.ToChar()},
        {@"&in;", 8712.ToChar()},
        {@"&incare;", 8453.ToChar()},
        {@"&infin;", 8734.ToChar()},
        {@"&infintie;", 10717.ToChar()},
        {@"&inodot;", 305.ToChar()},
        {@"&Int;", 8748.ToChar()},
        {@"&int;", 8747.ToChar()},
        {@"&intcal;", 8890.ToChar()},
        {@"&integers;", 8484.ToChar()},
        {@"&Integral;", 8747.ToChar()},
        {@"&intercal;", 8890.ToChar()},
        {@"&Intersection;", 8898.ToChar()},
        {@"&intlarhk;", 10775.ToChar()},
        {@"&intprod;", 10812.ToChar()},
        {@"&InvisibleComma;", 8291.ToChar()},
        {@"&InvisibleTimes;", 8290.ToChar()},
        {@"&IOcy;", 1025.ToChar()},
        {@"&iocy;", 1105.ToChar()},
        {@"&Iogon;", 302.ToChar()},
        {@"&iogon;", 303.ToChar()},
//{ @"&Iopf;", 120128.ToChar() },
//{ @"&iopf;", 120154.ToChar() },
        {@"&Iota;", 921.ToChar()},
        {@"&iota;", 953.ToChar()},
        {@"&iprod;", 10812.ToChar()},
        {@"&iquest;", 191.ToChar()},
        {@"&Iscr;", 8464.ToChar()},
//{ @"&iscr;", 119998.ToChar() },
        {@"&isin;", 8712.ToChar()},
        {@"&isindot;", 8949.ToChar()},
        {@"&isinE;", 8953.ToChar()},
        {@"&isins;", 8948.ToChar()},
        {@"&isinsv;", 8947.ToChar()},
        {@"&isinv;", 8712.ToChar()},
        {@"&it;", 8290.ToChar()},
        {@"&Itilde;", 296.ToChar()},
        {@"&itilde;", 297.ToChar()},
        {@"&Iukcy;", 1030.ToChar()},
        {@"&iukcy;", 1110.ToChar()},
        {@"&Iuml;", 207.ToChar()},
        {@"&iuml;", 239.ToChar()},
        {@"&Jcirc;", 308.ToChar()},
        {@"&jcirc;", 309.ToChar()},
        {@"&Jcy;", 1049.ToChar()},
        {@"&jcy;", 1081.ToChar()},
//{ @"&Jfr;", 120077.ToChar() },
//{ @"&jfr;", 120103.ToChar() },
        {@"&jmath;", 567.ToChar()},
//{ @"&Jopf;", 120129.ToChar() },
//{ @"&jopf;", 120155.ToChar() },
//{ @"&Jscr;", 119973.ToChar() },
//{ @"&jscr;", 119999.ToChar() },
        {@"&Jsercy;", 1032.ToChar()},
        {@"&jsercy;", 1112.ToChar()},
        {@"&Jukcy;", 1028.ToChar()},
        {@"&jukcy;", 1108.ToChar()},
        {@"&Kappa;", 922.ToChar()},
        {@"&kappa;", 954.ToChar()},
        {@"&kappav;", 1008.ToChar()},
        {@"&Kcedil;", 310.ToChar()},
        {@"&kcedil;", 311.ToChar()},
        {@"&Kcy;", 1050.ToChar()},
        {@"&kcy;", 1082.ToChar()},
//{ @"&Kfr;", 120078.ToChar() },
//{ @"&kfr;", 120104.ToChar() },
        {@"&kgreen;", 312.ToChar()},
        {@"&KHcy;", 1061.ToChar()},
        {@"&khcy;", 1093.ToChar()},
        {@"&KJcy;", 1036.ToChar()},
        {@"&kjcy;", 1116.ToChar()},
//{ @"&Kopf;", 120130.ToChar() },
//{ @"&kopf;", 120156.ToChar() },
//{ @"&Kscr;", 119974.ToChar() },
//{ @"&kscr;", 120000.ToChar() },
        {@"&lAarr;", 8666.ToChar()},
        {@"&Lacute;", 313.ToChar()},
        {@"&lacute;", 314.ToChar()},
        {@"&laemptyv;", 10676.ToChar()},
        {@"&lagran;", 8466.ToChar()},
        {@"&Lambda;", 923.ToChar()},
        {@"&lambda;", 955.ToChar()},
        {@"&Lang;", 10218.ToChar()},
        {@"&lang;", 10216.ToChar()},
        {@"&langd;", 10641.ToChar()},
        {@"&langle;", 10216.ToChar()},
        {@"&lap;", 10885.ToChar()},
        {@"&Laplacetrf;", 8466.ToChar()},
        {@"&laquo;", 171.ToChar()},
        {@"&Larr;", 8606.ToChar()},
        {@"&lArr;", 8656.ToChar()},
        {@"&larr;", 8592.ToChar()},
        {@"&larrb;", 8676.ToChar()},
        {@"&larrbfs;", 10527.ToChar()},
        {@"&larrfs;", 10525.ToChar()},
        {@"&larrhk;", 8617.ToChar()},
        {@"&larrlp;", 8619.ToChar()},
        {@"&larrpl;", 10553.ToChar()},
        {@"&larrsim;", 10611.ToChar()},
        {@"&larrtl;", 8610.ToChar()},
        {@"&lat;", 10923.ToChar()},
        {@"&lAtail;", 10523.ToChar()},
        {@"&latail;", 10521.ToChar()},
        {@"&late;", 10925.ToChar()},
        {@"&lates;", 10925.ToChar()},
        {@"&lBarr;", 10510.ToChar()},
        {@"&lbarr;", 10508.ToChar()},
        {@"&lbbrk;", 10098.ToChar()},
        {@"&lbrace;", 123.ToChar()},
        {@"&lbrack;", 91.ToChar()},
        {@"&lbrke;", 10635.ToChar()},
        {@"&lbrksld;", 10639.ToChar()},
        {@"&lbrkslu;", 10637.ToChar()},
        {@"&Lcaron;", 317.ToChar()},
        {@"&lcaron;", 318.ToChar()},
        {@"&Lcedil;", 315.ToChar()},
        {@"&lcedil;", 316.ToChar()},
        {@"&lceil;", 8968.ToChar()},
        {@"&lcub;", 123.ToChar()},
        {@"&Lcy;", 1051.ToChar()},
        {@"&lcy;", 1083.ToChar()},
        {@"&ldca;", 10550.ToChar()},
        {@"&ldquo;", 8220.ToChar()},
        {@"&ldquor;", 8222.ToChar()},
        {@"&ldrdhar;", 10599.ToChar()},
        {@"&ldrushar;", 10571.ToChar()},
        {@"&ldsh;", 8626.ToChar()},
        {@"&lE;", 8806.ToChar()},
        {@"&le;", 8804.ToChar()},
        {@"&LeftAngleBracket;", 10216.ToChar()},
        {@"&LeftArrow;", 8592.ToChar()},
        {@"&Leftarrow;", 8656.ToChar()},
        {@"&leftarrow;", 8592.ToChar()},
        {@"&LeftArrowBar;", 8676.ToChar()},
        {@"&LeftArrowRightArrow;", 8646.ToChar()},
        {@"&leftarrowtail;", 8610.ToChar()},
        {@"&LeftCeiling;", 8968.ToChar()},
        {@"&LeftDoubleBracket;", 10214.ToChar()},
        {@"&LeftDownTeeVector;", 10593.ToChar()},
        {@"&LeftDownVector;", 8643.ToChar()},
        {@"&LeftDownVectorBar;", 10585.ToChar()},
        {@"&LeftFloor;", 8970.ToChar()},
        {@"&leftharpoondown;", 8637.ToChar()},
        {@"&leftharpoonup;", 8636.ToChar()},
        {@"&leftleftarrows;", 8647.ToChar()},
        {@"&LeftRightArrow;", 8596.ToChar()},
        {@"&Leftrightarrow;", 8660.ToChar()},
        {@"&leftrightarrow;", 8596.ToChar()},
        {@"&leftrightarrows;", 8646.ToChar()},
        {@"&leftrightharpoons;", 8651.ToChar()},
        {@"&leftrightsquigarrow;", 8621.ToChar()},
        {@"&LeftRightVector;", 10574.ToChar()},
        {@"&LeftTee;", 8867.ToChar()},
        {@"&LeftTeeArrow;", 8612.ToChar()},
        {@"&LeftTeeVector;", 10586.ToChar()},
        {@"&leftthreetimes;", 8907.ToChar()},
        {@"&LeftTriangle;", 8882.ToChar()},
        {@"&LeftTriangleBar;", 10703.ToChar()},
        {@"&LeftTriangleEqual;", 8884.ToChar()},
        {@"&LeftUpDownVector;", 10577.ToChar()},
        {@"&LeftUpTeeVector;", 10592.ToChar()},
        {@"&LeftUpVector;", 8639.ToChar()},
        {@"&LeftUpVectorBar;", 10584.ToChar()},
        {@"&LeftVector;", 8636.ToChar()},
        {@"&LeftVectorBar;", 10578.ToChar()},
        {@"&lEg;", 10891.ToChar()},
        {@"&leg;", 8922.ToChar()},
        {@"&leq;", 8804.ToChar()},
        {@"&leqq;", 8806.ToChar()},
        {@"&leqslant;", 10877.ToChar()},
        {@"&les;", 10877.ToChar()},
        {@"&lescc;", 10920.ToChar()},
        {@"&lesdot;", 10879.ToChar()},
        {@"&lesdoto;", 10881.ToChar()},
        {@"&lesdotor;", 10883.ToChar()},
        {@"&lesg;", 8922.ToChar()},
        {@"&lesges;", 10899.ToChar()},
        {@"&lessapprox;", 10885.ToChar()},
        {@"&lessdot;", 8918.ToChar()},
        {@"&lesseqgtr;", 8922.ToChar()},
        {@"&lesseqqgtr;", 10891.ToChar()},
        {@"&LessEqualGreater;", 8922.ToChar()},
        {@"&LessFullEqual;", 8806.ToChar()},
        {@"&LessGreater;", 8822.ToChar()},
        {@"&lessgtr;", 8822.ToChar()},
        {@"&LessLess;", 10913.ToChar()},
        {@"&lesssim;", 8818.ToChar()},
        {@"&LessSlantEqual;", 10877.ToChar()},
        {@"&LessTilde;", 8818.ToChar()},
        {@"&lfisht;", 10620.ToChar()},
        {@"&lfloor;", 8970.ToChar()},
//{ @"&Lfr;", 120079.ToChar() },
//{ @"&lfr;", 120105.ToChar() },
        {@"&lg;", 8822.ToChar()},
        {@"&lgE;", 10897.ToChar()},
        {@"&lHar;", 10594.ToChar()},
        {@"&lhard;", 8637.ToChar()},
        {@"&lharu;", 8636.ToChar()},
        {@"&lharul;", 10602.ToChar()},
        {@"&lhblk;", 9604.ToChar()},
        {@"&LJcy;", 1033.ToChar()},
        {@"&ljcy;", 1113.ToChar()},
        {@"&Ll;", 8920.ToChar()},
        {@"&ll;", 8810.ToChar()},
        {@"&llarr;", 8647.ToChar()},
        {@"&llcorner;", 8990.ToChar()},
        {@"&Lleftarrow;", 8666.ToChar()},
        {@"&llhard;", 10603.ToChar()},
        {@"&lltri;", 9722.ToChar()},
        {@"&Lmidot;", 319.ToChar()},
        {@"&lmidot;", 320.ToChar()},
        {@"&lmoust;", 9136.ToChar()},
        {@"&lmoustache;", 9136.ToChar()},
        {@"&lnap;", 10889.ToChar()},
        {@"&lnapprox;", 10889.ToChar()},
        {@"&lnE;", 8808.ToChar()},
        {@"&lne;", 10887.ToChar()},
        {@"&lneq;", 10887.ToChar()},
        {@"&lneqq;", 8808.ToChar()},
        {@"&lnsim;", 8934.ToChar()},
        {@"&loang;", 10220.ToChar()},
        {@"&loarr;", 8701.ToChar()},
        {@"&lobrk;", 10214.ToChar()},
        {@"&LongLeftArrow;", 10229.ToChar()},
        {@"&Longleftarrow;", 10232.ToChar()},
        {@"&longleftarrow;", 10229.ToChar()},
        {@"&LongLeftRightArrow;", 10231.ToChar()},
        {@"&Longleftrightarrow;", 10234.ToChar()},
        {@"&longleftrightarrow;", 10231.ToChar()},
        {@"&longmapsto;", 10236.ToChar()},
        {@"&LongRightArrow;", 10230.ToChar()},
        {@"&Longrightarrow;", 10233.ToChar()},
        {@"&longrightarrow;", 10230.ToChar()},
        {@"&looparrowleft;", 8619.ToChar()},
        {@"&looparrowright;", 8620.ToChar()},
        {@"&lopar;", 10629.ToChar()},
//{ @"&Lopf;", 120131.ToChar() },
//{ @"&lopf;", 120157.ToChar() },
        {@"&loplus;", 10797.ToChar()},
        {@"&lotimes;", 10804.ToChar()},
        {@"&lowast;", 8727.ToChar()},
        {@"&lowbar;", 95.ToChar()},
        {@"&LowerLeftArrow;", 8601.ToChar()},
        {@"&LowerRightArrow;", 8600.ToChar()},
        {@"&loz;", 9674.ToChar()},
        {@"&lozenge;", 9674.ToChar()},
        {@"&lozf;", 10731.ToChar()},
        {@"&lpar;", 40.ToChar()},
        {@"&lparlt;", 10643.ToChar()},
        {@"&lrarr;", 8646.ToChar()},
        {@"&lrcorner;", 8991.ToChar()},
        {@"&lrhar;", 8651.ToChar()},
        {@"&lrhard;", 10605.ToChar()},
        {@"&lrm;", 8206.ToChar()},
        {@"&lrtri;", 8895.ToChar()},
        {@"&lsaquo;", 8249.ToChar()},
        {@"&Lscr;", 8466.ToChar()},
//{ @"&lscr;", 120001.ToChar() },
        {@"&Lsh;", 8624.ToChar()},
        {@"&lsh;", 8624.ToChar()},
        {@"&lsim;", 8818.ToChar()},
        {@"&lsime;", 10893.ToChar()},
        {@"&lsimg;", 10895.ToChar()},
        {@"&lsqb;", 91.ToChar()},
        {@"&lsquo;", 8216.ToChar()},
        {@"&lsquor;", 8218.ToChar()},
        {@"&Lstrok;", 321.ToChar()},
        {@"&lstrok;", 322.ToChar()},
        {@"&LT;", 60.ToChar()},
        {@"&Lt;", 8810.ToChar()},
        {@"&lt;", 60.ToChar()},
        {@"&ltcc;", 10918.ToChar()},
        {@"&ltcir;", 10873.ToChar()},
        {@"&ltdot;", 8918.ToChar()},
        {@"&lthree;", 8907.ToChar()},
        {@"&ltimes;", 8905.ToChar()},
        {@"&ltlarr;", 10614.ToChar()},
        {@"&ltquest;", 10875.ToChar()},
        {@"&ltri;", 9667.ToChar()},
        {@"&ltrie;", 8884.ToChar()},
        {@"&ltrif;", 9666.ToChar()},
        {@"&ltrPar;", 10646.ToChar()},
        {@"&lurdshar;", 10570.ToChar()},
        {@"&luruhar;", 10598.ToChar()},
        {@"&lvertneqq;", 8808.ToChar()},
        {@"&lvnE;", 8808.ToChar()},
        {@"&macr;", 175.ToChar()},
        {@"&male;", 9794.ToChar()},
        {@"&malt;", 10016.ToChar()},
        {@"&maltese;", 10016.ToChar()},
        {@"&Map;", 10501.ToChar()},
        {@"&map;", 8614.ToChar()},
        {@"&mapsto;", 8614.ToChar()},
        {@"&mapstodown;", 8615.ToChar()},
        {@"&mapstoleft;", 8612.ToChar()},
        {@"&mapstoup;", 8613.ToChar()},
        {@"&marker;", 9646.ToChar()},
        {@"&mcomma;", 10793.ToChar()},
        {@"&Mcy;", 1052.ToChar()},
        {@"&mcy;", 1084.ToChar()},
        {@"&mdash;", 8212.ToChar()},
        {@"&mDDot;", 8762.ToChar()},
        {@"&measuredangle;", 8737.ToChar()},
        {@"&MediumSpace;", 8287.ToChar()},
        {@"&Mellintrf;", 8499.ToChar()},
//{ @"&Mfr;", 120080.ToChar() },
//{ @"&mfr;", 120106.ToChar() },
        {@"&mho;", 8487.ToChar()},
        {@"&micro;", 181.ToChar()},
        {@"&mid;", 8739.ToChar()},
        {@"&midast;", 42.ToChar()},
        {@"&midcir;", 10992.ToChar()},
        {@"&middot;", 183.ToChar()},
        {@"&minus;", 8722.ToChar()},
        {@"&minusb;", 8863.ToChar()},
        {@"&minusd;", 8760.ToChar()},
        {@"&minusdu;", 10794.ToChar()},
        {@"&MinusPlus;", 8723.ToChar()},
        {@"&mlcp;", 10971.ToChar()},
        {@"&mldr;", 8230.ToChar()},
        {@"&mnplus;", 8723.ToChar()},
        {@"&models;", 8871.ToChar()},
//{ @"&Mopf;", 120132.ToChar() },
//{ @"&mopf;", 120158.ToChar() },
        {@"&mp;", 8723.ToChar()},
        {@"&Mscr;", 8499.ToChar()},
//{ @"&mscr;", 120002.ToChar() },
        {@"&mstpos;", 8766.ToChar()},
        {@"&Mu;", 924.ToChar()},
        {@"&mu;", 956.ToChar()},
        {@"&multimap;", 8888.ToChar()},
        {@"&mumap;", 8888.ToChar()},
        {@"&nabla;", 8711.ToChar()},
        {@"&Nacute;", 323.ToChar()},
        {@"&nacute;", 324.ToChar()},
        {@"&nang;", 8736.ToChar()},
        {@"&nap;", 8777.ToChar()},
        {@"&napE;", 10864.ToChar()},
        {@"&napid;", 8779.ToChar()},
        {@"&napos;", 329.ToChar()},
        {@"&napprox;", 8777.ToChar()},
        {@"&natur;", 9838.ToChar()},
        {@"&natural;", 9838.ToChar()},
        {@"&naturals;", 8469.ToChar()},
        {@"&nbsp;", 160.ToChar()},
        {@"&nbump;", 8782.ToChar()},
        {@"&nbumpe;", 8783.ToChar()},
        {@"&ncap;", 10819.ToChar()},
        {@"&Ncaron;", 327.ToChar()},
        {@"&ncaron;", 328.ToChar()},
        {@"&Ncedil;", 325.ToChar()},
        {@"&ncedil;", 326.ToChar()},
        {@"&ncong;", 8775.ToChar()},
        {@"&ncongdot;", 10861.ToChar()},
        {@"&ncup;", 10818.ToChar()},
        {@"&Ncy;", 1053.ToChar()},
        {@"&ncy;", 1085.ToChar()},
        {@"&ndash;", 8211.ToChar()},
        {@"&ne;", 8800.ToChar()},
        {@"&nearhk;", 10532.ToChar()},
        {@"&neArr;", 8663.ToChar()},
        {@"&nearr;", 8599.ToChar()},
        {@"&nearrow;", 8599.ToChar()},
        {@"&nedot;", 8784.ToChar()},
        {@"&NegativeMediumSpace;", 8203.ToChar()},
        {@"&NegativeThickSpace;", 8203.ToChar()},
        {@"&NegativeThinSpace;", 8203.ToChar()},
        {@"&NegativeVeryThinSpace;", 8203.ToChar()},
        {@"&nequiv;", 8802.ToChar()},
        {@"&nesear;", 10536.ToChar()},
        {@"&nesim;", 8770.ToChar()},
        {@"&NestedGreaterGreater;", 8811.ToChar()},
        {@"&NestedLessLess;", 8810.ToChar()},
        {@"&NewLine;", 10.ToChar()},
        {@"&nexist;", 8708.ToChar()},
        {@"&nexists;", 8708.ToChar()},
//{ @"&Nfr;", 120081.ToChar() },
//{ @"&nfr;", 120107.ToChar() },
        {@"&ngE;", 8807.ToChar()},
        {@"&nge;", 8817.ToChar()},
        {@"&ngeq;", 8817.ToChar()},
        {@"&ngeqq;", 8807.ToChar()},
        {@"&ngeqslant;", 10878.ToChar()},
        {@"&nges;", 10878.ToChar()},
        {@"&nGg;", 8921.ToChar()},
        {@"&ngsim;", 8821.ToChar()},
        {@"&nGt;", 8811.ToChar()},
        {@"&ngt;", 8815.ToChar()},
        {@"&ngtr;", 8815.ToChar()},
        {@"&nGtv;", 8811.ToChar()},
        {@"&nhArr;", 8654.ToChar()},
        {@"&nharr;", 8622.ToChar()},
        {@"&nhpar;", 10994.ToChar()},
        {@"&ni;", 8715.ToChar()},
        {@"&nis;", 8956.ToChar()},
        {@"&nisd;", 8954.ToChar()},
        {@"&niv;", 8715.ToChar()},
        {@"&NJcy;", 1034.ToChar()},
        {@"&njcy;", 1114.ToChar()},
        {@"&nlArr;", 8653.ToChar()},
        {@"&nlarr;", 8602.ToChar()},
        {@"&nldr;", 8229.ToChar()},
        {@"&nlE;", 8806.ToChar()},
        {@"&nle;", 8816.ToChar()},
        {@"&nLeftarrow;", 8653.ToChar()},
        {@"&nleftarrow;", 8602.ToChar()},
        {@"&nLeftrightarrow;", 8654.ToChar()},
        {@"&nleftrightarrow;", 8622.ToChar()},
        {@"&nleq;", 8816.ToChar()},
        {@"&nleqq;", 8806.ToChar()},
        {@"&nleqslant;", 10877.ToChar()},
        {@"&nles;", 10877.ToChar()},
        {@"&nless;", 8814.ToChar()},
        {@"&nLl;", 8920.ToChar()},
        {@"&nlsim;", 8820.ToChar()},
        {@"&nLt;", 8810.ToChar()},
        {@"&nlt;", 8814.ToChar()},
        {@"&nltri;", 8938.ToChar()},
        {@"&nltrie;", 8940.ToChar()},
        {@"&nLtv;", 8810.ToChar()},
        {@"&nmid;", 8740.ToChar()},
        {@"&NoBreak;", 8288.ToChar()},
        {@"&NonBreakingSpace;", 160.ToChar()},
        {@"&Nopf;", 8469.ToChar()},
//{ @"&nopf;", 120159.ToChar() },
        {@"&Not;", 10988.ToChar()},
        {@"&not;", 172.ToChar()},
        {@"&NotCongruent;", 8802.ToChar()},
        {@"&NotCupCap;", 8813.ToChar()},
        {@"&NotDoubleVerticalBar;", 8742.ToChar()},
        {@"&NotElement;", 8713.ToChar()},
        {@"&NotEqual;", 8800.ToChar()},
        {@"&NotEqualTilde;", 8770.ToChar()},
        {@"&NotExists;", 8708.ToChar()},
        {@"&NotGreater;", 8815.ToChar()},
        {@"&NotGreaterEqual;", 8817.ToChar()},
        {@"&NotGreaterFullEqual;", 8807.ToChar()},
        {@"&NotGreaterGreater;", 8811.ToChar()},
        {@"&NotGreaterLess;", 8825.ToChar()},
        {@"&NotGreaterSlantEqual;", 10878.ToChar()},
        {@"&NotGreaterTilde;", 8821.ToChar()},
        {@"&NotHumpDownHump;", 8782.ToChar()},
        {@"&NotHumpEqual;", 8783.ToChar()},
        {@"&notin;", 8713.ToChar()},
        {@"&notindot;", 8949.ToChar()},
        {@"&notinE;", 8953.ToChar()},
        {@"&notinva;", 8713.ToChar()},
        {@"&notinvb;", 8951.ToChar()},
        {@"&notinvc;", 8950.ToChar()},
        {@"&NotLeftTriangle;", 8938.ToChar()},
        {@"&NotLeftTriangleBar;", 10703.ToChar()},
        {@"&NotLeftTriangleEqual;", 8940.ToChar()},
        {@"&NotLess;", 8814.ToChar()},
        {@"&NotLessEqual;", 8816.ToChar()},
        {@"&NotLessGreater;", 8824.ToChar()},
        {@"&NotLessLess;", 8810.ToChar()},
        {@"&NotLessSlantEqual;", 10877.ToChar()},
        {@"&NotLessTilde;", 8820.ToChar()},
        {@"&NotNestedGreaterGreater;", 10914.ToChar()},
        {@"&NotNestedLessLess;", 10913.ToChar()},
        {@"&notni;", 8716.ToChar()},
        {@"&notniva;", 8716.ToChar()},
        {@"&notnivb;", 8958.ToChar()},
        {@"&notnivc;", 8957.ToChar()},
        {@"&NotPrecedes;", 8832.ToChar()},
        {@"&NotPrecedesEqual;", 10927.ToChar()},
        {@"&NotPrecedesSlantEqual;", 8928.ToChar()},
        {@"&NotReverseElement;", 8716.ToChar()},
        {@"&NotRightTriangle;", 8939.ToChar()},
        {@"&NotRightTriangleBar;", 10704.ToChar()},
        {@"&NotRightTriangleEqual;", 8941.ToChar()},
        {@"&NotSquareSubset;", 8847.ToChar()},
        {@"&NotSquareSubsetEqual;", 8930.ToChar()},
        {@"&NotSquareSuperset;", 8848.ToChar()},
        {@"&NotSquareSupersetEqual;", 8931.ToChar()},
        {@"&NotSubset;", 8834.ToChar()},
        {@"&NotSubsetEqual;", 8840.ToChar()},
        {@"&NotSucceeds;", 8833.ToChar()},
        {@"&NotSucceedsEqual;", 10928.ToChar()},
        {@"&NotSucceedsSlantEqual;", 8929.ToChar()},
        {@"&NotSucceedsTilde;", 8831.ToChar()},
        {@"&NotSuperset;", 8835.ToChar()},
        {@"&NotSupersetEqual;", 8841.ToChar()},
        {@"&NotTilde;", 8769.ToChar()},
        {@"&NotTildeEqual;", 8772.ToChar()},
        {@"&NotTildeFullEqual;", 8775.ToChar()},
        {@"&NotTildeTilde;", 8777.ToChar()},
        {@"&NotVerticalBar;", 8740.ToChar()},
        {@"&npar;", 8742.ToChar()},
        {@"&nparallel;", 8742.ToChar()},
        {@"&nparsl;", 11005.ToChar()},
        {@"&npart;", 8706.ToChar()},
        {@"&npolint;", 10772.ToChar()},
        {@"&npr;", 8832.ToChar()},
        {@"&nprcue;", 8928.ToChar()},
        {@"&npre;", 10927.ToChar()},
        {@"&nprec;", 8832.ToChar()},
        {@"&npreceq;", 10927.ToChar()},
        {@"&nrArr;", 8655.ToChar()},
        {@"&nrarr;", 8603.ToChar()},
        {@"&nrarrc;", 10547.ToChar()},
        {@"&nrarrw;", 8605.ToChar()},
        {@"&nRightarrow;", 8655.ToChar()},
        {@"&nrightarrow;", 8603.ToChar()},
        {@"&nrtri;", 8939.ToChar()},
        {@"&nrtrie;", 8941.ToChar()},
        {@"&nsc;", 8833.ToChar()},
        {@"&nsccue;", 8929.ToChar()},
        {@"&nsce;", 10928.ToChar()},
//{ @"&Nscr;", 119977.ToChar() },
//{ @"&nscr;", 120003.ToChar() },
        {@"&nshortmid;", 8740.ToChar()},
        {@"&nshortparallel;", 8742.ToChar()},
        {@"&nsim;", 8769.ToChar()},
        {@"&nsime;", 8772.ToChar()},
        {@"&nsimeq;", 8772.ToChar()},
        {@"&nsmid;", 8740.ToChar()},
        {@"&nspar;", 8742.ToChar()},
        {@"&nsqsube;", 8930.ToChar()},
        {@"&nsqsupe;", 8931.ToChar()},
        {@"&nsub;", 8836.ToChar()},
        {@"&nsubE;", 10949.ToChar()},
        {@"&nsube;", 8840.ToChar()},
        {@"&nsubset;", 8834.ToChar()},
        {@"&nsubseteq;", 8840.ToChar()},
        {@"&nsubseteqq;", 10949.ToChar()},
        {@"&nsucc;", 8833.ToChar()},
        {@"&nsucceq;", 10928.ToChar()},
        {@"&nsup;", 8837.ToChar()},
        {@"&nsupE;", 10950.ToChar()},
        {@"&nsupe;", 8841.ToChar()},
        {@"&nsupset;", 8835.ToChar()},
        {@"&nsupseteq;", 8841.ToChar()},
        {@"&nsupseteqq;", 10950.ToChar()},
        {@"&ntgl;", 8825.ToChar()},
        {@"&Ntilde;", 209.ToChar()},
        {@"&ntilde;", 241.ToChar()},
        {@"&ntlg;", 8824.ToChar()},
        {@"&ntriangleleft;", 8938.ToChar()},
        {@"&ntrianglelefteq;", 8940.ToChar()},
        {@"&ntriangleright;", 8939.ToChar()},
        {@"&ntrianglerighteq;", 8941.ToChar()},
        {@"&Nu;", 925.ToChar()},
        {@"&nu;", 957.ToChar()},
        {@"&num;", 35.ToChar()},
        {@"&numero;", 8470.ToChar()},
        {@"&numsp;", 8199.ToChar()},
        {@"&nvap;", 8781.ToChar()},
        {@"&nVDash;", 8879.ToChar()},
        {@"&nVdash;", 8878.ToChar()},
        {@"&nvDash;", 8877.ToChar()},
        {@"&nvdash;", 8876.ToChar()},
        {@"&nvge;", 8805.ToChar()},
        {@"&nvgt;", 62.ToChar()},
        {@"&nvHarr;", 10500.ToChar()},
        {@"&nvinfin;", 10718.ToChar()},
        {@"&nvlArr;", 10498.ToChar()},
        {@"&nvle;", 8804.ToChar()},
        {@"&nvlt;", 60.ToChar()},
        {@"&nvltrie;", 8884.ToChar()},
        {@"&nvrArr;", 10499.ToChar()},
        {@"&nvrtrie;", 8885.ToChar()},
        {@"&nvsim;", 8764.ToChar()},
        {@"&nwarhk;", 10531.ToChar()},
        {@"&nwArr;", 8662.ToChar()},
        {@"&nwarr;", 8598.ToChar()},
        {@"&nwarrow;", 8598.ToChar()},
        {@"&nwnear;", 10535.ToChar()},
        {@"&Oacute;", 211.ToChar()},
        {@"&oacute;", 243.ToChar()},
        {@"&oast;", 8859.ToChar()},
        {@"&ocir;", 8858.ToChar()},
        {@"&Ocirc;", 212.ToChar()},
        {@"&ocirc;", 244.ToChar()},
        {@"&Ocy;", 1054.ToChar()},
        {@"&ocy;", 1086.ToChar()},
        {@"&odash;", 8861.ToChar()},
        {@"&Odblac;", 336.ToChar()},
        {@"&odblac;", 337.ToChar()},
        {@"&odiv;", 10808.ToChar()},
        {@"&odot;", 8857.ToChar()},
        {@"&odsold;", 10684.ToChar()},
        {@"&OElig;", 338.ToChar()},
        {@"&oelig;", 339.ToChar()},
        {@"&ofcir;", 10687.ToChar()},
//{ @"&Ofr;", 120082.ToChar() },
//{ @"&ofr;", 120108.ToChar() },
        {@"&ogon;", 731.ToChar()},
        {@"&Ograve;", 210.ToChar()},
        {@"&ograve;", 242.ToChar()},
        {@"&ogt;", 10689.ToChar()},
        {@"&ohbar;", 10677.ToChar()},
        {@"&ohm;", 937.ToChar()},
        {@"&oint;", 8750.ToChar()},
        {@"&olarr;", 8634.ToChar()},
        {@"&olcir;", 10686.ToChar()},
        {@"&olcross;", 10683.ToChar()},
        {@"&oline;", 8254.ToChar()},
        {@"&olt;", 10688.ToChar()},
        {@"&Omacr;", 332.ToChar()},
        {@"&omacr;", 333.ToChar()},
        {@"&Omega;", 937.ToChar()},
        {@"&omega;", 969.ToChar()},
        {@"&Omicron;", 927.ToChar()},
        {@"&omicron;", 959.ToChar()},
        {@"&omid;", 10678.ToChar()},
        {@"&ominus;", 8854.ToChar()},
//{ @"&Oopf;", 120134.ToChar() },
//{ @"&oopf;", 120160.ToChar() },
        {@"&opar;", 10679.ToChar()},
        {@"&OpenCurlyDoubleQuote;", 8220.ToChar()},
        {@"&OpenCurlyQuote;", 8216.ToChar()},
        {@"&operp;", 10681.ToChar()},
        {@"&oplus;", 8853.ToChar()},
        {@"&Or;", 10836.ToChar()},
        {@"&or;", 8744.ToChar()},
        {@"&orarr;", 8635.ToChar()},
        {@"&ord;", 10845.ToChar()},
        {@"&order;", 8500.ToChar()},
        {@"&orderof;", 8500.ToChar()},
        {@"&ordf;", 170.ToChar()},
        {@"&ordm;", 186.ToChar()},
        {@"&origof;", 8886.ToChar()},
        {@"&oror;", 10838.ToChar()},
        {@"&orslope;", 10839.ToChar()},
        {@"&orv;", 10843.ToChar()},
        {@"&oS;", 9416.ToChar()},
//{ @"&Oscr;", 119978.ToChar() },
        {@"&oscr;", 8500.ToChar()},
        {@"&Oslash;", 216.ToChar()},
        {@"&oslash;", 248.ToChar()},
        {@"&osol;", 8856.ToChar()},
        {@"&Otilde;", 213.ToChar()},
        {@"&otilde;", 245.ToChar()},
        {@"&Otimes;", 10807.ToChar()},
        {@"&otimes;", 8855.ToChar()},
        {@"&otimesas;", 10806.ToChar()},
        {@"&Ouml;", 214.ToChar()},
        {@"&ouml;", 246.ToChar()},
        {@"&ovbar;", 9021.ToChar()},
        {@"&OverBar;", 8254.ToChar()},
        {@"&OverBrace;", 9182.ToChar()},
        {@"&OverBracket;", 9140.ToChar()},
        {@"&OverParenthesis;", 9180.ToChar()},
        {@"&par;", 8741.ToChar()},
        {@"&para;", 182.ToChar()},
        {@"&parallel;", 8741.ToChar()},
        {@"&parsim;", 10995.ToChar()},
        {@"&parsl;", 11005.ToChar()},
        {@"&part;", 8706.ToChar()},
        {@"&PartialD;", 8706.ToChar()},
        {@"&Pcy;", 1055.ToChar()},
        {@"&pcy;", 1087.ToChar()},
        {@"&percnt;", 37.ToChar()},
        {@"&period;", 46.ToChar()},
        {@"&permil;", 8240.ToChar()},
        {@"&perp;", 8869.ToChar()},
        {@"&pertenk;", 8241.ToChar()},
//{ @"&Pfr;", 120083.ToChar() },
//{ @"&pfr;", 120109.ToChar() },
        {@"&Phi;", 934.ToChar()},
        {@"&phi;", 966.ToChar()},
        {@"&phiv;", 981.ToChar()},
        {@"&phmmat;", 8499.ToChar()},
        {@"&phone;", 9742.ToChar()},
        {@"&Pi;", 928.ToChar()},
        {@"&pi;", 960.ToChar()},
        {@"&pitchfork;", 8916.ToChar()},
        {@"&piv;", 982.ToChar()},
        {@"&planck;", 8463.ToChar()},
        {@"&planckh;", 8462.ToChar()},
        {@"&plankv;", 8463.ToChar()},
        {@"&plus;", 43.ToChar()},
        {@"&plusacir;", 10787.ToChar()},
        {@"&plusb;", 8862.ToChar()},
        {@"&pluscir;", 10786.ToChar()},
        {@"&plusdo;", 8724.ToChar()},
        {@"&plusdu;", 10789.ToChar()},
        {@"&pluse;", 10866.ToChar()},
        {@"&PlusMinus;", 177.ToChar()},
        {@"&plusmn;", 177.ToChar()},
        {@"&plussim;", 10790.ToChar()},
        {@"&plustwo;", 10791.ToChar()},
        {@"&pm;", 177.ToChar()},
        {@"&Poincareplane;", 8460.ToChar()},
        {@"&pointint;", 10773.ToChar()},
        {@"&Popf;", 8473.ToChar()},
//{ @"&popf;", 120161.ToChar() },
        {@"&pound;", 163.ToChar()},
        {@"&Pr;", 10939.ToChar()},
        {@"&pr;", 8826.ToChar()},
        {@"&prap;", 10935.ToChar()},
        {@"&prcue;", 8828.ToChar()},
        {@"&prE;", 10931.ToChar()},
        {@"&pre;", 10927.ToChar()},
        {@"&prec;", 8826.ToChar()},
        {@"&precapprox;", 10935.ToChar()},
        {@"&preccurlyeq;", 8828.ToChar()},
        {@"&Precedes;", 8826.ToChar()},
        {@"&PrecedesEqual;", 10927.ToChar()},
        {@"&PrecedesSlantEqual;", 8828.ToChar()},
        {@"&PrecedesTilde;", 8830.ToChar()},
        {@"&preceq;", 10927.ToChar()},
        {@"&precnapprox;", 10937.ToChar()},
        {@"&precneqq;", 10933.ToChar()},
        {@"&precnsim;", 8936.ToChar()},
        {@"&precsim;", 8830.ToChar()},
        {@"&Prime;", 8243.ToChar()},
        {@"&prime;", 8242.ToChar()},
        {@"&primes;", 8473.ToChar()},
        {@"&prnap;", 10937.ToChar()},
        {@"&prnE;", 10933.ToChar()},
        {@"&prnsim;", 8936.ToChar()},
        {@"&prod;", 8719.ToChar()},
        {@"&Product;", 8719.ToChar()},
        {@"&profalar;", 9006.ToChar()},
        {@"&profline;", 8978.ToChar()},
        {@"&profsurf;", 8979.ToChar()},
        {@"&prop;", 8733.ToChar()},
        {@"&Proportion;", 8759.ToChar()},
        {@"&Proportional;", 8733.ToChar()},
        {@"&propto;", 8733.ToChar()},
        {@"&prsim;", 8830.ToChar()},
        {@"&prurel;", 8880.ToChar()},
//{ @"&Pscr;", 119979.ToChar() },
//{ @"&pscr;", 120005.ToChar() },
        {@"&Psi;", 936.ToChar()},
        {@"&psi;", 968.ToChar()},
        {@"&puncsp;", 8200.ToChar()},
//{ @"&Qfr;", 120084.ToChar() },
//{ @"&qfr;", 120110.ToChar() },
        {@"&qint;", 10764.ToChar()},
        {@"&Qopf;", 8474.ToChar()},
//{ @"&qopf;", 120162.ToChar() },
        {@"&qprime;", 8279.ToChar()},
//{ @"&Qscr;", 119980.ToChar() },
//{ @"&qscr;", 120006.ToChar() },
        {@"&quaternions;", 8461.ToChar()},
        {@"&quatint;", 10774.ToChar()},
        {@"&quest;", 63.ToChar()},
        {@"&questeq;", 8799.ToChar()},
        {@"&QUOT;", 34.ToChar()},
        {@"&quot;", 34.ToChar()},
        {@"&rAarr;", 8667.ToChar()},
        {@"&race;", 8765.ToChar()},
        {@"&Racute;", 340.ToChar()},
        {@"&racute;", 341.ToChar()},
        {@"&radic;", 8730.ToChar()},
        {@"&raemptyv;", 10675.ToChar()},
        {@"&Rang;", 10219.ToChar()},
        {@"&rang;", 10217.ToChar()},
        {@"&rangd;", 10642.ToChar()},
        {@"&range;", 10661.ToChar()},
        {@"&rangle;", 10217.ToChar()},
        {@"&raquo;", 187.ToChar()},
        {@"&Rarr;", 8608.ToChar()},
        {@"&rArr;", 8658.ToChar()},
        {@"&rarr;", 8594.ToChar()},
        {@"&rarrap;", 10613.ToChar()},
        {@"&rarrb;", 8677.ToChar()},
        {@"&rarrbfs;", 10528.ToChar()},
        {@"&rarrc;", 10547.ToChar()},
        {@"&rarrfs;", 10526.ToChar()},
        {@"&rarrhk;", 8618.ToChar()},
        {@"&rarrlp;", 8620.ToChar()},
        {@"&rarrpl;", 10565.ToChar()},
        {@"&rarrsim;", 10612.ToChar()},
        {@"&Rarrtl;", 10518.ToChar()},
        {@"&rarrtl;", 8611.ToChar()},
        {@"&rarrw;", 8605.ToChar()},
        {@"&rAtail;", 10524.ToChar()},
        {@"&ratail;", 10522.ToChar()},
        {@"&ratio;", 8758.ToChar()},
        {@"&rationals;", 8474.ToChar()},
        {@"&RBarr;", 10512.ToChar()},
        {@"&rBarr;", 10511.ToChar()},
        {@"&rbarr;", 10509.ToChar()},
        {@"&rbbrk;", 10099.ToChar()},
        {@"&rbrace;", 125.ToChar()},
        {@"&rbrack;", 93.ToChar()},
        {@"&rbrke;", 10636.ToChar()},
        {@"&rbrksld;", 10638.ToChar()},
        {@"&rbrkslu;", 10640.ToChar()},
        {@"&Rcaron;", 344.ToChar()},
        {@"&rcaron;", 345.ToChar()},
        {@"&Rcedil;", 342.ToChar()},
        {@"&rcedil;", 343.ToChar()},
        {@"&rceil;", 8969.ToChar()},
        {@"&rcub;", 125.ToChar()},
        {@"&Rcy;", 1056.ToChar()},
        {@"&rcy;", 1088.ToChar()},
        {@"&rdca;", 10551.ToChar()},
        {@"&rdldhar;", 10601.ToChar()},
        {@"&rdquo;", 8221.ToChar()},
        {@"&rdquor;", 8221.ToChar()},
        {@"&rdsh;", 8627.ToChar()},
        {@"&Re;", 8476.ToChar()},
        {@"&real;", 8476.ToChar()},
        {@"&realine;", 8475.ToChar()},
        {@"&realpart;", 8476.ToChar()},
        {@"&reals;", 8477.ToChar()},
        {@"&rect;", 9645.ToChar()},
        {@"&REG;", 174.ToChar()},
        {@"&reg;", 174.ToChar()},
        {@"&ReverseElement;", 8715.ToChar()},
        {@"&ReverseEquilibrium;", 8651.ToChar()},
        {@"&ReverseUpEquilibrium;", 10607.ToChar()},
        {@"&rfisht;", 10621.ToChar()},
        {@"&rfloor;", 8971.ToChar()},
        {@"&Rfr;", 8476.ToChar()},
//{ @"&rfr;", 120111.ToChar() },
        {@"&rHar;", 10596.ToChar()},
        {@"&rhard;", 8641.ToChar()},
        {@"&rharu;", 8640.ToChar()},
        {@"&rharul;", 10604.ToChar()},
        {@"&Rho;", 929.ToChar()},
        {@"&rho;", 961.ToChar()},
        {@"&rhov;", 1009.ToChar()},
        {@"&RightAngleBracket;", 10217.ToChar()},
        {@"&RightArrow;", 8594.ToChar()},
        {@"&Rightarrow;", 8658.ToChar()},
        {@"&rightarrow;", 8594.ToChar()},
        {@"&RightArrowBar;", 8677.ToChar()},
        {@"&RightArrowLeftArrow;", 8644.ToChar()},
        {@"&rightarrowtail;", 8611.ToChar()},
        {@"&RightCeiling;", 8969.ToChar()},
        {@"&RightDoubleBracket;", 10215.ToChar()},
        {@"&RightDownTeeVector;", 10589.ToChar()},
        {@"&RightDownVector;", 8642.ToChar()},
        {@"&RightDownVectorBar;", 10581.ToChar()},
        {@"&RightFloor;", 8971.ToChar()},
        {@"&rightharpoondown;", 8641.ToChar()},
        {@"&rightharpoonup;", 8640.ToChar()},
        {@"&rightleftarrows;", 8644.ToChar()},
        {@"&rightleftharpoons;", 8652.ToChar()},
        {@"&rightrightarrows;", 8649.ToChar()},
        {@"&rightsquigarrow;", 8605.ToChar()},
        {@"&RightTee;", 8866.ToChar()},
        {@"&RightTeeArrow;", 8614.ToChar()},
        {@"&RightTeeVector;", 10587.ToChar()},
        {@"&rightthreetimes;", 8908.ToChar()},
        {@"&RightTriangle;", 8883.ToChar()},
        {@"&RightTriangleBar;", 10704.ToChar()},
        {@"&RightTriangleEqual;", 8885.ToChar()},
        {@"&RightUpDownVector;", 10575.ToChar()},
        {@"&RightUpTeeVector;", 10588.ToChar()},
        {@"&RightUpVector;", 8638.ToChar()},
        {@"&RightUpVectorBar;", 10580.ToChar()},
        {@"&RightVector;", 8640.ToChar()},
        {@"&RightVectorBar;", 10579.ToChar()},
        {@"&ring;", 730.ToChar()},
        {@"&risingdotseq;", 8787.ToChar()},
        {@"&rlarr;", 8644.ToChar()},
        {@"&rlhar;", 8652.ToChar()},
        {@"&rlm;", 8207.ToChar()},
        {@"&rmoust;", 9137.ToChar()},
        {@"&rmoustache;", 9137.ToChar()},
        {@"&rnmid;", 10990.ToChar()},
        {@"&roang;", 10221.ToChar()},
        {@"&roarr;", 8702.ToChar()},
        {@"&robrk;", 10215.ToChar()},
        {@"&ropar;", 10630.ToChar()},
        {@"&Ropf;", 8477.ToChar()},
//{ @"&ropf;", 120163.ToChar() },
        {@"&roplus;", 10798.ToChar()},
        {@"&rotimes;", 10805.ToChar()},
        {@"&RoundImplies;", 10608.ToChar()},
        {@"&rpar;", 41.ToChar()},
        {@"&rpargt;", 10644.ToChar()},
        {@"&rppolint;", 10770.ToChar()},
        {@"&rrarr;", 8649.ToChar()},
        {@"&Rrightarrow;", 8667.ToChar()},
        {@"&rsaquo;", 8250.ToChar()},
        {@"&Rscr;", 8475.ToChar()},
//{ @"&rscr;", 120007.ToChar() },
        {@"&Rsh;", 8625.ToChar()},
        {@"&rsh;", 8625.ToChar()},
        {@"&rsqb;", 93.ToChar()},
        {@"&rsquo;", 8217.ToChar()},
        {@"&rsquor;", 8217.ToChar()},
        {@"&rthree;", 8908.ToChar()},
        {@"&rtimes;", 8906.ToChar()},
        {@"&rtri;", 9657.ToChar()},
        {@"&rtrie;", 8885.ToChar()},
        {@"&rtrif;", 9656.ToChar()},
        {@"&rtriltri;", 10702.ToChar()},
        {@"&RuleDelayed;", 10740.ToChar()},
        {@"&ruluhar;", 10600.ToChar()},
        {@"&rx;", 8478.ToChar()},
        {@"&Sacute;", 346.ToChar()},
        {@"&sacute;", 347.ToChar()},
        {@"&sbquo;", 8218.ToChar()},
        {@"&Sc;", 10940.ToChar()},
        {@"&sc;", 8827.ToChar()},
        {@"&scap;", 10936.ToChar()},
        {@"&Scaron;", 352.ToChar()},
        {@"&scaron;", 353.ToChar()},
        {@"&sccue;", 8829.ToChar()},
        {@"&scE;", 10932.ToChar()},
        {@"&sce;", 10928.ToChar()},
        {@"&Scedil;", 350.ToChar()},
        {@"&scedil;", 351.ToChar()},
        {@"&Scirc;", 348.ToChar()},
        {@"&scirc;", 349.ToChar()},
        {@"&scnap;", 10938.ToChar()},
        {@"&scnE;", 10934.ToChar()},
        {@"&scnsim;", 8937.ToChar()},
        {@"&scpolint;", 10771.ToChar()},
        {@"&scsim;", 8831.ToChar()},
        {@"&Scy;", 1057.ToChar()},
        {@"&scy;", 1089.ToChar()},
        {@"&sdot;", 8901.ToChar()},
        {@"&sdotb;", 8865.ToChar()},
        {@"&sdote;", 10854.ToChar()},
        {@"&searhk;", 10533.ToChar()},
        {@"&seArr;", 8664.ToChar()},
        {@"&searr;", 8600.ToChar()},
        {@"&searrow;", 8600.ToChar()},
        {@"&sect;", 167.ToChar()},
        {@"&semi;", 59.ToChar()},
        {@"&seswar;", 10537.ToChar()},
        {@"&setminus;", 8726.ToChar()},
        {@"&setmn;", 8726.ToChar()},
        {@"&sext;", 10038.ToChar()},
//{ @"&Sfr;", 120086.ToChar() },
//{ @"&sfr;", 120112.ToChar() },
        {@"&sfrown;", 8994.ToChar()},
        {@"&sharp;", 9839.ToChar()},
        {@"&SHCHcy;", 1065.ToChar()},
        {@"&shchcy;", 1097.ToChar()},
        {@"&SHcy;", 1064.ToChar()},
        {@"&shcy;", 1096.ToChar()},
        {@"&ShortDownArrow;", 8595.ToChar()},
        {@"&ShortLeftArrow;", 8592.ToChar()},
        {@"&shortmid;", 8739.ToChar()},
        {@"&shortparallel;", 8741.ToChar()},
        {@"&ShortRightArrow;", 8594.ToChar()},
        {@"&ShortUpArrow;", 8593.ToChar()},
        {@"&shy;", 173.ToChar()},
        {@"&Sigma;", 931.ToChar()},
        {@"&sigma;", 963.ToChar()},
        {@"&sigmaf;", 962.ToChar()},
        {@"&sigmav;", 962.ToChar()},
        {@"&sim;", 8764.ToChar()},
        {@"&simdot;", 10858.ToChar()},
        {@"&sime;", 8771.ToChar()},
        {@"&simeq;", 8771.ToChar()},
        {@"&simg;", 10910.ToChar()},
        {@"&simgE;", 10912.ToChar()},
        {@"&siml;", 10909.ToChar()},
        {@"&simlE;", 10911.ToChar()},
        {@"&simne;", 8774.ToChar()},
        {@"&simplus;", 10788.ToChar()},
        {@"&simrarr;", 10610.ToChar()},
        {@"&slarr;", 8592.ToChar()},
        {@"&SmallCircle;", 8728.ToChar()},
        {@"&smallsetminus;", 8726.ToChar()},
        {@"&smashp;", 10803.ToChar()},
        {@"&smeparsl;", 10724.ToChar()},
        {@"&smid;", 8739.ToChar()},
        {@"&smile;", 8995.ToChar()},
        {@"&smt;", 10922.ToChar()},
        {@"&smte;", 10924.ToChar()},
        {@"&smtes;", 10924.ToChar()},
        {@"&SOFTcy;", 1068.ToChar()},
        {@"&softcy;", 1100.ToChar()},
        {@"&sol;", 47.ToChar()},
        {@"&solb;", 10692.ToChar()},
        {@"&solbar;", 9023.ToChar()},
//{ @"&Sopf;", 120138.ToChar() },
//{ @"&sopf;", 120164.ToChar() },
        {@"&spades;", 9824.ToChar()},
        {@"&spadesuit;", 9824.ToChar()},
        {@"&spar;", 8741.ToChar()},
        {@"&sqcap;", 8851.ToChar()},
        {@"&sqcaps;", 8851.ToChar()},
        {@"&sqcup;", 8852.ToChar()},
        {@"&sqcups;", 8852.ToChar()},
        {@"&Sqrt;", 8730.ToChar()},
        {@"&sqsub;", 8847.ToChar()},
        {@"&sqsube;", 8849.ToChar()},
        {@"&sqsubset;", 8847.ToChar()},
        {@"&sqsubseteq;", 8849.ToChar()},
        {@"&sqsup;", 8848.ToChar()},
        {@"&sqsupe;", 8850.ToChar()},
        {@"&sqsupset;", 8848.ToChar()},
        {@"&sqsupseteq;", 8850.ToChar()},
        {@"&squ;", 9633.ToChar()},
        {@"&Square;", 9633.ToChar()},
        {@"&square;", 9633.ToChar()},
        {@"&SquareIntersection;", 8851.ToChar()},
        {@"&SquareSubset;", 8847.ToChar()},
        {@"&SquareSubsetEqual;", 8849.ToChar()},
        {@"&SquareSuperset;", 8848.ToChar()},
        {@"&SquareSupersetEqual;", 8850.ToChar()},
        {@"&SquareUnion;", 8852.ToChar()},
        {@"&squarf;", 9642.ToChar()},
        {@"&squf;", 9642.ToChar()},
        {@"&srarr;", 8594.ToChar()},
//{ @"&Sscr;", 119982.ToChar() },
//{ @"&sscr;", 120008.ToChar() },
        {@"&ssetmn;", 8726.ToChar()},
        {@"&ssmile;", 8995.ToChar()},
        {@"&sstarf;", 8902.ToChar()},
        {@"&Star;", 8902.ToChar()},
        {@"&star;", 9734.ToChar()},
        {@"&starf;", 9733.ToChar()},
        {@"&straightepsilon;", 1013.ToChar()},
        {@"&straightphi;", 981.ToChar()},
        {@"&strns;", 175.ToChar()},
        {@"&Sub;", 8912.ToChar()},
        {@"&sub;", 8834.ToChar()},
        {@"&subdot;", 10941.ToChar()},
        {@"&subE;", 10949.ToChar()},
        {@"&sube;", 8838.ToChar()},
        {@"&subedot;", 10947.ToChar()},
        {@"&submult;", 10945.ToChar()},
        {@"&subnE;", 10955.ToChar()},
        {@"&subne;", 8842.ToChar()},
        {@"&subplus;", 10943.ToChar()},
        {@"&subrarr;", 10617.ToChar()},
        {@"&Subset;", 8912.ToChar()},
        {@"&subset;", 8834.ToChar()},
        {@"&subseteq;", 8838.ToChar()},
        {@"&subseteqq;", 10949.ToChar()},
        {@"&SubsetEqual;", 8838.ToChar()},
        {@"&subsetneq;", 8842.ToChar()},
        {@"&subsetneqq;", 10955.ToChar()},
        {@"&subsim;", 10951.ToChar()},
        {@"&subsub;", 10965.ToChar()},
        {@"&subsup;", 10963.ToChar()},
        {@"&succ;", 8827.ToChar()},
        {@"&succapprox;", 10936.ToChar()},
        {@"&succcurlyeq;", 8829.ToChar()},
        {@"&Succeeds;", 8827.ToChar()},
        {@"&SucceedsEqual;", 10928.ToChar()},
        {@"&SucceedsSlantEqual;", 8829.ToChar()},
        {@"&SucceedsTilde;", 8831.ToChar()},
        {@"&succeq;", 10928.ToChar()},
        {@"&succnapprox;", 10938.ToChar()},
        {@"&succneqq;", 10934.ToChar()},
        {@"&succnsim;", 8937.ToChar()},
        {@"&succsim;", 8831.ToChar()},
        {@"&SuchThat;", 8715.ToChar()},
        {@"&Sum;", 8721.ToChar()},
        {@"&sum;", 8721.ToChar()},
        {@"&sung;", 9834.ToChar()},
        {@"&Sup;", 8913.ToChar()},
        {@"&sup;", 8835.ToChar()},
        {@"&sup1;", 185.ToChar()},
        {@"&sup2;", 178.ToChar()},
        {@"&sup3;", 179.ToChar()},
        {@"&supdot;", 10942.ToChar()},
        {@"&supdsub;", 10968.ToChar()},
        {@"&supE;", 10950.ToChar()},
        {@"&supe;", 8839.ToChar()},
        {@"&supedot;", 10948.ToChar()},
        {@"&Superset;", 8835.ToChar()},
        {@"&SupersetEqual;", 8839.ToChar()},
        {@"&suphsol;", 10185.ToChar()},
        {@"&suphsub;", 10967.ToChar()},
        {@"&suplarr;", 10619.ToChar()},
        {@"&supmult;", 10946.ToChar()},
        {@"&supnE;", 10956.ToChar()},
        {@"&supne;", 8843.ToChar()},
        {@"&supplus;", 10944.ToChar()},
        {@"&Supset;", 8913.ToChar()},
        {@"&supset;", 8835.ToChar()},
        {@"&supseteq;", 8839.ToChar()},
        {@"&supseteqq;", 10950.ToChar()},
        {@"&supsetneq;", 8843.ToChar()},
        {@"&supsetneqq;", 10956.ToChar()},
        {@"&supsim;", 10952.ToChar()},
        {@"&supsub;", 10964.ToChar()},
        {@"&supsup;", 10966.ToChar()},
        {@"&swarhk;", 10534.ToChar()},
        {@"&swArr;", 8665.ToChar()},
        {@"&swarr;", 8601.ToChar()},
        {@"&swarrow;", 8601.ToChar()},
        {@"&swnwar;", 10538.ToChar()},
        {@"&szlig;", 223.ToChar()},
        {@"&Tab;", 9.ToChar()},
        {@"&target;", 8982.ToChar()},
        {@"&Tau;", 932.ToChar()},
        {@"&tau;", 964.ToChar()},
        {@"&tbrk;", 9140.ToChar()},
        {@"&Tcaron;", 356.ToChar()},
        {@"&tcaron;", 357.ToChar()},
        {@"&Tcedil;", 354.ToChar()},
        {@"&tcedil;", 355.ToChar()},
        {@"&Tcy;", 1058.ToChar()},
        {@"&tcy;", 1090.ToChar()},
        {@"&tdot;", 8411.ToChar()},
        {@"&telrec;", 8981.ToChar()},
//{ @"&Tfr;", 120087.ToChar() },
//{ @"&tfr;", 120113.ToChar() },
        {@"&there4;", 8756.ToChar()},
        {@"&Therefore;", 8756.ToChar()},
        {@"&therefore;", 8756.ToChar()},
        {@"&Theta;", 920.ToChar()},
        {@"&theta;", 952.ToChar()},
        {@"&thetasym;", 977.ToChar()},
        {@"&thetav;", 977.ToChar()},
        {@"&thickapprox;", 8776.ToChar()},
        {@"&thicksim;", 8764.ToChar()},
        {@"&ThickSpace;", 8287.ToChar()},
        {@"&thinsp;", 8201.ToChar()},
        {@"&ThinSpace;", 8201.ToChar()},
        {@"&thkap;", 8776.ToChar()},
        {@"&thksim;", 8764.ToChar()},
        {@"&THORN;", 222.ToChar()},
        {@"&thorn;", 254.ToChar()},
        {@"&Tilde;", 8764.ToChar()},
        {@"&tilde;", 732.ToChar()},
        {@"&TildeEqual;", 8771.ToChar()},
        {@"&TildeFullEqual;", 8773.ToChar()},
        {@"&TildeTilde;", 8776.ToChar()},
        {@"&times;", 215.ToChar()},
        {@"&timesb;", 8864.ToChar()},
        {@"&timesbar;", 10801.ToChar()},
        {@"&timesd;", 10800.ToChar()},
        {@"&tint;", 8749.ToChar()},
        {@"&toea;", 10536.ToChar()},
        {@"&top;", 8868.ToChar()},
        {@"&topbot;", 9014.ToChar()},
        {@"&topcir;", 10993.ToChar()},
//{ @"&Topf;", 120139.ToChar() },
//{ @"&topf;", 120165.ToChar() },
        {@"&topfork;", 10970.ToChar()},
        {@"&tosa;", 10537.ToChar()},
        {@"&tprime;", 8244.ToChar()},
        {@"&TRADE;", 8482.ToChar()},
        {@"&trade;", 8482.ToChar()},
        {@"&triangle;", 9653.ToChar()},
        {@"&triangledown;", 9663.ToChar()},
        {@"&triangleleft;", 9667.ToChar()},
        {@"&trianglelefteq;", 8884.ToChar()},
        {@"&triangleq;", 8796.ToChar()},
        {@"&triangleright;", 9657.ToChar()},
        {@"&trianglerighteq;", 8885.ToChar()},
        {@"&tridot;", 9708.ToChar()},
        {@"&trie;", 8796.ToChar()},
        {@"&triminus;", 10810.ToChar()},
        {@"&TripleDot;", 8411.ToChar()},
        {@"&triplus;", 10809.ToChar()},
        {@"&trisb;", 10701.ToChar()},
        {@"&tritime;", 10811.ToChar()},
        {@"&trpezium;", 9186.ToChar()},
//{ @"&Tscr;", 119983.ToChar() },
//{ @"&tscr;", 120009.ToChar() },
        {@"&TScy;", 1062.ToChar()},
        {@"&tscy;", 1094.ToChar()},
        {@"&TSHcy;", 1035.ToChar()},
        {@"&tshcy;", 1115.ToChar()},
        {@"&Tstrok;", 358.ToChar()},
        {@"&tstrok;", 359.ToChar()},
        {@"&twixt;", 8812.ToChar()},
        {@"&twoheadleftarrow;", 8606.ToChar()},
        {@"&twoheadrightarrow;", 8608.ToChar()},
        {@"&Uacute;", 218.ToChar()},
        {@"&uacute;", 250.ToChar()},
        {@"&Uarr;", 8607.ToChar()},
        {@"&uArr;", 8657.ToChar()},
        {@"&uarr;", 8593.ToChar()},
        {@"&Uarrocir;", 10569.ToChar()},
        {@"&Ubrcy;", 1038.ToChar()},
        {@"&ubrcy;", 1118.ToChar()},
        {@"&Ubreve;", 364.ToChar()},
        {@"&ubreve;", 365.ToChar()},
        {@"&Ucirc;", 219.ToChar()},
        {@"&ucirc;", 251.ToChar()},
        {@"&Ucy;", 1059.ToChar()},
        {@"&ucy;", 1091.ToChar()},
        {@"&udarr;", 8645.ToChar()},
        {@"&Udblac;", 368.ToChar()},
        {@"&udblac;", 369.ToChar()},
        {@"&udhar;", 10606.ToChar()},
        {@"&ufisht;", 10622.ToChar()},
//{ @"&Ufr;", 120088.ToChar() },
//{ @"&ufr;", 120114.ToChar() },
        {@"&Ugrave;", 217.ToChar()},
        {@"&ugrave;", 249.ToChar()},
        {@"&uHar;", 10595.ToChar()},
        {@"&uharl;", 8639.ToChar()},
        {@"&uharr;", 8638.ToChar()},
        {@"&uhblk;", 9600.ToChar()},
        {@"&ulcorn;", 8988.ToChar()},
        {@"&ulcorner;", 8988.ToChar()},
        {@"&ulcrop;", 8975.ToChar()},
        {@"&ultri;", 9720.ToChar()},
        {@"&Umacr;", 362.ToChar()},
        {@"&umacr;", 363.ToChar()},
        {@"&uml;", 168.ToChar()},
        {@"&UnderBar;", 95.ToChar()},
        {@"&UnderBrace;", 9183.ToChar()},
        {@"&UnderBracket;", 9141.ToChar()},
        {@"&UnderParenthesis;", 9181.ToChar()},
        {@"&Union;", 8899.ToChar()},
        {@"&UnionPlus;", 8846.ToChar()},
        {@"&Uogon;", 370.ToChar()},
        {@"&uogon;", 371.ToChar()},
//{ @"&Uopf;", 120140.ToChar() },
//{ @"&uopf;", 120166.ToChar() },
        {@"&UpArrow;", 8593.ToChar()},
        {@"&Uparrow;", 8657.ToChar()},
        {@"&uparrow;", 8593.ToChar()},
        {@"&UpArrowBar;", 10514.ToChar()},
        {@"&UpArrowDownArrow;", 8645.ToChar()},
        {@"&UpDownArrow;", 8597.ToChar()},
        {@"&Updownarrow;", 8661.ToChar()},
        {@"&updownarrow;", 8597.ToChar()},
        {@"&UpEquilibrium;", 10606.ToChar()},
        {@"&upharpoonleft;", 8639.ToChar()},
        {@"&upharpoonright;", 8638.ToChar()},
        {@"&uplus;", 8846.ToChar()},
        {@"&UpperLeftArrow;", 8598.ToChar()},
        {@"&UpperRightArrow;", 8599.ToChar()},
        {@"&Upsi;", 978.ToChar()},
        {@"&upsi;", 965.ToChar()},
        {@"&upsih;", 978.ToChar()},
        {@"&Upsilon;", 933.ToChar()},
        {@"&upsilon;", 965.ToChar()},
        {@"&UpTee;", 8869.ToChar()},
        {@"&UpTeeArrow;", 8613.ToChar()},
        {@"&upuparrows;", 8648.ToChar()},
        {@"&urcorn;", 8989.ToChar()},
        {@"&urcorner;", 8989.ToChar()},
        {@"&urcrop;", 8974.ToChar()},
        {@"&Uring;", 366.ToChar()},
        {@"&uring;", 367.ToChar()},
        {@"&urtri;", 9721.ToChar()},
//{ @"&Uscr;", 119984.ToChar() },
//{ @"&uscr;", 120010.ToChar() },
        {@"&utdot;", 8944.ToChar()},
        {@"&Utilde;", 360.ToChar()},
        {@"&utilde;", 361.ToChar()},
        {@"&utri;", 9653.ToChar()},
        {@"&utrif;", 9652.ToChar()},
        {@"&uuarr;", 8648.ToChar()},
        {@"&Uuml;", 220.ToChar()},
        {@"&uuml;", 252.ToChar()},
        {@"&uwangle;", 10663.ToChar()},
        {@"&vangrt;", 10652.ToChar()},
        {@"&varepsilon;", 1013.ToChar()},
        {@"&varkappa;", 1008.ToChar()},
        {@"&varnothing;", 8709.ToChar()},
        {@"&varphi;", 981.ToChar()},
        {@"&varpi;", 982.ToChar()},
        {@"&varpropto;", 8733.ToChar()},
        {@"&vArr;", 8661.ToChar()},
        {@"&varr;", 8597.ToChar()},
        {@"&varrho;", 1009.ToChar()},
        {@"&varsigma;", 962.ToChar()},
        {@"&varsubsetneq;", 8842.ToChar()},
        {@"&varsubsetneqq;", 10955.ToChar()},
        {@"&varsupsetneq;", 8843.ToChar()},
        {@"&varsupsetneqq;", 10956.ToChar()},
        {@"&vartheta;", 977.ToChar()},
        {@"&vartriangleleft;", 8882.ToChar()},
        {@"&vartriangleright;", 8883.ToChar()},
        {@"&Vbar;", 10987.ToChar()},
        {@"&vBar;", 10984.ToChar()},
        {@"&vBarv;", 10985.ToChar()},
        {@"&Vcy;", 1042.ToChar()},
        {@"&vcy;", 1074.ToChar()},
        {@"&VDash;", 8875.ToChar()},
        {@"&Vdash;", 8873.ToChar()},
        {@"&vDash;", 8872.ToChar()},
        {@"&vdash;", 8866.ToChar()},
        {@"&Vdashl;", 10982.ToChar()},
        {@"&Vee;", 8897.ToChar()},
        {@"&vee;", 8744.ToChar()},
        {@"&veebar;", 8891.ToChar()},
        {@"&veeeq;", 8794.ToChar()},
        {@"&vellip;", 8942.ToChar()},
        {@"&Verbar;", 8214.ToChar()},
        {@"&verbar;", 124.ToChar()},
        {@"&Vert;", 8214.ToChar()},
        {@"&vert;", 124.ToChar()},
        {@"&VerticalBar;", 8739.ToChar()},
        {@"&VerticalLine;", 124.ToChar()},
        {@"&VerticalSeparator;", 10072.ToChar()},
        {@"&VerticalTilde;", 8768.ToChar()},
        {@"&VeryThinSpace;", 8202.ToChar()},
//{ @"&Vfr;", 120089.ToChar() },
//{ @"&vfr;", 120115.ToChar() },
        {@"&vltri;", 8882.ToChar()},
        {@"&vnsub;", 8834.ToChar()},
        {@"&vnsup;", 8835.ToChar()},
//{ @"&Vopf;", 120141.ToChar() },
//{ @"&vopf;", 120167.ToChar() },
        {@"&vprop;", 8733.ToChar()},
        {@"&vrtri;", 8883.ToChar()},
//{ @"&Vscr;", 119985.ToChar() },
//{ @"&vscr;", 120011.ToChar() },
        {@"&vsubnE;", 10955.ToChar()},
        {@"&vsubne;", 8842.ToChar()},
        {@"&vsupnE;", 10956.ToChar()},
        {@"&vsupne;", 8843.ToChar()},
        {@"&Vvdash;", 8874.ToChar()},
        {@"&vzigzag;", 10650.ToChar()},
        {@"&Wcirc;", 372.ToChar()},
        {@"&wcirc;", 373.ToChar()},
        {@"&wedbar;", 10847.ToChar()},
        {@"&Wedge;", 8896.ToChar()},
        {@"&wedge;", 8743.ToChar()},
        {@"&wedgeq;", 8793.ToChar()},
        {@"&weierp;", 8472.ToChar()},
//{ @"&Wfr;", 120090.ToChar() },
//{ @"&wfr;", 120116.ToChar() },
//{ @"&Wopf;", 120142.ToChar() },
//{ @"&wopf;", 120168.ToChar() },
        {@"&wp;", 8472.ToChar()},
        {@"&wr;", 8768.ToChar()},
        {@"&wreath;", 8768.ToChar()},
//{ @"&Wscr;", 119986.ToChar() },
//{ @"&wscr;", 120012.ToChar() },
        {@"&xcap;", 8898.ToChar()},
        {@"&xcirc;", 9711.ToChar()},
        {@"&xcup;", 8899.ToChar()},
        {@"&xdtri;", 9661.ToChar()},
//{ @"&Xfr;", 120091.ToChar() },
//{ @"&xfr;", 120117.ToChar() },
        {@"&xhArr;", 10234.ToChar()},
        {@"&xharr;", 10231.ToChar()},
        {@"&Xi;", 926.ToChar()},
        {@"&xi;", 958.ToChar()},
        {@"&xlArr;", 10232.ToChar()},
        {@"&xlarr;", 10229.ToChar()},
        {@"&xmap;", 10236.ToChar()},
        {@"&xnis;", 8955.ToChar()},
        {@"&xodot;", 10752.ToChar()},
//{ @"&Xopf;", 120143.ToChar() },
//{ @"&xopf;", 120169.ToChar() },
        {@"&xoplus;", 10753.ToChar()},
        {@"&xotime;", 10754.ToChar()},
        {@"&xrArr;", 10233.ToChar()},
        {@"&xrarr;", 10230.ToChar()},
//{ @"&Xscr;", 119987.ToChar() },
//{ @"&xscr;", 120013.ToChar() },
        {@"&xsqcup;", 10758.ToChar()},
        {@"&xuplus;", 10756.ToChar()},
        {@"&xutri;", 9651.ToChar()},
        {@"&xvee;", 8897.ToChar()},
        {@"&xwedge;", 8896.ToChar()},
        {@"&Yacute;", 221.ToChar()},
        {@"&yacute;", 253.ToChar()},
        {@"&YAcy;", 1071.ToChar()},
        {@"&yacy;", 1103.ToChar()},
        {@"&Ycirc;", 374.ToChar()},
        {@"&ycirc;", 375.ToChar()},
        {@"&Ycy;", 1067.ToChar()},
        {@"&ycy;", 1099.ToChar()},
        {@"&yen;", 165.ToChar()},
//{ @"&Yfr;", 120092.ToChar() },
//{ @"&yfr;", 120118.ToChar() },
        {@"&YIcy;", 1031.ToChar()},
        {@"&yicy;", 1111.ToChar()},
//{ @"&Yopf;", 120144.ToChar() },
//{ @"&yopf;", 120170.ToChar() },
//{ @"&Yscr;", 119988.ToChar() },
//{ @"&yscr;", 120014.ToChar() },
        {@"&YUcy;", 1070.ToChar()},
        {@"&yucy;", 1102.ToChar()},
        {@"&Yuml;", 376.ToChar()},
        {@"&yuml;", 255.ToChar()},
        {@"&Zacute;", 377.ToChar()},
        {@"&zacute;", 378.ToChar()},
        {@"&Zcaron;", 381.ToChar()},
        {@"&zcaron;", 382.ToChar()},
        {@"&Zcy;", 1047.ToChar()},
        {@"&zcy;", 1079.ToChar()},
        {@"&Zdot;", 379.ToChar()},
        {@"&zdot;", 380.ToChar()},
        {@"&zeetrf;", 8488.ToChar()},
        {@"&ZeroWidthSpace;", 8203.ToChar()},
        {@"&Zeta;", 918.ToChar()},
        {@"&zeta;", 950.ToChar()},
        {@"&Zfr;", 8488.ToChar()},
//{ @"&zfr;", 120119.ToChar() },
        {@"&ZHcy;", 1046.ToChar()},
        {@"&zhcy;", 1078.ToChar()},
        {@"&zigrarr;", 8669.ToChar()},
        {@"&Zopf;", 8484.ToChar()},
//{ @"&zopf;", 120171.ToChar() },
//{ @"&Zscr;", 119989.ToChar() },
//{ @"&zscr;", 120015.ToChar() },
        {@"&zwj;", 8205.ToChar()},
        {@"&zwnj;", 8204.ToChar()},
    };
}
