using DataManagement.Extensions;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DataManagement.IntegrationTests.MsSql
{
    [TestFixture]
    class ManagerTests
    {
        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MSSQL);
            TestTools.SetConfigurationForConstantConsolidation(false);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);
        }

        [Test]
        public void Select_DataFromCache_ReturnsTrue()
        {
            List<LogTestGuid> list = LogTestGuid.SelectAll();
            Result result = LogTestGuid.SelectResult(new Parameter(nameof(LogTestGuid.Id), list[0].Id));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Hash.Count, 0);
        }

        [Test]
        public void InsertBlog_NewObject_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => TestTools.GetBlogModel(true).Insert());
        }

        [Test]
        public void InsertPost_NewObject_ReturnsNoError()
        {
            List<Blog> blogs = Blog.SelectAll();
            List<Author> authors = Author.SelectAll();

            TestTools.GetPostModel(true).BlogId = blogs[0].Id;
            TestTools.GetPostModel(false).AuthorId = authors[0].Id;
            Assert.DoesNotThrow(() => TestTools.GetPostModel(false).Insert());
        }

        [Test]
        public void InsertComment_NewObject_ReturnsNoError()
        {
            List<Post> posts = Post.SelectAll();

            TestTools.GetCommentModel(true).PostId = posts[0].Id;
            Assert.DoesNotThrow(() => TestTools.GetCommentModel(false).Insert());
        }

        [Test]
        public void InsertAuthor_NewObject_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => TestTools.GetAuthorModel(true).Insert());
        }

        [Test]
        public void SelectBlog_DataFromDB_ReturnsNoError()
        {
            //var result = Blog.Select(new Parameter(nameof(Blog.Id), Guid.Parse("8B4870AC-2D19-4D1C-B6FB-6CE2C46C974A")))
            //                        .Include(typeof(Post)).Posts
            //                        .Include(posts => posts.Comments, new Comment());
            //Assert.IsTrue(true);
        }

    }
}
