using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Umbraco.Core.Models;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    [TestFixture]
    public class When_Comparing_Document_Type : ComparisonTestBase
    {
        [Test]
        public void For_Existing_Then_Is_Not_New()
        {
            When_Umbraco_Has_Document_Type(Mock.Of<IContentType>(), Has.Property("IsNew").False);
        }

        [Test]
        public void For_Unknown_Then_Is_New()
        {
            When_Umbraco_Has_Document_Type(null, Has.Property("IsNew").True);
        }

        [Test]
        public void With_New_Compositions_Of_Existing_DocumentTypes_Then_Is_Safe()
        {
            StubContentType(1, "contentType");
            StubContentType(2, "compositeA");
            StubContentType(3, "compositeB");

            Config.ContentType("contentType")
                .Compositions("compositeA", "compositeB");

            var diff = Config.Compare();

            Assert.That(diff.DocumentTypes["contentType"].IsUnsafe, Is.False);
        }

        [Test]
        public void With_NonExisting_Compositions_Then_Is_Unsafe()
        {
            StubContentType(1, "contentType", Mock.Of<IContentType>());

            Config.ContentType("contentType")
                .Compositions("compositeA", "compositeB");

            var fullDiff = Config.Compare();
            var diff = fullDiff.DocumentTypes["contentType"];

            Assert.That(
                diff, 
                Has.Property("IsUnsafe").True &
                Has.Property("Comparisons").With.Exactly(2).With
                    .Property("Key").EqualTo("Compositions").And
                    .Property("Result").EqualTo(ComparisonResult.Invalid)
                );

        }

        [Test]
        public void With_Compositions_In_Configuration_Then_Is_Safe()
        {
            StubContentType(1, "contentType", Mock.Of<IContentType>());

            Config.ContentType("compositeA");

            Config.ContentType("contentType")
                .Compositions("compositeA", "compositeB");

            var diff = ContentTypeDiff();

            Assert.That(
                diff,
                Has.Property("IsUnsafe").True &
                Has.Property("Comparisons").With.Exactly(2).With
                    .Property("Key").EqualTo("Compositions") &
                Has.Property("Comparisons").With.Exactly(1).Property("Result").EqualTo(ComparisonResult.New) &
                Has.Property("Comparisons").With.Exactly(1).Property("Result").EqualTo(ComparisonResult.Invalid)
                );
        }

        [Test]
        public void For_New_Then_Existing_Parent_Is_Safe()
        {
            StubContentType(2, "parent");

            Config.ContentType("contentType")
                .Parent("parent");

            var diff = ContentTypeDiff();

            Assert.That(diff.IsUnsafe, Is.False);
        }

        [Test]
        public void For_New_Then_Configured_Parent_Is_Safe()
        {
            Config
                .ContentType("parent")
                .ContentType("contentType")
                    .Parent("parent");

            var diff = ContentTypeDiff();

            Assert.That(diff.IsUnsafe, Is.False);
        }

        [Test]
        public void For_Parented_Then_Other_Parent_Is_Unsafe()
        {
            var type = StubContentType(1, "contentType");
            StubContentType(2, "parent");
            StubContentType(3, "otherParent");

            type.ParentId = 2;

            Config.ContentType("contentType")
                .Parent("otherParent");

            var diff = ContentTypeDiff();

            Assert.That(diff.IsUnsafe, Is.True);
        }

        [Test]
        public void For_Parented_Then_No_Parent_Is_Unsafe()
        {
            var type = StubContentType(1, "contentType");
            StubContentType(2, "parent");
            type.ParentId = 2;

            Config.ContentType("contentType");

            var diff = ContentTypeDiff();

            Assert.That(diff.IsUnsafe, Is.True);
        }

        [Test]
        public void For_Orphan_Then_New_Parent_Is_Safe()
        {
            StubContentType(1, "contentType");
            StubContentType(2, "parent");

            Config.ContentType("contentType")
                .Parent("parent");

            var diff = ContentTypeDiff();

            Assert.That(
                diff,
                Has.Property("IsUnsafe").False &
                Has.Property("Comparisons").With.Exactly(1).With.Property("Key").EqualTo("Parent").And.Property("Result").EqualTo(ComparisonResult.New)
                );
        }

        [Test]
        public void With_New_Allowed_Children_Of_Existing_Types_Then_Is_Safe()
        {
            StubContentType(1, "contentType");
            StubContentType(2, "child");

            Config.ContentType("contentType")
                .Children("child");

            var diff = ContentTypeDiff();

            Assert.That(
                diff, 
                Has.Property("IsUnsafe").False &
                Has.Property("Comparisons").With.Exactly(1).With.Property("Key").EqualTo("Children").And.Property("Result").EqualTo(ComparisonResult.New));
        }

        [Test]
        public void With_New_Allowed_Children_Of_Invalid_Types_Then_Is_UnSafe()
        {
            StubContentType(1, "contentType");

            Config.ContentType("contentType")
                .Children("child");

            var diff = ContentTypeDiff();

            Assert.That(
                diff, 
                Has.Property("IsUnsafe").True &
                Has.Property("Comparisons").With.Exactly(1).With.Property("Key").EqualTo("Children").And.Property("Result").EqualTo(ComparisonResult.Invalid));
        }


        private DocumentTypeDiffgram ContentTypeDiff()
        {
            var fullDiff = Config.Compare();
            var diff = fullDiff.DocumentTypes["contentType"];
            return diff;
        }

        private void When_Umbraco_Has_Document_Type(IContentType contentType, Constraint constraint)
        {
            var alias = "contentType";
            StubContentType(1, alias, contentType);
            Config.ContentType(alias);
            var diffgram = Config.Compare();
            Assert.That(diffgram.DocumentTypes[alias], constraint);
        }

        public void SampleUsage()
        {
            var config = new FluentContentTypeConfiguration(Support.ServiceContext);

            config.DataType("richtext")
                .PropertyEditor("Umbraco.TinyMCEv3")
                .DataType(DataTypeDatabaseType.Ntext);

            config.ContentType("contentType")
                .Tab("Content")
                    .Property("text")
                        .DisplayName("The text")
                        .Description("#TextDescription")
                        .DataType("richtext")
                    .Property("Number")
                        .DataType("number");

            var diffgram = config.Compare();

            if (diffgram.Safe)
                config.Ensure(diffgram);
        }
    }
}
