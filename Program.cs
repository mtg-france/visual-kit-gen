using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.CommandLine;
using visual_kit_gen;

const int DefaultRatioBackWidth = 2560;
const int DefaultRatioBackHeight = 1440;
const int DefaultMargin = 50;
const float DefaultBlend = .9f;
const int RectInflate = 20;

var communityArgument = new Argument<string>(name: "Community", description: "Name of the community");

var backOption = new Option<string>(name: "--back", description: "The background image to use");
backOption.AddAlias("-b");

var outOption = new Option<string>(name: "--out", description: "The filename to write the image to. defaults to back + community name");
outOption.AddAlias("-o");

var lightOption = new Option<bool?>(name: "--light", description: "Render the logo for light mode (dark text). defaults to autodetect based on file's name");
lightOption.AddAlias("-l");

var scaleOption = new Option<float>(name: "--scale", description: "Scale factor for the logo");
scaleOption.AddAlias("-s");

var marginOption = new Option<int>(name: "--margin", () => DefaultMargin, description: "Margin in pixel with the border of the image");
marginOption.AddAlias("-m");

var positionOption = new Option<LogoPosition>(name: "--position", () => LogoPosition.Center, description: "Logo position");
positionOption.AddAlias("-p");

var rectOption = new Option<bool>(name: "--rect", () => false, description: "Draws a back rectangle behind the community name");
rectOption.AddAlias("-r");

var rectOpacityOption = new Option<float>(name: "--rect-opacity", () => DefaultBlend, description: "Opacity of the back rectangle. 1 for fully opaque, 0 for transparent");
rectOpacityOption.AddAlias("-ro");

var rectColorOption = new Option<string>(name: "--rect-color", description: "Color of the back rectangle, in #hex format, #ffffff by default for light, #2f2f2f for dark");
rectColorOption.AddAlias("-rc");

////////////////////////////////////////////////
//
// Commande par défaut
//
// Génère une image, avec les paramètres indiqués
// 

var rootCommand = new RootCommand("Generate a community logo");
rootCommand.AddArgument(communityArgument);
rootCommand.AddOption(backOption);
rootCommand.AddOption(outOption);
rootCommand.AddOption(marginOption);
rootCommand.AddOption(lightOption);
rootCommand.AddOption(scaleOption);
rootCommand.AddOption(positionOption);
rootCommand.AddOption(rectOption);
rootCommand.AddOption(rectOpacityOption);
rootCommand.AddOption(rectColorOption);

rootCommand.SetHandler(context =>
{
    var community = context.ParseResult.GetValueForArgument(communityArgument);
    var back = context.ParseResult.GetValueForOption(backOption);
    var @out = context.ParseResult.GetValueForOption(outOption);
    var light = context.ParseResult.GetValueForOption(lightOption);
    var scale = context.ParseResult.GetValueForOption(scaleOption);
    var margin = context.ParseResult.GetValueForOption(marginOption);
    var position = context.ParseResult.GetValueForOption(positionOption);
    var rect = context.ParseResult.GetValueForOption(rectOption);
    var rectOpacity = context.ParseResult.GetValueForOption(rectOpacityOption);
    var rectColor = context.ParseResult.GetValueForOption(rectColorOption);

    back ??= "screen_01";

    // default based on file's name ending
    light ??= (back.EndsWith("light"));

    // we support omitting the .png extension
    if (Path.GetExtension(back) == string.Empty)
        back += ".png";

    // we support omitting the backs\ prefix
    if (!File.Exists(back) && File.Exists(Path.Combine("backs", back)))
        back = Path.Combine("backs", back);

    if (string.IsNullOrEmpty(@out))
        @out = "out";
    
    @out = WriteLogoImage(community, @out, back, position: position, light ?? false, scale, margin, rect, rectOpacity, rectColor);

    Console.WriteLine($"Logo {community} généré pour le fond {back} dans le fichier {@out}");
});

////////////////////////////////////////////////
//
// Commande pack
//
// Génère toutes les images
// 

