using System.Collections.Generic;

namespace Harpy
{
    // Stolen off StackOverflow
    // https://stackoverflow.com/questions/748387/how-to-remove-a-stack-item-which-is-not-on-the-top-of-the-stack-in-c-sharp
    public class PseudoStack<T> : List<T>
    {
        public void Push(T item)
        {
            Add(item);
        }

        public T Pop()
        {
            if (Count > 0)
            {
                T temp = this[Count - 1];
                RemoveAt(Count - 1);
                return temp;
            }
            else
                return default(T);
        }

        public T Peek()
        {
            return this[Count - 1];
        }

        public void Remove(int itemAtPosition)
        {
            RemoveAt(itemAtPosition);
        }
    }
}
