using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TekaTeka.Plugins;
using tja2fumen;

namespace TekaTeka.Utils
{
    internal class TjaSongMod : SongMod
    {
        TjaSongEntry song;
        string modPath;
        static int id = 5900;

        public TjaSongMod(string folder)
        {
            this.modFolder = folder;
            this.enabled = true;
            this.name = folder;
            this.song = new TjaSongEntry(this.modFolder);
            this.modPath = Path.Combine(CustomSongLoader.songsPath, "TJAsongs", this.modFolder, this.modFolder);
        }

        public override bool IsValidMod()
        {

            return File.Exists(this.modPath + ".tja") &&
                   (File.Exists(this.modPath + ".wav") || File.Exists(this.modPath + ".ogg"));
        }

        public override void AddMod(ModdedSongsManager manager)
        {
            var musicInfo = this.song.musicInfo;

            var tjaSong = tja2fumen.Parsers.ParseTja(this.modPath + ".tja");
            uint songHash = _3rdParty.MurmurHash2.Hash(File.ReadAllBytes(this.modPath + ".tja")) & 0xFFFF_FFF;
            manager.currentSongs.Add(id);
            manager.songFileToMod.Add($"SONG_{songHash}", this);
            manager.uniqueIdToMod.Add(id, this);
            id++;
            manager.idToMod.Add(songHash.ToString(), this);
            manager.musicData.AddMusicInfo(ref musicInfo);

            manager.initialPossessionData.InitialPossessionInfoAccessers.Add(
                new InitialPossessionDataInterface.InitialPossessionInfoAccessor(
                    (int)InitialPossessionDataInterface.RewardTypes.Song, musicInfo.UniqueId));
        }

        public override SongEntry GetSongEntry(string id, bool idIsSongFile = false)
        {
            song.songFile = id;
            return this.song;
        }

        public override string GetModFolder()
        {
            return this.modFolder;
        }
    }
}
