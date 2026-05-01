namespace Assets.Scripts.Features.Battle.Core
{
    /// <summary>
    /// パス上の 2 点を結ぶ線分。
    /// </summary>
    public struct BattlePathSegment
    {
        public BattlePathSegment(BattleGridPosition from, BattleGridPosition to)
        {
            From = from;
            To = to;
        }

        public BattleGridPosition From { get; }

        public BattleGridPosition To { get; }

        public bool HasEndpoint(BattleGridPosition position)
        {
            return From == position || To == position;
        }

        public bool SharesEndpointWith(BattlePathSegment other)
        {
            return HasEndpoint(other.From) || HasEndpoint(other.To);
        }

        public override string ToString()
        {
            return $"{From}->{To}";
        }
    }
}
