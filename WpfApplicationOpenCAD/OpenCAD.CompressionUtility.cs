using System.IO;
using System.IO.Compression;

namespace OpenCAD
{
    static class CompressionUtility
    {
        public static MemoryStream CompressStream(Stream input, CompressionMode mode)
        {
            using (MemoryStream outstream = new MemoryStream())
            using (DeflateStream compstream = new DeflateStream(outstream, mode))
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(compstream);

                return outstream;
            }
        }

        public static byte[] CompressBuffer(byte[] input, CompressionMode mode)
        {
            using (MemoryStream instream = new MemoryStream(input))
            using (MemoryStream outstream = CompressStream(instream, CompressionMode.Compress))
                return outstream.ToArray();
        }
    }
}
