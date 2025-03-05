using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace NextBepLoader.Core.Utils;

public class TextEnvironment
{
    private readonly Dictionary<string, string> replacements = new();
    private readonly List<IReplaceRule> rules = [];

    public string this[string key]
    {
        get => replacements[key];
        set => replacements[key] = value;
    }

    public string Replace(string text)
    {
        var result = rules.Aggregate(text, (current, rule) => rule.Replace(current, replacements));
        return result;
    }

    public TextEnvironment RegisterRule(IReplaceRule rule)
    {
        rules.Add(rule);
        return this;
    }

    public TextEnvironment Register(string key, string value)
    {
        replacements[key] = value;
        return this;
    }

    public static explicit operator Dictionary<string, string>(TextEnvironment env) => env.replacements;
    public static explicit operator ReadOnlyCollection<IReplaceRule>(TextEnvironment env) => env.rules.AsReadOnly();
}

public interface IReplaceRule
{
    public string Replace(string text, Dictionary<string, string> replacements);
    public string Replace(string text, string key, string value);
}

public class CharReplaceRule(char @char) : BaseReplaceRule
{
    public override string Replace(string text, string key, string value) =>
        base.Replace(text, $"{@char}{key}{@char}", value);
}

public class RegexReplaceRule(Regex regex) : BaseReplaceRule
{
    public override string Replace(string text, string key, string value) =>
        !AllowKeys.Contains(key) ? text : regex.Replace(text, value);
}

public class BaseReplaceRule(string[]? allowKeys = null) : IReplaceRule
{
    public string[] AllowKeys { get; set; } = allowKeys ?? [];

    public virtual string Replace(string text, Dictionary<string, string> replacements)
    {
        var result = text;
        foreach (var (key, value) in replacements)
            result = Replace(result, key, value);
        return result;
    }

    public virtual string Replace(string text, string key, string value)
    {
        if (AllowKeys.Any() && !AllowKeys.Contains(key))
            return text;
        return text.Replace(key, value);
    }
}
