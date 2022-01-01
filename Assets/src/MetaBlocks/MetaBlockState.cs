using System;
using System.Collections.Generic;
using System.IO;

namespace src.MetaBlocks
{
    public static class MetaBlockState
    {
        public static string ToString(StateMsg msg, string type)
        {
            return msg switch
            {
                StateMsg.Loading => LoadingMsg(type),
                StateMsg.InvalidUrl => InvalidObjUrlMsg(type),
                StateMsg.InvalidData => InvalidObjDataMsg(type),
                StateMsg.OutOfBound => OutOfBoundMsg(type),
                StateMsg.ConnectionError => ConnectionErrorMsg(type),
                _ => "" // TODO
            };
        }

        private static string LoadingMsg(string type)
        {
            return $"Loading {type} ...";
        }
        
        private static string InvalidObjUrlMsg(string type)
        {
            return $"Invalid {type} url";
        }
        
        private static string InvalidObjDataMsg(string type)
        {
            return $"Invalid {type} data";
        }
        
        private static string OutOfBoundMsg(string type)
        {
            return $"{type.Substring(0, 1).ToUpper()}{(type.Length > 1 ? type.Substring(1): "")} exceeds land boundaries";
        }
        
        private static string ConnectionErrorMsg(string type)
        {
            return "Connection Error";
        }
    }

    public enum StateMsg
    {
        Loading,
        InvalidUrl,
        InvalidData,
        OutOfBound,
        ConnectionError,
        Ok
    }
}