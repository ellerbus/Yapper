using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Yapper
{
    #region Core

    public interface ISqlQuery
    {
        string Query { get; }

        ExpandoObject Parameters { get; }
    }

    public interface IWhereBuilder<S, T>
    {
        S Where(object where);
        S Where(Expression<Func<T, bool>> expression);
    }

    public interface IWhereAndOrBuilder<S, T>
    {
        S And(object where);
        S And(Expression<Func<T, bool>> expression);
        S Or(object where);
        S Or(Expression<Func<T, bool>> expression);
    }

    #endregion

    #region Update

    public interface IUpdateBuilder<T> : ISqlQuery,
        IWhereBuilder<IUpdateWhereAndOrBuilder<T>, T>
    {
        IUpdateBuilder<T> Set(object values);
        IUpdateBuilder<T> Set<K, V>(Expression<Func<T, K>> field, Expression<Func<T, V>> expression);
    }

    public interface IUpdateWhereAndOrBuilder<T> : ISqlQuery,
        IWhereAndOrBuilder<IUpdateWhereAndOrBuilder<T>, T>
    {
    }

    #endregion

    #region Delete

    public interface IDeleteBuilder<T> : ISqlQuery,
        IWhereBuilder<IDeleteAndOrBuilder<T>, T>
    {
    }

    public interface IDeleteAndOrBuilder<T> : ISqlQuery,
        IWhereAndOrBuilder<IDeleteAndOrBuilder<T>, T>
    {
    }

    #endregion

    #region Select

    public interface ISelectBuilder<T> : ISqlQuery,
        IWhereBuilder<ISelectAndOrOrderByBuilder<T>, T>,
        ISelectOrderByBuilder<T>,
        ISelectPageBuilder<T>
    {
        ISelectBuilder<T> Top(int top);
        ISelectBuilder<T> Select<K>(Expression<Func<T, K>> select);
        ISelectBuilder<T> Max<K>(Expression<Func<T, K>> max);
        ISelectBuilder<T> Min<K>(Expression<Func<T, K>> min);
        ISelectBuilder<T> Avg<K>(Expression<Func<T, K>> avg);
        ISelectBuilder<T> Sum<K>(Expression<Func<T, K>> sum);
    }

    public interface ISelectAndOrOrderByBuilder<T> : ISqlQuery,
        IWhereAndOrBuilder<ISelectAndOrOrderByBuilder<T>, T>,
        ISelectOrderByBuilder<T>,
        ISelectPageBuilder<T>
    {
    }

    public interface ISelectOrderByBuilder<T> : ISqlQuery,
        ISelectPageBuilder<T>
    {
        ISelectThenByBuilder<T> OrderBy<K>(Expression<Func<T, K>> orderby);
        ISelectThenByBuilder<T> OrderByDescending<K>(Expression<Func<T, K>> orderby);
    }

    public interface ISelectThenByBuilder<T> : ISqlQuery,
        ISelectPageBuilder<T>
    {
        ISelectThenByBuilder<T> ThenBy<K>(Expression<Func<T, K>> thenby);
        ISelectThenByBuilder<T> ThenByDescending<K>(Expression<Func<T, K>> thenby);
    }

    public interface ISelectPageBuilder<T> : ISqlQuery
    {
        ISqlQuery Page(int page, int itemsPerPage);
    }

    #endregion
}