# Transaction Layer for Platform.Data.Doublets

This directory contains the implementation of a transaction layer for the Platform.Data.Doublets library, inspired by Neo4j's transaction model.

## Overview

The transaction layer provides ACID properties for link operations by implementing an in-memory transaction system that stores all changes as state transitions and applies them atomically during commit.

## Key Components

### 1. ITransactionalLinks<TLinkAddress>
Main interface that extends ILinks with transaction support:
- `BeginTransaction()` - Starts a new transaction
- `CommitTransaction()` - Commits the current transaction
- `RollbackTransaction()` - Rolls back the current transaction
- `CurrentTransaction` - Gets/sets the active transaction

### 2. ILinksTransaction<TLinkAddress>
Interface representing a single transaction:
- `StateTransitions` - Dictionary of all link state changes
- `RecordTransition()` - Records a state change
- `GetCurrentState()` - Gets current state of a link within transaction
- `HasConflictsWith()` - Detects conflicts with other transactions
- `Commit()` / `Rollback()` - Transaction finalization methods

### 3. LinkStateTransition<TLinkAddress>
Represents a single link state transition:
- `LinkAddress` - The link being modified
- `BeforeState` - State before the change
- `AfterState` - State after the change
- `TransitionType` - Type of operation (Create, Update, Delete)

### 4. TransactionalLinksDecorator<TLinkAddress>
Main implementation that wraps any ILinks implementation with transaction support:
- Intercepts all CRUD operations during transactions
- Stores changes in memory until commit
- Provides isolation by showing transaction view during reads
- Applies all changes atomically during commit with write locks

## How It Works

### Transaction Isolation
- **Read Operations**: When in a transaction, reads return the "transaction view" - if a link has been modified in the current transaction, return the modified state; otherwise, delegate to underlying storage.
- **Write Operations**: All writes are recorded as state transitions but not immediately applied to underlying storage.

### Conflict Detection
Conflicts occur when:
1. Two transactions modify the same link from different source states
2. Two transactions apply different target states to the same link

### Atomic Commit Process
1. Acquire write lock on underlying storage
2. Re-check for conflicts (in a full implementation)
3. Apply all recorded transitions in order
4. Release write lock
5. Mark transaction as committed

### State Transition Chaining
If a link is modified multiple times within the same transaction, the transitions are chained together, maintaining only the original "before" state and the final "after" state.

## Usage Example

```csharp
// Create transactional wrapper around any ILinks implementation
var links = new UnitedMemoryLinks<uint>(memory);
var transactionalLinks = new TransactionalLinksDecorator<uint>(links);

// Start transaction
var transaction = transactionalLinks.BeginTransaction();

try
{
    // Perform operations - these are recorded but not yet applied
    var link1 = transactionalLinks.CreatePoint();
    var link2 = transactionalLinks.CreateAndUpdate(link1, link1);
    transactionalLinks.Update(link2, link1, link2);
    
    // Commit all changes atomically
    transactionalLinks.CommitTransaction();
}
catch (Exception)
{
    // Rollback on error
    transactionalLinks.RollbackTransaction();
}
```

## Neo4j Inspiration

This implementation follows Neo4j's transaction principles:
- **Isolation**: Each transaction sees a consistent view of the data
- **Consistency**: Constraints are maintained across all operations
- **Atomicity**: All operations succeed or fail together
- **Durability**: Committed changes are persisted (delegated to underlying storage)

## Testing

The `TransactionalLinksTests` class provides comprehensive test coverage including:
- Transaction lifecycle management
- State transition recording
- Conflict detection
- Atomic commit/rollback behavior
- Transaction chaining scenarios

## Limitations and Future Improvements

### Current Limitations
- Nested transactions are not supported
- Conflict detection is basic (could be enhanced with timestamp-based or other advanced strategies)
- Read operations within transactions don't fully implement transaction isolation yet
- Temporary link address generation is simplified

### Potential Improvements
- Add support for nested transactions
- Implement more sophisticated conflict resolution strategies
- Add transaction timeout handling
- Implement read-time isolation for better consistency
- Add transaction logging and recovery mechanisms
- Optimize memory usage for large transactions
- Add distributed transaction support

## Thread Safety

The `TransactionalLinksDecorator` uses `ISynchronization` (default: `ReaderWriterLockSynchronization`) to ensure thread-safe commits. However, individual transactions are not thread-safe and should only be used from a single thread.