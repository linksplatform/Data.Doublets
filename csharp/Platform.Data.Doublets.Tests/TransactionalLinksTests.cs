using System;
using System.Collections.Generic;
using Xunit;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Transactions;
using Platform.Memory;

namespace Platform.Data.Doublets.Tests
{
    public class TransactionalLinksTests
    {
        [Fact]
        public void BeginTransaction_CreatesNewTransaction()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);

            // Act
            var transaction = transactionalLinks.BeginTransaction();

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(transaction, transactionalLinks.CurrentTransaction);
            Assert.False(transaction.IsCommitted);
            Assert.False(transaction.IsRolledBack);
        }

        [Fact]
        public void BeginTransaction_WhenTransactionActive_ThrowsException()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);
            transactionalLinks.BeginTransaction();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => transactionalLinks.BeginTransaction());
        }

        [Fact]
        public void CommitTransaction_WhenNoTransaction_ThrowsException()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => transactionalLinks.CommitTransaction());
        }

        [Fact]
        public void RollbackTransaction_WhenNoTransaction_ThrowsException()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => transactionalLinks.RollbackTransaction());
        }

        [Fact]
        public void CreateInTransaction_RecordsTransition()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);
            var transaction = transactionalLinks.BeginTransaction();

            // Act
            var newLink = transactionalLinks.Create(new uint[] { 0, 1, 2 }, null);

            // Assert
            Assert.Single(transaction.StateTransitions);
            Assert.True(transaction.StateTransitions.ContainsKey(newLink));
            var transition = transaction.StateTransitions[newLink];
            Assert.True(transition.IsCreation);
            Assert.Null(transition.BeforeState);
            Assert.NotNull(transition.AfterState);
        }

        [Fact]
        public void UpdateInTransaction_RecordsTransition()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);
            
            // Create a link outside transaction first
            var linkId = links.CreatePoint();
            
            var transaction = transactionalLinks.BeginTransaction();

            // Act
            transactionalLinks.Update(new uint[] { linkId, 1, 2 }, new uint[] { linkId, 3, 4 }, null);

            // Assert
            Assert.Single(transaction.StateTransitions);
            Assert.True(transaction.StateTransitions.ContainsKey(linkId));
            var transition = transaction.StateTransitions[linkId];
            Assert.True(transition.IsUpdate);
            Assert.NotNull(transition.BeforeState);
            Assert.NotNull(transition.AfterState);
        }

        [Fact]
        public void DeleteInTransaction_RecordsTransition()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);
            
            // Create a link outside transaction first
            var linkId = links.CreatePoint();
            
            var transaction = transactionalLinks.BeginTransaction();

            // Act
            transactionalLinks.Delete(new uint[] { linkId }, null);

            // Assert
            Assert.Single(transaction.StateTransitions);
            Assert.True(transaction.StateTransitions.ContainsKey(linkId));
            var transition = transaction.StateTransitions[linkId];
            Assert.True(transition.IsDeletion);
            Assert.NotNull(transition.BeforeState);
            Assert.Null(transition.AfterState);
        }

        [Fact]
        public void ConflictDetection_SameLinkDifferentStates_ReturnsTrue()
        {
            // Arrange
            var transaction1 = new LinksTransaction<uint>();
            var transaction2 = new LinksTransaction<uint>();

            var linkId = 1u;
            var beforeState = new List<uint> { linkId, 1, 2 };
            var afterState1 = new List<uint> { linkId, 3, 4 };
            var afterState2 = new List<uint> { linkId, 5, 6 };

            // Act
            transaction1.RecordTransition(linkId, beforeState, afterState1);
            transaction2.RecordTransition(linkId, beforeState, afterState2);

            // Assert
            Assert.True(transaction1.HasConflictsWith(transaction2));
            Assert.True(transaction2.HasConflictsWith(transaction1));
        }

        [Fact]
        public void ConflictDetection_SameLinkSameStates_ReturnsFalse()
        {
            // Arrange
            var transaction1 = new LinksTransaction<uint>();
            var transaction2 = new LinksTransaction<uint>();

            var linkId = 1u;
            var beforeState = new List<uint> { linkId, 1, 2 };
            var afterState = new List<uint> { linkId, 3, 4 };

            // Act
            transaction1.RecordTransition(linkId, beforeState, afterState);
            transaction2.RecordTransition(linkId, beforeState, afterState);

            // Assert
            Assert.False(transaction1.HasConflictsWith(transaction2));
            Assert.False(transaction2.HasConflictsWith(transaction1));
        }

        [Fact]
        public void ConflictDetection_DifferentLinks_ReturnsFalse()
        {
            // Arrange
            var transaction1 = new LinksTransaction<uint>();
            var transaction2 = new LinksTransaction<uint>();

            var linkId1 = 1u;
            var linkId2 = 2u;
            var beforeState = new List<uint> { 0, 1, 2 };
            var afterState = new List<uint> { 0, 3, 4 };

            // Act
            transaction1.RecordTransition(linkId1, beforeState, afterState);
            transaction2.RecordTransition(linkId2, beforeState, afterState);

            // Assert
            Assert.False(transaction1.HasConflictsWith(transaction2));
            Assert.False(transaction2.HasConflictsWith(transaction1));
        }

        [Fact]
        public void CommitTransaction_AppliesChangesToUnderlyingStorage()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);

            var initialCount = links.Count();
            var transaction = transactionalLinks.BeginTransaction();

            // Act - Create a point in transaction
            transactionalLinks.CreatePoint();
            
            // Verify link not yet in underlying storage
            Assert.Equal(initialCount, links.Count());

            // Commit transaction
            transactionalLinks.CommitTransaction();

            // Assert - Changes should now be applied
            Assert.True(links.Count() > initialCount);
            Assert.Null(transactionalLinks.CurrentTransaction);
        }

        [Fact]
        public void RollbackTransaction_DiscardsChanges()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var transactionalLinks = new TransactionalLinksDecorator<uint>(links);

            var initialCount = links.Count();
            var transaction = transactionalLinks.BeginTransaction();

            // Act - Create a point in transaction
            transactionalLinks.CreatePoint();
            
            // Rollback transaction
            transactionalLinks.RollbackTransaction();

            // Assert - No changes should be applied
            Assert.Equal(initialCount, links.Count());
            Assert.Null(transactionalLinks.CurrentTransaction);
        }

        [Fact]
        public void TransactionChaining_MultipleOperationsSameLink_MergesCorrectly()
        {
            // Arrange
            var transaction = new LinksTransaction<uint>();
            var linkId = 1u;
            var originalState = new List<uint> { linkId, 1, 2 };
            var intermediateState = new List<uint> { linkId, 3, 4 };
            var finalState = new List<uint> { linkId, 5, 6 };

            // Act - Chain multiple operations on the same link
            transaction.RecordTransition(linkId, originalState, intermediateState);
            transaction.RecordTransition(linkId, intermediateState, finalState);

            // Assert - Should have single transition from original to final state
            Assert.Single(transaction.StateTransitions);
            var transition = transaction.StateTransitions[linkId];
            Assert.Equal(originalState, transition.BeforeState);
            Assert.Equal(finalState, transition.AfterState);
        }

        [Fact]
        public void TransactionDispose_AutomaticallyRollsBack()
        {
            // Arrange
            var transaction = new LinksTransaction<uint>();

            // Act
            transaction.Dispose();

            // Assert
            Assert.True(transaction.IsRolledBack);
            Assert.False(transaction.IsCommitted);
        }
    }
}