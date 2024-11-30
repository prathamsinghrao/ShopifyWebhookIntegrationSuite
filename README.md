# Shopify Webhook Handler

## Description

**Shopify Webhook Handler** is a comprehensive solution for managing Shopify webhooks using **.NET Core**. The project includes a set of Web API endpoints to receive webhook notifications from Shopify, a Service Bus queue setup for message handling, and a Queue Receiver console application that processes the queued messages. The solution is modular, featuring class libraries for seamless message processing and database integration.

This solution helps businesses automate webhook processing from Shopify, ensuring scalability, error handling, and robust message processing with a focus on high performance.

## Features

- **Web API Endpoints**: Exposes RESTful API endpoints to receive webhook notifications from Shopify.
- **Service Bus Integration**: Uses a Service Bus queue for decoupling the message processing from the webhook reception, ensuring reliable and scalable message handling.
- **Queue Receiver**: A console application that reads messages from the Service Bus queue and processes them accordingly.
- **Database Integration**: Modular architecture includes support for saving and querying webhook data in a database, ensuring data persistence.
- **Error Handling & Retry Logic**: Built-in error handling and message retry capabilities for reliable webhook processing.

## Tech Stack

- **C#** and **.NET Core**: Primary language and framework for building the solution.
- **ASP.NET Core Web API**: For building the API endpoints to receive Shopify webhooks.
- **Azure Service Bus**: Used for queueing the webhook messages.
- **SQL Database**: For persisting webhook data and processed information.
- **Docker**: Containerization to simplify deployment.

## Architecture Overview

This solution is structured into multiple components to ensure scalability and maintainability:

1. **Web API**: Exposes endpoints to receive incoming webhook notifications from Shopify and forward them to the Service Bus queue for processing.
2. **Service Bus Queue**: A Service Bus queue is used to decouple the webhook reception from the processing logic, providing reliability and scalability.
3. **Queue Receiver Console App**: A console application that listens to the Service Bus queue, processes the messages, and integrates with the database.
4. **Database Integration**: The system integrates with a database to store webhook data and track the processing state of each message.

## Installation

### Prerequisites

Before you start, ensure you have the following installed:

- **.NET SDK 6.0** or later
- **Azure Service Bus**: Create an Azure Service Bus instance and obtain the connection string.
- **SQL Server**: A database instance to store webhook data.
- **Docker** (optional): For containerized deployment.

### Setup Instructions

1. Clone the repository to your local machine:

    ```bash
    git clone https://github.com/<your-github-username>/Shopify-Webhook-Handler.git
    ```

2. Navigate to the project directory:

    ```bash
    cd Shopify-Webhook-Handler
    ```

3. Restore the dependencies:

    ```bash
    dotnet restore
    ```

4. Configure the application settings (e.g., Service Bus connection string, SQL connection string) in the `appsettings.json` or through environment variables.

    Example:
    ```json
    {
      "ServiceBus": {
        "ConnectionString": "your-service-bus-connection-string",
        "QueueName": "shopify-webhook-queue"
      },
      "Database": {
        "ConnectionString": "your-database-connection-string"
      }
    }
    ```

5. Build and run the Web API application:

    ```bash
    dotnet run --project ShopifyWebhookHandler.API
    ```

6. Run the Queue Receiver console application to process messages from the Service Bus queue:

    ```bash
    dotnet run --project ShopifyWebhookHandler.QueueReceiver
    ```

    The console application will start processing messages from the queue and will save data to the database as specified.

### Running with Docker

1. Build the Docker image for both the Web API and Queue Receiver applications:

    ```bash
    docker build -t shopify-webhook-handler .
    ```

2. Run the Docker container:

    ```bash
    docker run -p 8080:80 shopify-webhook-handler
    ```

The Web API will now be available at `http://localhost:8080`, and the Queue Receiver will be processing messages from the queue.

## Usage

### Webhook Handling

Once the solution is up and running, Shopify can send webhook notifications to the API endpoints.

Example request from Shopify:

- **POST /api/webhooks/shopify**  
  - **Request body**: Shopify's webhook payload (JSON)
  - **Response**: Acknowledgement that the message has been received.

Example request body from Shopify webhook:
```json
{
  "id": 123456,
  "shop": "example-shop",
  "event": "order_created",
  "data": {
    "order_id": 98765,
    "customer_name": "John Doe",
    "total": 199.99
  }
}
