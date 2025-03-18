# Creating Mods

> [!WARNING]
> Please, keep in mind that every specification here is not definitive and is subject to change.

Each mod or Song Pack it's saved on the folder `TekaSongs`, each one in a separate folder.

## Folder structure

All the files and folders inside the mod will have the same structure as the `StreamingAssets` folder of the original game, that is:

```
TekaSongs/
└── my-awesome-mod/
    ├── fumen/
    │   └── chart files
    ├── fumencsv/
    │   └── practice files
    ├── ReadAssets/
    │   └── song database
    └── sound/
        └── sound files
```

## Music files

Every file can be either Encrypted or Decrypted, and TekaTeka will assume the later by using the file extension

| Folder/File type                  | Description                                                                                                                                            | Example                                                        | Encrypted Extension | Decrypted Extension |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------- | ------------------- | ------------------- |
| `fumen/{Chart file}`              | File containing the chart of the song, one for each difficulty                                                                                         | `fumen/clscam_e.bin` `fumen/clscam_h.bin` `fumen/clscam_m.bin` | `*.bin`             | `*.fumen`           |
| `fumencsv/{Practice Division}`    | CSV File containing the divisions used for practice mode                                                                                               | `fumencsv/clscam.bin`                                          | `*.bin`             | `*.csv`             |
| `sound/SONG_{Full song file}`     | This file contains the song file on the Criware proprietary format [ACB](https://game.criware.jp/manual/native/adx2_en/latest/criatom_basics_acb.html) | `sound/SONG_clscam.bin`                                        | `*.bin`             | `*.acb`             |
| `sound/PSONG_{Preview song file}` | File on the same proprietary format, but used for preview on the select song scree                                                                     | `sound/PSONG_clscam.bin`                                       | `*.bin`             | `*.acb`             |
| `ReadAssets/musicinfo.bin`        | Json file containing all the information about the songs like titles, file names, difficulties, etc.                                                   | `ReadAssets/musicinfo.bin`                                     | `musicinfo.bin`     | `musicinfo.json`    |

## Songs Database (`musicinfo.bin`)

Every time you add a song to your mod, you will need to add the corresponding metadata and file names to your `ReadAssets/musicinfo.bin`/`ReadAssets/musicinfo.json`. \
Since you can use both types, it's best to use unencrypted while making mods, and later encrypt the file if wanted. \
The `musicinfo.json` its on the JSON format, and has the following format:

```json
{
  "items": [
    {
      "UniqueId": 2,
      "Id": "himyak",
      "SongFileName": "SONG_HIMYAK",
      "Session": "",
      "Order": 101690,
      "GenreNo": 0,
      "InPackage": 2,
      "HasPreviewInPackage": false,
      "IsDefault": false,
      "CanMyBattleSong": true,
      "IsCap": false,
      "IsOfflinePickUp": false,
      "BranchEasy": false,
      "BranchNormal": false,
      "BranchHard": false,
      "BranchMania": false,
      "BranchUra": false,
      "StarEasy": 3,
      "StarNormal": 4,
      "StarHard": 5,
      "StarMania": 7,
      "StarUra": 0,
      "ShinutiEasy": 10240,
      "ShinutiNormal": 6960,
      "ShinutiHard": 5420,
      "ShinutiMania": 4210,
      "ShinutiUra": 1000,
      "ShinutiEasyDuet": 10240,
      "ShinutiNormalDuet": 6960,
      "ShinutiHardDuet": 5420,
      "ShinutiManiaDuet": 4210,
      "ShinutiUraDuet": 1000,
      "Debug": false,
      "PlayableRegionList": "1,2,3",
      "SubscriptionRegionList": "1,2,3",
      "DlcRegionList": "1,2,3",
      "IsPS5ShareScreen": false,
      "Reserve2": "",
      "Reserve3": "",
      "IsDispJpSongName": true,
      "DancerSet": "",
      "SongNameJP": "<font=jp>ひまわりの約束",
      "SongNameEN": "<font=efigs>Himawari no Yakusoku",
      "SongNameFR": "<font=efigs>Himawari no Yakusoku",
      "SongNameIT": "<font=efigs>Himawari no Yakusoku",
      "SongNameDE": "<font=efigs>Himawari no Yakusoku",
      "SongNameES": "<font=efigs>Himawari no Yakusoku",
      "SongNameTW": "<font=tw>向日葵的約定",
      "SongNameCN": "<font=cn>向日葵的约定",
      "SongNameKO": "<font=ko>해바라기의 약속",
      "SongSubJP": "<font=jp>",
      "SongSubEN": "<font=efigs>",
      "SongSubFR": "<font=efigs>",
      "SongSubIT": "<font=efigs>",
      "SongSubDE": "<font=efigs>",
      "SongSubES": "<font=efigs>",
      "SongSubTW": "<font=tw>",
      "SongSubCN": "<font=cn>",
      "SongSubKO": "<font=ko>"
    }
    // .....
  ]
}
```

All the songs are contained on the JSON list `items`.\
Each Attribute is self-explanatory, but the most important are:

- `UniqueId`: This is the unique numeric id of your song, this is mostly used for saves, make sure you don't reuse the same id or use an id from the original game
- `Id`: This is the text id of your song, this will be used when searching for the fumen and fumencsv files(eg. `fumen/himyak_e.bin`)
- `Order`: This is the order for the song library, where you can arrange the ordering for convenient organization.
- `GenreNo`: Genre ID, where you can set any various custom charts into designated genres (0 = POP, 1 = Anime, 2 = VOCALOID, 3 = Variety, 4 = Children(Unused), 5 = Classical, 6 = Game Music, 7 = Namco Orignal)
- `HasPreviewInPackage`: A boolean whether to play `P(SONGNAME.bin)` in your custom chart
- `IsDefault`: Option to remove "DLC" tag in the rectangle and designate as a default chart
- `SongFileName`: This is name of the song file you are gonna use, it's also used for the preview song file if `HasPreviewInPackage` is false, adding a `P` at the start of the name(eg. `SONG_HIMYAK` -> `PSONG_HIMYAK.bin`)
- `IsDispJpSongName`: If your chart/song's name is written in English (eg. `Only my railgun` (written in English, and boolean is `false`), if `true`, it shows normally as `さいたま2000`)
- `IsPS5ShareScreen`: Option to include audio in when recording gameplay in PS5 port of Rhythm Festival. (_due to DRM and licensing issues, PS5 removes audio when recording gameplay for copyright protection_)

Difficulty Settings
Legend (serving diffName as suffix):

Difficulty (Maximum of star rating)
- `Easy`: Easy (max. 5)
- `Normal`: Normal (max. 7)
- `Hard`: Hard (max. 8)
- `Mania`: Extreme (max. 10)
- `Ura` = Ura (same as `Mania`)

- `Star(diffName)`: How many stars will you measure for each difficulty (maximum of 10)
- `Shinuti(diffName)`: Shinuchi score value
- `Shinuti(diffName)Duet`: Shinuchi score value when playing with 2 players

## Encrypted/Decrypted Priority

TekaTeka will have this priority when searching songs

```
1- Original file from the StreamingAssets folder
2- Decrypted Modded file
3- Encrypted Modded file
```

That means, if you add any file with the same name as the original game, it will be skipped, or if you add both Decrypted and Encrypted files, the Decrypted ones are gonna be loaded
