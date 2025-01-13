using AsmResolver;

namespace TekaTeka.Utils
{
    internal abstract class SongEntry
    {
        public string songFile { get; set; }

        public MusicDataInterface.MusicInfo musicInfo { get; set; }

        public abstract byte[] GetFumenBytes();

        public abstract byte[] GetSongBytes(bool isPreview = false);

        public abstract string GetFilePath();
    }
}
