# Contributing to ECU Simulator & Diagnostic Toolchain

Thank you for your interest in contributing to the ECU Simulator & Diagnostic Toolchain! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [How to Contribute](#how-to-contribute)
4. [Development Workflow](#development-workflow)
5. [Coding Standards](#coding-standards)
6. [Testing](#testing)
7. [Documentation](#documentation)
8. [Community](#community)

## Code of Conduct

This project adheres to a Code of Conduct that all participants are expected to follow. Please read and respect it to ensure a positive experience for everyone.

In short:
- Be respectful and inclusive
- Be collaborative
- Be constructive in feedback
- Focus on what is best for the community

## Getting Started

1. **Set up your development environment**
   - Follow the instructions in [INSTALL.md](INSTALL.md) to set up your environment
   - Ensure you have .NET 6.0 SDK or newer installed
   - Install socat for virtual serial port creation

2. **Fork and clone the repository**
   - Fork the repository on GitHub
   - Clone your fork locally
   - Add the original repository as a remote named "upstream"

3. **Build and run the project**
   - Build the solution using `dotnet build`
   - Run the simulation using the provided script or manual setup

## How to Contribute

There are many ways to contribute to this project:

1. **Report bugs**
   - Use the issue tracker to report bugs
   - Include detailed steps to reproduce the bug
   - Include information about your environment

2. **Suggest features**
   - Use the issue tracker to suggest new features
   - Explain why the feature would be useful
   - Discuss potential implementation approaches

3. **Submit pull requests**
   - For bug fixes
   - For new features
   - For documentation improvements

4. **Improve documentation**
   - Fix typos or clarify existing documentation
   - Add examples or tutorials
   - Translate documentation

## Development Workflow

1. **Create a branch**
   - Create a branch from the main branch
   - Use a descriptive name (e.g., `feature/add-o2-sensor` or `fix/dtc-clearing-bug`)

2. **Make your changes**
   - Follow the coding standards
   - Keep changes focused on a single issue
   - Add or update tests as needed
   - Update documentation as needed

3. **Commit your changes**
   - Write clear, concise commit messages
   - Reference issue numbers in commit messages
   - Use present tense ("Add feature" not "Added feature")

4. **Submit a pull request**
   - Push your branch to your fork
   - Submit a pull request to the main repository
   - Describe your changes in detail
   - Reference any related issues

5. **Review process**
   - Maintainers will review your pull request
   - Address any feedback or requested changes
   - Once approved, your changes will be merged

## Coding Standards

1. **C# Code Style**
   - Follow Microsoft's C# coding conventions
   - Use meaningful names for variables, methods, and classes
   - Include XML documentation comments for public APIs
   - Keep methods focused and not too long

2. **Project Structure**
   - Keep the existing project structure
   - Place new features in appropriate locations
   - Create new classes for significant new functionality

3. **Error Handling**
   - Use try-catch blocks appropriately
   - Provide meaningful error messages
   - Log errors for debugging

## Testing

1. **Manual Testing**
   - Test your changes thoroughly
   - Verify that existing functionality still works
   - Test edge cases and error conditions

2. **Automated Testing**
   - Add unit tests for new functionality
   - Ensure all tests pass before submitting a pull request

## Documentation

1. **Code Documentation**
   - Add XML documentation comments to public APIs
   - Include examples where appropriate
   - Explain complex algorithms or logic

2. **Project Documentation**
   - Update README.md for significant changes
   - Update EXTENDING.md for new extension points
   - Create or update guides as needed

## Community

- Be respectful and constructive in all communications
- Help others who have questions
- Share your knowledge and experiences
- Recognize and acknowledge contributions from others

---

Thank you for contributing to the ECU Simulator & Diagnostic Toolchain! Your efforts help make this project better for everyone.