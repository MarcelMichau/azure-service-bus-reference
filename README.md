# Azure Service Bus Reference for .NET

This sample project provides examples of using Azure Service Bus with the Azure Service Bus SDK for .NET.

The solution consists of two projects:

- Producer - contains examples for sending messages to Azure Service Bus Queues & Topics
- Consumer - contains examples for receiving messages from Azure Service Bus Queues & Topic Subscriptions

## Prerequisites

These steps assume that you have an existing Resource Group with permissions to deploy resources into it.

1. Create an Azure Service Bus Namespace for testing:

```
az servicebus namespace create --resource-group <resource_group_name> --name <your_service_bus_namespace_name> --location southafricanorth
```

2. Grab the Service Bus Endpoint URL in the format `<your_service_bus_namespace_name>.servicebus.windows.net` and paste that into both `Program.cs` files in each of the Producer & Consumer projects.

3. The examples create/delete the necessary Queues + Topics + Subscriptions in code. In order to allow this, assign yourself the `Azure Service Bus Data Owner` Role to the Service Bus you just created:

```
Get your Azure AD Object ID:
az ad signed-in-user show --query objectId

Create the Role Assignment:
az role assignment create --assignee <your_azure_ad_object_id> --role Azure "Service Bus Data Owner" --scope /subscriptions/<your_subscription_id>/resourceGroups/<resource_group_name>/providers/Microsoft.ServiceBus/namespaces/<your_service_bus_namespace_name>
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
