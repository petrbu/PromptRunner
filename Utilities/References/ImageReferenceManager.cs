using System;
using System.Collections.Generic;

namespace PromptRunner.Utilities.References
{
    /// <summary>
    /// Manages dynamic image references that can be used in prompts
    /// </summary>
    public class ImageReferenceManager
    {
        private readonly Dictionary<string, string> _imageReferences;
        
        public ImageReferenceManager()
        {
            _imageReferences = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds an image reference that can be used in prompts
        /// </summary>
        /// <param name="key">The placeholder name to use in prompts</param>
        /// <param name="imagePath">The actual file path to the image</param>
        public void AddImageReference(string key, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Image reference key cannot be empty", nameof(key));

            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("Image path cannot be empty", nameof(imagePath));

            if (!System.IO.File.Exists(imagePath))
                throw new ArgumentException($"Image file does not exist at path: {imagePath}", nameof(imagePath));

            _imageReferences[key] = imagePath;
        }
        
        /// <summary>
        /// Gets the actual file path for a reference key
        /// </summary>
        /// <param name="key">The reference key</param>
        /// <returns>The file path or null if not found</returns>
        public string GetImagePath(string key)
        {
            string path;
            _imageReferences.TryGetValue(key, out path);
            return path;
        }
        
        /// <summary>
        /// Checks if a reference key exists
        /// </summary>
        /// <param name="key">The reference key</param>
        /// <returns>True if the key exists</returns>
        public bool HasImageReference(string key)
        {
            return _imageReferences.ContainsKey(key);
        }
    }
}