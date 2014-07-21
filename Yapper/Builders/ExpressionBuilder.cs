using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Augment;
using EnsureThat;
using Yapper.Dialects;
using Yapper.Mappers;

namespace Yapper.Builders
{
    /// <summary>
    /// Used to build SQL Expressions from lambdas
    /// </summary>
    internal sealed class ExpressionBuilder : ISqlQuery
    {
        #region Members

        /// <summary>
        /// 
        /// </summary>
        public class PartialSqlString
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            public PartialSqlString(string text) { Text = text; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public static implicit operator PartialSqlString(string text) { return new PartialSqlString(text); }

            /// <summary>
            /// 
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString() { return Text; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class EnumMemberAccess : PartialSqlString
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            /// <param name="propertyMap"></param>
            public EnumMemberAccess(string text, PropertyMap propertyMap)
                : base(text)
            {
                Ensure.That(propertyMap.ObjectProperty.PropertyType.IsEnum)
                    .WithExtraMessageOf(() => "Invalid Type (must be enum)")
                    .IsTrue()
                    ;

                PropertyMap = propertyMap;
            }

            /// <summary>
            /// 
            /// </summary>
            public PropertyMap PropertyMap { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public Type EnumType { get { return PropertyMap.ObjectProperty.PropertyType; } }
        }

        private int _existingParms;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialect"></param>
        /// <param name="existingParms"></param>
        public ExpressionBuilder(ISqlDialect dialect, int existingParms = 0)
        {
            Dialect = dialect;

            BuilderParameters = new Dictionary<string, object>();

            _existingParms = existingParms;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public void Compile(Expression where)
        {
            Query = Visit(where).ToString();

            Parameters = BuilderParameters.ToExpando();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string AddParameter(object value)
        {
            string nm = GetParameterName();

            BuilderParameters.Add(nm, value);

            return Dialect.ParameterIdentifier + nm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetParameterName()
        {
            return "p{0}".FormatArgs(BuilderParameters.Count + _existingParms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private PropertyMap GetPropertyMap(MemberExpression m)
        {
            PropertyInfo p = m.Member as PropertyInfo;

            ObjectMap omap = ObjectMapCollection.Default.GetMapFor(p.DeclaringType);

            PropertyMap pmap = omap.Properties[p.Name];

            return pmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool IsPropertyMap(MethodCallExpression m)
        {
            if (m.Object != null && m.Object as MethodCallExpression != null)
            {
                return IsPropertyMap(m.Object as MethodCallExpression);
            }

            MemberExpression exp = m.Object as MemberExpression;

            if (exp != null && exp.Expression != null && exp.Expression.Type != null)
            {
                if (exp.Expression.NodeType == ExpressionType.Parameter)
                {
                    ObjectMap omap = ObjectMapCollection.Default.GetMapFor(exp.Expression.Type);

                    return omap.Properties.Contains(exp.Member.Name);
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private object GetTrueExpression()
        {
            return new PartialSqlString("(1 = 1)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private object GetFalseExpression()
        {
            return new PartialSqlString("(0 = 1)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object GetTrueValue(MemberExpression m)
        {
            PropertyMap map = GetPropertyMap(m);

            PartialSqlString str = "1";

            if (map.UsesValueMap)
            {
                str = AddParameter(map.ValueMap.ToSql(true));
            }

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object GetFalseValue(MemberExpression m)
        {
            PropertyMap map = GetPropertyMap(m);

            PartialSqlString str = "0";

            if (map.UsesValueMap)
            {
                str = AddParameter(map.ValueMap.ToSql(false));
            }

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        private object GetEnumValue(Type enumType, object enumValue)
        {
            ValueMap vmap = EnumMapCollection.Default.GetMapFor(enumType);

            object value = Enum.ToObject(enumType, enumValue);

            PartialSqlString str = AddParameter(vmap.ToSql(value));

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool IsStaticArrayMethod(MethodCallExpression m)
        {
            if (m.Object == null && m.Method.Name == "Contains")
            {
                return m.Arguments.Count == 2;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool IsEnumerableMethod(MethodCallExpression m)
        {
            if (m.Object != null
                && m.Object.Type.IsOrHasGenericInterfaceTypeOf(typeof(IEnumerable<>))
                && m.Object.Type != typeof(string)
                && m.Method.Name == "Contains")
            {
                return m.Arguments.Count == 1;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberExpr"></param>
        /// <param name="quotedColName"></param>
        /// <returns></returns>
        private object ToInPartialString(Expression memberExpr, object quotedColName)
        {
            //UnaryExpression member = Expression.Convert(memberExpr, typeof(object));

            //var lambda = Expression.Lambda<Func<object>>(member);

            //var getter = lambda.Compile();

            //var inArgs = Sql.Flatten(getter() as IEnumerable);

            //var sIn = new StringBuilder();

            //if (inArgs.Count > 0)
            //{
            //    foreach (object e in inArgs)
            //    {
            //        sIn.AppendIf(sIn.Length > 0, ", ").Append(Dialect.GetQuotedValue(e, e.GetType()));
            //    }
            //}
            //else
            //{
            //    sIn.Append("null");
            //}

            //var statement = "{0} {1} ({2})".FormatArgs(quotedColName, "In", sIn);

            //return new PartialSqlString(statement);

            return null;
        }

        #endregion

        #region Visit Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private object Visit(Expression exp)
        {
            if (exp == null)
            {
                return string.Empty;
            }

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(exp as LambdaExpression);

                case ExpressionType.MemberAccess:
                    return VisitMemberAccess(exp as MemberExpression);

                case ExpressionType.Constant:
                    return VisitConstant(exp as ConstantExpression);

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary(exp as BinaryExpression);

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary(exp as UnaryExpression);


                case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);

                case ExpressionType.Call:
                    return VisitMethodCall(exp as MethodCallExpression);

                case ExpressionType.New:
                    return VisitNew(exp as NewExpression);

                case ExpressionType.NewArrayInit:

                case ExpressionType.NewArrayBounds:
                    return VisitNewArray(exp as NewArrayExpression);

                case ExpressionType.MemberInit:
                    return VisitMemberInit(exp as MemberInitExpression);

                default:
                    return exp.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private object VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression m = lambda.Body as MemberExpression;

                if (m.Expression != null)
                {
                    string r = VisitMemberAccess(m).ToString();

                    return "({0} = {1})".FormatArgs(r, GetTrueValue(m));
                }

            }
            return Visit(lambda.Body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private object VisitBinary(BinaryExpression b)
        {
            object left, right;

            string operand = BindOperant(b.NodeType);

            if (operand == "and" || operand == "or")
            {
                MemberExpression m = b.Left as MemberExpression;

                if (m != null && m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {
                    left = "({0} = {1})".FormatArgs(VisitMemberAccess(m), GetTrueValue(m));
                }
                else
                {
                    left = Visit(b.Left);
                }

                m = b.Right as MemberExpression;

                if (m != null && m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {
                    right = "({0} = {1})".FormatArgs(VisitMemberAccess(m), GetTrueValue(m));
                }
                else
                {
                    right = Visit(b.Right);
                }

                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    object result = Expression.Lambda(b).Compile().DynamicInvoke();

                    string nm = AddParameter(result);

                    return nm;
                }

                if (left.GetType() == typeof(bool))
                {
                    left = ((bool)left) ? GetTrueExpression() : GetFalseExpression();
                }
                if (right.GetType() == typeof(bool))
                {
                    right = ((bool)right) ? GetTrueExpression() : GetFalseExpression();
                }
            }
            else
            {
                left = Visit(b.Left);

                right = Visit(b.Right);

                EnumMemberAccess leftEnum = left as EnumMemberAccess;
                EnumMemberAccess rightEnum = right as EnumMemberAccess;

                bool rightNeedsCoercing = leftEnum != null && rightEnum == null;
                bool leftNeedsCoercing = rightEnum != null && leftEnum == null;

                if (rightNeedsCoercing)
                {
                    PartialSqlString rightPartialSql = right as PartialSqlString;

                    if (rightPartialSql == null)
                    {
                        right = GetEnumValue(leftEnum.EnumType, right);
                    }
                }
                else if (leftNeedsCoercing)
                {
                    PartialSqlString leftPartialSql = left as PartialSqlString;

                    if (leftPartialSql == null)
                    {
                        left = GetEnumValue(rightEnum.EnumType, left);
                    }
                }
                else if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    var result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return result;
                }
                else if (left as PartialSqlString == null)
                {
                    left = AddParameter(left);
                }
                else if (right as PartialSqlString == null)
                {
                    right = AddParameter(right);
                }
            }

            if (operand == "=" && right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                operand = "is";
            }
            else if (operand == "<>" && right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                operand = "is not";
            }

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new PartialSqlString("{0}({1},{2})".FormatArgs(operand, left, right));

                default:
                    return new PartialSqlString("({0} {1} {2})".FormatArgs(left, operand, right));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null)
            {
                if (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert)
                {
                    PropertyInfo p = m.Member as PropertyInfo;

                    ObjectMap omap = ObjectMapCollection.Default.GetMapFor(p.DeclaringType);

                    PropertyMap pm = omap.Properties[p.Name];

                    //string identifier = Dialect.EscapeIdentifier(omap.Alias) +
                    //    Dialect.IdentifierSeparator +
                    //    Dialect.EscapeIdentifier(pm.SourceName)
                    //    ;

                    string identifier = Dialect.EscapeIdentifier(pm.SourceName);

                    if (p.PropertyType.IsEnum)
                    {
                        return new EnumMemberAccess(identifier, pm);
                    }

                    return new PartialSqlString(identifier);
                }
            }

            UnaryExpression member = Expression.Convert(m, typeof(object));

            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);

            Func<object> getter = lambda.Compile();

            return getter();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private object VisitMemberInit(MemberInitExpression exp)
        {
            return Expression.Lambda(exp).Compile().DynamicInvoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nex"></param>
        /// <returns></returns>
        private object VisitNew(NewExpression nex)
        {
            // TODO : check !
            UnaryExpression member = Expression.Convert(nex, typeof(object));
            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(member);

            try
            {
                Func<object> getter = lambda.Compile();

                return getter();
            }
            catch (InvalidOperationException)
            {
                var exprs = VisitExpressionList(nex.Arguments);

                StringBuilder r = new StringBuilder();

                foreach (object e in exprs)
                {
                    r.AppendIf(r.Length > 0, ", ").Append(e);
                }

                return r.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private object VisitParameter(ParameterExpression p)
        {
            return p.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private object VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
            {
                return new PartialSqlString("null");
            }

            return c.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        private object VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    object o = Visit(u.Operand);

                    if (o as PartialSqlString == null)
                    {
                        return !((bool)o);
                    }

                    if (u.Operand.NodeType == ExpressionType.MemberAccess)
                    {
                        MemberExpression me = u.Operand as MemberExpression;

                        if (me != null)
                        {
                            PropertyMap pm = GetPropertyMap(me);

                            if (pm != null)
                            {
                                PartialSqlString str = "({0} = {1})".FormatArgs(o, GetFalseValue(me));

                                return str;
                            }
                        }
                    }

                    return new PartialSqlString("not (" + o + ")");

                case ExpressionType.Convert:
                    if (u.Method != null)
                    {
                        return Expression.Lambda(u).Compile().DynamicInvoke();
                    }
                    break;
            }

            return Visit(u.Operand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object VisitMethodCall(MethodCallExpression m)
        {
            //if (m.Method.DeclaringType == typeof(Sql))
            //{
            //    return VisitSqlMethodCall(m);
            //}

            if (IsStaticArrayMethod(m))
            {
                return VisitStaticArrayMethodCall(m);
            }

            if (IsEnumerableMethod(m))
            {
                return VisitEnumerableMethodCall(m);
            }

            if (IsPropertyMap(m))
            {
                return VisitColumnAccessMethod(m);
            }

            return Expression.Lambda(m).Compile().DynamicInvoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private List<object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<object> list = new List<object>();

            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (original[i].NodeType == ExpressionType.NewArrayInit || original[i].NodeType == ExpressionType.NewArrayBounds)
                {
                    list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
                }
                else
                {
                    list.Add(Visit(original[i]));
                }
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="na"></param>
        /// <returns></returns>
        private object VisitNewArray(NewArrayExpression na)
        {
            List<object> exprs = VisitExpressionList(na.Expressions);

            StringBuilder r = new StringBuilder();

            foreach (object e in exprs)
            {
                r.AppendIf(r.Length > 0, ",").Append(e);
            }

            return r.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="na"></param>
        /// <returns></returns>
        private List<object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {
            List<object> exprs = VisitExpressionList(na.Expressions);

            return exprs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string BindOperant(ExpressionType e)
        {
            switch (e)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.OrElse:
                    return "or";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                default:
                    return e.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object VisitStaticArrayMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    List<object> args = VisitExpressionList(m.Arguments);

                    object quotedColName = args[1];

                    Expression memberExpr = m.Arguments[0];

                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                    {
                        memberExpr = (m.Arguments[0] as MemberExpression);
                    }

                    return ToInPartialString(memberExpr, quotedColName);

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object VisitEnumerableMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    List<object> args = VisitExpressionList(m.Arguments);

                    object quotedColName = args[0];

                    return ToInPartialString(m.Object, quotedColName);

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<object> args = VisitExpressionList(m.Arguments);

            object quotedColName = args[0];

            args.RemoveAt(0);

            string statement = "";

            //    switch (m.Method.Name)
            //    {
            //        //case "In":

            //        //    var member = Expression.Convert(m.Arguments[1], typeof(object));
            //        //    var lambda = Expression.Lambda<Func<object>>(member);
            //        //    var getter = lambda.Compile();

            //        //    var inArgs = Sql.Flatten(getter() as IEnumerable);

            //        //    var sIn = new StringBuilder();

            //        //    foreach (object e in inArgs)
            //        //    {
            //        //        if (!(e is ICollection))
            //        //        {
            //        //            if (sIn.Length > 0)
            //        //                sIn.Append(",");

            //        //            sIn.Append(Dialect.GetQuotedValue(e, e.GetType()));
            //        //        }
            //        //        else
            //        //        {
            //        //            var listArgs = e as ICollection;
            //        //            foreach (object el in listArgs)
            //        //            {
            //        //                if (sIn.Length > 0)
            //        //                    sIn.Append(",");

            //        //                sIn.Append(Dialect.GetQuotedValue(el, el.GetType()));
            //        //            }
            //        //        }
            //        //    }

            //        //    statement = "{0} {1} ({2})".FormatArgs(quotedColName, m.Method.Name, sIn.ToString());
            //        //    break;

            //        default:
            //            throw new NotSupportedException();
            //    }

            return new PartialSqlString(statement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private object VisitColumnAccessMethod(MethodCallExpression m)
        {
            List<object> args = VisitExpressionList(m.Arguments);

            string quotedColName = Visit(m.Object).ToString();

            string statement = "";

            switch (m.Method.Name)
            {
                case "Trim":
                    statement = "ltrim(rtrim({0}))".FormatArgs(quotedColName);
                    break;

                case "TrimStart":
                    statement = "ltrim({0})".FormatArgs(quotedColName);
                    break;

                case "TrimEnd":
                    statement = "rtrim({0})".FormatArgs(quotedColName);
                    break;

                case "ToUpper":
                    statement = "ucase({0})".FormatArgs(quotedColName);
                    break;

                case "ToLower":
                    statement = "lcase({0})".FormatArgs(quotedColName);
                    break;

                case "Replace":
                    {
                        string p0 = AddParameter(args[0]);
                        string p1 = AddParameter(args[1]);

                        statement = "replace({0}, {1}, {2})".FormatArgs(quotedColName, p0, p1);
                    }
                    break;

                case "StartsWith":
                    {
                        string p0 = AddParameter(args[0] + "%");

                        statement = "({0} like {1})".FormatArgs(quotedColName, p0);
                    }
                    break;

                case "EndsWith":
                    {
                        string p0 = AddParameter("%" + args[0]);

                        statement = "({0} like {1})".FormatArgs(quotedColName, p0);
                    }
                    break;

                case "Contains":
                    {
                        string p0 = AddParameter("%" + args[0] + "%");

                        statement = "({0} like {1})".FormatArgs(quotedColName, p0);
                    }
                    break;

                case "Substring":
                    {
                        int startIndex = Int32.Parse(args[0].ToString()) + 1;

                        string p0 = AddParameter(startIndex);

                        if (args.Count == 2)
                        {
                            int length = Int32.Parse(args[1].ToString());

                            string p1 = AddParameter(length);

                            statement = "substr({0}, {1}, {2})".FormatArgs(quotedColName, p0, p1);
                        }
                        else
                        {
                            statement = "substr({0}, {1})".FormatArgs(quotedColName, p0);
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ISqlDialect Dialect { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private IDictionary<string, object> BuilderParameters { get; set; }

        #endregion

        #region ISqlQuery Members

        public string Query { get; private set; }

        public ExpandoObject Parameters { get; private set; }

        #endregion
    }
}