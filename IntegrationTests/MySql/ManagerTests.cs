using DataManagement.Extensions;
using DataManagement.Models;
using DataManagement.Models.Test;
using DataManagement.Tools.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DataManagement.IntegrationTests.MySql
{
    [TestFixture]
    class ManagerTests
    {
        [OneTimeSetUp]
        public void PerformSetupForTesting_DoesNotThrow()
        {
            TestTools.SetDefaultConfiguration(Enums.ConnectionTypes.MySQL);
            TestTools.SetConfigurationForConstantConsolidation(false);
            TestTools.SetConfigurationForAutoCreate(true);
            TestTools.SetConfigurationForAutoAlter(true);
        }

        [Test]
        public void SelectGuid_DataFromCache_ReturnsTrue()
        {
            TestTools.GetBlogModel(true).Insert();
            List<Blog> list = Blog.SelectAll();
            Result result = Blog.SelectResult(new Parameter(nameof(Blog.Id), list[0].Id));
            TestTools.GetBlogModel(false).Delete();

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Hash.Count, 0);
        }

        [Test]
        public void SelectInt_DataFromCache_ReturnsTrue()
        {
            TestTools.GetLogTestIntModel(true).Insert();
            List<LogTestInt> list = LogTestInt.SelectAll();
            Result result = LogTestInt.SelectResult(new Parameter(nameof(LogTestInt.Id), list[0].Id));
            TestTools.GetLogTestIntModel(false).Delete();

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
            //var result = Blog.Select(new Parameter(nameof(Blog.Id), Guid.Parse("36e693e6-e936-4acc-bd6a-2dfb80449590")))
            //                         .Include(typeof(Post)).Posts
            //                         .Include(posts => posts.Comments, new Comment());
            Assert.IsTrue(true);
        }
    }
}
