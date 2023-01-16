using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.CommandLine;

const int DefaultRatioBackWidth = 2560;
const int DefaultRatioBackHeight = 1440;
const int DefaultMargin = 50;

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

var rectOption = new Option<bool>(name: "--rect", description: "Draws a back rectangle behind the community name");
rectOption.AddAlias("-r");

var rectOpacityOption = new Option<float>(name: "--rect-opacity", () => .9f, description: "Opacity of the back rectangle. 1 for fully opaque, 0 for transparent");
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

    WriteLogoImage(community, outDir, @"backs/screen_01.png", margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_02.png", margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_03.png", margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_04_dark.png", position: LogoPosition.CenterLeft, margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_04_light.png", position: LogoPosition.CenterLeft, light: true, margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_05_dark.png", position: LogoPosition.TopLeft, margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_05_light.png", position: LogoPosition.TopLeft, light: true, margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_06_dark.png", position: LogoPosition.BottomLeft, margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_06_light.png", position: LogoPosition.BottomLeft, light: true, margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_07_dark.png", margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_07_light.png", light: true, margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_08_dark.png", margin: margin);
    WriteLogoImage(community, outDir, @"backs/screen_08_light.png", light: true, margin: margin);
    WriteLogoImage(community, outDir, @"backs/twitter_dark.png", margin: margin, customScale: 0.9f);
    WriteLogoImage(community, outDir, @"backs/twitter_light.png", light: true, margin: margin, customScale: 0.9f);
    WriteLogoImage(community, outDir, @"backs/event_dark.png", margin: margin, position: LogoPosition.TopLeft, customScale: 0.7f);
    WriteLogoImage(community, outDir, @"backs/event_light.png", light: true, margin: margin, position: LogoPosition.TopLeft, customScale: 0.7f);

}, communityArgument, outOption, marginOption);

rootCommand.AddCommand(packCommand);

return await rootCommand.InvokeAsync(args);

string WriteLogoImage(string community, string @out, string back, LogoPosition position = LogoPosition.Center, bool light = false, float customScale = 0, int margin = DefaultMargin, bool rect = false, float rectOpacity = 0.9f, string? rectColor = null)
{
    using var backImg = AddLogoToImage(community, back, position, light, customScale, margin, rect, rectOpacity, rectColor);

    @out = GenerateOutFileName(@out, back, community);

    backImg.SaveAsPng(@out);

    return @out;
}

Image AddLogoToImage(string community, string back, LogoPosition position = LogoPosition.Center, bool light = false, float customScale = 0, int margin = DefaultMargin, bool rect = false, float rectOpacity = 0.9f, string? rectColor = null)
{
    FontCollection collection = new();
    var family = collection.Add("Lato-Bold.ttf");

    var hasBack = !string.IsNullOrEmpty(back);

    Image? backImg = null;
    if (hasBack)
        backImg = Image.Load(back);


    // par défaut on ajuste le ratio du logo en fonction de la dimension de l'image de fond pour toujours représenter le même % de l'image
    var scale = 1f;
    if (backImg != null)
        scale = Math.Max(backImg.Width / (float)DefaultRatioBackWidth, backImg.Height / (float)DefaultRatioBackHeight);

    // si l'échelle a été personnalisé, on l'applique toujours en priorité
    if (customScale != 0)
        scale = customScale;

    var baseMargin = DefaultMargin;

    using var logo = CreateMtgLogo(community, family, light, scale);

    if (backImg == null)
        backImg = new Image<Abgr32>(logo.Width, logo.Height);

    var marginx = margin;
    var marginy = margin;

    Point p;

    switch (position)
    {
        case LogoPosition.TopLeft:
            p = new Point(baseMargin, baseMargin);
            break;
        case LogoPosition.CenterLeft:
            p = new Point(baseMargin, (backImg.Height - logo.Height) / 2);
            marginy = 0;
            break;
        case LogoPosition.BottomLeft:
            p = new Point(baseMargin, backImg.Height - logo.Height - baseMargin);
            marginy *= -1;
            break;

        case LogoPosition.Center:
            p = new Point((backImg.Width - logo.Width) / 2, (backImg.Height - logo.Height) / 2);
            marginx = 0;
            marginy = 0;
            break;

        case LogoPosition.TopRight:
            p = new Point(backImg.Width - logo.Width - baseMargin, baseMargin);
            marginx *= -1;
            break;

        case LogoPosition.CenterRight:
            p = new Point(backImg.Width - logo.Width - baseMargin, (backImg.Height - logo.Height) / 2 - baseMargin);
            marginx *= -1;
            marginy = 0;
            break;

        case LogoPosition.BottomRight:
            p = new Point(backImg.Width - logo.Width - baseMargin, backImg.Height - logo.Height - baseMargin);
            marginx *= -1;
            marginy *= -1;
            break;
        default:
            throw new NotSupportedException(position.ToString());
    }

    p.Offset(marginx, marginy);

    if (rect)
    {
        var rectBounds = new RectangleF(p.X, p.Y, logo.Width, logo.Height);
        rectBounds.Inflate(20, 20);

        var rectColorValue = rectColor switch
        {
            "" or null => light ? Color.White : Color.FromRgb(0x2f, 0x2f, 0x2f),
            _ => Color.ParseHex(rectColor.TrimStart('#'))
        };

        var go = new DrawingOptions { GraphicsOptions = new GraphicsOptions { BlendPercentage = rectOpacity, ColorBlendingMode = PixelColorBlendingMode.Darken } };
        backImg.Mutate(x => x.Fill(go, rectColorValue, rectBounds));
    }

    backImg.Mutate(x => x.DrawImage(logo, p, 1));

    return backImg;
}

