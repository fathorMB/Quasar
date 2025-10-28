# Outbox & Inbox Phase 2

## Overview

- Added background dispatch and inbox cleanup services with configurable polling and retention settings.
- Introduced transport abstractions via IOutboxPublisher, including Kafka and RabbitMQ implementations with DI helpers.
- Expanded EF Core store to surface pending messages, track delivery attempts, and respect retry limits.
- Updated unit tests to cover dispatch flow, store filtering, and renamed EF Core package structure.

## Next Steps

- Wire dispatcher and publishers into sample applications, selecting transports and destinations via configuration.
- Add health checks and metrics emitting (e.g., IMeterFactory, Prometheus) for dispatcher success/failure rates.
- Implement dead-letter/parking storage for messages exceeding retry thresholds and expose administrative tooling.

