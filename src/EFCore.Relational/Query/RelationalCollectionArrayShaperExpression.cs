// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents creation of an array collection for relational provider in
    ///         <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalCollectionArrayShaperExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalCollectionShaperExpression" /> class.
        /// </summary>
        /// <param name="collectionId"> A unique id for the collection being shaped. </param>
        /// <param name="outerProjection"> An outer projection binding that maps the array location. </param>
        /// <param name="innerSelectExpression"> An expression where elements will be fetched from. </param>
        /// <param name="innerShaper"> An expression used to create individual elements of the collection. </param>
        /// <param name="navigation"> A navigation associated with this collection, if any. </param>
        /// <param name="elementType"> The clr type of individual elements in the collection. </param>
        public RelationalCollectionArrayShaperExpression(
            int collectionId,
            [NotNull] ProjectionBindingExpression outerProjection,
            [NotNull] SelectExpression innerSelectExpression,
            [NotNull] Expression innerShaper,
            [CanBeNull] INavigationBase? navigation,
            [NotNull] Type elementType)
        {
            Check.NotNull(outerProjection, nameof(outerProjection));
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));
            Check.NotNull(innerShaper, nameof(innerShaper));
            Check.NotNull(elementType, nameof(elementType));

            CollectionId = collectionId;
            OuterProjection = outerProjection;
            InnerSelectExpression = innerSelectExpression;
            InnerShaper = innerShaper;
            Navigation = navigation;
            ElementType = elementType;
        }

        /// <summary>
        ///     A unique id for this collection shaper.
        /// </summary>
        public virtual int CollectionId { get; }

        /// <summary>
        ///     The outer projection binding that maps the array location.
        /// </summary>
        public virtual ProjectionBindingExpression OuterProjection { get; }

        /// <summary>
        ///     The expression where elements will be fetched from.
        /// </summary>
        public virtual SelectExpression InnerSelectExpression { get; }

        /// <summary>
        ///     The expression to create inner elements.
        /// </summary>
        public virtual Expression InnerShaper { get; }

        /// <summary>
        ///     The navigation if associated with the collection.
        /// </summary>
        public virtual INavigationBase? Navigation { get; }

        /// <summary>
        ///     The clr type of elements of the collection.
        /// </summary>
        public virtual Type ElementType { get; }

        /// <inheritdoc />
        public override Type Type
            => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var innerShaper = visitor.Visit(InnerShaper);

            return Update(innerShaper);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
         /// <param name="innerShaper"> The <see cref="InnerShaper" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual RelationalCollectionArrayShaperExpression Update(
            [NotNull] Expression innerShaper)
        {
            Check.NotNull(innerShaper, nameof(innerShaper));

            return innerShaper != InnerShaper
                    ? new RelationalCollectionArrayShaperExpression(
                        CollectionId, OuterProjection, InnerSelectExpression, innerShaper, Navigation, ElementType)
                    : this;
        }

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("RelationalCollectionArrayShaper:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine($"CollectionId: {CollectionId}");
                expressionPrinter.Append("InnerShaper:");
                expressionPrinter.Visit(InnerShaper);
                expressionPrinter.AppendLine();
                expressionPrinter.AppendLine($"Navigation: {Navigation?.Name}");
            }
        }
    }
}
