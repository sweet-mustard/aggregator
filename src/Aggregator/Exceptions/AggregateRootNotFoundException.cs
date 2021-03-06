using System;
using Aggregator.Persistence;

namespace Aggregator.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="IRepository{TIdentifier, TEventBase, TAggregateRoot}"/> when the requested aggregate root was not found.
    /// </summary>
    /// <typeparam name="TIdentifier">The identifier type.</typeparam>
    public class AggregateRootNotFoundException<TIdentifier> : AggregateRootException<TIdentifier>
        where TIdentifier : IEquatable<TIdentifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootNotFoundException{TIdentifier}"/> class.
        /// </summary>
        /// <param name="identifier">The aggregate root identifier.</param>
        public AggregateRootNotFoundException(TIdentifier identifier)
            : base(identifier, "Aggregate root not found")
        {
        }
    }
}
