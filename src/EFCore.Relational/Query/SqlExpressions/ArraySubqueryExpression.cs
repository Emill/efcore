// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents projecting an array SQL value from a subquery.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ArraySubqueryExpression : ScalarSubqueryExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ArraySubqueryExpression" /> class.
        /// </summary>
        /// <param name="subquery"> A subquery projecting multiple rows with a single scalar projection. </param>
        public ArraySubqueryExpression([NotNull] SelectExpression subquery)
            : base(subquery)
        {
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="subquery"> The <see cref="ScalarSubqueryExpression.Subquery" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public override ArraySubqueryExpression Update([NotNull] SelectExpression subquery)
        {
            Check.NotNull(subquery, nameof(subquery));

            return subquery != Subquery
                ? new ArraySubqueryExpression(subquery)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("ARRAY");
            base.Print(expressionPrinter);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ArraySubqueryExpression arraySubqueryExpression
                    && Equals(arraySubqueryExpression));

        private bool Equals(ArraySubqueryExpression arraySubqueryExpression)
            => base.Equals(arraySubqueryExpression)
                && Subquery.Equals(arraySubqueryExpression.Subquery);

        /// <inheritdoc />
        public override int GetHashCode()
            => base.GetHashCode();
    }
}
