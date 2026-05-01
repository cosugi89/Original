using System;

namespace Assets.Scripts.Features.Battle.Core
{
    /// <summary>
    /// 盤面上の 1 マスを表す座標。
    /// </summary>
    public struct BattleGridPosition : IEquatable<BattleGridPosition>
    {
        public BattleGridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public bool IsAdjacent8Way(BattleGridPosition other)
        {
            var dx = Math.Abs(X - other.X);
            var dy = Math.Abs(Y - other.Y);
            return (dx > 0 || dy > 0) && dx <= 1 && dy <= 1;
        }

        public bool Equals(BattleGridPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleGridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public static bool operator ==(BattleGridPosition left, BattleGridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BattleGridPosition left, BattleGridPosition right)
        {
            return !left.Equals(right);
        }
    }
}
