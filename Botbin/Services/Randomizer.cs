using System;
using System.Collections.Generic;
using System.Linq;
using Botbin.Services.Interfaces;
using Optional;

namespace Botbin.Services {
    public class Randomizer : IRandomizer {
        private readonly Random _rnd;

        public Randomizer(Random rnd) => _rnd = rnd;

        public int Integer() => _rnd.Next();

        public int Integer(int max) => _rnd.Next(max);

        public int Integer(int min, int max) => _rnd.Next(min, max);
    }
    public static class RandomizerExtensions {
        public static Option<T> Collection<T>(this IRandomizer randomizer, IReadOnlyList<T> collection) =>
            collection.Any()
                ? Option.Some(collection[randomizer.Integer(collection.Count)])
                : Option.None<T>();
    }
}