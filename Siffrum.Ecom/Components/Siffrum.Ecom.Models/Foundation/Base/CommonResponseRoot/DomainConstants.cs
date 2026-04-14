namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class DomainConstants : DomainConstantsRoot
    {
        public class Generic : GenericRoot
        {
            //public const string LogToGhDbCompleted = "LogToGhDbCompleted";
        }
        public class Claims : ClaimsRoot
        {
            //public const string Claim_ClientCode = "clCd";
        }
        public class Headers : HeadersRoot
        {
            //public const string Header_CallerName = "CallerName";
        }
        public class Clients : ClaimsRoot
        {
            //public const string PollyContext_ServiceUrlHit = "ServiceURLHit";
        }
        public class DisplayMessages : DisplayMessagesRoot
        {
            public const string Display_InvalidCreds = "Invalid Credentials.";
            public const string Display_InvalidRequiredDataInputs = "Passed details invalid, please enter all required details correctly.";
            public const string Display_UserNotFound = "User details not found in database.";
            public const string Display_UserDisabled = "User is disabled for login, please contact your service team.";
            public const string Display_UserNotVerified = "User is disabled for login, please verify your mobile and email before loggingin";
            public const string Display_UserPasswordResetRequired = "User details are valid, but password change is required to login.";
        }
    }
}
