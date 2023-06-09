# Visual Novel Terminal

A customaizable reskin for the terminal that makes it look like a visual novel.

![screenshot of Visual Novel Terminal](docs/screenshot.png)

## Quick Start

1. Clone this repository.
2. Unpack [`dist.zip`](/dist.zip).
3. In the unpacked file, run `vn-terminal.exe`.

## Configuration

Visual Novel Terminal allows user to customize and configure their terminal experience. In the `vn-terminal_Data` directory, create a `config.json` file if it doesn't already exist. Modify the value of the following feels to fit your needs.

| Field 	| Type 	| Description 	| Default 	|
|---	|---	|---	|---	|
| `repeatRate` 	| float 	| The amount of time between each character appearing in seconds. Controls the scroll speed.  	| 0.01 	|
| `primaryColor` 	| RGBA hex string 	| The color of the text backgrounds. 	| #F59AB8CE 	|
| `secondaryColor` 	| RGBA hex string 	| The color of the borders. 	| #FFFFFFFF 	|
| `standardOutputColor` 	| RGBA hex string 	| The color of the text when the terminal sends to the standard output stream. 	| #FFFFFFFF 	|
| `standardErrorColor` 	| RGBA hex string 	| The color of the text when the terminal sends to the standard error stream. 	| #D6251DFF 	|
| `standardOutputCharacterSprites` 	| array of absolute file paths 	| The paths to the character sprites displayed whenever the terminal sends to the standard output stream. 	| [] (default sprites used) 	|
| `standardErrorCharacterSprites` 	| array of absolute file paths 	| The paths to the character sprites displayed whenever the terminal sends to the standard error stream. 	| [] (default sprites used) 	|
| `canvasBackgroundSprite` 	| absolute file path 	| The path to the background image. 	| "" (default sprite used) 	|
| `workingDirectory` 	| absolute file path 	| The directory that the terminal starts in. 	| Windows: %USERPROFILE%<br>OSX: $HOME<br>Linux: $HOME 	|
| `shellFilePath` 	| absolute file path 	| The directory of the shell program. 	| Windows: %COMSPEC%<br>OSX: $SHELL<br>Linux: $SHELL 	|

## Screenshots

![screenshot of Visual Novel terminal with Blahaj and the Trans flag](docs/screenshot_blahaj.png)

![screenshot of Visual Novel terminal with Patrick Bateman from American Psycho in his apartment](docs/screenshot_american_psycho.png)

![screenshot of Visual Novel terminal with the tbh creature](docs/screenshot_yippee.png)


## Credits

Default backgrounds created by [Noraneko Games](https://noranekogames.itch.io/yumebackground).

Default character models created by [Sutemo](https://sutemo.itch.io/female-character).

American Psycho Apartment render created by [Alex Patej](https://www.artstation.com/artwork/w64NkO).
