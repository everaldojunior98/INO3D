using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Utils
{
    public static class DependencySorter
    {
        public static HashSet<T> Sort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges)
        {
            var l = new HashSet<T>();
            var s = new HashSet<T>(nodes.Where(n => edges.All(e => e.Item2.Equals(n) == false)));

            while (s.Any())
            {
                var n = s.First();
                s.Remove(n);

                l.Add(n);
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;
                    edges.Remove(e);

                    if (edges.All(me => me.Item2.Equals(m) == false))
                        s.Add(m);
                }
            }

            return edges.Any() ? null : new HashSet<T>(l.Reverse());
        }
    }
}