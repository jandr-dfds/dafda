using System;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dafda.Tests.Builders;

internal class ConsumerBuilder
{
    private ConsumerScope _consumerScope;
    private IServiceScopeFactory _serviceScopeFactory;
    private Pipeline _pipeline;
    private ConsumerErrorHandler _errorHandler;
    private bool _enableAutoCommit;

    public ConsumerBuilder()
    {
        _consumerScope = new ConsumerScopeStub(new MessageResultBuilder().Build());
        _serviceScopeFactory = new FakeServiceScopeFactory(type => throw new InvalidOperationException($"{type.Name} type registered"));
        _pipeline = new Pipeline();
        _errorHandler = new ConsumerErrorHandler(_ => Task.FromResult(ConsumerFailureStrategy.Default), new DummyApplicationLifetime());
    }

    public ConsumerBuilder WithConsumerScope(ConsumerScope consumerScope)
    {
        _consumerScope = consumerScope;
        return this;
    }

    public ConsumerBuilder WithEnableAutoCommit(bool enableAutoCommit)
    {
        _enableAutoCommit = enableAutoCommit;
        return this;
    }

    public ConsumerBuilder WithServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        return this;
    }

    public ConsumerBuilder WithPipeline(Pipeline pipeline)
    {
        _pipeline = pipeline;
        return this;
    }

    public ConsumerBuilder WithErrorHandler(ConsumerErrorHandler errorHandler)
    {
        _errorHandler = errorHandler;
        return this;
    }

    public Consumer Build()
    {
        return new Consumer(
            logger: NullLogger<Consumer>.Instance,
            consumerScopeFactory: () => _consumerScope,
            serviceScopeFactory: _serviceScopeFactory,
            pipeline: _pipeline,
            errorHandler: _errorHandler,
            isAutoCommitEnabled: _enableAutoCommit);
    }

    private class FakeServiceScopeFactory : IServiceScopeFactory, IServiceScope, IServiceProvider
    {
        private readonly Func<Type, object> _resolve;

        public FakeServiceScopeFactory(Func<Type, object> resolve)
        {
            _resolve = resolve;
        }

        public IServiceScope CreateScope()
        {
            return this;
        }

        public void Dispose()
        {
        }

        public IServiceProvider ServiceProvider => this;

        public object GetService(Type serviceType)
        {
            return _resolve(serviceType);
        }
    }
}