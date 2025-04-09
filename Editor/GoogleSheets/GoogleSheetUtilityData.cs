namespace TapEmpire.Utility.GoogleSheet
{
    [System.Serializable]
    public class ServiceAccountData
    {
        public string type;
        public string project_id;
        public string private_key_id;
        public string private_key;
        public string client_email;
        public string client_id;
    }

    [System.Serializable]
    public class TokenResponse
    {
        public string access_token;
        public int expires_in;
        public string token_type;
    }
}