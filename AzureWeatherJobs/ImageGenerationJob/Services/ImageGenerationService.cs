using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherImageGenerator.ImageGenerationJob.Entities;

namespace ImageGenerationJob.Services
{
    public class ImageGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageGenerationService> _logger;
        private const string ImageUrl = "https://picsum.photos/200/300";

        public ImageGenerationService(HttpClient httpClient, ILogger<ImageGenerationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MemoryStream> GenerateImageAsync(WeatherStation weatherStation)
        {
            try
            {
                var imageResponse = await _httpClient.GetAsync(ImageUrl);
                if (imageResponse.IsSuccessStatusCode)
                {
                    using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
                    var memoryStream = await OverlayTextOnImage(imageStream, weatherStation);

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
                using (var image = await Image.LoadAsync(imageStream))
                {
                    var fontCollection = new FontCollection();
                    var font = SystemFonts.CreateFont("Verdana", 16);
                    var color = Rgba32.ParseHex("#23b9c5");

                    // Draw texts
                    var texts = new (string text, float x, float y)[]
                    {
                        ($"Region: {weatherStation.Region}", 10, 30),
                        ($"Temp: {weatherStation.Temperature}°C", 10, 60),
                        ($"Wind: {weatherStation.WindSpeed} km/h", 10, 90)
                    };

                    foreach (var (text, x, y) in texts)
                    {
                        image.Mutate(ctx => ctx.DrawText(text, font, Color.White, new PointF(x, y)));
                    }

                    await image.SaveAsPngAsync(memoryStream);
                }

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
