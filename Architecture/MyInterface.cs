using System;
using System.Drawing;

namespace MyGame
{
    public interface IFlyingObject
    {
        string GetFileName { get; }
        Point Location { get; }
        Tuple<int, int> Size { get; }
        void Act(Game game);
        void Conflict(Game game);
    }
}