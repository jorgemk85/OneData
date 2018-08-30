using DataManagement.DAO;
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
            LogTestGuid newObj = new LogTestGuid();
            List<LogTestGuid> list = newObj.SelectAll();
            Result result = newObj.SelectResult(new Parameter(nameof(LogTestGuid.Id), list[0].Id));

            Assert.IsTrue(result.IsFromCache);
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreNotEqual(result.Data.Rows.Count, 0);
        }

        [Test]
        public void InsertBlog_NewObject_ReturnsNoError()
        {
            Blog newObj = new Blog();
            Assert.DoesNotThrow(() => newObj.Insert(TestTools.GetBlogModel(true)));
        }

        [Test]
        public void InsertPost_NewObject_ReturnsNoError()
        {
            Blog newBlog = new Blog();
            Author newAuthor = new Author();
            Post newPost = new Post();
            List<Blog> blogs = newBlog.SelectAll();
            List<Author> authors = newAuthor.SelectAll();

            TestTools.GetPostModel(true).BlogId = blogs[0].Id;
            TestTools.GetPostModel(false).AuthorId = authors[0].Id;
            Assert.DoesNotThrow(() => newPost.Insert(TestTools.GetPostModel(false)));
        }

        [Test]
        public void InsertAuthor_NewObject_ReturnsNoError()
        {
            Author newAuthor = new Author();
            Assert.DoesNotThrow(() => newAuthor.Insert(TestTools.GetAuthorModel(true)));
        }

        [Test]
        public void SelectBlog_DataFromDB_ReturnsNoError()
        {
            Blog newBlog = new Blog();
            var result = newBlog.Select(new Parameter(nameof(Blog.Id), Guid.Parse("8B4870AC-2D19-4D1C-B6FB-6CE2C46C974A"))).Include<Post, Guid>();
            Assert.IsTrue(true);
        }

    }
}
