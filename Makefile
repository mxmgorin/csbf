# Makefile for the csbf Blazor WebAssembly playground (src/Csbf.Web).
# dotnet here lives under ~/.dotnet; if it's on your PATH instead, run e.g.
#   make serve DOTNET=dotnet

DOTNET_ROOT ?= $(HOME)/.dotnet
DOTNET      ?= $(DOTNET_ROOT)/dotnet
export DOTNET_ROOT

WEB         := src/Csbf.Web/Csbf.Web.csproj
PUBLISH_DIR := publish
BASE_HREF   := /csbf/
PROFILE     := http

.DEFAULT_GOAL := help
.PHONY: help serve build publish clean wasm-tools

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-12s\033[0m %s\n", $$1, $$2}'

serve: ## Run the dev server at http://localhost:5006 (Ctrl+C to stop)
	$(DOTNET) run --project $(WEB) --launch-profile $(PROFILE)

build: ## Build the web app (Release)
	$(DOTNET) build $(WEB) -c Release

publish: ## Publish the GitHub Pages artifact to ./publish (rewrites base href, adds .nojekyll)
	$(DOTNET) publish $(WEB) -c Release -o $(PUBLISH_DIR)
	sed -i 's|<base href="/" />|<base href="$(BASE_HREF)" />|' $(PUBLISH_DIR)/wwwroot/index.html
	touch $(PUBLISH_DIR)/wwwroot/.nojekyll
	@echo "-> published to $(PUBLISH_DIR)/wwwroot (base href $(BASE_HREF))"

wasm-tools: ## Install the wasm-tools workload (one-time prerequisite)
	$(DOTNET) workload install wasm-tools

clean: ## Remove build output and the publish dir
	rm -rf $(PUBLISH_DIR) src/Csbf.Web/bin src/Csbf.Web/obj
	@echo "-> cleaned"
