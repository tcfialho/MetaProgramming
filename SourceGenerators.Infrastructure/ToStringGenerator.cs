using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace SourceGenerators.Infrastructure
{
    [Generator]
    public class ToStringGenerator : ISourceGenerator
    {
        public void Execute(SourceGeneratorContext context)
        {
            var syntaxReceiver = (ClassSyntaxReceiver)context.SyntaxReceiver;

            var userClass = syntaxReceiver.ClassToAugment;
            if (userClass is null)
            {
                return;
            }

            var propertyToStringSourceCode = new StringBuilder();

            foreach (var member in userClass.Members.OfType<PropertyDeclarationSyntax>())
            {
                propertyToStringSourceCode.AppendLine(@$"text += $""{member.Identifier.Text}: {{{member.Identifier.Text}}} "";");
            }

            var namespaceName = ((NamespaceDeclarationSyntax)userClass.Parent).Name.ToString();

            var sourceCode = $@"namespace {namespaceName}
                                {{
                                    public partial class {userClass.Identifier}
                                    {{
                                        public override string ToString()
                                        {{
                                            var text = string.Empty;
                                            {propertyToStringSourceCode}
                                            return text;
                                        }}
                                    }}
                                }}";

            // add the generated implementation to the compilation
            var sourceText = SourceText.From(sourceCode, Encoding.UTF8);

            context.AddSource("Account.Generated.cs", sourceText);
        }

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ClassSyntaxReceiver());
        }
    }

    internal class ClassSyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax ClassToAugment { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is ClassDeclarationSyntax cds &&
                ((NamespaceDeclarationSyntax)cds.Parent).Name.ToString().Contains("Entities"))
            {
                ClassToAugment = cds;
            }
        }
    }
}
