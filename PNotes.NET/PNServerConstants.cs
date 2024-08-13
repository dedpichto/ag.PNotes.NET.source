
namespace PNCommon
{
    public static class PNServerConstants
    {
        public const string END_OF_RECEVIER = "<EOR>";
        public const char DELIM = '|';
        public const string SUCCESS = "SUCCESS";
        public const string REQUEST_HEADER = "<REQ_HEAD>";
        public const string NET_SERVICE_NAME = "PNExchSvc";
        public const string PROG_SERVICE_NAME = "PNService";
        public const string CHECK = "check";
        public const int DEF_PORT = 27351;
        public const int SEND_TIMEOUT = 5;
        public const int ATTEMPTS_LOCAL_IP = 120;
    }
}
