using System;
using System.IO;

namespace PromptRunner.Utilities
{
    public static class ImageHelper
    {
        public static string ImageToBase64(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                {
                    Console.WriteLine($"Image file not found: {imagePath}");
                    return null;
                }

                byte[] imageBytes = File.ReadAllBytes(imagePath);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting image to base64: {ex.Message}");
                return null;
            }
        }
    }
}