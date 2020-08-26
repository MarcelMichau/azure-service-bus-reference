using Azure.Identity;

namespace Common
{
    public static class Config
    {
        // To run these examples, first create a Service Bus Namespace with the Standard Tier in Azure & retrieve the Namespace value & set it here:
        public const string Namespace = "<your-service-bus-namespace>.servicebus.windows.net";

        public static readonly DefaultAzureCredential Credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            // VisualStudioTenantId = "" // Set this GUID if you need to connect to a specific Azure AD Tenant
        });

        // Un-comment this when having auth issues using DefaultAzureCredential
        //public static readonly InteractiveBrowserCredential Credential = new InteractiveBrowserCredential();
    }
}