Image CreateMtgLogo(string community, FontFamily family, bool light, float ratio, bool backRect = true)
{
    var textColor = Color.FromRgb(0xee, 0xee, 0xee);
    if (light)
        textColor = Color.FromRgb(0x73, 0x73, 0x73);

    var fontSize = 140 * ratio;
    var bulletOffset = 40 * ratio;
    var bulletSpacing = 32 * ratio;
    var logoRatio = .1f * ratio;
    var cornerXOffset = 64 * ratio;
    var cornerYOffset = 17 * ratio;

    var font = family.CreateFont(fontSize, FontStyle.Italic);

    var text = $"MTG  {community}";
    var textRect = TextMeasurer.Measure(text, new TextOptions(font));
    var mtgRect = TextMeasurer.Measure($"MTG", new TextOptions(font));

    var width = textRect.Width + cornerXOffset;
    var height = textRect.Height + cornerYOffset;

    var centerX = width / 2;
    var centerY = height / 2;

    var img = new Image<Argb32>((int)width, (int)height);

    var opt = new TextOptions(font);
    opt.VerticalAlignment = VerticalAlignment.Center;
    opt.HorizontalAlignment = HorizontalAlignment.Center;
    opt.Origin = new PointF(centerX, centerY);

    img.Mutate(x => x.DrawText(opt, text, textColor));

    using Image topLeftImg = Image.Load("logo_topleft.png");
    topLeftImg.Mutate(x => x.Resize((int)(topLeftImg.Width * logoRatio), (int)(topLeftImg.Height * logoRatio)));
    img.Mutate(x => x.DrawImage(topLeftImg, new Point(0, 0), 1));

    using Image bulletImg = Image.Load("logo_bullets.png");
    bulletImg.Mutate(x => x.Resize((int)(bulletImg.Width * logoRatio), (int)(bulletImg.Height * logoRatio)));
    img.Mutate(x => x.DrawImage(bulletImg, new Point((int)(mtgRect.Width + bulletOffset + ((bulletSpacing - bulletImg.Width) / 2)), (img.Height - bulletImg.Height) / 2), 1));

    using Image bottomRightImg = Image.Load("logo_bottomRight.png");
    bottomRightImg.Mutate(x => x.Resize((int)(bottomRightImg.Width * logoRatio), (int)(bottomRightImg.Height * logoRatio)));
    var p = new Point((int)(width - bottomRightImg.Width), (int)(height - bottomRightImg.Height));
    img.Mutate(x => x.DrawImage(bottomRightImg, p, 1));

    return img;
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

enum LogoPosition
{
    Center,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    CenterLeft,
    CenterRight,
}
