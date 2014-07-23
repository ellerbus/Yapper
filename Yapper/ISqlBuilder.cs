using System;
using System.Dynamic;
using System.Linq.Expressions;
using Yapper.Dialects;

namespace Yapper
{
    #region Core

    /// <summary>
    /// Provides the interface that generates a SQL Statement, and corresponding Parameters
    /// </summary>
    public interface ISqlQuery
    {
        /// <summary>
        /// A fully constructed usable SQL statement written against the corresponding
        /// <see cref="ISqlDialect"/>
        /// </summary>
        string Query { get; }

        /// <summary>
        /// An <see cref="ExpandoObject"/> that represents the parameters used by the
        /// SQL Statement that was generated
        /// </summary>
        ExpandoObject Parameters { get; }
    }

    /// <summary>
    /// Provides a Fluent interface for starting a SQL Where clause
    /// </summary>
    /// <typeparam name="S">Container to this interface</typeparam>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface IWhereBuilder<S, T>
    {
        /// <summary>
        /// Creates a Where clause using the public properties of an object
        /// either concrete or anonymous
        /// </summary>
        /// <param name="where">An <see cref="Object"/> that represents a Where clause</param>
        /// <returns>The container to this interface</returns>
        S Where(object where);

        /// <summary>
        /// Creates a Where clause using the an expression
        /// </summary>
        /// <param name="expression">An expression to generate a Where clause</param>
        /// <returns>The container to this interface</returns>
        S Where(Expression<Func<T, bool>> expression);
    }

