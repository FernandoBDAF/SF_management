# Web API

## What is an API?
    - Application Programming Interface
    - A way to application talk to each other in a standardized manner
    - GUI: graphical user interface need that

## When to use?
    - Microservices
    - Mobile apps
    - Client-side web apps
    - to have a second UI
    - to scale your app
    - to protect your app

## What is REST?
    - Requests are managed through HTTP
    - Stateless client-server communication
    - Cacheable data
    - Uniform interface

## Secrets
    - appsettings.json will be in the repo, so its not safe
    - right click in the project and click on manage secrets - all variables there will overwrite whatever be in the appsettings.

## Creating Tokens
    ### Nu packages
        - Microsoft.AspNetCore.Authehtication.JwtBearer
        - Microsoft.IdentityModel.Tokens
        - System.IdentityModel.Tokens.Jwt