using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rhinox.Lightspeed
{
    public class Accessor<T>
    {
        private Action<T> Setter;
        private Func<T> Getter;

        public Accessor(Expression<Func<T>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            var instanceExpression = memberExpression.Expression;
            var parameter = Expression.Parameter(typeof(T));

            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                Setter = Expression.Lambda<Action<T>>(Expression.Call(instanceExpression, propertyInfo.GetSetMethod(), parameter), parameter).Compile();
                Getter = Expression.Lambda<Func<T>>(Expression.Call(instanceExpression, propertyInfo.GetGetMethod())).Compile();
            }
            else if (memberExpression.Member is FieldInfo fieldInfo)
            {
                Setter = Expression.Lambda<Action<T>>(Expression.Assign(memberExpression, parameter), parameter).Compile();
                Getter = Expression.Lambda<Func<T>>(Expression.Field(instanceExpression,fieldInfo)).Compile();
            }

        }

        public void Set(T value) => Setter(value);

        public T Get() => Getter();
    }
}