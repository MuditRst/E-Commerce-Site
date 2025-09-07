# 🛒 Real-Time Distributed Order Management System

A full-stack **E-Commerce Order Management Platform** built with **ASP.NET Core, SQL Server, Kafka, SignalR, and React**, deployed on **Azure**.  
The system supports real-time order updates, admin dashboards, and scalable event-driven processing with Kafka.

---

## 🚀 Overview

This project demonstrates how to build a modern, cloud-ready, event-driven system using **.NET and Azure**. It includes:

- 📦 Real-time order placement and tracking  
- 🔄 Event-driven processing with Apache Kafka / Azure Event Hubs  
- 📊 Admin dashboard with analytics and Kafka log monitoring  
- 🔐 Secure authentication with JWT  
- ☁️ Cloud deployment to Azure App Services  

---

## ✨ Features

### ✅ User Features
- Place new orders  
- Track order status in real-time via **SignalR**  
- Secure login & authentication  

### ✅ Admin Features
- Manage & update order status  
- Kafka logs dashboard for event monitoring  
- Analytics dashboard with charts (orders by status, user activity)  

### ✅ Architecture Features
- Event-driven with **Kafka Producer + Consumer**  
- Orders & logs persisted in **SQL Server + Cosmos DB**  
- Scalable microservice-friendly design  
- Cloud-native deployment on **Azure**  

---

## 🏗️ Tech Stack

- **Frontend**: React, TypeScript, TailwindCSS  
- **Backend**: ASP.NET Core 8, SignalR, EF Core, JWT Auth  
- **Messaging**: Apache Kafka (Azure Event Hub)  
- **Database**: CosmosDB (Orders), Cosmos DB (Logs)  
- **Cloud**: Azure App Services, Azure Event Hub  
- **Other Tools**: Swagger (API docs), Docker (containerization)  

---

## 📊 Architecture Diagram

```mermaid
User[Frontend (React)] --> API["ASP.NET Core API"]
    API --> KafkaProducer[Kafka Producer]
    KafkaProducer --> EventHub[(Azure Event Hub / Kafka Topic)]
    EventHub --> KafkaConsumer[Kafka Consumer Service]

    subgraph CosmosDB[Azure Cosmos DB]
        Orders[(Orders Container)]
        Users[(Users Container)]
        Logs[(Kafka Logs Container)]
    end

    API -- Stores data --> Users
    API -- Stores data --> Orders
    KafkaConsumer -- Consumes order event --> Processor[Order Processor Microservice]
    Processor -- Processes and logs --> Logs
    Processor -- Notifies --> SignalR[SignalR Hub]
    SignalR --> Admin[Admin Dashboard]

    subgraph KafkaProducerFlow[Producer Flow]
        API -- Publishes order-created event --> KafkaProducer
    end

    subgraph KafkaConsumerFlow[Consumer Flow]
        KafkaConsumer -- Consumes order-created event --> Processor
    end
```

## 🔗 Live Demo - https://orange-flower-079d22910.2.azurestaticapps.net/

## 🔑 Demo Admin Account For quick access to the Admin Dashboard, you can use the pre-created admin credentials:
Username: admin Password: admin123 

## ⚠️ Note: This account is only for demo purposes. In a production system, admin roles would be assigned securely via role-based access control (RBAC).
