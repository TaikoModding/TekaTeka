using TekaTeka.Plugins;

namespace TekaTeka.Utils
{
    internal class TjaSongEntry : SongEntry
    {
        string modFolder;
        string modPath;

        tja2fumen.TJASong song;

        public TjaSongEntry(string modFolder, int id)
        {
            this.modFolder = modFolder;
            this.modPath = Path.Combine(CustomSongLoader.songsPath, "TJAsongs", this.modFolder);
            string tjaPath = Path.Combine(this.modPath, this.modFolder + ".tja");
            song = tja2fumen.Parsers.ParseTja(tjaPath);

            uint songHash = _3rdParty.MurmurHash2.Hash(File.ReadAllBytes(tjaPath)) & 0xFFFF_FFF;

            this.musicInfo = new MusicDataInterface.MusicInfo { SongNameJP = this.song.metadata.titleJA,
                                                                SongNameEN = this.song.metadata.titleEN,
                                                                SongNameKO = this.song.metadata.titleKO,
                                                                SongNameCN = this.song.metadata.titleCN,
                                                                SongNameTW = this.song.metadata.titleTW,
                                                                SongNameFR = this.song.metadata.title,
                                                                SongNameES = this.song.metadata.title,
                                                                SongNameDE = this.song.metadata.title,
                                                                SongNameIT = this.song.metadata.title,
                                                                SongSubJP = this.song.metadata.subtitleJA,
                                                                SongSubEN = this.song.metadata.subtitleEN,
                                                                SongSubKO = this.song.metadata.subtitleKO,
                                                                SongSubCN = this.song.metadata.subtitleCN,
                                                                SongSubTW = this.song.metadata.subtitleTW,
                                                                SongSubFR = this.song.metadata.subtitle,
                                                                SongSubES = this.song.metadata.subtitle,
                                                                SongSubDE = this.song.metadata.subtitle,
                                                                SongSubIT = this.song.metadata.subtitle,
                                                                SongFileName = $"SONG_{songHash}",
                                                                GenreNo = 0,
                                                                Order = 103170,
                                                                HasPreviewInPackage = true,
                                                                IsDefault = true,
                                                                CanMyBattleSong = false,
                                                                IsCap = true,
                                                                IsOfflinePickUp = true,
                                                                InPackage = 1 };

            foreach (var branch in tja2fumen.Constants.COURSE_IDS.Keys)
            {

                string fumenBranch = branch;
                if (fumenBranch == "Oni")
                {
                    fumenBranch = "Mania";
                }
                var branchProperty = typeof(MusicDataInterface.MusicInfo).GetProperty($"Branch{fumenBranch}");
                var starProperty = typeof(MusicDataInterface.MusicInfo).GetProperty($"Star{fumenBranch}");
                if (branchProperty != null)
                {
                    branchProperty.SetValue(
                        musicInfo, song.courses.ContainsKey(branch) ? song.courses[branch].hasBranches : false);
                }
                if (starProperty != null)
                {
                    starProperty.SetValue(musicInfo, song.courses.ContainsKey(branch) ? song.courses[branch].level : 0);
                }
            }

            musicInfo.UniqueId = id;
            musicInfo.Id = songHash.ToString();
        }

        public override string GetFilePath()
        {
            return Path.Combine(CustomSongLoader.songsPath, "TJAsongs", this.modFolder, this.modFolder + ".tja");
        }

        public override byte[] GetFumenBytes()
        {
            string diff = this.songFile.Split('_')[1];
            string ? tjaDiff;
            tja2fumen.Constants.COURSE_IDS_REVERSE.TryGetValue(diff, out tjaDiff);
            if (tjaDiff == null)
            {
                tjaDiff = "Ura";
            }
            var tjaParsed = tja2fumen.Parsers.ParseTja(this.GetFilePath());
            tja2fumen.FumenCourse? fumenCourse = null;
            if (tjaParsed.courses.ContainsKey(tjaDiff))
            {
                fumenCourse = tja2fumen.Converters.ConvertTjaToFumen(tjaParsed.courses[tjaDiff]);
            }
            else
            {
                // If the course doesn't exists, get the first course from hardest to easiest
                foreach (var course in tja2fumen.Constants.COURSE_IDS.Keys.Reverse())
                {
                    if (tjaParsed.courses.ContainsKey(course))
                    {
                        fumenCourse = tja2fumen.Converters.ConvertTjaToFumen(tjaParsed.courses[course]);
                        break;
                    }
                }
            }

            if (fumenCourse != null)
            {
                tja2fumen.Converters.FixDkNoteTypesCourse(fumenCourse);
                return tja2fumen.Writer.getFumenBytes(fumenCourse);
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        public override byte[] GetSongBytes(bool isPreview = false)
        {
            var directory = Path.GetDirectoryName(this.modPath);

            string acbFile = Path.GetFileNameWithoutExtension(this.modPath) + ".acb";
            int offset;
            if (isPreview)
            {
                acbFile = "P" + acbFile;
                offset = (int)(this.song.metadata.demoStart);
            }
            else
            {
                offset = (int)((60 / this.song.bpm * 1000) * 4);
            }

            var acbPath = Path.Combine(this.modPath, acbFile);
            string songFilePath = Path.Combine(this.modPath, this.song.metadata.wave);
            tja2fumen.TJAConvert.FileType fileType;
            switch (Path.GetExtension(songFilePath))
            {
            case ".wav":
                fileType = tja2fumen.TJAConvert.FileType.WAV;
                break;
            case ".mp3":
                fileType = tja2fumen.TJAConvert.FileType.MP3;
                break;
            case ".ogg":
                fileType = tja2fumen.TJAConvert.FileType.OGG;
                break;
            default:
                fileType = tja2fumen.TJAConvert.FileType.UNK;
                break;
            }

            if (tja2fumen.TJAConvert.ConvertToAcb(songFilePath, fileType, isPreview, offset))
            {
                return File.ReadAllBytes(acbPath);
            }
            return null;
        }
    }
}
