namespace MassTransit.EventStore;

using System.Linq.Expressions;
using System.Reflection;

public class QueryExpressionVisitor<TInstance> : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;

    public QueryExpressionVisitor(ParameterExpression parameter)
    {
        _parameter = parameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return _parameter;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member.MemberType != MemberTypes.Property && node.Member.MemberType != MemberTypes.Field)
        {
            throw new NotImplementedException();
        }

        bool isField = node.Member.MemberType == MemberTypes.Field;

        //name of a member referenced in original expression in your
        //sample Id in mine Prop
        string memberName = node.Member.Name;

        /*Fix*/
        //visit left side of this expression p.Id this would be p
        Expression? inner = Visit(node.Expression);

        if (isField)
        {
            FieldInfo? fieldInfo = inner!.Type.GetField(memberName);
            fieldInfo.ThrowIfNull();
            return Expression.Field(inner, fieldInfo);
        }

        //find property on type T (=PersonData) by name
        PropertyInfo? otherMember = inner!.Type.GetProperty(memberName);
        otherMember.ThrowIfNull();
        return Expression.Property(inner, otherMember);
    }
}
