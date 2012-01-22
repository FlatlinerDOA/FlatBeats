namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;

    public sealed class BlockingQueue<T>
    {
        private readonly object syncRoot = new object();

        private readonly Queue<T> queue = new Queue<T>();

        public void Enqueue(T item)
        {
            lock (this.syncRoot)
            {
                this.queue.Enqueue(item);
            }
        }

        public void EnqueueRange(IEnumerable<T> items)
        {
            lock (this.syncRoot)
            {
                foreach (var item in items)
                {
                    this.queue.Enqueue(item);
                }
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (this.syncRoot)
            {
                if (this.queue.Count == 0)
                {
                    item = default(T);
                    return false;
                }

                item = this.queue.Dequeue();
                return true;
            }
        }

        public void Clear()
        {
            lock (this.syncRoot)
            {
                this.queue.Clear();
            }   
        }
    }
}