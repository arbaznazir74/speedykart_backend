namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class DomainConstantsRoot
    {
        public class GenericRoot
        {
            public const string LogToGhDbCompleted = "LogToGhDbCompleted";

            public const string ExpData_TimeTakenForHttpCallMs = "TimeTakenForHttpCallMs";

            public const string ExpData_ContentOnError = "ContentOnError";

            public const string ExpData_IsErrorInGateway = "IsGatewayError";

            public const string MultiForm_File_PrePend = "file-";

            public const string MultiForm_ApiReq_KeyName = "apireq";

            public const string AES_PasswordPrefix = "-_AE_-";
        }

        public class ClaimsRoot
        {
            public const string Claim_ClientCode = "clCd";

            public const string Claim_ClientId = "clId";

            public const string Claim_LoginUserType = "uTyp";

            public const string Claim_DbRecordId = "dbRId";
        }

        public class HeadersRoot
        {
            public const string Header_CallerName = "CallerName";

            public const string Header_SourceName = "SourceName";

            public const string Header_CallerBaseClientVersion = "CallerBaseClientVersion";

            public const string Header_CallerAppVersion = "AppVersion";

            public const string Header_IsDeveloperApk = "IsDeveloperApk";

            public const string Header_TargetApiType = "TargetApiType";

            public const string Header_TargetCompCode = "t_ccd";

            public const string Header_TargetCompId = "t_cid";

            public const string Header_TargetClientEmpUserId = "t_ce_id";

            public const string Header_Authorization = "Authorization";

            public const string Header_Skip = "$skip";

            public const string Header_Top = "$top";

            public const string Header_FilterCommand = "flt_cmd";

            public const string Header_OrderByCommand = "ord_cmd";

            public const string Header_SearchByCommand = "srch_cmd";

            public const string Header_SearchByText = "srch_txt";

            public const string Header_SearchOnColumns = "srch_on";

            public const string Header_SearchType = "srch_typ";

            public const string Header_SearchType_Cointais = "cnt";

            public const string Header_SearchType_StartsWith = "strt";

            public const string Header_SearchType_EndsWith = "end";

            public const string Header_SkipLogging = "skpLog";

            public const string Header_TracingId = "traceid";
        }

        public class ClientsRoot
        {
            public const string PollyContext_ServiceUrlHit = "ServiceURLHit";
        }

        public class DisplayMessagesRoot
        {
            public const string Display_GlobalErrorApi = "Unknown Error Occured in Api. Please retry after sometime or contact service team.";

            public const string Display_GlobalErrorClient = "Unknown Error Occured in Client. Please retry after sometime or contact service team.";

            public const string Display_NoCallerName = "No or Unknown caller name found in request.";

            public const string Display_CallerDisabled = "Requested operation disabled for caller/client.";

            public const string Display_ReqDataNotFormed = "Check input params 'ReqData' not formed properly.";

            public const string Display_PassedDataNotSaved = "Passed data not saved/updated, please try again later.";

            public const string Display_IdNotFound = "Passed Id Not Found";

            public const string Display_IdInvalid = "Passed Id is invalid, please check and try again";

            public const string Display_IdNotInClaims = "Invalid user, Expected CompanyId Not Found In Claims";

            public const string Display_FileNotSaved = "Passed File Could not be saved properly";

            public const string Display_InvalidRequiredDataInputs = "Invalid Required Data";

            public const string Display_UserNotFound = "User Not Found";

            public const string Display_UserDisabled = "User Id Disabled";

            public const string Display_UserPasswordResetRequired = "User Password Reset Required";

            public const string Display_UserNotVerified = "User Not Verified";

            public const string Display_WrongCredentials = "Invalid Credentials";
        }
    }
}
