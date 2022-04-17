using System;
using System.Collections.Generic;
using Godot;

namespace OpenScadGraphEditor.Widgets
{
    public readonly struct ValidityChecker
    {
        private readonly List<string> _errors;
        private readonly Label _errorLabel;
        private readonly Button _okButton;

        private ValidityChecker(Label errorLabel, Button okButton)
        {
            _okButton = okButton;
            _errorLabel = errorLabel;
            _errors = new List<string>();
        }


        public static ValidityChecker For(Label errorLabel, Button okButton)
        {
            return new ValidityChecker(errorLabel, okButton);
        }

        /// <summary>
        /// Checks the given condition and if it is false, adds the error message to the list of errors.
        /// </summary>
        public ValidityChecker Check(bool condition, string message)
        {
            if (!condition)
            {
                _errors.Add(message);
            }

            return this;
        }
        
        public ValidityChecker CheckAll<T>(IEnumerable<T> input, Predicate<T> valid, Func<T,string> errorMessage)
        {
            foreach (var item in input)
            {
                if (!valid(item))
                {
                    _errors.Add(errorMessage(item));
                }
            }

            return this;
        }

        public void UpdateUserInterface()
        {
            var hasErrors = _errors.Count > 0;
            if (hasErrors)
            {
                _errorLabel.Text = string.Join("\n", _errors);
            }
            
            _okButton.Disabled = hasErrors;
            _errorLabel.Visible = hasErrors;
        }
    }
}