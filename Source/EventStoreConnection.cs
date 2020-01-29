// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Lifecycle;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Store.MongoDB.Aggregates;
using Dolittle.Runtime.Events.Store.MongoDB.EventLog;
using MongoDB.Driver;

namespace Dolittle.Runtime.Events.Store.MongoDB
{
    /// <summary>
    /// Represents a connection to the MongoDB EventStore database.
    /// </summary>
    [SingletonPerTenant]
    public class EventStoreConnection
    {
        readonly DatabaseConnection _connection;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStoreConnection"/> class.
        /// </summary>
        /// <param name="connection">A connection to the MongoDB database.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public EventStoreConnection(DatabaseConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;

            MongoClient = connection.MongoClient;

            EventLog = connection.Database.GetCollection<Event>(Constants.EventLogCollection);
            Aggregates = connection.Database.GetCollection<AggregateRoot>(Constants.AggregateRootInstanceCollection);

            CreateCollectionsAndIndexes();
        }

        /// <summary>
        /// Gets the <see cref="IMongoClient"/> configured for the MongoDB database.
        /// </summary>
        public IMongoClient MongoClient { get; }

        /// <summary>
        /// Gets the <see cref="IMongoCollection{Event}"/> where Events are stored.
        /// </summary>
        public IMongoCollection<Event> EventLog { get; }

        /// <summary>
        /// Gets the <see cref="IMongoCollection{AggregateRoot}"/> where Aggregate Roots are stored.
        /// </summary>
        public IMongoCollection<AggregateRoot> Aggregates { get; }

        void CreateCollectionsAndIndexes()
        {
            CreateCollectionsAndIndexesForEventLog();
            CreateCollectionsAndIndexesForAggregates();
        }

        void CreateCollectionsAndIndexesForEventLog()
        {
            EventLog.Indexes.CreateOne(new CreateIndexModel<Event>(
                Builders<Event>.IndexKeys
                    .Ascending(_ => _.Aggregate.EventSourceId)));

            EventLog.Indexes.CreateOne(new CreateIndexModel<Event>(
                Builders<Event>.IndexKeys
                    .Ascending(_ => _.Aggregate.EventSourceId)
                    .Ascending(_ => _.Aggregate.TypeId)));
        }

        void CreateCollectionsAndIndexesForAggregates()
        {
            Aggregates.Indexes.CreateOne(new CreateIndexModel<AggregateRoot>(
                Builders<AggregateRoot>.IndexKeys
                    .Ascending(_ => _.EventSource)
                    .Ascending(_ => _.AggregateType),
                new CreateIndexOptions
                {
                    Unique = true,
                }));
        }
    }
}