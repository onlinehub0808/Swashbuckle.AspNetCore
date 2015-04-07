﻿using System;
using System.Linq;
using System.Xml.XPath;
using System.Reflection;
using Microsoft.AspNet.Mvc;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlActionComments : IOperationFilter
    {
        private const string MethodExpression = "/doc/members/member[@name='M:{0}.{1}{2}']";
        private const string SummaryExpression = "summary";
        private const string RemarksExpression = "remarks";
        private const string ParameterExpression = "param";
        private const string ResponseExpression = "response";
        
        private readonly XPathNavigator _navigator;

        public ApplyXmlActionComments(string path)
        {
            _navigator = new XPathDocument(path).CreateNavigator();
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return;

            var methodXPath = GetMethodXPath(controllerActionDescriptor.MethodInfo);
            var methodNode = _navigator.SelectSingleNode(methodXPath);
            if (methodNode == null) return;

            var summaryNode = methodNode.SelectSingleNode(SummaryExpression);
            if (summaryNode != null)
                operation.summary = summaryNode.ExtractContent();

            var remarksNode = methodNode.SelectSingleNode(RemarksExpression);
            if (remarksNode != null)
                operation.description = remarksNode.ExtractContent();

            ApplyParamComments(operation, methodNode);
        }

		private static string GetMethodXPath(MethodInfo methodInfo)
        {
            var classKey = methodInfo.DeclaringType.XmlCommentsId();
            var actionName = methodInfo.Name;

            var paramTypeKeys = methodInfo.GetParameters()
                .Select(paramInfo => paramInfo.ParameterType.XmlCommentsId())
                .ToArray();

            var parameters = (paramTypeKeys.Any())
                ? string.Format("({0})", String.Join(",", paramTypeKeys))
                : string.Empty;

            return string.Format(MethodExpression, classKey, actionName, parameters);
        }

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode)
        {
            var paramNodes = methodNode.Select(ParameterExpression);
            while (paramNodes.MoveNext())
            {
                var paramNode = paramNodes.Current;
                var parameter = operation.parameters
                    .SingleOrDefault(param => param.name == paramNode.GetAttribute("name", ""));
                if (parameter != null)
                    parameter.description = paramNode.ExtractContent();
            }
        }
    }
}