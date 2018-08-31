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
            Blog.Insert(TestTools.GetBlogModel(true));
            List<Blog> list = Blog.SelectAll();
            Result<Blog, Guid> result = Blog.SelectResult(new Parameter(nameof(Blog.Id), list[0].Id));
            Blog.Delete(TestTools.GetBlogModel(false));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Collection.Count, 0);
        }

        [Test]
        public void SelectInt_DataFromCache_ReturnsTrue()
        {
            LogTestInt.Insert(TestTools.GetLogTestIntModel(true));
            List<LogTestInt> list = LogTestInt.SelectAll();
            Result<LogTestInt, int> result = LogTestInt.SelectResult(new Parameter(nameof(LogTestInt.Id), list[0].Id));
            LogTestInt.Delete(TestTools.GetLogTestIntModel(false));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Collection.Count, 0);
        }

        [Test]
        public void InsertBlog_NewObject_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => Blog.Insert(TestTools.GetBlogModel(true)));
        }

        [Test]
        public void InsertPost_NewObject_ReturnsNoError()
        {
            List<Blog> blogs = Blog.SelectAll();
            List<Author> authors = Author.SelectAll();

            TestTools.GetPostModel(true).BlogId = blogs[0].Id;
            TestTools.GetPostModel(false).AuthorId = authors[0].Id;
            Assert.DoesNotThrow(() => Post.Insert(TestTools.GetPostModel(false)));
        }

        [Test]
        public void InsertComment_NewObject_ReturnsNoError()
        {
            List<Post> posts = Post.SelectAll();

            TestTools.GetCommentModel(true).PostId = posts[0].Id;
            Assert.DoesNotThrow(() => Comment.Insert(TestTools.GetCommentModel(false)));
        }

        [Test]
        public void InsertAuthor_NewObject_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => Author.Insert(TestTools.GetAuthorModel(true)));
        }

        [Test]
        public void SelectBlog_DataFromDB_ReturnsNoError()
        {
            var result = Blog.Select(new Parameter(nameof(Blog.Id), Guid.Parse("36e693e6-e936-4acc-bd6a-2dfb80449590")))
                                     .Include(typeof(Post)).Posts
                                     .Include(typeof(Comment));
            Assert.IsTrue(true);
        }
    }
}
