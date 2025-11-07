namespace PromptRunner.AI.Models
{
    public class ChatMessage
    {
        public string Role { get; set; } // e.g., "user", "system"
        public string Content { get; set; }
        public ImageAttachment[] Images { get; set; } = new ImageAttachment[0];
        public bool HasImages => Images != null && Images.Length > 0;

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
        
        public ChatMessage(string role, string content, ImageAttachment[] images)
        {
            Role = role;
            Content = content;
            Images = images ?? new ImageAttachment[0];
        }
    }
    
    public class ImageAttachment
    {
        public string Path { get; set; }
        public string Base64Data { get; set; }
        public string MimeType { get; set; }
        
        public ImageAttachment(string path)
        {
            Path = path;
            MimeType = GetMimeTypeFromPath(path);
        }
        
        private string GetMimeTypeFromPath(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
                default:
                    return "image/jpeg"; // Default
            }
        }
    }
}