# Copilot Capstone

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