using Microsoft.Extensions.Logging;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using WeatherImageGenerator.ImageGenerationJob.Entities;

namespace ImageGenerationJob.Services
{
    public class ImageGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageGenerationService> _logger;
        private const string ImageUrl = "https://picsum.photos/200/300"; // Picsum API for random images

        public ImageGenerationService(HttpClient httpClient, ILogger<ImageGenerationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MemoryStream> GenerateImageAsync(WeatherStation weatherStation)
        {
            try
            {
                // Download the image from Picsum
                var imageResponse = await _httpClient.GetAsync(ImageUrl);

                if (imageResponse.IsSuccessStatusCode)
                {
                    using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
                    var memoryStream = await OverlayTextOnImage(imageStream, weatherStation);

                    // Return the generated image as a MemoryStream
                    return memoryStream;
                }
                else
                {
                    _logger.LogError($"Failed to get image from Picsum. Status code: {imageResponse.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating image: {ex.Message}");
                return null;
            }
        }

        private async Task<MemoryStream> OverlayTextOnImage(Stream imageStream, WeatherStation weatherStation)
        {
            var memoryStream = new MemoryStream();

            try
            {
                // Load the image from the provided stream
                using (var image = Image.Load<Rgba32>(imageStream))
                {
                    // Define the texts to overlay on the image
                    var texts = new (string text, (float x, float y) position, int fontSize, string colorHex)[]
                    {
                        ($"Region: {weatherStation.Region}", (10, 30), 20, "#FFFFFF"),
                        ($"Temp: {weatherStation.Temperature}°C", (10, 60), 20, "#FFFFFF"),
                        ($"Wind: {weatherStation.WindSpeed} km/h", (10, 90), 20, "#FFFFFF")
                    };

                    // Overlay the text on the image
                    foreach (var (text, (x, y), fontSize, colorHex) in texts)
                    {
                        // Load the font
                        var font = SystemFonts.CreateFont("Verdana", fontSize);
                        var color = Rgba32.ParseHex(colorHex);

                        // Draw the text onto the image
                        image.Mutate(img => img.DrawText(text, font, color, new PointF(x, y)));
                    }

                    // Save the image to the memory stream as PNG
                    image.SaveAsPng(memoryStream);
                }

                // Reset the memory stream position to the beginning
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error overlaying text on image: {ex.Message}");
                throw;
            }
        }
    }
}
