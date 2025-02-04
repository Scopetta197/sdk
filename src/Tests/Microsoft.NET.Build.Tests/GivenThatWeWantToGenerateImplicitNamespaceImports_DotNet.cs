// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.NET.TestFramework;
using Microsoft.NET.TestFramework.Assertions;
using Microsoft.NET.TestFramework.Commands;
using Microsoft.NET.TestFramework.ProjectConstruction;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.NET.Build.Tests
{
    public class GivenThatWeWantToGenerateImplicitNamespaceImports_DotNet : SdkTest
    {
        public GivenThatWeWantToGenerateImplicitNamespaceImports_DotNet(ITestOutputHelper log) : base(log) { }

        [RequiresMSBuildVersionFact("17.0.0.32901")]
        public void It_generates_dotnet_imports_and_builds_successfully()
        {
            var tfm = "net6.0";
            var testProject = CreateTestProject(tfm);
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var importFileName = $"{testAsset.TestProject.Name}.ImplicitNamespaceImports.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Pass();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().HaveFile(importFileName);

            File.ReadAllText(Path.Combine(outputDirectory.FullName, importFileName)).Should().Be(
@"// <autogenerated />
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
");
        }

        [Fact]
        public void It_can_disable_dotnet_imports()
        {
            var tfm = "net6.0";
            var testProject = CreateTestProject(tfm);
            testProject.AdditionalProperties["DisableImplicitNamespaceImports_DotNet"] = "true";
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var importFileName = $"{testAsset.TestProject.Name}.ImplicitNamespaceImports.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Fail();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().NotHaveFile(importFileName);
        }

        [RequiresMSBuildVersionFact("17.0.0.32901")]
        public void It_can_remove_specific_imports_in_project_file()
        {
            var tfm = "net6.0";
            var testProject = CreateTestProject(tfm);
            testProject.AddItem("Import", "Remove", "System.IO");
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var importFileName = $"{testAsset.TestProject.Name}.ImplicitNamespaceImports.cs";


            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Pass();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().HaveFile(importFileName);

            File.ReadAllText(Path.Combine(outputDirectory.FullName, importFileName)).Should().Be(
@"// <autogenerated />
global using global::System;
global using global::System.Collections.Generic;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
");
        }

        [Fact]
        public void It_can_generate_custom_imports()
        {
            var tfm = "net6.0";
            var testProject = CreateTestProject(tfm);
            testProject.AdditionalProperties["DisableImplicitNamespaceImports_DotNet"] = "true";
            testProject.AddItem("Import", "Include", "CustomNamespace");
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var importFileName = $"{testAsset.TestProject.Name}.ImplicitNamespaceImports.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Fail();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().HaveFile(importFileName);

            File.ReadAllText(Path.Combine(outputDirectory.FullName, importFileName)).Should().Be(
@"// <autogenerated />
global using global::CustomNamespace;
");
        }

        [Fact]
        public void It_ignores_duplicate_imports()
        {
            var tfm = "net6.0";
            var testProject = CreateTestProject(tfm);
            testProject.AdditionalProperties["DisableImplicitNamespaceImports_DotNet"] = "true";
            testProject.AddItem("Import", "Include", "CustomNamespace;CustomNamespace");
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var importFileName = $"{testAsset.TestProject.Name}.ImplicitNamespaceImports.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Fail();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().HaveFile(importFileName);

            File.ReadAllText(Path.Combine(outputDirectory.FullName, importFileName)).Should().Be(
@"// <autogenerated />
global using global::CustomNamespace;
");
        }

        [Fact]
        public void It_can_disable_import_generation()
        {
            var tfm = "net6.0";
            var testProject = CreateTestProject(tfm);
            testProject.AdditionalProperties["DisableImplicitNamespaceImports"] = "true";
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var importFileName = $"{testAsset.TestProject.Name}.ImplicitNamespaceImports.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Fail();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().NotHaveFile(importFileName);
        }

        [Fact]
        public void It_ignores_invalid_tfms()
        {
            var tfm = "net5.0";
            var testProject = CreateTestProject(tfm);
            var testAsset = _testAssetsManager.CreateTestProject(testProject);
            var importFileName = $"{testAsset.TestProject.Name}.ImplicitNamespaceImports.cs";

            var buildCommand = new BuildCommand(testAsset);
            buildCommand
                .Execute()
                .Should()
                .Fail();

            var outputDirectory = buildCommand.GetIntermediateDirectory(tfm);

            outputDirectory.Should().NotHaveFile(importFileName);
        }

        private TestProject CreateTestProject(string tfm)
        {
            var testProject = new TestProject
            {
                IsExe = true,
                TargetFrameworks = tfm,
                ProjectSdk = "Microsoft.NET.Sdk"
            };
            testProject.SourceFiles["Program.cs"] = @"
namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}
";
            return testProject;
        }
    }
}
