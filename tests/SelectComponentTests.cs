using System;
using PromptCLI;
using Xunit;
using Moq;
using System.Linq;
using System.Collections.Generic;

namespace PromptCLITests
{
    public class SelectComponentTests
    {
        public List<string> GetList(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i => $"Test {i}")
                .ToList();
        }

        [Theory]
        [InlineData(1)]    
        [InlineData(5)]    
        [InlineData(9)]    
        public void Draw_NList_Drawed(int count)
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            var list = GetList(count);

            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.Draw();

            // make sure the input is wrote
            console.Verify(i => i.WriteLine(text, It.IsAny<ConsoleColor>(), null), Times.Once);

            foreach (var item in list)
                console.Verify(i => i.WriteLine(string.Format("( ) {0}", item)), Times.Once);
        }

        [Fact]
        public void Draw_Range_ValidWithFormat()
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            var list = GetList(3);

            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.Draw();

            // make sure the input is wrote
            console.Verify(i => i.WriteLine(text,  It.IsAny<ConsoleColor>(), null), Times.Once);

            foreach (var item in list)
                console.Verify(i => i.WriteLine(string.Format("( ) {0}", item)), Times.Once);

            Assert.Equal(1, component.Range.Start.Value);
            Assert.Equal(2, component.Range.End.Value);
        }
        [Fact]
        public void Draw_DefaultSelected_Set()
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            var list = GetList(3);

            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.Draw();

            Assert.Equal(list[0], component.Result.Status);
        }

        [Fact]
        public void SetTopPosition_CustomInput_Set()
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            const int top = 10;
            var list = GetList(3);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.SetTopPosition(top);

            Assert.Equal(top + 1, component.CursorTop);
        }
        
        [Theory]
        [InlineData(ConsoleKey.Multiply)]
        [InlineData(ConsoleKey.E)]
        [InlineData(ConsoleKey.X)]
        public void Handle_UnknowChar_NotValid(ConsoleKey key)
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            var list = GetList(3);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            
            component.Draw();
            component.InvokeHandle(key);

            console.Verify(i => i.ClearCurrentPosition(component.CursorLeft, component.CursorTop), Times.Once);
            
            Assert.Equal(list[0], component.Result.Status);
        }

        
        [Fact]
        public void Handle_BoundControlUpInf_StayInBound()
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            const int top = 10;
            var list = GetList(3);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);

            component.SetTopPosition(top);
            component.Draw();
        
            component.InvokeHandle(ConsoleKey.UpArrow);
            component.InvokeHandle(ConsoleKey.UpArrow);
            component.InvokeHandle(ConsoleKey.UpArrow);
            component.InvokeHandle(ConsoleKey.UpArrow);

            Assert.Equal(top + 1, component.CursorTop);
            Assert.Equal(list[0], component.Result.Status);
        }
        
        [Theory]
        [InlineData(10, 5)]
        [InlineData(6, 2)]
        [InlineData(20, 7)]
        public void Handle_BoundControlDown_StayInBound(int count, int n)
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            const int top = 10;
            var list = GetList(count);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);

            component.SetTopPosition(top);
            component.Draw();

            for (int i = 0; i < n; i++)
                component.InvokeHandle(ConsoleKey.DownArrow);

            Assert.Equal(top + n + 1, component.CursorTop);
        }

        [Fact]
        public void Handle_BoundControlDownInf_StayInBound()
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            const int top = 10;
            var list = GetList(3);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);

            component.SetTopPosition(top);
            component.Draw();
        
            component.InvokeHandle(ConsoleKey.DownArrow);
            component.InvokeHandle(ConsoleKey.DownArrow);
            component.InvokeHandle(ConsoleKey.DownArrow);
            component.InvokeHandle(ConsoleKey.DownArrow);
            component.InvokeHandle(ConsoleKey.DownArrow);

            Assert.Equal(top + list.Count, component.CursorTop);
            Assert.Equal(list[0], component.Result.Status);
        }

        [Fact]
        public void Handle_ArrowDown_SelectSecond()
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            var list = GetList(3);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.SetTopPosition(0);
            component.Draw();
            
            component.InvokeHandle(ConsoleKey.DownArrow);
            component.InvokeHandle(ConsoleKey.Spacebar);


            Assert.Equal(list[1], component.Result.Status);
        }

        
        [Theory]
        [InlineData(3)]
        [InlineData(8)]
        [InlineData(11)]
        public void Complete_ClearLineAll_Cleared(int count)
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            const int top = 10;
            var list = GetList(count);

            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.Draw();
            component.SetTopPosition(top);

            component.Complete();
            
            for (int j = top; j < top + list.Count + 1; j++)
                console.Verify(i => i.ClearLine(j, 0), Times.Once);
        }

        [Fact]
        public void Complete_TopValue_Set()
        {
            var console = new Mock<IConsoleBase>();

            const string text = "Input text";
            var list = GetList(3);
            const int top = 10;
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.Draw();
            component.SetTopPosition(top);
            component.Complete();
            
            Assert.Equal(top, component.CursorTop);
        }

        [Fact]
        public void Complete_Input_Redraw()
        {
            var console = new Mock<IConsoleBase>();
            const string text = "Question";
            const int top = 10;
            var list = GetList(3);

            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            component.Draw();
            component.SetTopPosition(top);

            component.Complete();
            
            console.Verify(i => i.Write(text,  It.IsAny<ConsoleColor>(), null));
        }

        [Fact]
        public void Complete_Callback_Invoked()
        {
            var console = new Mock<IConsoleBase>();
            var mockCallback = new Mock<Action<string>>();

            const string text = "Input text";
            var list = GetList(3);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);

            component.Draw();
            component.Callback(mockCallback.Object);
            component.Complete();
            mockCallback.Verify(i => i.Invoke(component.Result.Status));
        }

        
        [Fact]
        public void Bind_Prompt_Binded()
        {
            var console = new Mock<IConsoleBase>();
            var mockPrompt = new Mock<Prompt>();

            const string text = "Input text";
            var list = GetList(3);
            SelectComponent<string> component = new SelectComponent<string>(text, list, console.Object);
            
            component.Draw();
            component.Bind(mockPrompt.Object);

            var result = component.Callback(It.IsAny<Action<string>>());

            Assert.Equal(mockPrompt.Object, result);
        }
    }
}
