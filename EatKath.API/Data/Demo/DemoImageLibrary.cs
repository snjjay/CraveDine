using System.Text.RegularExpressions;

namespace EatKath.API.Data.Demo
{
    // The local demo photo library: wwwroot/uploads/demo/{category}-{nn}.jpg
    // (git-ignored; recreate with Data/Demo/download-demo-images.ps1, sources
    // in Data/Demo/demo-image-attribution.json).
    public static class DemoImageLibrary
    {
        private static readonly Regex FileName = new(@"^(?<category>[a-z]+)-(?<n>\d+)\.jpg$", RegexOptions.IgnoreCase);

        // category -> URLs ("/uploads/demo/momo-01.jpg"), in file-name order.
        public static IReadOnlyDictionary<string, IReadOnlyList<string>> Load(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new InvalidOperationException(
                    $"Demo image folder not found: {directory}. Run Data/Demo/download-demo-images.ps1 first.");
            }

            return Directory.GetFiles(directory, "*.jpg")
                .Select(Path.GetFileName)
                .Select(name => (Name: name!, Match: FileName.Match(name!)))
                .Where(x => x.Match.Success)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .GroupBy(x => x.Match.Groups["category"].Value.ToLowerInvariant())
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(x => $"/uploads/demo/{x.Name}").ToList());
        }
    }
}
