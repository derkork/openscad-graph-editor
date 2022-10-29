using System;
using System.Collections.Generic;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class SelectBox : LiteralWidgetBase<OptionButton, IScadLiteral>
    {
        private OptionButton _optionButton;
        private List<(IScadLiteral Value, StringLiteral Label)> _options = new List<(IScadLiteral Value, StringLiteral Label)>();

        public List<(IScadLiteral Value, StringLiteral Label)> Options
        {
            get => _options;
            set
            {
                _options = value;
                SetupOptionButton();
            }
        }

        protected override OptionButton CreateControl()
        {
            _optionButton = new OptionButton();

            SetupOptionButton();


            _optionButton.Connect("item_selected")
                .To(this, nameof(OnValueChanged));

            return _optionButton;
        }

        private void SetupOptionButton()
        {
            if (_optionButton == null)
            {
                return;
            }

            _optionButton.Clear();
            // add options
            foreach (var (_, label) in _options)
            {
                _optionButton.AddItem(label.Value);
            }
        }


       private void OnValueChanged(int index)
        {
          
            var value = _options[index].Value;
            EmitValueChange(value);
        }

        protected override void ApplyControlValue()
        {
            // find index of value
            var index = _options.FindIndex(x => x.Value.SerializedValue.Equals(Literal.SerializedValue));
            _optionButton.Select(index);
        }
    }
}