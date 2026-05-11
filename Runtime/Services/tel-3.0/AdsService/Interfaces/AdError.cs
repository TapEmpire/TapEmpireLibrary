namespace TapEmpire.Experimental
{
    public readonly struct AdError
    {
        public readonly AdNetwork Network;
        public readonly int Code;
        public readonly string Message;

        public AdError(AdNetwork network, int code, string message)
        {
            Network = network;
            Code = code;
            Message = message;
        }
    }
}
