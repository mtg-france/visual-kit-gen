using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.CommandLine;

const int DefaultRatioBackWidth = 2560;
const int DefaultRatioBackHeight = 1440;

var communityArgument = new Argument<string>(name: "Community", description: "Name of the community");

var backOption = new Option<string>(name: "--back", description: "The background image to use");
var outOption = new Option<string>(name: "--out", description: "The filename to write the image to. defaults to back + community name");
var lightOption = new Option<bool>(name: "--light", description: "Render the logo for light mode (dark text)");
var scaleOption = new Option<float>(name: "--scale", description: "Scale factor for the logo");

var rootCommand = new RootCommand("Generate a community logo");
rootCommand.AddArgument(communityArgument);
rootCommand.AddOption(backOption);
rootCommand.AddOption(outOption);
rootCommand.AddOption(lightOption);
rootCommand.AddOption(scaleOption);
rootCommand.SetHandler((community, back, @out, light, scale) =>
{
    WriteLogoImage(community, @out, back, LogoPosition.Center, light, scale);
}, communityArgument, backOption, outOption, lightOption, scaleOption);

var packCommand = new Command("pack", "Generate a visual pack for a community");
packCommand.AddArgument(communityArgument);
packCommand.SetHandler(community =>
{
    var outDir = Path.Combine(Environment.CurrentDirectory, "out");
    if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
    WriteLogoImage(community, outDir, @"backs/screen_01.png");
    WriteLogoImage(community, outDir, @"backs/screen_02.png");
    WriteLogoImage(community, outDir, @"backs/screen_03.png");
    WriteLogoImage(community, outDir, @"backs/screen_04_dark.png", position: LogoPosition.CenterLeft);
    WriteLogoImage(community, outDir, @"backs/screen_04_light.png", position: LogoPosition.CenterLeft, light: true);
    WriteLogoImage(community, outDir, @"backs/screen_05_dark.png");
    WriteLogoImage(community, outDir, @"backs/screen_05_light.png", light: true);
    WriteLogoImage(community, outDir, @"backs/screen_06_dark.png");
    WriteLogoImage(community, outDir, @"backs/screen_06_light.png", light: true);
    WriteLogoImage(community, outDir, @"backs/screen_07_dark.png");
    WriteLogoImage(community, outDir, @"backs/screen_07_light.png", light: true);

}, communityArgument);

rootCommand.AddCommand(packCommand);

return await rootCommand.InvokeAsync(args);

void WriteLogoImage(string community, string @out, string back, LogoPosition position = LogoPosition.Center, bool light = false, float scale = 0)
{
    using var backImg = AddLogoToImage(community, back, position, light, scale);
    @out = GenerateOutFileName(@out, back, community);
    backImg.SaveAsPng(@out);
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


Image AddLogoToImage(string community, string back, LogoPosition position = LogoPosition.Center, bool light = true, float scale = 0)
{
    FontCollection collection = new();
    var family = collection.Add("Lato-Bold.ttf");

    var hasBack = !string.IsNullOrEmpty(back);

    Image? backImg = null;
    if (hasBack)
        backImg = Image.Load(back);

    var ratio = 1f;

    if (backImg != null)
        ratio = Math.Min(backImg.Width / (float)DefaultRatioBackWidth, backImg.Height / (float)DefaultRatioBackHeight);

    if (scale != 0)
        ratio = scale;

    var textColor = Color.FromRgb(0xee, 0xee, 0xee);
    if (light)
        textColor = Color.FromRgb(0x73, 0x73, 0x73);

    using var logo = CreateMtgLogo(community, family, textColor, ratio);

    if (backImg == null)
        backImg = new Image<Abgr32>(logo.Width, logo.Height);

    if (position == LogoPosition.Center)
    {
        backImg.Mutate(x => x.DrawImage(logo, new Point((backImg.Width - logo.Width) / 2, (backImg.Height - logo.Height) / 2), 1));
    }
    else
    {
        Point p;
        var logoScale = .5f;

        if (position == LogoPosition.BottomRight)
        {
            p = new Point((int)(backImg.Width - logo.Width - (30 * ratio)), (int)(backImg.Height - logo.Height - (30 * ratio)));
        }
        else if (position == LogoPosition.TopRight)
        {
            p = new Point((int)(backImg.Width - logo.Width - (30 * ratio)), (int)((30 * ratio)));
        }
        else if (position == LogoPosition.BottomLeft)
        {
            p = new Point((int)((30 * ratio)), (int)(backImg.Height - logo.Height - (30 * ratio)));
        }
        else if (position == LogoPosition.CenterLeft)
        {
            p = new Point((int)((30 * ratio)), (int)((backImg.Height - logo.Height) / 2));
            logoScale = 1f;
        }
        else if (position == LogoPosition.CenterRight)
        {
            p = new Point((int)(backImg.Width - logo.Width - (30 * ratio)), (int)((backImg.Height - logo.Height) / 2 - (30 * ratio)));
            logoScale = 1f;
        }
        else // TopLeft
        {
            p = new Point((int)(30 * ratio), (int)((30 * ratio)));
        }

        if (logoScale != 1f)
        {
            logo.Mutate(x => x.Resize((int)(logo.Width * logoScale), (int)(logo.Height * logoScale)));
        }

        backImg.Mutate(x => x.DrawImage(logo, p, 1));
    }

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
    img.Mutate(x => x.DrawImage(bulletImg, new Point((int)(mtgRect.Width + bulletOffset + ((bulletSpacing - bulletImg.Width) / 2)), (int)((img.Height - bulletImg.Height) / 2)), 1));

    using Image bottomRightImg = Image.Load("logo_bottomRight.png");
    bottomRightImg.Mutate(x => x.Resize((int)(bottomRightImg.Width * logoRatio), (int)(bottomRightImg.Height * logoRatio)));
    var p = new Point((int)(width - bottomRightImg.Width), (int)(height - bottomRightImg.Height));
    img.Mutate(x => x.DrawImage(bottomRightImg, p, 1));

    return img;
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
