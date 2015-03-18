﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Description;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Application
{
    public class SwaggerGeneratorOptionsBuilder
    {
        private VersionInfoBuilder _versionInfoBuilder;
        private Func<ApiDescription, string, bool> _versionSupportResolver;
        private IEnumerable<string> _schemes;
        private IDictionary<string, SecuritySchemeBuilder> _securitySchemeBuilders;
        private bool _ignoreObsoleteActions;
        private Func<ApiDescription, string> _groupNameSelector;
        private IComparer<string> _groupNameComparer;
        private IList<Func<IDocumentFilter>> _documentFilters;
        private Func<IEnumerable<ApiDescription>, ApiDescription> _conflictingActionsResolver;

        public SwaggerGeneratorOptionsBuilder()
        {
            _securitySchemeBuilders = new Dictionary<string, SecuritySchemeBuilder>();
            _documentFilters = new List<Func<IDocumentFilter>>();

            SingleApiVersion("v1", "API V1");
        }

        public InfoBuilder SingleApiVersion(string version, string title)
        {
            _versionInfoBuilder = new VersionInfoBuilder();
            return _versionInfoBuilder.Version(version, title);
        }

        public void MultipleApiVersions(
            Func<ApiDescription, string, bool> versionSupportResolver,
            Action<VersionInfoBuilder> configureVersions)
        {
            _versionSupportResolver = versionSupportResolver;
            _versionInfoBuilder = new VersionInfoBuilder();
            configureVersions(_versionInfoBuilder);
        }

        public void Schemes(IEnumerable<string> schemes)
        {
            _schemes = schemes;
        }

        public BasicAuthSchemeBuilder BasicAuth(string name)
        {
            var schemeBuilder = new BasicAuthSchemeBuilder();
            _securitySchemeBuilders[name] = schemeBuilder;
            return schemeBuilder;
        }

        public ApiKeySchemeBuilder ApiKey(string name)
        {
            var schemeBuilder = new ApiKeySchemeBuilder();
            _securitySchemeBuilders[name] = schemeBuilder;
            return schemeBuilder;
        }

        public OAuth2SchemeBuilder OAuth2(string name)
        {
            var schemeBuilder = new OAuth2SchemeBuilder();
            _securitySchemeBuilders[name] = schemeBuilder;
            return schemeBuilder;
        }

        public void IgnoreObsoleteActions()
        {
            _ignoreObsoleteActions = true;
        }

        public void GroupActionsBy(Func<ApiDescription, string> groupNameSelector)
        {
            _groupNameSelector = groupNameSelector;
        }

        public void OrderActionGroupsBy(IComparer<string> groupNameComparer)
        {
            _groupNameComparer = groupNameComparer;
        }

        public void DocumentFilter<TFilter>()
            where TFilter : IDocumentFilter, new()
        {
            DocumentFilter(() => new TFilter());
        }

        public void DocumentFilter(Func<IDocumentFilter> createFilter)
        {
            _documentFilters.Add(createFilter);
        }

        public void ResolveConflictingActions(
            Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver)
        {
            _conflictingActionsResolver = conflictingActionsResolver;
        }

        public SwaggerGeneratorOptions Build()
        {
            var securityDefinitions = _securitySchemeBuilders
                .ToDictionary(entry => entry.Key, entry => entry.Value.Build());

            var documentFilters = _documentFilters.Select(factory => factory());

            return new SwaggerGeneratorOptions(
                apiVersions: _versionInfoBuilder.Build(),
                versionSupportResolver: _versionSupportResolver,
                schemes: _schemes,
                securityDefinitions: securityDefinitions,
                ignoreObsoleteActions: _ignoreObsoleteActions,
                groupNameSelector: _groupNameSelector,
                groupNameComparer: _groupNameComparer,
                documentFilters: documentFilters,
                conflictingActionsResolver: _conflictingActionsResolver);
        }
    }
}