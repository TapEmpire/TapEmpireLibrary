namespace TapEmpire.Experimental
{
    public readonly struct AdImpressionData
    {
        public readonly AdNetwork Mediation;
        public readonly AdFormat Format;
        public readonly string Network;
        public readonly string AdUnitId;
        public readonly string Placement;
        public readonly double Revenue;
        public readonly string Currency;
        public readonly string Precision;

        public AdImpressionData(
            AdNetwork mediation,
            AdFormat format,
            string network,
            string adUnitId,
            string placement,
            double revenue,
            string currency,
            string precision)
        {
            Mediation = mediation;
            Format = format;
            Network = network;
            AdUnitId = adUnitId;
            Placement = placement;
            Revenue = revenue;
            Currency = currency;
            Precision = precision;
        }
    }
}
