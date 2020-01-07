using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XanBotCore.Utility
{

    /// <summary>
    /// Offers an extension to <see cref="IEnumerable{T}"/> that sharply enhances the speed of the Skip method.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// A more robust version of <seealso cref="Enumerable.Skip{TSource}(IEnumerable{TSource}, int)"/> that is incredibly optimized for <seealso cref="List{T}"/> types (+500% enumeration speed)
        /// </summary>
        /// <typeparam name="T">The type stored in the IEnumerable</typeparam>
        /// <param name="source">The source IEnumerable.</param>
        /// <param name="count">The amount of objects to skip.</param>
        /// <returns></returns>
        public static IEnumerable<T> FastSkip<T>(this IEnumerable<T> source, int count)
        {
            using (IEnumerator<T> e = source.GetEnumerator())
            {
                if (source is IList<T>)
                {
                    // List optimization: Typed list
                    IList<T> list = (IList<T>)source;
                    for (int i = count; i < list.Count; i++)
                    {
                        e.MoveNext();
                        yield return list[i];
                    }
                }
                else if (source is IList)
                {
                    // List optimization: Generic list
                    IList list = (IList)source;
                    for (int i = count; i < list.Count; i++)
                    {
                        e.MoveNext();
                        yield return (T)list[i];
                    }
                }

                // .NET stock fallback code. Targets anything that isn't a list.
                else
                {
                    while (count > 0 && e.MoveNext())
                        count--;
                    if (count <= 0)
                    {
                        while (e.MoveNext())
                            yield return e.Current;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the contents of this <see cref="IEnumerable{T}"/> are identical to the contents of <paramref name="other"/> via using the Contains method. Consider using SequenceEquals instead if you do not need to test via Contains and can instead test via the objects' default equality methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool ContentEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source.Count() != other.Count())
                return false;

            using (IEnumerator<T> srcEnumerator = source.GetEnumerator())
            {
                for (int i = 0; i < source.Count(); i++)
                {
                    srcEnumerator.MoveNext();
                    if (!other.Contains(srcEnumerator.Current))
                        return false;
                }
            }

            return true;
        }
    }
}
