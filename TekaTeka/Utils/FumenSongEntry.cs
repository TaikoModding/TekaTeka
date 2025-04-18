using System.Text;
using TekaTeka.Plugins;

namespace TekaTeka.Utils
{
    internal class FumenSongEntry : SongEntry
    {
        bool isEncrypted;
        string modFolder;

        public FumenSongEntry(string modFolder, MusicDataInterface.MusicInfo musicInfo)
        {
            this.musicInfo = musicInfo;
            this.modFolder = modFolder;
        }

        public override byte[] GetFumenBytes()
        {

            string songPath =
                Path.Combine(CustomSongLoader.songsPath, this.modFolder, CustomSongLoader.CHARTS_FOLDER, this.songFile);
            if (File.Exists(songPath + ".fumen"))
            {
                return File.ReadAllBytes(songPath + ".fumen");
            }
            else
            {
                return Cryptgraphy.ReadAllAesAndGZipBytes(songPath + ".bin", Cryptgraphy.AesKeyType.Type2);
            }
        }

        public override string GetFilePath()
        {
            string songPath =
                Path.Combine(CustomSongLoader.songsPath, this.modFolder, CustomSongLoader.SONGS_FOLDER, this.songFile);
            return songPath;
        }

        public override string GetSongDivisions()
        {
            string filePath = Path.Combine(CustomSongLoader.songsPath, this.modFolder,
                                           CustomSongLoader.PRACTICE_DIVISIONS_FOLDER, this.songFile);
            if (!File.Exists(filePath + ".bin") && !File.Exists(filePath + ".csv"))
            {
                return "";
            };

            bool isEncrypted = File.Exists(filePath + ".bin") && !File.Exists(filePath + ".csv");

            string csvString;

            if (isEncrypted)
            {
                var bytes = Cryptgraphy.ReadAllAesAndGZipBytes(filePath + ".bin", Cryptgraphy.AesKeyType.Type2);
                csvString = Encoding.UTF8.GetString(bytes);
            }
            else
            {
                csvString = File.ReadAllText(filePath + ".csv");
            }

            return csvString;
        }

        public override byte[] GetSongBytes(bool isPreview = false)
        {
            string songFile;
            string songPath = Path.Combine(CustomSongLoader.songsPath, this.modFolder, CustomSongLoader.SONGS_FOLDER);

            if (isPreview)
            {
                songFile = Path.Combine(songPath, "P" + this.musicInfo.SongFileName);
            }
            else
            {
                songFile = Path.Combine(songPath, this.musicInfo.SongFileName);
            }
            byte[] bytes;
            if (File.Exists(songFile + ".acb"))
            {
                bytes = File.ReadAllBytes(songFile + ".acb");
            }
            else
            {
                bytes = Cryptgraphy.ReadAllAesBytes(songFile + ".bin", Cryptgraphy.AesKeyType.Type0);
            }

            return bytes;
        }
    }
}
