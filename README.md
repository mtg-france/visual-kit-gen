# Community Logo Pack Generator

## Compilation

Nécessite au moins le SDK .NET 7.0.

```
dotnet build -c Release
```




## Usage

```Description:
  Generate a community logo

Usage:
  visual-kit-gen [<Community>] [command] [options]

Arguments:
  <Community>  Name of the community

Options:
  -b, --back <back>                                           The background image to use
  -o, --out <out>                                             The filename to write the image to. defaults to back + community name
  -m, --margin <margin>                                                                   Margin in pixel with the border of the image [default: 50]
  -l, --light                                                 Render the logo for light mode (dark text). defaults to autodetect based on file's name
  -s, --scale <scale>                                         Scale factor for the logo
  -p, --position                                              Logo position [default: Center]
  <BottomLeft|BottomRight|Center|CenterLeft|CenterRight|TopLeft|TopRight>
  -r, --rect                                                  Draws a back rectangle behind the community name
  -ro, --rect-opacity <rect-opacity>                          Opacity of the back rectangle. 1 for fully opaque, 0 for transparent [default: 0,9]
  -rc, --rect-color <rect-color>                              Color of the back rectangle, in #hex format, #ffffff by default for light, #2f2f2f for dark
  --version                                                   Show version information
  -?, -h, --help                                              Show help and usage information

Commands:
  pack <Community>  Generate a visual pack for a community

Usage:
  visual-kit-gen [<Community>] pack [<Community>] [options]

Arguments:
  <Community>  Name of the community

Options:
  -m, --margin <margin>  Margin in pixel with the border of the image [default: 50]
  -o, --out <out>        The filename to write the image to. defaults to back + community name
  -?, -h, --help         Show help and usage information
```
