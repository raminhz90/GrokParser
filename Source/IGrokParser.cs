namespace GrokParser
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IGrokParser
    {
        /// <summary>
        /// Parse the input string and return the result as a dictionary.
        /// </summary>
        /// <param name="input">string to be parsed by grok</param>
        /// <returns>a dictionary containing matched named groups in grok</returns>
        public Dictionary<string, dynamic> Parse(string input);
        /// <summary>
        /// Parse the input string and return the result as a dictionary.
        /// </summary>
        /// <param name="input">string to be parsed by grok</param>
        /// <param name="cancellationToken"></param>
        /// <returns> a task that returns the matched grok result when completed</returns>
        public Task<Dictionary<string, dynamic>> ParseAsync(string input, CancellationToken cancellationToken = default);
    }

}
