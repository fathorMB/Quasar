# Outbox & Inbox Phase 1

## Overview

- Introduced core abstractions for outbox and inbox messaging inside `Quasar.EventSourcing.Abstractions`.
- Added optional integration to `EventSourcedRepository` so domain events are captured when outbox services are registered.
- Provided a default outbox message factory and DI helpers in `Quasar.EventSourcing.Outbox`.
- Implemented EF Core backed stores plus service registration helpers in `Quasar.EventSourcing.Outbox.EntityFramework`.
- Extended web DI bootstrapper to register the default message factory and preserve backwards compatibility.

## Next Steps

- Wire concrete transport publishers (Kafka, RabbitMQ) to consume persisted messages.
- Add hosted services for dispatching the outbox queue and for inbox cleanup routines.
- Expand documentation and samples to showcase configuration patterns.

