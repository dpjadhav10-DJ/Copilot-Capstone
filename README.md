# Musafir Cafe

A locally hostable Cafe Management Web Application home page for US-001. The application uses ASP.NET Core (.NET 8), TypeScript, SQL Server, and Selenium WebDriver with Chrome.

## Run locally

1. Execute `database/001-create-cafe-story.sql` in SQL Server Management Studio. The script creates the `MusafirCafe` database, `CafeStory` table, unique active-story index, and initial story seed.
2. Confirm the connection string in `src/CafeManagement/appsettings.json` points to the local SQL Server instance.
3. Build the browser client from `src/CafeManagement` with `npm install` followed by `npm run build`. The checked-in `wwwroot/main.js` bundle is also available for environments without Node.js.
4. From the repository root, run `dotnet restore CafeManagement.sln` and `dotnet run --project src/CafeManagement/CafeManagement.csproj`.
5. Open [http://localhost:8080](http://localhost:8080).

## Selenium tests

Start the application, ensure Chrome is installed, then run:

```text
dotnet test tests/CafeManagement.UiTests/CafeManagement.UiTests.csproj
```

Set `CAFE_BASE_URL` to override the default test URL. The Selenium test expects ChromeDriver to be available through Selenium Manager or the machine PATH.

## Scope

The page displays Musafir Cafe branding, the required definition line, four future-navigation items linking to `https://coffeeformusafir.in/`, and the cafe story retrieved from SQL Server. Bill calculation, menu management, contact, and location workflows remain outside US-001.

## Workflow artifacts

- [User story](UserStories/US-001/US-001-Description.txt)
- [Requirement analysis](UserStories/US-001/US-001-RequirementAnalysis.md)
- [System architecture](UserStories/US-001/US-001-SystemArchitecture.md)
- [Implementation plan](UserStories/US-001/US-001-ImplementationPlan.md)

Copilot Capstone is a GitHub Copilot-powered project for the EPAM AI Learnings track. The repository currently contains a set of agent definitions that outline a structured delivery workflow for a Cafe Management Web Application.

## Overview

The repository is organized around a multi-step Copilot workflow that supports:

- requirement analysis
- system architecture creation
- design review
- implementation planning
- application implementation
- code review
- application testing
- changes publishing

The available agent instructions suggest the project focuses on a **Cafe Management Web Application** with:

- **C#** backend development
- **SQL** database work
- **Selenium** UI testing

## Repository Contents

The current repository primarily contains GitHub Copilot agent instruction files under `.github/agents`.

### Available agents

- `Requirement Analyst`
- `System Architecture Creator`
- `Design Reviewer`
- `Implementation Planner`
- `Application Implementor`
- `Code Reviewer`
- `Application Tester`
- `Changes Publisher`
- `Cafe Management Orchestrator`

## How the workflow is intended to work

The orchestrator agent coordinates the delivery process in this order:

1. Requirement analysis
2. System architecture creation
3. Design review
4. Implementation planning
5. Application implementation
6. Code review
7. Testing
8. Publishing changes

Each step produces an artifact used by the next step, helping keep the workflow structured and traceable.

## Getting Started

Because this repository currently contains agent definitions rather than application source code, there is no runtime setup documented yet.

If you are extending this repository, you may want to add:

- a solution or application source code
- build and run instructions
- test execution instructions
- contribution guidelines

## Notes

- Repository name: `Copilot-Capstone`
- Owner: `dpjadhav10-DJ`
- Default branch: `main`
- Description: `Copilot Capstone Project for EPAM AI Learnings`

## License

No license file was present in the repository at the time this README was generated.