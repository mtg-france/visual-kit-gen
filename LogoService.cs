using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace visual_kit_gen
{
    public class LogoService
    {
        const int DefaultRatioBackWidth = 2560;
        const int DefaultRatioBackHeight = 1440;
        const int DefaultMargin = 50;
        const float DefaultBlend = .9f;
        const int RectInflate = 20;


        public Image AddLogoToImage(
         string community,
         string back,
         LogoPosition position = LogoPosition.Center,
         bool light = false,
         float customScale = 0,
         int margin = DefaultMargin,
         bool rect = false,
         float rectOpacity = DefaultBlend,
         string? rectColor = null)
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
                rectBounds.Inflate(RectInflate, RectInflate);

                var rectColorValue = rectColor switch
                {
                    "" or null => light ? Color.White : Color.FromRgb(0x2f, 0x2f, 0x2f),
                    _ => Color.ParseHex(rectColor.TrimStart('#'))
                };

                var go = new DrawingOptions { GraphicsOptions = new GraphicsOptions { BlendPercentage = rectOpacity, ColorBlendingMode = PixelColorBlendingMode.Normal } };
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


    }

    public enum LogoPosition
    {
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        CenterLeft,
        CenterRight,
    }
}
