using System;
using Godot.Collections;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// This refactoring duplicates an existing variable (including all settings for the customizer) and adds it to the project.
    /// </summary>
    public class DuplicateVariableRefactoring : Refactoring
    {
        private readonly VariableDescription _toDuplicate;

        public DuplicateVariableRefactoring(VariableDescription toDuplicate)
        {
            _toDuplicate = toDuplicate;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            GdAssert.That(context.Project.IsDefinedInThisProject(_toDuplicate),
                "Tried to duplicate a function that is not defined in this project");

            // first serialize this
            var savedVariable = new SavedVariableDescription();
            _toDuplicate.SaveInto(savedVariable);

            // then change the ID and name
            savedVariable.Id = Guid.NewGuid().ToString();
            // give the invokable a unique name by appending an ascending number
            // verify that no other invokable or variable in the project has the same name (including libraries)
            // if there is a name conflict, increase the number until there is no conflict
            var number = 2;
            var newName = savedVariable.Name + number;
            while (context.Project.IsNameUsed(newName))
            {
                number++;
                newName = savedVariable.Name + number;
            }

            savedVariable.Name = newName;

            // load it into a new variable description
            var newVariable = new VariableDescription();
            newVariable.LoadFrom(savedVariable);

            // add it to the project
            context.Project.AddVariable(newVariable);
        }
    }
}