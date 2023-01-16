# Community Logo Pack Generator

Cet outil vous permet de g�n�rer un kit visuel complet pour votre communaut�, personnalis� avec son nom.
Vous pouvez utiliser directement les fichiers g�n�r�s, ou bien les personnaliser.

* Le dossier `backs` contient tous les fichiers de r�f�rence utilis�s en fond
* Le dossier `france` contient un exemple de rendu de la commande `dotnet run -- France pack`

N�cessite le SDK .NET 7.0 sur votre poste.

Utilisez la commande `dotnet run -- <Community> pack` pour g�n�rer tous les logos pour votre communaut� dans un dossier `out`.

Si vous souhaitez personnaliser le rendu d'un visuel, utilisez la commande `dotnet run -- <Community> -b <back>` en rempla�ant back par le nom d'un fichier
de fond pr�sent dans le dossier `backs`.

Vous pouvez ensuite utiliser l'une des options d�crites ci-dessous pour personnaliser le rendu de l'image.

## Options

```
  -b, --back <back>                                           The background image to use
  -o, --out <out>                                             The filename to write the image to. defaults to back + community name
  -m, --margin <margin>                                       Margin in pixel with the border of the image [default: 50]
  -l, --light                                                 Render the logo for light mode (dark text). defaults to autodetect based on file's name.
  -s, --scale <scale>                                         Scale factor for the logo.
  -p, --position                                              Logo position [default: Center]
  <BottomLeft|BottomRight|Center|CenterLeft|CenterRight|TopLeft|TopRight>
  -r, --rect                                                  Draws a semi-transparent back rectangle behind the community name
  -ro, --rect-opacity <rect-opacity>                          Opacity of the back rectangle. 1 for fully opaque, 0 for transparent [default: 0,9]
  -rc, --rect-color <rect-color>                              Color of the back rectangle, in #hex format, #ffffff by default for light, #2f2f2f for dark
```

Avec la commande `pack` seules les options `--margin` et `--out` sont support�es.

Lorsque vous ex�cutez la commande pack, chaque image du dossier back est utilis�e pour g�n�rer une image avec votre communaut�, avec des r�glages parfois personnalis� selon l'image.
Ces r�glages peuvent �tre r�cup�r�s � partir du fichier [Program.cs](https://github.com/mtg-france/visual-kit-gen/blob/main/Program.cs#L113).