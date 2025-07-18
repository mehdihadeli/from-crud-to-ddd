# Configuration
PROJECT := src/SmartCharging/SmartCharging.csproj

# Default target
.PHONY: all
all: build

# Restore NuGet packages
.PHONY: restore
restore:
	dotnet restore

# Build the project
.PHONY: build
build: 
	dotnet build 

# CI build (without restore step)
.PHONY: ci-build
ci-build:
	dotnet build --no-restore

# Clean build artifacts
.PHONY: clean
clean:
	dotnet clean 

# Run the application
.PHONY: run
run: build
	dotnet run --project $(PROJECT)

# Run unit-tests
.PHONY: unit-tests
unit-tests: 
	dotnet test ./tests/SmartCharging.UnitTests/SmartCharging.UnitTests.csproj

# Run integration-tests
.PHONY: integration-tests
integration-tests: 
	dotnet test ./tests/SmartCharging.IntegrationTests/SmartCharging.IntegrationTests.csproj

# Run end-to-end-tests
.PHONY: end-to-end-tests
end-to-end-tests: 
	dotnet test ./tests/SmartCharging.EndToEndTests/SmartCharging.EndToEndTests.csproj

# Run all tests (unit, integration, and end-to-end)
.PHONY: test
test: unit-tests integration-tests end-to-end-tests

# Format code using CSharpier
.PHONY: format
format:
	dotnet tool restore
	dotnet csharpier .

# Run docker-compose
.PHONY: run-docker-compose
run-docker-compose:
	docker-compose -f .\deployments\docker-compose\docker-compose.yaml up -d    

# Stop docker-compose
.PHONY: stop-docker-compose
stop-docker-compose:
	docker-compose -f .\deployments\docker-compose\docker-compose.yaml down 

# Generate development certificate for HTTPS
.PHONY: cert
cert:
	dotnet dev-certs https --clean
	@echo "Certificate generated"

# Install tools
.PHONY: install-csharpier
install-tools:
	dotnet tool install
