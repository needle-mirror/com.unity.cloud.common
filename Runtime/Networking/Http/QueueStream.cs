using System;
using System.IO;
using System.Threading;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// The QueueStream class represent a queue of data, where one producer writes to the "Writer" Stream and
    /// one consumer reads using the "Reader" Stream. Concurrent read and write are supported.
    ///
    /// The api of QueueStream is kept simply by design. Aside from construction, it simply gives access to a "Reader" Stream
    /// and a "Writer" Stream. Both "Reader" and "Writer" Stream object needs to be Disposed.
    /// </summary>
    class QueueStream
    {
        readonly ReaderStream m_Reader;
        readonly WriterStream m_Writer;

        AutoResetEvent m_AutoResetEvent = new (false);
        readonly object m_Lock = new ();

        MemoryStream m_InnerStream; // Needs Lock() before accessing
        long m_WriterPosition; // Needs Lock() before accessing. Needs to be accessed with 'Volatile'

        volatile bool m_ReaderDisposed; // Called by multiple threads. Needs to be volatile
        volatile bool m_WriterDisposed; // Called by multiple threads. Needs to be volatile
        volatile bool m_Disposed;

        public QueueStream()
        {
            m_InnerStream = new MemoryStream();

            m_Reader = new ReaderStream(this);
            m_Writer = new WriterStream(this);
        }

        public Stream Reader => m_Reader;
        public Stream Writer => m_Writer;

        long ReaderPosition // Needs Lock() before accessing
        {
            get;
            set;
        }

        long WriterPosition // Needs Lock() before accessing
        {
            get => Volatile.Read(ref m_WriterPosition);
            set => Volatile.Write(ref m_WriterPosition, value);
        }

        bool WriterDisposed
        {
            get => m_WriterDisposed;
        }

        Stream Stream // Needs Lock() before calling anything on the returned object
        {
            get => m_InnerStream;
        }

        void Lock()
        {
            Monitor.Enter(m_Lock);
        }

        void Unlock()
        {
            Monitor.Exit(m_Lock);
        }

        void WaitForIncomingData()
        {
            if (m_AutoResetEvent != null)
            {
                m_AutoResetEvent.WaitOne();
            }
        }

        void NotifyIncomingData()
        {
            if (m_AutoResetEvent != null)
            {
                m_AutoResetEvent.Set();
            }
        }

        void PrepareForRead() // Needs Lock() before accessing this method
        {
            if (m_InnerStream != null)
            {
                m_InnerStream.Position = ReaderPosition;
            }
        }

        void FinalizeRead() // Needs Lock() before accessing this method
        {
            if (m_InnerStream != null)
            {
                ReaderPosition = m_InnerStream.Position;
            }
        }

        void PrepareForWrite() // Needs Lock() before accessing this method
        {
            if (m_InnerStream != null)
            {
                m_InnerStream.Position = WriterPosition;
            }
        }

        void FinalizeWrite() // Needs Lock() before accessing this method
        {
            if (m_InnerStream != null)
            {
                WriterPosition = m_InnerStream.Position;
            }
        }

        void DisposeReader()
        {
            m_ReaderDisposed = true;
            TryDispose();
        }

        void DisposeWriter()
        {
            m_WriterDisposed = true;
            TryDispose();
        }

        void TryDispose()
        {
            if (m_ReaderDisposed && m_WriterDisposed)
            {
                Lock();

                if (!m_Disposed)
                {
                    m_InnerStream.Dispose();
                    m_InnerStream = null;

                    m_AutoResetEvent.Set();
                    m_AutoResetEvent.Dispose();
                    m_AutoResetEvent = null;

                    m_Disposed = true;
                }

                Unlock();
            }
        }

        class WriterStream: Stream
        {
            readonly QueueStream m_Stream;

            public WriterStream(QueueStream stream)
            {
                m_Stream = stream;
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException(); // since CanSeek = false

            public override long Position
            {
                get => throw new NotSupportedException(); // since CanSeek = false
                set => throw new NotSupportedException(); // since CanSeek = false
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                try
                {
                    m_Stream.Lock();
                    m_Stream.PrepareForWrite();
                    m_Stream.Stream.Write(buffer, offset, count);
                    m_Stream.FinalizeWrite();
                }
                finally
                {
                    m_Stream.Unlock();
                    m_Stream.NotifyIncomingData();
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    m_Stream.DisposeWriter();
                    m_Stream.NotifyIncomingData();
                }

                base.Dispose(disposing);
            }

            public override void Flush() { /* do nothing */ }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException(); // since CanRead = false
            public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException(); // since CanSeek = false
            public override void SetLength(long value) => throw new NotImplementedException(); // since CanSeek = false
        }

        class ReaderStream: Stream
        {
            readonly QueueStream m_Stream;

            internal ReaderStream(QueueStream stream)
            {
                m_Stream = stream;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException(); // since CanSeek = false

            public override long Position
            {
                get => throw new NotSupportedException(); // since CanSeek = false
                set => throw new NotSupportedException(); // since CanSeek = false
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                while (true)
                {
                    m_Stream.Lock();
                    if (m_Stream.ReaderPosition >= m_Stream.WriterPosition &&
                        !m_Stream.WriterDisposed)
                    {
                        m_Stream.Unlock();
                        m_Stream.WaitForIncomingData();
                    }
                    else
                        break;
                }

                try
                {
                    m_Stream.PrepareForRead();
                    int bytesRead = m_Stream.Stream.Read(buffer, offset, count);
                    m_Stream.FinalizeRead();

                    return bytesRead;
                }
                finally
                {
                    m_Stream.Unlock();
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    m_Stream.DisposeReader();
                }

                base.Dispose(disposing);
            }

            public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException(); // since CanWrite = false
            public override void Flush() { /* do nothing */ }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException(); // since CanSeek = false
            public override void SetLength(long value) => throw new NotImplementedException(); // since CanSeek = false
        }
    }
}
