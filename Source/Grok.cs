namespace GrokParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IGrokParser
    {
        public Dictionary<string, dynamic> Parse(string input);
        public Task<Dictionary<string, dynamic>> ParseAsync(string input, CancellationToken cancellationToken);
    }
    internal class Grok : IGrokParser
    {
        private readonly Regex mainRegex;
        private readonly IDictionary<string, Regex>? postProcessors;
        private readonly IDictionary<string, string> nameMaps;
        private readonly IDictionary<string, string> typeMaps;
        private readonly IEnumerable<string>? filters;

        public Grok(Regex mainRegex,
                    IDictionary<string, Regex>? postProcessors,
                    IDictionary<string, string> nameMaps,
                    IDictionary<string, string> typeMaps,
                    IEnumerable<string>? filters)
        {
            this.mainRegex = mainRegex;
            this.postProcessors = postProcessors;
            this.nameMaps = nameMaps;
            this.typeMaps = typeMaps;
            this.filters = filters;
        }

        public Dictionary<string, dynamic> Parse(string input)
        {
            var result = new Dictionary<string, dynamic>();
            // process main regex
            this.ParseWithRegex(this.mainRegex, input, result);
            // process post processors
            if (this.postProcessors != null)
            {
                foreach (var item in this.postProcessors)
                {
                    this.ParseWithRegex(item.Value, result[item.Key], result);
                }
            }
            // process filters
            if (this.filters != null)
            {
                foreach (var filter in this.filters)
                {
                    _ = result.Remove(filter);
                }
            }
            return result;
        }

        public Task<Dictionary<string, dynamic>> ParseAsync(string input, CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, dynamic>();
            // process main regex
            this.ParseWithRegex(this.mainRegex, input, result, cancellationToken);
            // process post processors
            if (this.postProcessors != null)
            {
                foreach (var item in this.postProcessors)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    this.ParseWithRegex(item.Value, result[item.Key], result);
                }
            }
            // process filters
            if (this.filters != null)
            {
                foreach (var filter in this.filters)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _ = result.Remove(filter);
                }
            }
            return Task.FromResult(result);
        }
        private void ParseWithRegex(Regex regex, string input, Dictionary<string, dynamic> result, CancellationToken cancellationToken = default)
        {
            var match = regex.Match(input);
            if (match.Success)
            {
                foreach (var groupName in regex.GetGroupNames().Where(g => !int.TryParse(g, out _))) // g !="0" is probably enough and faster
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var value = match.Groups[groupName].Value;
                    if (this.nameMaps.TryGetValue(groupName, out var name))
                    {
                        _ = result.Remove(name);
                        if (this.typeMaps.TryGetValue(name, out var type))
                        {
                            result.Add(name, this.MapToType(type, value));
                        }
                        else
                        {
                            result.Add(name, value);
                        }
                    }
                    else
                    {
                        result.Add(groupName, value);
                    }
                }
            }
        }

        private dynamic MapToType(string type, string value) => type switch
        {
            "int" => int.Parse(value),
            "long" => long.Parse(value),
            "float" => float.Parse(value),
            "double" => double.Parse(value),
            "bool" => bool.Parse(value),
            "datetime" => DateTime.Parse(value),
            "guid" => Guid.Parse(value),
            "uri" => new Uri(value),
            "datetimeoffset" => DateTimeOffset.Parse(value),
            "timespan" => TimeSpan.Parse(value),
            "string" => value,
            _ => value,
        };
    }

}
