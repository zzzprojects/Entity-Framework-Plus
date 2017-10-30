﻿using System.Collections.Generic;
using System.Linq.Expressions;

namespace Z.EntityFramework.Plus
{
    public class QueryIncludeFilterPathVisitor : ExpressionVisitor
    {
        public List<Expression> LambdaToChecks = new List<Expression>();
        public List<string> Paths = new List<string>();
        public Expression RootExpression;

        public void AddMemberExpression(MemberExpression memberExpression)
        {
            // ADD
            // x => x.Single.Single.Many to x => x.Single.Single
            // x => x.Single.Single to x => x.Single
            // 
            // NOT
            // x => x.Single

            Expression currentExpression = memberExpression;

            var reverseList = new List<string>();
            while ((memberExpression = currentExpression as MemberExpression) != null)
            {
                reverseList.Add(memberExpression.Member.Name);

                currentExpression = memberExpression.Expression;
            }
            reverseList.Reverse();
            Paths.AddRange(reverseList);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node == RootExpression || LambdaToChecks.Contains(node))
            {
                var currentNode = node.Body;
                var memberExpression = node.Body as MemberExpression;

                if (memberExpression != null)
                {
                    AddMemberExpression(memberExpression);
                }
                else
                {
                    MethodCallExpression callExpression;
                    while ((callExpression = currentNode as MethodCallExpression) != null)
                    {
						
                        var isSelectMethod = callExpression.Method.ReflectedType != null
                                             && callExpression.Method.ReflectedType.FullName == "System.Linq.Enumerable"
                                             && (callExpression.Method.Name == "Select"
                                                 || callExpression.Method.Name == "SelectMany");

                        if (isSelectMethod)
                        {
                            // ADD
                            // x => x.Many.Select(y => Many.Select(z => z.Many) to x.Many.Select(y => y.Many)
                            // x => x.Many.Select(y => y.Many) to x => x.Many
                            LambdaToChecks.Add(callExpression.Arguments[1]);
                        }

                        currentNode = callExpression.Arguments[0];

                        // ONLY one member expression can exist by lambda expression
                        memberExpression = currentNode as MemberExpression;
                        if (memberExpression != null)
                        {
                            AddMemberExpression(memberExpression);
                            break;
                        }
                    }
                }
            }

            return base.VisitLambda(node);
        }
    }
}