    /// <summary>
    /// Provides a Fluent interface for appending to a SQL Where clause
    /// </summary>
    /// <typeparam name="S">Container to this interface</typeparam>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface IWhereAndOrBuilder<S, T>
    {
        /// <summary>
        /// Appends the Where clause using the *and* operator using the public
        /// properties of an object either concrete or anonymous
        /// </summary>
        /// <param name="where">An <see cref="Object"/> that represents a Where clause</param>
        /// <returns>The container to this interface</returns>
        S And(object where);

        /// <summary>
        /// Appends the Where clause using the *and* operator using an expression
        /// </summary>
        /// <param name="expression">An expression to generate a Where clause</param>
        /// <returns>The container to this interface</returns>
        S And(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Appends the Where clause using the *or* operator using the public
        /// properties of an object either concrete or anonymous
        /// </summary>
        /// <param name="where">An <see cref="Object"/> that represents a Where clause</param>
        /// <returns>The container to this interface</returns>
        S Or(object where);

        /// <summary>
        /// Appends the Where clause using the *or* operator using an expression
        /// </summary>
        /// <param name="expression">An expression to generate a Where clause</param>
        /// <returns>The container to this interface</returns>
        S Or(Expression<Func<T, bool>> expression);
    }

    #endregion

    #region Update

    /// <summary>
    /// Represents a Fluent interface to start an Update statement
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface IUpdateBuilder<T> : ISqlQuery,
        IWhereBuilder<IUpdateWhereAndOrBuilder<T>, T>
    {
        /// <summary>
        /// Creates a Set clause using the public properties of an object
        /// either concrete or anonymous
        /// </summary>
        /// <param name="values">An <see cref="Object"/> that represents a Set clause</param>
        /// <returns>An instance of <see cref="IUpdateBuilder{T}"/></returns>
        IUpdateBuilder<T> Set(object values);
        /// <summary>
        /// Creates a Set clause using an expression
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="field">The column to assign the resulting expression</param>
        /// <param name="expression">An expression to generate a Set clause</param>
        /// <returns>An instance of <see cref="IUpdateBuilder{T}"/></returns>
        IUpdateBuilder<T> Set<K, V>(Expression<Func<T, K>> field, Expression<Func<T, V>> expression);
    }

    /// <summary>
    /// Represents a Fluent interface to provide a Where clause to an Update statement
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface IUpdateWhereAndOrBuilder<T> : ISqlQuery,
        IWhereAndOrBuilder<IUpdateWhereAndOrBuilder<T>, T>
    {
    }

    #endregion

    #region Delete

    /// <summary>
    /// Represents a Fluent interface to start a Delete statement
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface IDeleteBuilder<T> : ISqlQuery,
        IWhereBuilder<IDeleteAndOrBuilder<T>, T>
    {
    }

    /// <summary>
    /// Represents a Fluent interface to provide a Where clause to a Delete statement
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface IDeleteAndOrBuilder<T> : ISqlQuery,
        IWhereAndOrBuilder<IDeleteAndOrBuilder<T>, T>
    {
    }

    #endregion

    #region Select

    /// <summary>
    /// Represents a Fluent interface to start a Select statement
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface ISelectBuilder<T> : ISqlQuery,
        IWhereBuilder<ISelectAndOrOrderByBuilder<T>, T>,
        ISelectOrderByBuilder<T>,
        ISelectPageBuilder<T>
    {
        /// <summary>
        /// Represents a generic method to provide a limited subset of a Select statement
        /// given a <see cref="ISqlDialect"/>
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        ISelectBuilder<T> Top(int top);
        /// <summary>
        /// Specifically identifies a single column to be returned via a Select statement
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="select">Member of T that represents the column to Select</param>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Column<K>(Expression<Func<T, K>> select);
        /// <summary>
        /// Specifically identifies a single column to be aggreageated to determine the MAX
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="max">Essentially the property to assign the resulting value to</param>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Max<K>(Expression<Func<T, K>> max);
        /// <summary>
        /// Specifically identifies a single column to be aggreageated to determine the MIN
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="min">Essentially the property to assign the resulting value to</param>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Min<K>(Expression<Func<T, K>> min);
        /// <summary>
        /// Specifically identifies a single column to be aggreageated to determine the AVG
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="avg">Essentially the property to assign the resulting value to</param>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Avg<K>(Expression<Func<T, K>> avg);
        /// <summary>
        /// Specifically identifies a single column to be aggreageated to determine the SUM
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="sum">Essentially the property to assign the resulting value to</param>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Sum<K>(Expression<Func<T, K>> sum);
        /// <summary>
        /// Specifically identifies a single column to be aggreageated to determine the COUNT
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="count">Essentially the property to assign the resulting value to</param>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Count<K>(Expression<Func<T, K>> count);
        /// <summary>
        /// Usefull for counting all items of a Where clause (i.e. select COUNT(1) from T).
        /// </summary>
        /// <returns>An instance of <see cref="ISelectBuilder{T}"/></returns>
        ISelectBuilder<T> Count();
    }

    /// <summary>
    /// Represents a Fluent interface to append a Select statement with Where, Order By, Paging Clauses
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface ISelectAndOrOrderByBuilder<T> : ISqlQuery,
        IWhereAndOrBuilder<ISelectAndOrOrderByBuilder<T>, T>,
        ISelectOrderByBuilder<T>,
        ISelectPageBuilder<T>
    {
    }

    /// <summary>
    /// Represents a Fluent interface to append a Select statement with Order By, Paging Clauses
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface ISelectOrderByBuilder<T> : ISqlQuery,
        ISelectPageBuilder<T>
    {
        /// <summary>
        /// Starts an Order By Clause
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="orderby">Member of T that represents the column to order</param>
        /// <returns>A instance of <see cref="ISelectThenByBuilder{T}"/> to append additional Order By Clauses</returns>
        ISelectThenByBuilder<T> OrderBy<K>(Expression<Func<T, K>> orderby);
        /// <summary>
        /// Starts an Order By Clause
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="orderby">Member of T that represents the column to order</param>
        /// <returns>A instance of <see cref="ISelectThenByBuilder{T}"/> to append additional Order By Clauses</returns>
        ISelectThenByBuilder<T> OrderByDescending<K>(Expression<Func<T, K>> orderby);
    }

    /// <summary>
    /// Represents a Fluent interface to append a Select statement with Order By, Paging Clauses
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface ISelectThenByBuilder<T> : ISqlQuery,
        ISelectPageBuilder<T>
    {
        /// <summary>
        /// Continues an Order By Clause
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="thenby">Member of T that represents the column to order</param>
        /// <returns>A instance of <see cref="ISelectThenByBuilder{T}"/> to append additional Order By Clauses</returns>
        ISelectThenByBuilder<T> ThenBy<K>(Expression<Func<T, K>> thenby);
        /// <summary>
        /// Continues an Order By Clause
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="thenby">Member of T that represents the column to order</param>
        /// <returns>A instance of <see cref="ISelectThenByBuilder{T}"/> to append additional Order By Clauses</returns>
        ISelectThenByBuilder<T> ThenByDescending<K>(Expression<Func<T, K>> thenby);
    }

    /// <summary>
    /// Represents a Fluent interface to append a Select statement with a Paging Clause
    /// </summary>
    /// <typeparam name="T">Type used for generating SQL</typeparam>
    public interface ISelectPageBuilder<T> : ISqlQuery
    {
        /// <summary>
        /// Generic function to provide Paging for various <see cref="ISqlDialect"/>
        /// </summary>
        /// <param name="page">Page Number (one based)</param>
        /// <param name="itemsPerPage">Number of Items per page</param>
        /// <returns>A instance of <see cref="ISqlQuery"/></returns>
        ISqlQuery Page(int page, int itemsPerPage);
    }

    #endregion
}