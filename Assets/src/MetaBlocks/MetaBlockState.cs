using src.MetaBlocks.TdObjectBlock;

namespace src.MetaBlocks
{
    public static class MetaBlockState
    {
        public static string ToString(State msg, string type = null)
        {
            return msg switch
            {
                State.Loading => LoadingMsg(type),
                State.LoadingMetadata => LoadingMetadataMsg(),
                State.InvalidUrlOrData => InvalidObjUrlOrDataMsg(type),
                State.InvalidData => InvalidObjDataMsg(type),
                State.OutOfBound => OutOfBoundMsg(type),
                State.ConnectionError => ConnectionErrorMsg(),
                State.SizeLimit => SizeLimitMsg(type),
                _ => "" // TODO
            };
        }

        private static string LoadingMsg(string type)
        {
            return $"Loading {type} ...";
        }
        
        private static string LoadingMetadataMsg()
        {
            return $"Loading metadata ...";
        }

        private static string InvalidObjUrlOrDataMsg(string type)
        {
            return $"Invalid {type} url/data";
        }

        private static string InvalidObjDataMsg(string type)
        {
            return $"Invalid {type} data";
        }

        private static string OutOfBoundMsg(string type)
        {
            return
                $"{type.Substring(0, 1).ToUpper()}{(type.Length > 1 ? type.Substring(1) : "")} exceeds land boundaries";
        }

        private static string ConnectionErrorMsg()
        {
            return "Connection Error";
        }

        private static string SizeLimitMsg(string type)
        {
            return
                $"{type.Substring(0, 1).ToUpper()}{(type.Length > 1 ? type.Substring(1) : "")} exceeds the size limit of {TdObjectBlockObject.DownloadLimitMb} MB";
        }

        public static bool IsErrorState(State state)
        {
            return state != State.Ok && state != State.Empty && state != State.Loading && state != State.LoadingMetadata;
        }
    }

    public enum State
    {
        Loading,
        LoadingMetadata,
        InvalidUrlOrData,
        InvalidData,
        OutOfBound,
        ConnectionError,
        Ok,
        Empty,
        SizeLimit
    }
}