# Introduction 
This repository hosts the code for the Identity Project. The idea is to use DevOps Git along with Azure CI CD pipeline to deploy the code to Azure serverless environment.

# Documentation
1.  [Identity Service Domain - High Level Solution Design]( https://cricketaustralia.atlassian.net/wiki/spaces/KP/pages/1976500749/Identity+Service+Domain+-+High+Level+Solution+Design) 

# System Requirements
- Git Client
- Visual Studio 2019
- .NET Core 3.1 (LTS)
- Azure Functions Core Tools SDK
- ReSharper License (Optional)
- xUnit test runner if not using ReSharper
- [You will need access to some of the services listed here](https://cricketaustralia.atlassian.net/wiki/spaces/Engineering/pages/710606869/Accounts+for+a+New+Starter)

# Project Folder Structure
## High Level Folder Structure
```
grassroots-Identity/
    Common/
    Functions/
    Test/
    .gitignore
    Grassroots.Identity.sln
    README.md
```
- `grassroots-Identity/` is the root of the repository.
- `Common/` Contains projects with functionality which can be refactored and used across the solution.
- `Functions/` Contains projects related to the azure functions.
- `Test/` Contains Unit test project.
- `Grassroots.Identity.sln` Visual studio solution file encapsulating all the above folder structure
## Detailed Folder Structure

### Common Folder
Contains projects with functionality which can be refactored and used across the solution.

```
Common/
    Grassroots.Identity.API.Common/
    Grassroots.Identity.API.Common.DependencyContainer/
    
```
- ` Grassroots.Identity.API.Common/` Encapsulates all common refactored functionality E.g. Common Validations, Code Utilities etc.
- ` Grassroots.Identity.API.Common. DependencyContainer /` Project for centrally resolving IoC Container dependencies.


### Functions Folder
Contains projects related to the Azure Functions.

```
Functions/
    Grassroots.Identity.Functions/

```
- `Grassroots.Identity.Functions/` This is a azure function project. This defines all the azure functions being used in Identity project.

### Test Folder
Contains Unit test project.

```
Test/
    Functions/
        Grassroots.Identity.Functions.Test/ 
        BlobStoreFunction/
        FeedParserFunction/        
```

# Databases
No Database is used as of now in this project.

# Unit Testing
Unit testing is performed using the xunit framework.

### Concept 
Mock every dependency your unit test touches. Write Unit test cases whereever you come across any static independent implementation of any functionality Like: Reading Config. 

There should be enough Unit/Integration test cases to support each implementation for the code coverage. This should help us follow TDD approach and gives confidence for the CI/CD approach with enough code coverage.
Service, Database, Webjob or any other common or individual logic implemntation needs to be covered with separate Unit test cases for each layer.

### Mocking objects
We are using Moq Nugget Package to Mock any objects. Mock all your dependencies for the integration test cases.

Supply enought test data to run your unit test cases to cover all possibilities with the implementation.

###  Independent Implementations

For the independent logic implementations like config reads, please write straight forward & simple Unit test cases with XUnits.

# Environment
There are four environments – DEV, SIT, UAT, PROD for Identity [Environments can be found in this confluence article]( https://cricketaustralia.atlassian.net/wiki/spaces/KP/pages/1270350062/Environments)
