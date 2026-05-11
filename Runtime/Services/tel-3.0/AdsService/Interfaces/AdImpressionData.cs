namespace TapEmpire.Experimental
{
    public readonly struct AdImpressionData
    {
        public readonly AdNetwork Network;
        public readonly string Mediation;
        public readonly string AdUnitId;
        public readonly string Placement;
        public readonly double Revenue;
        public readonly string Currency;
        public readonly string Precision;

        public AdImpressionData(
            AdNetwork network,
            string mediation,
            string adUnitId,
            string placement,
            double revenue,
            string currency,
            string precision)
        {
            Network = network;
            Mediation = mediation;
            AdUnitId = adUnitId;
            Placement = placement;
            Revenue = revenue;
            Currency = currency;
            Precision = precision;
        }
    }
}
