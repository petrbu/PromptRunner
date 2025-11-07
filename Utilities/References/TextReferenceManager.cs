using System;
using System.Collections.Generic;

namespace PromptRunner.Utilities.References
{
    /// <summary>
    /// Manages dynamic text references that can be used in prompts
    /// </summary>
    public class TextReferenceManager
    {
        private readonly Dictionary<string, string> _textReferences;
        
        public TextReferenceManager()
        {
            _textReferences = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a text reference that can be used in prompts
        /// </summary>
        /// <param name="key">The placeholder name to use in prompts</param>
        /// <param name="text">The actual text content</param>
        public void AddTextReference(string key, string text)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Text reference key cannot be empty", nameof(key));

            _textReferences[key] = text ?? string.Empty;
        }
        
        /// <summary>
        /// Gets the actual text for a reference key
        /// </summary>
        /// <param name="key">The reference key</param>
        /// <returns>The text content or null if not found</returns>
        public string GetText(string key)
        {
            string text;
            _textReferences.TryGetValue(key, out text);
            return text;
        }
        
        /// <summary>
        /// Checks if a reference key exists
        /// </summary>
        /// <param name="key">The reference key</param>
        /// <returns>True if the key exists</returns>
        public bool HasTextReference(string key)
        {
            return _textReferences.ContainsKey(key);
        }
    }
}
