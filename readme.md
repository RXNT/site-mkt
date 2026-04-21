
# Problem Statement

The marketing department is launching a new promotional landing page that displays a 'hello world' message with the current date, pulled from a sensitive SQL database, which is cached for 5 seconds. They expect significant traffic from 10:00 AM to 8:00 PM EST, especially during the first few days of the campaign.

Your task is to use Terraform to provision the Azure infrastructure to support this application. You are responsible for designing the full solution architecture to meet the requirements of a production environment.

### Strategic Context

This deployment is more than a one-time project. It is a Proof of Concept (POC) designed to establish a reusable and standardized pattern for deploying similar containerized applications within the organization. A successful outcome will serve as a template, allowing other teams to quickly adopt a secure and scalable architecture for their own initiatives.

## Requirements

- **Use Terraform** to provision all Azure infrastructure
- **Deploy to Azure** (your choice of services)

## Deliverables

A repository link containing a git repository with your Terraform code, and a readme file with instructions for deployment. The README should also include a brief overview of your design choices.


# Further Information

This site is the first version of the Hello World Site. It is composed of two projects `Site` and `Api`.

## Requirements to run the site

The Site container expects the following environment variables:
- `REDIS_CONNECTION_STRING` to contain a connection string for a Redis server.
- `MarketingApi__BaseUrl` to contain the base URL of the Api project.

The Api container expects the following environment variables:
- `DB_CONNECTION_STRING` to contain a connection string for a SQL Server.

The solution also contains a `docker-compose.yml` file that can be used to run the site locally using Docker.

## Azure Authentication

`azure-setup.ps1` creates a custom role and service principal intended for authenticating Terraform. Before executing the script, use `az login` to login and select the desired subscription.