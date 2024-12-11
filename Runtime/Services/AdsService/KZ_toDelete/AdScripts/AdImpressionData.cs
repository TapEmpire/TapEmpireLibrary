public struct AdImpressionData
{
    public string AdUnit;
    public AdFormat Format;

    public AdImpressionData(string adUnit, AdFormat format)
    {
        AdUnit = adUnit;
        Format = format;
    }
}