var packCommand = new Command("pack", "Generate a visual pack for a community");
packCommand.AddArgument(communityArgument);
packCommand.AddOption(marginOption);
packCommand.AddOption(outOption);
packCommand.SetHandler((community, @out, margin) =>
{
    @out ??= "out";
    var outDir = Path.Combine(Environment.CurrentDirectory, @out);
    if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

    var images = GenerateImages(community, margin);
    foreach (var (img, source) in images)
    {
        var imageOut = GenerateOutFileName(@out, source, community);
        img.SaveAsPng(@out);
    }

}, communityArgument, outOption, marginOption);

rootCommand.AddCommand(packCommand);

return await rootCommand.InvokeAsync(args);

IEnumerable<(Image Image, string SourceFile)> GenerateImages(string community, int margin)
{
    var service = new LogoService();
    yield return (service.AddLogoToImage(community, @"backs/screen_01.png", margin: margin), @"backs/screen_01.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_02.png", margin: margin), @"backs/screen_02.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_03.png", margin: margin), @"backs/screen_03.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_04_dark.png", position: LogoPosition.CenterLeft, margin: margin), @"backs/screen_04_dark.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_04_light.png", position: LogoPosition.CenterLeft, light: true, margin: margin), @"backs/screen_04_light.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_05_dark.png", position: LogoPosition.TopLeft, margin: margin), @"backs/screen_05_dark.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_05_light.png", position: LogoPosition.TopLeft, light: true, margin: margin), @"backs/screen_05_light.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_06_dark.png", position: LogoPosition.BottomLeft, margin: margin), @"backs/screen_06_dark.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_06_light.png", position: LogoPosition.BottomLeft, light: true, margin: margin), @"backs/screen_06_light.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_07_dark.png", margin: margin), @"backs/screen_07_dark.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_07_light.png", light: true, margin: margin), @"backs/screen_07_light.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_08_dark.png", margin: margin), @"backs/screen_08_dark.png");
    yield return (service.AddLogoToImage(community, @"backs/screen_08_light.png", light: true, margin: margin), @"backs/screen_08_light.png");
    yield return (service.AddLogoToImage(community, @"backs/twitter_dark.png", margin: margin, customScale: 0.9f), @"backs/twitter_dark.png");
    yield return (service.AddLogoToImage(community, @"backs/twitter_light.png", light: true, margin: margin, customScale: 0.9f, rect: true), @"backs/twitter_light.png");
    yield return (service.AddLogoToImage(community, @"backs/event_dark.png", margin: margin, position: LogoPosition.TopLeft, customScale: 0.7f), @"backs/event_dark.png");
    yield return (service.AddLogoToImage(community, @"backs/event_light.png", light: true, margin: margin, position: LogoPosition.TopLeft, customScale: 0.7f), @"backs/event_light.png");
}

string WriteLogoImage(
            string community,
            string @out,
            string back,
            LogoPosition position = LogoPosition.Center,
            bool light = false,
            float customScale = 0,
            int margin = DefaultMargin,
            bool rect = false,
            float rectOpacity = DefaultBlend,
            string? rectColor = null)
{
    var service = new LogoService();
    using var backImg = service.AddLogoToImage(community, back, position, light, customScale, margin, rect, rectOpacity, rectColor);

    @out = GenerateOutFileName(@out, back, community);

    backImg.SaveAsPng(@out);

    return @out;
}

string GenerateOutFileName(string @out, string back, string community)
{
    if (string.IsNullOrEmpty(@out))
    {
        if (string.IsNullOrEmpty(back))
        {
            return $"{community.ToLowerInvariant()}_dark.png";
        }
        else
        {
            return $"{Path.GetFileNameWithoutExtension(back)}_{community.ToLowerInvariant()}.png";
        }
    }
    else if (Directory.Exists(@out))
    {
        if (string.IsNullOrEmpty(back))
        {
            return Path.Combine(@out, $"{community.ToLowerInvariant()}_dark.png");
        }
        else
        {
            return Path.Combine(@out, $"{Path.GetFileNameWithoutExtension(back)}_{community.ToLowerInvariant()}.png");
        }
    }
    return @out;
}