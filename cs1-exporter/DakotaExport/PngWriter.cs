using System;
using System.IO;
using System.IO.Compression;

namespace DakotaExport
{
    /// <summary>
    /// Self-contained greyscale PNG encoder (8 or 16 bit). Tries DeflateStream for compression and
    /// falls back to stored (uncompressed) deflate blocks if the runtime's zlib is unavailable.
    /// </summary>
    internal static class PngWriter
    {
        /// <summary>16-bit grey; samples[row * width + col], row 0 is the top of the image.</summary>
        public static string WriteGray16(string path, int width, int height, ushort[] samples)
        {
            var raw = new byte[height * (1 + width * 2)];
            int p = 0;
            for (int y = 0; y < height; y++)
            {
                raw[p++] = 0;
                for (int x = 0; x < width; x++)
                {
                    ushort v = samples[y * width + x];
                    raw[p++] = (byte)(v >> 8);
                    raw[p++] = (byte)v;
                }
            }

            return Write(path, width, height, 16, raw);
        }

        /// <summary>8-bit grey; samples[row * width + col], row 0 is the top of the image.</summary>
        public static string WriteGray8(string path, int width, int height, byte[] samples)
        {
            var raw = new byte[height * (1 + width)];
            int p = 0;
            for (int y = 0; y < height; y++)
            {
                raw[p++] = 0;
                Buffer.BlockCopy(samples, y * width, raw, p, width);
                p += width;
            }

            return Write(path, width, height, 8, raw);
        }

        /// <returns>"deflate" or "stored", the compression actually used.</returns>
        private static string Write(string path, int width, int height, int bitDepth, byte[] raw)
        {
            string method;
            byte[] zlib = Zlib(raw, out method);
            SafeFile.Write(path, tmp =>
            {
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write))
            {
                fs.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);
                var ihdr = new byte[13];
                PutBE(ihdr, 0, (uint)width);
                PutBE(ihdr, 4, (uint)height);
                ihdr[8] = (byte)bitDepth;
                ihdr[9] = 0; // greyscale
                ihdr[10] = 0;
                ihdr[11] = 0;
                ihdr[12] = 0;
                Chunk(fs, "IHDR", ihdr);
                Chunk(fs, "IDAT", zlib);
                Chunk(fs, "IEND", new byte[0]);
            }
            });

            return method;
        }

        private static byte[] Zlib(byte[] raw, out string method)
        {
            byte[] body = null;
            method = "deflate";
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var ds = new DeflateStream(ms, CompressionMode.Compress, true))
                    {
                        ds.Write(raw, 0, raw.Length);
                    }

                    body = ms.ToArray();
                }

                if (body.Length == 0 || !RoundTrips(body, raw)) body = null;
            }
            catch (Exception)
            {
                body = null;
            }

            if (body == null)
            {
                method = "stored";
                body = Stored(raw);
            }

            var outBuf = new byte[2 + body.Length + 4];
            outBuf[0] = 0x78;
            outBuf[1] = 0x01;
            Buffer.BlockCopy(body, 0, outBuf, 2, body.Length);
            PutBE(outBuf, 2 + body.Length, Adler32(raw));
            return outBuf;
        }

        /// <summary>Inflates the compressed body and compares it with the input, to never ship a bad PNG.</summary>
        private static bool RoundTrips(byte[] body, byte[] raw)
        {
            using (var ms = new MemoryStream(body))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            {
                var buf = new byte[65536];
                int pos = 0, n;
                while ((n = ds.Read(buf, 0, buf.Length)) > 0)
                {
                    if (pos + n > raw.Length) return false;
                    for (int i = 0; i < n; i++)
                        if (buf[i] != raw[pos + i]) return false;
                    pos += n;
                }

                return pos == raw.Length;
            }
        }

        private static byte[] Stored(byte[] raw)
        {
            const int max = 65535;
            int blocks = Math.Max(1, (raw.Length + max - 1) / max);
            var body = new byte[raw.Length + blocks * 5];
            int src = 0, dst = 0;
            for (int b = 0; b < blocks; b++)
            {
                int len = Math.Min(max, raw.Length - src);
                body[dst++] = (byte)(b == blocks - 1 ? 1 : 0);
                body[dst++] = (byte)len;
                body[dst++] = (byte)(len >> 8);
                body[dst++] = (byte)~len;
                body[dst++] = (byte)(~len >> 8);
                Buffer.BlockCopy(raw, src, body, dst, len);
                src += len;
                dst += len;
            }

            return body;
        }

        private static uint Adler32(byte[] data)
        {
            uint a = 1, b = 0;
            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % 65521;
                b = (b + a) % 65521;
            }

            return (b << 16) | a;
        }

        private static readonly uint[] s_crcTable = BuildCrcTable();

        private static uint[] BuildCrcTable()
        {
            var t = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
                t[n] = c;
            }

            return t;
        }

        private static void Chunk(Stream s, string type, byte[] data)
        {
            var len = new byte[4];
            PutBE(len, 0, (uint)data.Length);
            s.Write(len, 0, 4);
            var typeBytes = new byte[] { (byte)type[0], (byte)type[1], (byte)type[2], (byte)type[3] };
            s.Write(typeBytes, 0, 4);
            s.Write(data, 0, data.Length);
            uint crc = 0xFFFFFFFFu;
            foreach (byte b in typeBytes) crc = s_crcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
            foreach (byte b in data) crc = s_crcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
            var crcBytes = new byte[4];
            PutBE(crcBytes, 0, crc ^ 0xFFFFFFFFu);
            s.Write(crcBytes, 0, 4);
        }

        private static void PutBE(byte[] buf, int offset, uint v)
        {
            buf[offset] = (byte)(v >> 24);
            buf[offset + 1] = (byte)(v >> 16);
            buf[offset + 2] = (byte)(v >> 8);
            buf[offset + 3] = (byte)v;
        }
    }
}
