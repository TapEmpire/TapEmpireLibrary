using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TapEmpire.Utility
{
    public static class QueueExtensions
    {
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items) queue.Enqueue(item);
        }
    }
}