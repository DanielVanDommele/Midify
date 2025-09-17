using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Midify.logic
{
    public class MidiTrackStream : Stream
    {
        private readonly byte[] trackBytes = [];
        private long streamPointer = 0;
        private readonly long startPointer = 0;
        private readonly long endPointer = 0;

        private MidiTrackStream(byte[] buffer)
        {
            trackBytes = buffer;
            startPointer = 0;
            endPointer = buffer.Length;
            streamPointer = startPointer;
        }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanWrite { get {  return false; } }

        public override long Length { get { return endPointer; } }

        public override long Position { get { return streamPointer; } set { streamPointer = value; } }

        public static MidiTrackStream FromFileStream(FileStream fs, int trackLengthinBytes)
        {
            return new MidiTrackStream(BinaryHelper.ReadNext(fs, trackLengthinBytes, true));
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public int Read(byte[] buffer, int count)
        {
            if (streamPointer + count > endPointer)
                count = (int)(endPointer - streamPointer);

            Array.Copy(trackBytes, streamPointer, buffer, 0, count);
            streamPointer += count;
            return count;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (streamPointer + count > endPointer)
                count = (int)(endPointer - streamPointer);
            Array.Copy(trackBytes, streamPointer, buffer, offset, count);
            streamPointer += count;
            return count;
        }

        /// <summary>
        /// Reads the next byte and advances the stream pointer.
        /// </summary>
        /// <returns>value of the byte (int)</returns>
        public override int ReadByte()
        {
            if (streamPointer >= endPointer)
                return -1;
            return trackBytes[streamPointer++];
        }

        /// <summary>
        /// Reads the next byte without advancing the stream pointer.
        /// </summary>
        /// <returns>value of the byte (int)</returns>
        public int ScanByte()
        {
            if (streamPointer >= endPointer)
                return -1;
            return trackBytes[streamPointer];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
