﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MoreDynamicQuery
{
    public static class MoreDynamicQueryExtensions
    {
        private static bool DynamicQueryContains(object source, object value)
        {
            if (source is string && value is string)
            {
                return source.ToString().Contains(value.ToString());
            }

            if (source is Array array)
            {
                return Array.IndexOf(array, value) >= 0;
            }

            if (source is IEnumerable)
            {
                return ((IEnumerable<object>)source).Contains(value);
            }
            return false;
        }

        private static bool DynamicQueryDoesNotContain(object source, object value)
        {
            if (source is string && value is string)
            {
                return !source.ToString().Contains(value.ToString());
            }

            if (source is Array array)
            {
                return !(Array.IndexOf(array, value) >= 0);
            }

            if (source is IEnumerable)
            {
                return !((IEnumerable<object>)source).Contains(value);
            }
            return false;
        }

        private static bool DynamicQueryDoesNotEndWith(object source, object value)
        {
            if (source is string && value is string)
            {
                return !source.ToString().EndsWith(value.ToString());
            }
            return false;
        }

        private static bool DynamicQueryDoesNotStartWith(object source, object value)
        {
            if (source is string && value is string)
            {
                return !source.ToString().StartsWith(value.ToString());
            }
            return false;
        }

        private static bool DynamicQueryEndsWith(object source, object value)
        {
            if (source is string && value is string)
            {
                return source.ToString().EndsWith(value.ToString());
            }
            return false;
        }

#pragma warning disable RCS1163 // Unused parameter.
        private static bool DynamicQueryIsNotNullOrWhiteSpace(object source, object value)
#pragma warning restore RCS1163 // Unused parameter.
        {
            if (source is string)
            {
                return !string.IsNullOrWhiteSpace(source?.ToString());
            }

            if (source is Array)
            {
                return source != null && ((Array)source).Length > 0;
            }

            if (source is IEnumerable)
            {
                return source != null && ((IEnumerable<object>)source).Any();
            }
            return false;
        }

#pragma warning disable RCS1163 // Unused parameter.
        private static bool DynamicQueryIsNullOrWhiteSpace(object source, object value)
#pragma warning restore RCS1163 // Unused parameter.
        {
            if (source is string)
            {
                return string.IsNullOrWhiteSpace(source?.ToString());
            }

            if (source is Array)
            {
                return source == null || ((Array)source).Length == 0;
            }

            if (source is IEnumerable)
            {
                return source == null || !((IEnumerable<object>)source).Any();
            }
            return false;
        }

#pragma warning disable RCS1163 // Unused parameter.
        private static bool DynamicQueryIsNotNullOrEmpty(object source, object value)
#pragma warning restore RCS1163 // Unused parameter.
        {
            if (source is string)
            {
                return !string.IsNullOrEmpty(source?.ToString());
            }

            if (source is Array)
            {
                return source != null && ((Array)source).Length > 0;
            }

            if (source is IEnumerable)
            {
                return source != null && ((IEnumerable<object>)source).Any();
            }
            return false;
        }

#pragma warning disable RCS1163 // Unused parameter.
        private static bool DynamicQueryIsNullOrEmpty(object source, object value)
#pragma warning restore RCS1163 // Unused parameter.
        {
            if (source is string)
            {
                return string.IsNullOrEmpty(source?.ToString());
            }

            if (source is Array)
            {
                return source == null || ((Array)source).Length == 0;
            }

            if (source is IEnumerable)
            {
                return source == null || !((IEnumerable<object>)source).Any();
            }
            return false;
        }

        private static bool DynamicQueryLike(object source, object value)
        {
            return
                DynamicQueryStartsWith(source, value) ||
                DynamicQueryContains(source, value) ||
                DynamicQueryEndsWith(source, value);
        }

        private static bool DynamicQueryNotLike(object source, object value)
        {
            return
                !DynamicQueryStartsWith(source, value) &&
                !DynamicQueryContains(source, value) &&
                !DynamicQueryEndsWith(source, value);
        }

        private static bool DynamicQueryStartsWith(object source, object value)
        {
            if (source is string && value is string)
            {
                return source.ToString().StartsWith(value.ToString());
            }
            return false;
        }

        public static IQueryable<TModel> DynamicWhere<TModel>(this IQueryable<TModel> queryable, params DynamicFilter[] dynamicFilters)
        {
            return queryable.Where(Filter<TModel>(dynamicFilters));
        }
        public static IQueryable<TModel> DynamicWhere<TModel>(this TModel[] array, params DynamicFilter[] dynamicFilters)
        {
            return array.AsQueryable().Where(Filter<TModel>(dynamicFilters));
        }
        public static IQueryable<TModel> DynamicWhere<TModel>(this IEnumerable<TModel> enumerable, params DynamicFilter[] dynamicFilters)
        {
            return enumerable.AsQueryable().DynamicWhere(dynamicFilters);
        }

        public static IQueryable<TModel> DynamicWhere<TModel>(this IQueryable<TModel> queryable, IEnumerable<DynamicFilter> dynamicFilters)
        {
            return queryable.Where(Filter<TModel>(dynamicFilters));
        }

        public static IQueryable<TModel> DynamicWhere<TModel>(this IEnumerable<TModel> enumerable, IEnumerable<DynamicFilter> dynamicFilters)
        {
            return enumerable.AsQueryable().DynamicWhere(dynamicFilters);
        }

        public static IQueryable<TModel> DynamicWhere<TModel>(this TModel[] array, IEnumerable<DynamicFilter> dynamicFilters)
        {
            return array.AsQueryable().DynamicWhere(dynamicFilters);
        }

        private static Expression<Func<TModel, bool>> Filter<TModel>(IEnumerable<DynamicFilter> dynamicModel)
        {
            Expression<Func<TModel, bool>> result = _ => true;
            foreach (var item in dynamicModel)
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TModel));
                MemberExpression memberExpression = Expression.Property(parameterExpression, item.PropertyName);
                ConstantExpression constantExpression = Expression.Constant(item.PropertyValue);
                BinaryExpression comparison = GetBinaryExpression(item.ComparisonFilter, memberExpression, constantExpression);
                var expression = Expression.Lambda<Func<TModel, bool>>(comparison, parameterExpression);
                var param = Expression.Parameter(typeof(TModel), "x");
                var body = Expression.AndAlso(
                            Expression.Invoke(result, param),
                            Expression.Invoke(expression, param)
                        );
                result = Expression.Lambda<Func<TModel, bool>>(body, param);
            }
            return result;
        }

        private static BinaryExpression GetBinaryExpression(ComparisonFilter comparisonMethod, MemberExpression memberExpression, ConstantExpression constantExpression)
        {
            switch (comparisonMethod)
            {
                case ComparisonFilter.Equal:
                    return Expression.Equal(memberExpression, constantExpression);
                case ComparisonFilter.LessThan:
                    return Expression.LessThan(memberExpression, constantExpression);
                case ComparisonFilter.GreaterThan:
                    return Expression.GreaterThan(memberExpression, constantExpression);
                case ComparisonFilter.NotEqual:
                    return Expression.NotEqual(memberExpression, constantExpression);
                case ComparisonFilter.GreaterThanEqual:
                    return Expression.GreaterThanOrEqual(memberExpression, constantExpression);
                case ComparisonFilter.LessThanEqual:
                    return Expression.LessThanOrEqual(memberExpression, constantExpression);
                case ComparisonFilter.IsNullOrEmpty:
                    MethodInfo isNullOrEmptyMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryIsNullOrEmpty), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, isNullOrEmptyMethod);
                case ComparisonFilter.IsNotNullOrEmpty:
                    MethodInfo isNotNullOrEmptyMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryIsNotNullOrEmpty), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, isNotNullOrEmptyMethod);
                case ComparisonFilter.IsNullOrWhiteSpace:
                    MethodInfo isNullOrWhiteSpaceMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryIsNullOrWhiteSpace), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, isNullOrWhiteSpaceMethod);
                case ComparisonFilter.IsNotNullOrWhiteSpace:
                    MethodInfo isNotNullOrWhiteSpaceMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryIsNotNullOrWhiteSpace), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, isNotNullOrWhiteSpaceMethod);
                case ComparisonFilter.Contains:
                    MethodInfo containsMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryContains), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, containsMethod);
                case ComparisonFilter.DoesNotContain:
                    MethodInfo doesNotContainMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryDoesNotContain), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, doesNotContainMethod);
                case ComparisonFilter.StartsWith:
                    MethodInfo startsWithMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryStartsWith), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, startsWithMethod);
                case ComparisonFilter.DoesNotStartWith:
                    MethodInfo doesNotStartWithMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryDoesNotStartWith), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, doesNotStartWithMethod);
                case ComparisonFilter.EndsWith:
                    MethodInfo endsWithMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryEndsWith), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, endsWithMethod);
                case ComparisonFilter.DoesNotEndWith:
                    MethodInfo doesNotEndWithMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryDoesNotEndWith), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, doesNotEndWithMethod);
                case ComparisonFilter.Like:
                    MethodInfo likeMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryLike), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, likeMethod);
                case ComparisonFilter.NotLike:
                    MethodInfo notLikeMethod = typeof(MoreDynamicQueryExtensions).GetMethod(nameof(DynamicQueryNotLike), BindingFlags.NonPublic | BindingFlags.Static);
                    return Expression.MakeBinary(ExpressionType.Equal, memberExpression, constantExpression, false, notLikeMethod);
                default:
                    return null;
            }
        }
    }
}
