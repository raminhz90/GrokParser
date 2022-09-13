namespace GrokParser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class GrokBuilder
    {
        private static readonly Regex GrokPatternExtractor = new Regex(@"%{(\w+)(?::?)([\w\.]+)?(?::?)(\w+)?}", RegexOptions.Compiled);
        private readonly string grokString;
        private readonly Dictionary<string, string> nameMaps = new Dictionary<string, string>();
        private readonly Dictionary<string, string> typeMaps = new Dictionary<string, string>();
        private readonly IDictionary<string, string>? customPatterns;
        private readonly IDictionary<string, string>? postProcessors;
        private readonly IEnumerable<string>? filters;
        public GrokBuilder(string grok,
                           IDictionary<string, string>? customPatterns = null,
                           IDictionary<string, string>? postProcessors = null,
                           IEnumerable<string>? filters = null)
        {
            this.grokString = grok;
            this.customPatterns = customPatterns;
            this.postProcessors = postProcessors;
            this.filters = filters;
        }
        public IGrokParser Build()
        {
            var mainRegex = this.BuildRegexFromGrok(this.grokString, 0);
            var postProcessors = new Dictionary<string, Regex>();
            if (this.postProcessors != null)
            {
                var counter = 1;
                foreach (var item in this.postProcessors)
                {
                    postProcessors.Add(item.Key, this.BuildRegexFromGrok(this.grokString, counter));
                    counter++;
                }
            }
            return new Grok(mainRegex, postProcessors, this.nameMaps, this.typeMaps, this.filters);

        }
        public Task<IGrokParser> BuildAsync(CancellationToken cancellationToken)
        {
            var mainRegex = this.BuildRegexFromGrok(this.grokString, 0, cancellationToken);
            var postProcessors = new Dictionary<string, Regex>();
            if (this.postProcessors != null)
            {
                var counter = 1;
                foreach (var item in this.postProcessors)
                {
                    postProcessors.Add(item.Key, this.BuildRegexFromGrok(this.grokString, counter, cancellationToken));
                    counter++;
                }
            }
            var result = new Grok(mainRegex, postProcessors, this.nameMaps, this.typeMaps, this.filters);
            return Task.FromResult<IGrokParser>(result);
        }
        private Regex BuildRegexFromGrok(string pattern, int patternId, CancellationToken cancellationToken = default)
        {

            var counter = 0;
            var regexFromGrok = pattern;
            while (!cancellationToken.IsCancellationRequested)
            {
                var groks = GrokPatternExtractor.Matches(regexFromGrok);
                if (groks == null || groks.Count == 0)
                {
                    break;
                }
                for (var i = 0; i < groks.Count; i++)
                {
                    if (groks[i] == null)
                    {
                        continue;
                    }

                    var uniqeuName = this.GenerateUniqueName(patternId, counter, i);
                    this.nameMaps.Add(uniqeuName, groks[i].Groups[1].Value);
                    if (!string.IsNullOrEmpty(groks[i].Groups[2].Value))
                    {
                        this.typeMaps.Add(uniqeuName, groks[i].Groups[2].Value.ToLowerInvariant());
                    }
                }
                regexFromGrok = GrokPatternExtractor.Replace(regexFromGrok, this.GrokReplace);
                counter++;

            }
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            return new Regex(regexFromGrok, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            // var groupNames = finalRegex.GetGroupNames().Where(x => !int.TryParse(x, out _));

        }
        private string GrokReplace(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }
            var pattern = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            if (string.IsNullOrEmpty(name))
            {
                return this.ReplaceUnNamedGrok(pattern);
            }
            else
            {
                return this.ReplaceNamedGrok(pattern, name);
            }
        }
        private string ReplaceNamedGrok(string pattern, string name)
        {
            if (this.customPatterns != null && this.customPatterns.TryGetValue(pattern, out var regex))
            {
                return $"(?<{name}>{regex})";
            }
            if (DefaultGrokPatterns.DefaultPatterns.TryGetValue(pattern, out regex))
            {
                return $"(?<{name}>{regex})";
            }
            return $"(?<{name}>)";
        }
        private string ReplaceUnNamedGrok(string pattern)
        {
            if (this.customPatterns != null && this.customPatterns.TryGetValue(pattern, out var regex))
            {
                return $"({regex})";
            }
            if (DefaultGrokPatterns.DefaultPatterns.TryGetValue(pattern, out regex))
            {
                return $"({regex})";
            }
            return "()";
        }
        private string GenerateUniqueName(int patternId, int id, int num)
        {
            var builder = new StringBuilder();
            while (patternId > 0)
            {
                var remainder = patternId % 25;
                _ = builder.Append((char)(remainder + 97));
                patternId /= 25;
            }
            _ = builder.Append('Z');
            while (id > 0)
            {
                var remainder = id % 25;
                _ = builder.Append((char)(remainder + 97));
                id /= 25;
            }
            _ = builder.Append('Z');
            while (num > 0)
            {
                var remainder = num % 25;
                _ = builder.Append((char)(remainder + 97));
                num /= 25;
            }
            return builder.ToString();
        }
    }
}
