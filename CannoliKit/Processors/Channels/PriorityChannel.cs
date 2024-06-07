using CannoliKit.Enums;
using CannoliKit.Interfaces;
using System.Threading.Channels;

namespace CannoliKit.Processors.Channels
{
    internal sealed class PriorityChannel<T> : ICannoliJobQueueChannel<T>, IDisposable
    {
        private readonly Channel<T> _highPriorityChannel;
        private readonly Channel<T> _normalPriorityChannel;

        internal PriorityChannel()
        {
            _highPriorityChannel = Channel.CreateUnbounded<T>();
            _normalPriorityChannel = Channel.CreateUnbounded<T>();
        }

        public void Write(T item, Priority priority)
        {
            switch (priority)
            {
                case Priority.High:
                    _highPriorityChannel.Writer.TryWrite(item);
                    break;
                case Priority.Normal:
                    _normalPriorityChannel.Writer.TryWrite(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
            }
        }

        public async Task<T> ReadAsync()
        {
            while (true)
            {
                // If there are items available immediately, return the highest priority one.
                if (_highPriorityChannel.Reader.TryRead(out var highPriorityItem)) return highPriorityItem;

                if (_normalPriorityChannel.Reader.TryRead(out var normalPriorityItem)) return normalPriorityItem;

                var allChannelsClosed = _highPriorityChannel.Reader.Completion.IsCompleted &&
                                        _normalPriorityChannel.Reader.Completion.IsCompleted;

                if (allChannelsClosed)
                {
                    throw new InvalidOperationException("All channels are closed.");
                }

                // If no items are available, await any of the channels to have data.
                await Task.WhenAny(
                        _highPriorityChannel.Reader.WaitToReadAsync().AsTask(),
                        _normalPriorityChannel.Reader.WaitToReadAsync().AsTask())
                    .ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _normalPriorityChannel.Writer.Complete();
            _highPriorityChannel.Writer.Complete();
        }
    }
}
