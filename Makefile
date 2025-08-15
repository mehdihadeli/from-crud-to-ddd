# Show help
help:
	@echo "Available targets:"
	@echo "  prepare                    - Install Husky and .NET tools"
	@echo "  install-dev-cert           - Install dev cert (Bash)"
	@echo "  run                        - Run aspire dashboard"
	@echo "  run-smart-charging-api     - Run SmartChargingApi"
	@echo "  run-smart-charging-statistics-api - Run SmartChargingStatisticsApi"
	@echo "  restore                    - Restore NuGet packages"
	@echo "  build                      - Build the project"
	@echo "  ci-build                   - CI build (without restore step)"
	@echo "  clean                      - Clean build artifacts"
	@echo "  unit-tests                 - Run unit tests"
	@echo "  integration-tests          - Run integration tests"
	@echo "  end-to-end-tests           - Run end-to-end tests"
	@echo "  test                       - Run all tests (unit, integration, end-to-end)"
	@echo "  upgrade-packages           - Upgrade .NET packages"
	@echo "  run-docker-compose         - Run docker-compose"
	@echo "  stop-docker-compose        - Stop docker-compose"
	@echo "  check-format               - Check C# formatting"
	@echo "  check-style                - Check C# style rules"
	@echo "  check-analyzers            - Check C# analyzer rules"
	@echo "  fix-format                 - Fix formatting and stage changes"
	@echo "  fix-style                  - Fix style rules for all projects (error level)"
	@echo "  fix-style-warn             - Fix style rules (warn level)"
	@echo "  fix-style-info             - Fix style rules (info level)"
	@echo "  fix-analyzers              - Fix analyzer rules (error level)"
	@echo "  fix-analyzers-warn         - Fix analyzer rules (warn level)"
	@echo "  fix-analyzers-info         - Fix analyzer rules (info level)"
	@echo "  check-all                  - Run all validation checks"
	@echo "  fix-all                    - Run all fixes"
	@echo ""
	@echo "Variables:"
	@echo "  PROJECT_PATH - Path to project (default: \".\")"

# Configuration
SMART_CHARGING_API_PROJECT := src/Services/SmartChargingApi/SmartChargingApi.csproj
SMART_CHARGING_STATISTICS_API_PROJECT := src/Services/SmartChargingStatisticsApi/SmartChargingStatisticsApi.csproj
SOLUTION_ROOT := "."

# Install Husky and .NET tools
prepare:
	husky
	dotnet tool restore

# Install dev cert (Bash)
install-dev-cert:
	curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v vs2019 -l ~/vsdbg

# Run aspire dashboard
run:
	aspire run

# Run SmartChargingAPi
run-smart-charging-api: 
	dotnet run --project $(SMART_CHARGING_API_PROJECT)

# Run SmartChargingStatisticsAPi
run-smart-charging-statistics-api: 
	dotnet run --project $(SMART_CHARGING_STATISTICS_API_PROJECT)

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

# Upgrade .NET packages
upgrade-packages:
	dotnet outdated -u

# Run docker-compose
.PHONY: run-docker-compose
run-docker-compose:
	docker-compose -f .\deployments\docker-compose\docker-compose.yaml up -d    

# Stop docker-compose
.PHONY: stop-docker-compose
stop-docker-compose:
	docker-compose -f .\deployments\docker-compose\docker-compose.yaml down 

# Check C# formatting
check-format:
	dotnet csharpier check $(SOLUTION_ROOT)

# Check C# style rules
check-style:
	dotnet format style $(SOLUTION_ROOT) --verify-no-changes --severity error --verbosity diagnostic

# Check C# analyzer rules
check-analyzers:
	dotnet format analyzers $(SOLUTION_ROOT) --verify-no-changes --severity error --verbosity diagnostic

# Fix formatting and stage changes
fix-format:
	dotnet csharpier format $(SOLUTION_ROOT)

# Fix style rules for all projects (error level)
fix-style:
	dotnet format style $(SOLUTION_ROOT) --severity error --verbosity diagnostic

# Fix style rules (warn level)
fix-style-warn:
	dotnet format style $(SOLUTION_ROOT) --severity warn --verbosity diagnostic

# Fix style rules (info level)
fix-style-info:
	dotnet format style $(SOLUTION_ROOT) --severity info --verbosity diagnostic

# Fix analyzer rules (error level)
fix-analyzers:
	dotnet format analyzers $(SOLUTION_ROOT) --severity error --verbosity diagnostic

# Fix analyzer rules (warn level)
fix-analyzers-warn:
	dotnet format analyzers $(SOLUTION_ROOT) --severity warn --verbosity diagnostic

# Fix analyzer rules (info level)
fix-analyzers-info:
	dotnet format analyzers $(SOLUTION_ROOT) --severity info --verbosity diagnostic

# Run all validation checks
check-all: check-analyzers check-format check-style

# Run all fixes
fix-all: fix-analyzers fix-format fix-style