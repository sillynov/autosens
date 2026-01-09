
# Autosens

Autosens is a tool which converts a chosen cm/360 into a given games' sensitivity and applies it through that game's config file. It also displays the current cm/360 for supported games, and allows users to add their own games as they wish.

## Supported games
Autosens currently supports:
- Battlefield V
- Battlefield 6
- Counter-Strike 2
- Deadlock
- The Finals
- Valorant (partial support)
- Overwatch 2 (partial support)
- ARC Raiders
- Apex Legends
- Team Fortress 2
- Battlefield 4
- Black Ops 7
- Fortnite (partial support)
- Gunfire Reborn
- Modern Warfare III (2023)

## User settings
Upon startup, autosens will ask for your DPI (mandatory), your SteamID (found at `C:\Program Files (x86)\Steam\userdata`, not mandatory unless a game such as CS2 is storing the config there), and your default cm/360 (not mandatory). 

## Adding games
Your own games/programs can be added through the games.json file. Doing so requires the following strings:

A name (`name`), which is purely cosmetic.

A sensitivity formula (`conversionCalc`). Please enter this as though it is at 1600DPI, your individual DPI can be changed in your user settings. It should be a mathematical operation, substituting `[cm]` for the cm/360 value. For example, CS2 uses `25.977 / [cm]`.

A formula to calculate the cm/360 from the existing sensitivity (`reverseCalc`). This is not mandatory, and should also be a mathematical operation, substituting `[sens]` for the existing sensitivity. CS2 is therefore `25.977 / [sens]`.

The file path template (`configPathTemplate`) of the games' config file. There are five possible substitutions (`[LOCALAPPDATA]`,`[APPDATA]`,`[DOCUMENTS]`,`[STEAMID]`,`[UNKNOWN]`) in order to make this work across systems however, you can simply input your individual file path if you prefer. `[UNKNOWN]` is used for user specific folders, such as Valorant, which stores the sensitivity in a subdirectory that is named after a unique Riot indentifier. To give two examples, Vaolrant is located at `[LOCALAPPDATA]\\VALORANT\\Saved\\Config\\[UNKNOWN]\\Windows\\RiotUserSettings.ini`, while CS2 is at `C:\\Program Files (x86)\\Steam\\userdata\\[STEAMID]\\730\\local\\cfg\\cs2_user_convars_0_slot0.vcfg`

The name of the variable within the config file (`replacementText`). This must be unique within that file, as duplicates may result in the wrong variable being changed. Do not include the actual variable, just the text that precedes it. If a game has multiple sensitivities, you can split them with `[AND]`, such as the case of ARC Raiders: `replacementText = "SensitivityXAxis[AND]SensitivityYAxis"`.

The error message to show when a file isn't found (`notFoundText`). Has `[SENS]` which will have the calculated sensitivity substituted, and `[PATH]` to display the original path.

Whether or not to allow a path update by the user (`allowUpdate`). This is disabled for partially supported games so as to not cause confusion.


## Contributing

Contributions are always welcome!

If you have a new game that you think should be added to the base program, please feel free to open a pull request or issue!


## Acknowledgements

Thanks to the Kovaaks team for allowing the use of their formulae, this would've been impossible without them <3

Thanks to [@hytioo](https://www.github.com/hytioo) for helping with Steam Library detection and a few other things.


## License

[MIT](https://choosealicense.com/licenses/mit/)


## Authors

- [@sillynov](https://www.github.com/sillynov)

