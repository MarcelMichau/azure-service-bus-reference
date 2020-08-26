# Azure Service Bus Reference for .NET

This sample project provides examples of using Azure Service Bus with the Azure Service Bus SDK for .NET.

The solution consists of two projects:

- Producer - contains examples for sending messages to Azure Service Bus Queues & Topics
- Consumer - contains examples for receiving messages from Azure Service Bus Queues & Topic Subscriptions

## Prerequisites

The code samples use the [Azure Identity client library for .NET](https://docs.microsoft.com/en-gb/dotnet/api/overview/azure/identity-readme?view=azure-dotnet) to authenticate with Azure Service Bus. Depending on how you run the samples, make sure the following is in place:

- Visual Studio - Sign into Visual Studio with the same Microsoft Account you use for Azure.
- .NET CLI - Sign into the Azure CLI with the same Microsoft Account you use for Azure using `az login`.

These steps assume that you have an existing Resource Group with permissions to deploy resources into it.

1. Ensure you have the latest version of the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) installed.

2. Create an Azure Service Bus Namespace for testing:

```
az servicebus namespace create --resource-group <resource_group_name> --name <your_service_bus_namespace_name> --location southafricanorth
```

3. Grab the Service Bus Endpoint URL in the format `<your_service_bus_namespace_name>.servicebus.windows.net` and paste that into `Common/Config.cs`.

4. The examples create/delete the necessary Queues + Topics + Subscriptions in code. In order to allow this, assign yourself the `Azure Service Bus Data Owner` Role to the Service Bus you just created:

```
Get your Azure AD Object ID:
az ad signed-in-user show --query objectId

Create the Role Assignment:
az role assignment create --assignee <your_azure_ad_object_id> --role "Azure Service Bus Data Owner" --scope /subscriptions/<your_subscription_id>/resourceGroups/<resource_group_name>/providers/Microsoft.ServiceBus/namespaces/<your_service_bus_namespace_name>
```

## Running the examples

Firstly, open up either of the Producer or Consumer project's `Program.cs` files & un-comment the example you'd like to run, making sure to un-comment the corresponding line in the opposite project.

Open up a Terminal & change into the Consumer directory & run the Consumer project

```
dotnet run
```

Change into the Producer directory & run the Producer project

```
dotnet run
```

## Known Issues

### AuthenticationFailedException is thrown when starting Console app

Sometimes when running the samples from the .NET CLI using `dotnet run`, an `AuthenticationFailedException` can be thrown indicating an Authorization issue with the Azure CLI. To workaround this, use the `InteractiveBrowserCredential` for `Azure.Identity` by commenting out the `DefaultAzureCredential` in `Common/Config.cs` & replacing it with the `InteractiveBrowserCredential` option below it.
