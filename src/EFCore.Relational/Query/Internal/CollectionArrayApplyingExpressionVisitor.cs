// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CollectionArrayApplyingExpressionVisitor : ExpressionVisitor
    {
        private readonly bool _noConfiguredBehavior;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private int _collectionId;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CollectionArrayApplyingExpressionVisitor([NotNull] RelationalQueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _noConfiguredBehavior = queryCompilationContext.QuerySplittingBehavior == null;
            _logger = queryCompilationContext.Logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            if (extensionExpression is CollectionShaperExpression collectionShaperExpression)
            {
                var collectionId = _collectionId++;

                var projectionBindingExpression = (ProjectionBindingExpression)collectionShaperExpression.Projection;
                var selectExpression = (SelectExpression)projectionBindingExpression.QueryExpression;

                var innerShaper = Visit(collectionShaperExpression.InnerShaper);

                var collectionJoin = selectExpression.ApplyCollectionArray(
                    projectionBindingExpression.Index!.Value,
                    collectionId,
                    innerShaper,
                    collectionShaperExpression.Navigation,
                    collectionShaperExpression.ElementType);

                return collectionJoin!;
            }

            return extensionExpression is ShapedQueryExpression shapedQueryExpression
                ? shapedQueryExpression.UpdateShaperExpression(Visit(shapedQueryExpression.ShaperExpression))
                : base.VisitExtension(extensionExpression);
        }
    }
}
