#nullable enable
using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Middleware
{
    public class TestMiddlewareEngine
    {
        [Fact]
        public async Task pre_actions_are_invoked_in_expected_order()
        {
            var sut = new MiddlewareEngineBuilder<SpyContext>().Build();
            sut.AddMiddleware(new[]
            {
                FakeMiddleware<SpyContext>.WithPreAction(ctx => {ctx.Text += "A";}),
                FakeMiddleware<SpyContext>.WithPreAction(ctx => {ctx.Text += "B";}),
                FakeMiddleware<SpyContext>.WithPreAction(ctx => {ctx.Text += "C";}),
            });

            var spy = new SpyContext();
            await sut.Execute(spy, ctx => Task.CompletedTask);

            Assert.Equal("ABC", spy.Text);
        }

        [Fact]
        public async Task post_actions_are_invoked_in_expected_order()
        {
            var sut = new MiddlewareEngineBuilder<SpyContext>().Build();
            sut.AddMiddleware(new[]
            {
                FakeMiddleware<SpyContext>.WithPostAction(ctx => {ctx.Text += "A";}),
                FakeMiddleware<SpyContext>.WithPostAction(ctx => {ctx.Text += "B";}),
                FakeMiddleware<SpyContext>.WithPostAction(ctx => {ctx.Text += "C";}),
            });

            var spy = new SpyContext();
            await sut.Execute(spy, ctx => Task.CompletedTask);

            Assert.Equal("CBA", spy.Text);
        }

        [Fact]
        public async Task pre_and_inner_and_post_actions_are_invoked_in_expected_order()
        {
            var sut = new MiddlewareEngineBuilder<SpyContext>().Build();
            sut.AddMiddleware(new[]
            {
                FakeMiddleware<SpyContext>.WithPreAndPostActions(
                    preAction: (ctx) => { ctx.Text += "A"; },
                    postAction: (ctx) => { ctx.Text += "A"; }
                ),
                FakeMiddleware<SpyContext>.WithPreAndPostActions(
                    preAction: (ctx) => { ctx.Text += "B"; },
                    postAction: (ctx) => { ctx.Text += "B"; }
                ),
                FakeMiddleware<SpyContext>.WithPreAndPostActions(
                    preAction: (ctx) => { ctx.Text += "C"; },
                    postAction: (ctx) => { ctx.Text += "C"; }
                ),
            });

            var spy = new SpyContext();
            await sut.Execute(spy, ctx =>
            {
                ctx.Text += "-";
                return Task.CompletedTask;
            });

            Assert.Equal("ABC-CBA", spy.Text);
        }

        [Fact]
        public async Task not_calling_next_in_middleware_will_short_circuit_execution()
        {
            var sut = new MiddlewareEngineBuilder<SpyContext>().Build();
            sut.AddMiddleware(new IMiddleware<SpyContext>[]
            {
                FakeMiddleware<SpyContext>.WithPreAction(ctx => {ctx.Text += "A";}),
                new ShortCircuitMiddleware(),
                FakeMiddleware<SpyContext>.WithPreAction(ctx => {ctx.Text += "B";}),
                FakeMiddleware<SpyContext>.WithPreAction(ctx => {ctx.Text += "C";}),
            });

            var spy = new SpyContext();
            await sut.Execute(spy, ctx =>
            {
                ctx.Text += "D";
                return Task.CompletedTask;
            });

            Assert.Equal("A", spy.Text);
        }

        #region private helper classes

        private class ShortCircuitMiddleware : IMiddleware<SpyContext>
        {
            public Task Invoke(SpyContext context, MiddlewareDelegate next) 
                => Task.CompletedTask;
        }

        private class SpyContext
        {
            public string Text { get; set; } = "";
        }

        #endregion
    }
}