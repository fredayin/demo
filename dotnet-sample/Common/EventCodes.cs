using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class EventCodes
    {
        public static class General
        {
            /// <summary>
            /// Error codes begin 1000XX
            /// </summary>
            public static class Trace
            {
                public readonly static EventId PLACEHOLDER = new(100001, "PLACEHOLDER");

            }

            /// <summary>
            /// Error codes begin 1001XX
            /// </summary>
            public static class Information
            {
                public readonly static EventId HOSTPROCESS_STARTING = new(100100, "HOSTPROCESS_STARTING");
                public readonly static EventId EXTERNALCONNECTIONRETRY_NONSUCCESS = new(100101, "EXTERNALCONNECTIONRETRY_NONSUCCESS");
                public readonly static EventId CONFIGURATION_LOADING = new(100102, "CONFIGURATION_LOADING");

            }

            /// <summary>
            /// Error codes begin 1002XX
            /// </summary>
            public static class Debug
            {
                public readonly static EventId PLACEHOLDER = new(100201, "PLACEHOLDER");
            }

            /// <summary>
            /// Error codes begin 1003XX
            /// </summary>
            public static class Warn
            {
                public readonly static EventId GRAPHAPISERVICE_USERID_NULL = new(100302, "GRAPHAPISERVICE_USERID_NULL");
            }

            /// <summary>
            /// Error codes begin 1004XX
            /// </summary>
            public static class Errors
            {
                public readonly static EventId UNHANDLED_EXCEPTION = new(100400, "UNHANDLED_EXCEPTION");
                public readonly static EventId GRAPHAPISERVICE_REDISCACHE_FAILURE = new(100403, "GRAPHAPISERVICE_REDISCACHE_FAILURE");
                public readonly static EventId NONSUCCESSCODE = new(100406, "NONSUCCESSCODE");

            }

            /// <summary>
            /// Error codes begin 1005XX
            /// </summary>
            public static class Fatal
            {
                public readonly static EventId PLACEHOLDER = new(100501, "PLACEHOLDER");
            }
        }

        public static class BearerTokens
        {
            public static class Trace
            {
                public readonly static EventId BEARERTOKENS_OBTAINBEARERSUCCESS = new(111001, "BEARERTOKENS_OBTAINBEARERSUCCESS");
                public readonly static EventId BEARERTOKENS_NEWREQUEST = new(111002, "BEARERTOKENS_NEWREQUEST");

            }

            public static class Warn
            {
                public readonly static EventId BEARERTOKENS_UNAUTHORIZEDACCESSEXCEPTION = new(111301, "BEARERTOKENS_UNAUTHORIZEDACCESSEXCEPTION");

            }

            /// <summary>
            /// Error codes begin 1114XX
            /// </summary>
            public static class Errors
            {
                public readonly static EventId BEARERTOKENS_DATAREADEXCEPTION = new(111401, "BEARERTOKENS_DATAREADEXCEPTION");
                public readonly static EventId BEARERTOKENS_DATAWRITEEXCEPTION = new(111402, "BEARERTOKENS_DATAWRITEEXCEPTION");
                public readonly static EventId BEARERTOKENS_NOTIMPLEMENTED = new(111403, "BEARERTOKENS_NOTIMPLEMENTED");
                public readonly static EventId BEARERTOKENS_UNREADABLETOKEN = new(111404, "BEARERTOKENS_UNREADABLETOKEN");


            }
        }

        public static class AuthCheck
        {
            public static class Trace
            {
                public readonly static EventId VALIDATECREDENTIALS_NEWREQUEST = new(109001, "VALIDATECREDENTIALS_NEWREQUEST");
                public readonly static EventId VALIDATECREDENTIALS_NEWREQUEST_SUCCESS = new(109002, "VALIDATECREDENTIALS_NEWREQUEST_SUCCESS");
                public readonly static EventId AUTHCONTROLLER_CONSTRUCTOR = new(109003, "AUTHCONTROLLER_CONSTRUCTOR");            

            }

            public static class Information
            {
                public readonly static EventId VALIDATECREDENTIALS_NEWREQUEST_FAILURE = new(109100, "VALIDATECREDENTIALS_NEWREQUEST_FAILURE");

            }

            public static class Errors
            {
                public readonly static EventId VALIDATECREDENTIALS_NEWREQUEST_FAILURE = new(109201, "VALIDATECREDENTIALS_NEWREQUEST_FAILURE");

            }
        }

        public static class Users
        {
            public static class Trace
            {
                public readonly static EventId USERS_GETUSERREQUEST = new(109301, "USERS_GETUSERREQUEST");
                public readonly static EventId USERS_GETUSERREQUEST_NOTFOUND = new(109302, "USERS_GETUSERREQUEST_NOTFOUND");
                public readonly static EventId USERS_GETUSERREQUEST_SUCCESS = new(109303, "USERS_GETUSERREQUEST_SUCCESS");
                public readonly static EventId USERS_GETUSERREQUEST_FAILURE = new(109304, "USERS_GETUSERREQUEST_FAILURE");
                public readonly static EventId USERS_UPDATEUSERPATCHREQUEST = new(109305, "USERS_UPDATEUSERPATCHREQUEST");                
                public readonly static EventId USERS_UPDATEUSERPATCHREQUEST_USERNAMEALREADYEXISTS = new(109306, "USERS_UPDATEUSERPATCHREQUEST_USERNAMEALREADYEXISTS");
                
            }

            public static class Information
            {
                public readonly static EventId USERS_INVALID_GRAPHAPI_FILTER = new(109401, "USERS_INVALID_GRAPHAPI_FILTER");
                public readonly static EventId USERS_GETUSERREQUEST_NOTFOUND = new(109402, "USERS_GETUSERREQUEST_NOTFOUND");
                public readonly static EventId USERS_GETUSERREQUEST_MULTIPLERECORDS = new(109403, "USERS_GETUSERREQUEST_MULTIPLERECORDS");
            }

            public static class Errors
            {
                public readonly static EventId USERS_GETUSER_ARGUMENTEXCEPTION = new(109501, "USERS_GETUSER_ARGUMENTEXCEPTION");
                public readonly static EventId USERS_GETUSER_FAILURE = new(109502, "USERS_GETUSER_FAILURE");
                public readonly static EventId USERS_APPCLIENTID_FORMATEXCEPTION = new(109503, "USERS_APPCLIENTID_FORMATEXCEPTION");
                public readonly static EventId USERS_UPDATEUSERPATCHFAILED = new(109504, "USERS_UPDATEUSERPATCHFAILED");
                
            }
        }
        public static class UserDetails
        {
            public static class Trace
            {
                public readonly static EventId USERDETAILS_NEWGETREQUEST = new(109601, "USERDETAILS_NEWGETREQUEST");
                public readonly static EventId USERDETAILS_NEWGETREQUEST_SUCCESS = new(109602, "USERDETAILS_NEWGETREQUEST_SUCCESS");
                public readonly static EventId USERDETAILS_NEWGETREQUEST_NOTFOUND = new(109603, "USERDETAILS_NEWGETREQUEST_NOTFOUND");
                public readonly static EventId USERDETAILS_NEWGETREQUEST_FAILURE = new(109604, "USERDETAILS_NEWGETREQUEST_FAILURE");
            }

            public static class Errors
            {
                public readonly static EventId USERDETAILS_GETUSERDETAILS_ARGUMENTEXCEPTION = new(109701, "USERDETAILS_GETUSERDETAILS_ARGUMENTEXCEPTION");
                public readonly static EventId USERDETAILS_GETUSERDETAILS_MISSINGARGUMENTEXCEPTION = new(109702, "USERDETAILS_GETUSERDETAILS_MISSINGARGUMENTEXCEPTION");
            }

        }
        

        public static class WebApp1
        {
            public static class UI
            {
                /// <summary>
                /// Error codes begin 5020XX
                /// </summary>
                public static class Trace
                {
                    public readonly static EventId UI_WEBAPP1_FILE_UPLOAD_STARTED = new(304001, "UI_WEBAPP1_FILE_UPLOAD_STARTED");
                    public readonly static EventId UI_WEBAPP1_FILE_UPLOAD_FINISHED = new(304002, "UI_WEBAPP1_FILE_UPLOAD_FINISHED");
                    public readonly static EventId UI_WEBAPP1_BATCH_GENERATION_LINE_SKIPPED = new(304003, "UI_WEBAPP1_BATCH_GENERATION_LINE_SKIPPED");
                    public readonly static EventId UI_WEBAPP1_BATCH_GENERATION_FINISHED = new(304004, "UI_WEBAPP1_BATCH_GENERATION_FINISHED");
                    public readonly static EventId UI_WEBAPP1_BATCH_GENERATION_STARTED = new(304005, "UI_WEBAPP1_BATCH_GENERATION_STARTED");
                    public readonly static EventId UI_WEBAPP1_DELETE_FILE_INFO = new(304006, "UI_WEBAPP1_DELETE_FILE_INFO");
                    public readonly static EventId UI_WEBAPP1_DELETE_SUCCESS = new(304007, "UI_WEBAPP1_DELETE_SUCCESS");
                    public readonly static EventId UI_WEBAPP1_SAVE_FILE_INFO = new(304008, "UI_WEBAPP1_SAVE_FILE_INFO");
                    public readonly static EventId UI_WEBAPP1_SAVE_SUCCESS = new(304009, "UI_WEBAPP1_SAVE_SUCCESS");
                    public readonly static EventId UI_WEBAPP1_USER_TOOL_USER_FOUND = new(304010, "UI_WEBAPP1_USER_TOOL_USER_FOUND");
                    public readonly static EventId UI_WEBAPP1_USER_TOOL_USER_NOT_FOUND = new(304011, "UI_WEBAPP1_USER_TOOL_USER_NOT_FOUND");
                    public readonly static EventId UI_WEBAPP1_CSV_DOWNLOAD = new(304012, "UI_WEBAPP1_CSV_DOWNLOAD");
                    public readonly static EventId UI_WEBAPP1_USER_TOOL_USER_SEARCHED = new(304013, "UI_WEBAPP1_USER_TOOL_USER_SEARCHED");
                    public readonly static EventId UI_WEBAPP1_USER_TOOL_ACCOUNT_MARKED_AS_DISABLED = new(304015, "UI_WEBAPP1_USER_TOOL_ACCOUNT_MARKED_AS_DISABLED");
                }
                /// <summary>
                /// Error codes begin 5021XX
                /// </summary>
                public static class Information
                {
                    public readonly static EventId PLACEHOLDER = new(304101, "PLACEHOLDER");
                }
                /// <summary>
                /// Error codes begin 5022XX
                /// </summary>
                public static class Debug
                {
                    public readonly static EventId PLACEHOLDER = new(304201, "PLACEHOLDER");
                }
                /// <summary>
                /// Error codes begin 5023XX
                /// </summary>
                public static class Warn
                {
                    public readonly static EventId PLACEHOLDER = new(304301, "PLACEHOLDER");
                }
                /// <summary>
                /// Error codes begin 5024XX
                /// </summary>
                public static class Errors
                {
                    public readonly static EventId UI_WEBAPP1_BATCH_MESSAGE_NOT_SENT = new(304401, "UI_WEBAPP1_BATCH_MESSAGE_NOT_SENT");
                    public readonly static EventId UI_WEBAPP1_UNABLE_TO_MAP_TO_FILEINFO = new(304402, "UI_WEBAPP1_UNABLE_TO_MAP_TO_FILEINFO");
                    public readonly static EventId UI_WEBAPP1_UNABLE_TO_MAP_TO_SUCCESS = new(304403, "UI_WEBAPP1_UNABLE_TO_MAP_TO_SUCCESS");
                    public readonly static EventId UI_WEBAPP1_FAILURE_DOWNLOADING_FILE = new(304406, "UI_WEBAPP1_FAILURE_DOWNLOADING_FILE");
                    public readonly static EventId UI_WEBAPP1_USER_TOOL_PATCH_NOOBJECTID = new(304408, "UI_WEBAPP1_USER_TOOL_PATCH_NOOBJECTID");
                    public readonly static EventId UI_WEBAPP1_USER_TOOL_ACCOUNT_MARKED_AS_DISABLED_FAILED = new(304409, "UI_WEBAPP1_USER_TOOL_ACCOUNT_MARKED_AS_DISABLED_FAILED");
                }
                /// <summary>
                /// Error codes begin 5025XX
                /// </summary>
                public static class Fatal
                {
                    public readonly static EventId PLACEHOLDER = new(304501, "PLACEHOLDER");
                }
            }
        }
    }
}
