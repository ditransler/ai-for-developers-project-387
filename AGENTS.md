# Agent conventions

## Design First

We agree on system behavior and the **HTTP API contract before** writing application code. The contract is the **single source of truth** for both the client and the server: implement against the specification instead of inferring behavior from the other side’s internals.

When requirements change, **update the contract first**, then align the frontend and backend with that change.

## TypeSpec

The API contract is expressed in **TypeSpec**. TypeSpec is a DSL for describing APIs; it can **emit OpenAPI** and supports **code generation** for many languages. It is not a general-purpose programming language—only the contract and related metadata belong there.

- Contract sources: [`api-contract/main.tsp`](api-contract/main.tsp)
- Emitted OpenAPI (regenerate with `make tsp-openapi`): [`api-contract/openapi.yaml`](api-contract/openapi.yaml)

See [TypeSpec](https://typespec.io/).
