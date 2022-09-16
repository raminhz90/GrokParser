namespace GrokParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class GrokBuilder
    {
        private static readonly Regex GrokPatternExtractor = new Regex(@"%{(\w+)(?::?)([\w\.]+)?(?::?)(\w+)?}", RegexOptions.Compiled);
        private readonly string grokString;
        private readonly Dictionary<string, string> nameMaps = new Dictionary<string, string>();
        private readonly Dictionary<string, string> typeMaps = new Dictionary<string, string>();
        private readonly IDictionary<string, string> customPatterns = new Dictionary<string, string>();
        private readonly List<KeyValuePair<string, string>> postProcessors = new List<KeyValuePair<string, string>>();
        private readonly List<string> filters = new List<string>();
        public GrokBuilder(string grok,
                           IDictionary<string, string>? customPatterns = null,
                           IEnumerable<KeyValuePair<string, string>>? postProcessors = null,
                           IEnumerable<string>? filters = null)
        {
            this.grokString = grok;
            this.customPatterns = customPatterns ?? this.customPatterns;
            this.postProcessors = postProcessors?.ToList() ?? this.postProcessors;
            this.filters = filters?.ToList() ?? this.filters;
        }
        public IGrokParser Build()
        {
            var mainRegex = this.BuildRegexFromGrok(this.grokString, 0);
            var postProcessors = new List<KeyValuePair<string, Regex>>();
            if (this.postProcessors != null)
            {
                var counter = 1;
                foreach (var item in this.postProcessors)
                {
                    var postprocessor = new KeyValuePair<string, Regex>(item.Key, this.BuildRegexFromGrok(this.grokString, counter));
                    postProcessors.Add(postprocessor);
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

        public GrokBuilder AddPostProcessor(string name, string pattern)
        {
            if (this.postProcessors == null)
            {
                throw new InvalidOperationException("Post processors are not supported");
            }
            this.postProcessors.Add(new KeyValuePair<string, string>(name, pattern));
            return this;
        }

        public GrokBuilder AddFilter(string filter)
        {
            this.filters.Add(filter);
            return this;
        }
        public GrokBuilder AddFilter(IEnumerable<string> filters)
        {
            if (this.filters == null)
            {
                throw new InvalidOperationException("Filters are not supported");
            }
            this.filters.AddRange(filters);
            return this;
        }
        public GrokBuilder AddCustomPattern(string name, string pattern, bool replace = false)
        {
            if (this.customPatterns.ContainsKey(name))
            {
                if (replace)
                {
                    _ = this.customPatterns.Remove(name);
                }
                else
                {
                    throw new InvalidOperationException($"Custom pattern with name {name} already exists");
                }
            }
            this.customPatterns.Add(name, pattern);
            return this;
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

                    if (!string.IsNullOrWhiteSpace(groks[i].Groups[2].Value))
                    {
                        var uniqeuName = NameGenerator.UniqueNameGenerator.GenerateUniqueName(patternId, counter, i);
                        this.nameMaps.Add(uniqeuName, groks[i].Groups[2].Value);

                        if (!string.IsNullOrEmpty(groks[i].Groups[3].Value))
                        {
                            this.typeMaps.Add(uniqeuName, groks[i].Groups[3].Value.ToLowerInvariant());
                        }
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

        }


        private string GrokReplace(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }
            var pattern = match.Groups[1].Value;
            if (string.IsNullOrEmpty(match.Groups[2].Value))
            {
                return this.ReplaceUnNamedGrok(pattern);
            }
            else
            {
                var name = this.nameMaps.Where(x => x.Value == match.Groups[2].Value).Select(x => x.Key).First();
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

    }
}
