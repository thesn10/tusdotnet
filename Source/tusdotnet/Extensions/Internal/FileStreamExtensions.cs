using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace tusdotnet.Extensions
{
    internal static class FileStreamExtensions
    {
#if netfull

		public static byte[] CalculateSha1(this FileStream fileStream, long chunkStartPosition)
		{
			byte[] fileHash;
			using (var sha1 = new SHA1Managed())
			{
			    var originalPos = fileStream.Position;
			    fileStream.Seek(chunkStartPosition, SeekOrigin.Begin);
				fileHash = sha1.ComputeHash(fileStream);
			    fileStream.Seek(originalPos, SeekOrigin.Begin);
			}

			return fileHash;
		}

#endif

#if netstandard

        public static byte[] CalculateSha1(this FileStream fileStream, long chunkStartPosition)
        {
            byte[] fileHash;
            using (var sha1 = SHA1.Create())
            {
                var originalPos = fileStream.Position;
                fileStream.Seek(chunkStartPosition, SeekOrigin.Begin);
                fileHash = sha1.ComputeHash(fileStream);
                fileStream.Seek(originalPos, SeekOrigin.Begin);
            }

            return fileHash;
        }

#endif

#if pipelines

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteToDisk(this FileStream stream, ReadOnlySequence<byte> buffer, long offset, bool flush = false)
        {
#if net6fileapi
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                List<ReadOnlyMemory<byte>> segmentList = new List<ReadOnlyMemory<byte>>();
                foreach (var segment in buffer)
                {
                    segmentList.Add(segment);
                }

                // scatter/gather write using pwritev()
                await RandomAccess.WriteAsync(stream.SafeFileHandle, segmentList, offset);
                return;
            }
#endif

            foreach (var segment in buffer)
            {
                await stream.WriteAsync(segment);
            }

            if (flush) await stream.FlushAsync();
        }

#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task FlushFileToDisk(this FileStream fileStream, byte[] fileWriteBuffer, int writeBufferNextFreeIndex)
        {
            await fileStream.WriteAsync(fileWriteBuffer, 0, writeBufferNextFreeIndex);
            await fileStream.FlushAsync();
        }

    }
}
