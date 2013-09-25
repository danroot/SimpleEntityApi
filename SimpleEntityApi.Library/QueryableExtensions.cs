using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleEntityApi
{
    public static class QueryableExtensions
    {
        public static IQueryable<TSource> WhereKey<TSource, TKey>(this IQueryable<TSource> query, TKey key)
        {

            var type = typeof (TSource);
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var keyProps =
                props.Where(
                    p =>
                        p.CustomAttributes.Any(
                            a => a.AttributeType == typeof (System.ComponentModel.DataAnnotations.KeyAttribute)) ||
                        p.Name == "Id");


            ParameterExpression[] baseTypeParams = new[] {Expression.Parameter(typeof (TSource), "")};



            foreach (var prop in keyProps)
            {

                var equalsMethod = prop.PropertyType.GetMethod("Equals", new Type[] {prop.PropertyType});
                var whereExpression
                    = (Expression<Func<TSource, bool>>) Expression.Lambda(
                        Expression.Call(
                            Expression.Property(baseTypeParams[0], prop.Name),
                            equalsMethod,
                            Expression.Constant(key)), baseTypeParams
                        );


                query = query.Where(whereExpression);
            }
            return query;
        }


        public static IQueryable<TSource> WhereKeysMatch<TSource>(this IQueryable<TSource> query, TSource other)
        {

            var type = typeof (TSource);
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var keyProps =
                props.Where(
                    p =>
                        p.CustomAttributes.Any(
                            a => a.AttributeType == typeof (System.ComponentModel.DataAnnotations.KeyAttribute)) ||
                        p.Name == "Id");


            ParameterExpression[] baseTypeParams = new[] {Expression.Parameter(typeof (TSource), "")};



            foreach (var prop in keyProps)
            {
                var key = prop.GetValue(other);
                var equalsMethod = prop.PropertyType.GetMethod("Equals", new Type[] {prop.PropertyType});
                var whereExpression
                    = (Expression<Func<TSource, bool>>) Expression.Lambda(
                        Expression.Call(
                            Expression.Property(baseTypeParams[0], prop.Name),
                            equalsMethod,
                            Expression.Constant(key)), baseTypeParams
                        );


                query = query.Where(whereExpression);
            }
            return query;
        }
    }
}