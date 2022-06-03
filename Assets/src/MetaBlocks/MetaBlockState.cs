using src.MetaBlocks.TdObjectBlock;

namespace src.MetaBlocks
{
    public static class MetaBlockState
    {
        public static string ToString(StateMsg msg, string type = null)
        {
            return msg switch
            {
                StateMsg.Loading => LoadingMsg(type),
                StateMsg.LoadingMetadata => LoadingMetadataMsg(),
                StateMsg.InvalidUrlOrData => InvalidObjUrlOrDataMsg(type),
                StateMsg.InvalidData => InvalidObjDataMsg(type),
                StateMsg.OutOfBound => OutOfBoundMsg(type),
                StateMsg.ConnectionError => ConnectionErrorMsg(),
                StateMsg.SizeLimit => SizeLimitMsg(type),
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
    }

    public enum StateMsg
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