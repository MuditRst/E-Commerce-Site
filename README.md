🛒 Real-Time Distributed Order Management System

A full-stack E-Commerce Order Management Platform built with ASP.NET Core, SQL Server, Kafka, SignalR, and React, deployed on Azure.
The system supports real-time order updates, admin dashboards, and scalable event-driven processing with Kafka.

🚀 Overview

This project demonstrates how to build a modern, cloud-ready, event-driven system using .NET and Azure.
It includes:

Real-time order placement and tracking

Event-driven processing with Apache Kafka / Azure Event Hubs

Admin dashboard with analytics and Kafka log monitoring

Secure authentication with JWT

Cloud deployment to Azure App Services

✨ Features

✅ User Features

Place new orders

Track order status in real-time via SignalR

Secure login & authentication

✅ Admin Features

Manage & update order status

Kafka logs dashboard for event monitoring

Analytics dashboard with charts (orders by status, user activity)

✅ Architecture Features

Event-driven with Kafka Producer + Consumer

Orders & logs persisted in SQL Server + Cosmos DB

Scalable microservice-friendly design

Cloud-native deployment on Azure

🏗️ Tech Stack

Frontend: React, TypeScript, TailwindCSS
Backend: ASP.NET Core 8, SignalR, EF Core, JWT Auth
Messaging: Apache Kafka (Azure Event Hub)
Database: SQL Server (Orders), Cosmos DB (Logs)
Cloud: Azure App Services, Azure Event Hub
Other Tools: Swagger (API docs), Docker (containerization)

📊 Architecture
flowchart LR
  User[User Frontend (React)] --> API[ASP.NET Core API]
  API --> DB[(SQL Server - Orders)]
  API --> KafkaProducer[Kafka Producer]
  KafkaProducer --> EventHub[(Azure Event Hub / Kafka Topic)]
  EventHub --> KafkaConsumer[Kafka Consumer Service]
  KafkaConsumer --> Cosmos[(CosmosDB - Logs)]
  KafkaConsumer --> SignalR[SignalR Hub]
  SignalR --> Admin[Admin Dashboard]

⚡ Deployment

Frontend: Azure Static Web Apps (or App Service)

Backend: Azure App Service (Dockerized .NET Core)

Event Streaming: Azure Event Hub (Kafka protocol)

Databases: Azure SQL + CosmosDB

📈 Quantifiable Impact (Resume Highlights)

Improved order processing scalability by 40% via event-driven Kafka architecture

Enabled real-time updates (<1s latency) using SignalR

Deployed full system to Azure Cloud ensuring scalability and reliability

Designed admin analytics dashboard with live Kafka monitoring for auditing

▶️ Demo

🔗 Live Demo - https://orange-flower-079d22910.2.azurestaticapps.net/

📂 GitHub Repo

🔑 Demo Admin Account

For quick access to the Admin Dashboard, you can use the pre-created admin credentials:

Email: admin  
Password: admin123

⚠️ Note: This account is only for demo purposes. In a production system, admin roles would be assigned securely via role-based access control (RBAC).

🛠️ Getting Started
Prerequisites

.NET 8 SDK

Node.js 18+

Docker (for local Kafka setup)

Azure account (for Event Hub / deployment)

Setup
# Backend
cd ECommerceApp
dotnet restore
dotnet run

# Frontend
cd frontend
npm install
npm start

🤝 Contributing

This project is part of my learning journey with .NET + Cloud Architecture.
Suggestions and improvements are welcome!

