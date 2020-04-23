using System;
using System.Text.RegularExpressions;

namespace PromptCLI
{
    public class InputComponent : ComponentBase, IComponent<string>
    {
        private readonly string _defaultValue;
        private readonly Input<string> _input;

        public Range Range => _range;
        public string Result => _input.Status;
        public bool IsCompleted { get; set; }

        public InputComponent(Input<string> input, string defaultValue = default)
        {
            _defaultValue = defaultValue;
            _input = input;
            _regex = "^[a-zA-Z0-9. _\b]";
        }

        public void Draw(bool defaultValue = true)
        {
            int startPoint = prefix.Length + _input.Text.Length + 1;
            Console.Write(prefix, ConsoleColor.Green);
            Console.Write(_input.Text);

            if (defaultValue && !string.IsNullOrEmpty(_defaultValue))
            {
                var format = string.Format(" ({0})", _defaultValue);
                Console.Write(format, ConsoleColor.DarkGray);
            }

            if (defaultValue)
                Console.WriteLine();

            _range = startPoint..;
            ResetCursorToBegining();
        }

        private void ResetCursorToBegining()
        {
            _cursorPointLeft = _range.Start.Value;
            SetPosition();
        }

        private void Reset()
        {
            Console.ClearCurrentLine(_cursorPointTop, _cursorPointLeft);
            SetPosition();
        }

        public void Handle(ConsoleKeyInfo act)
        {
            // Special for each component
            var (result, key) = IsKeyAvailable(act);
            if (result == KeyInfo.Unknown)
            {
                SetPosition();
                return;
            }
            else if (result == KeyInfo.Direction)
            {
                SetPosition();
                return;
            }

            if (string.IsNullOrEmpty(_input.Status) && _defaultValue != default)
            {
                _cursorPointLeft = Console.CursorLeft;
                Reset();
            }

            if (key == ConsoleKey.Backspace)
            {
                // Go back one character insert a space and go back again.
                GoBack();
                // Remove from status
                _input.Status = _input.Status.Remove(_input.Status.Length - 1);
            }
            else
            {
                // SetPosition();
                _input.Status += act.KeyChar;
                // SetPosition();
            }

        }


        public void SetTopPosition(int top)
        {
            _cursorPointTop = top;
            _maxTop = top + 1;
        }


        public int GetTopPosition()
        {
            return 1;
        }

        public void Complete()
        {
            if (string.IsNullOrEmpty(_input.Status) && _defaultValue != default)
            {
                _input.Status = _defaultValue;
            }
        }
    }

}