using AsmResolver;

namespace TekaTeka.Utils
{
    internal abstract class SongEntry
    {
        public string SongFile { get; set; } = "";

        public MusicDataInterface.MusicInfo musicInfo { get; set; } = new MusicDataInterface.MusicInfo();

        public abstract byte[] GetFumenBytes();

        public abstract byte[]? GetSongBytes(bool isPreview = false);

        public abstract string GetFilePath();
    }
}
