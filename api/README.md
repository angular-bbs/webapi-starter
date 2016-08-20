# Webapi Starter Kit

This is a seed for Dotnet Core Webapi server. To see how it works in action, try out our [github-client](https://github.com/angular-bbs/github-client) repository. 

## Features:

### Database

Database type is SQLLite

### Account Management
1. Login with Github account: `/api/account/login-github`
1. An on site account will be created and linked to the github account.
1. On site account can only be created automatically by login with github account.
1. User can login with local account or login with Github account.
1. Comprehensive account management:
    * Create Password: `api/account/create-password`
    * Change Password: `api/account/change-password`
    * Recover Password: `api/account/forgot-password` and `api/account/reset-password`

# Usage
1. Register an OAuth application on github: Login in your github account: `Settings` -> `OAuth applications` -> `Developer applications`, register a new application:
    * Call back url: `http://localhost:4200/user-center/login-github`
1. Add your secret: follow the [Safe storage of app secrets during development](https://docs.asp.net/en/latest/security/app-secrets.html), add your secret acquired from previous step.
1. Get the Webapi server running: in the project directory, run the following commands:
    ```
    dotnet restore
    ```
    ```
    dotnet ef database update
    ```
    ```
    dotnet watch run
    ```
1. The should be running and listening to port 5000(http) and 44396(https). Both 127.0.0.1 and localhost are working.

# Test Client

Please see repository [Github-client](https://github.com/angular-bbs/github-client), it is an Angular-cli generated project with Angular 2 RC5. Interface is made up with Semantic-ui and ng-semantic.

Note: replace the ClientId with your own clientId in the [Github-client](https://github.com/angular-bbs/github-client).
