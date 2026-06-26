using System;
using UnityEngine;

namespace Apoplexy.AI
{
    public readonly struct NoiseEvent
    {
        public NoiseEvent(Vector3 position, float radius, GameObject source)
        {
            Position = position;
            Radius = radius;
            Source = source;
        }

        public Vector3 Position { get; }
        public float Radius { get; }
        public GameObject Source { get; }
    }

    public static class NoiseSystem
    {
        public static event Action<NoiseEvent> NoiseEmitted;

        public static void Emit(Vector3 position, float radius, GameObject source)
        {
            if (radius <= 0f)
            {
                return;
            }

            NoiseEmitted?.Invoke(new NoiseEvent(position, radius, source));
        }
    }
}
