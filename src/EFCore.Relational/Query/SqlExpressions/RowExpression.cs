// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a ROW constructor in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RowExpression : SqlExpression
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RowExpression" /> class.
        /// </summary>
        /// <param name="arguments">The arguments for the row.</param>
        public RowExpression([NotNull] IEnumerable<SqlExpression> arguments)
            : base(typeof(object[]), null)
        {
            Arguments = arguments.ToList();
        }

        /// <summary>
        ///     The list of arguments of this function.
        /// </summary>
        public virtual IReadOnlyList<SqlExpression> Arguments { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var arguments = new SqlExpression[Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
                changed |= arguments[i] != Arguments[i];
            }

            return changed ? new RowExpression(arguments) : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="arguments"> The <see cref="Arguments" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual RowExpression Update([CanBeNull] IReadOnlyList<SqlExpression>? arguments)
        {
            return (arguments != null && Arguments != null && !arguments.SequenceEqual(Arguments))
                ? new RowExpression(arguments)
                : this;
        }

        /// <inheritdoc />
        protected override void Print([NotNull] ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("ROW(");
            expressionPrinter.VisitCollection(Arguments);
            expressionPrinter.Append(")");
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is RowExpression rowExpression
                    && Equals(rowExpression));

        private bool Equals(RowExpression rowExpression)
            => base.Equals(rowExpression)
                && ((Arguments == null && rowExpression.Arguments == null)
                    || (Arguments != null && rowExpression.Arguments != null
                        && Arguments.SequenceEqual(rowExpression.Arguments)));

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());

            if (Arguments != null)
            {
                for (var i = 0; i < Arguments.Count; i++)
                {
                    hash.Add(Arguments[i]);
                }
            }

            return hash.ToHashCode();
        }
    }
}
