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

var marginOption = new Option<int>(name: "--margin", () => DefaultMargin, description: "Margin in pixel with the border of the image. defaults to 60px");
marginOption.AddAlias("-m");

var positionOption = new Option<LogoPosition>(name: "--position", () => LogoPosition.Center, description: "Logo position");
positionOption.AddAlias("-p");

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
rootCommand.AddOption(lightOption);
rootCommand.AddOption(scaleOption);
rootCommand.AddOption(positionOption);
rootCommand.SetHandler((community, back, @out, light, scale, margin, position) =>
{
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

    WriteLogoImage(community, @out, back, position: position, light ?? false, scale, margin);
}, communityArgument, backOption, outOption, lightOption, scaleOption, marginOption, positionOption);

////////////////////////////////////////////////
//
// Commande pack
//
// Génère toutes les images, 
// 

var packCommand = new Command("pack", "Generate a visual pack for a community");
packCommand.AddArgument(communityArgument);

packCommand.SetHandler((community, margin) =>
{
    var outDir = Path.Combine(Environment.CurrentDirectory, "out");
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
    WriteLogoImage(community, outDir, @"backs/twitter_dark.png", margin: margin, customScale: 0.9f);
    WriteLogoImage(community, outDir, @"backs/twitter_light.png", light: true, margin: margin, customScale: 0.9f);

}, communityArgument, marginOption);

rootCommand.AddCommand(packCommand);

return await rootCommand.InvokeAsync(args);

void WriteLogoImage(string community, string @out, string back, LogoPosition position = LogoPosition.Center, bool light = false, float customScale = 0, int margin = DefaultMargin)
{
    using var backImg = AddLogoToImage(community, back, position, light, customScale);

    @out = GenerateOutFileName(@out, back, community);

    backImg.SaveAsPng(@out);
}

Image AddLogoToImage(string community, string back, LogoPosition position = LogoPosition.Center, bool light = false, float customScale = 0, int margin = DefaultMargin)
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

    var textColor = Color.FromRgb(0xee, 0xee, 0xee);
    if (light)
        textColor = Color.FromRgb(0x73, 0x73, 0x73);

    using var logo = CreateMtgLogo(community, family, textColor, scale);

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
            p = new Point(backImg.Width - logo.Width - (baseMargin), baseMargin);
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

    backImg.Mutate(x => x.DrawImage(logo, p, 1));

    return backImg;
}

Image CreateMtgLogo(string community, FontFamily family, Color textColor, float ratio)
{
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

    var img = new Image<Argb32>((int)(width), (int)(height));

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
