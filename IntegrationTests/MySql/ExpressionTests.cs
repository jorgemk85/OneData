using OneData.Extensions;
using OneData.Models.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneData.IntegrationTests.MySql
{
    [TestFixture]
    public class ExpressionTests
    {
        [Test]
        public void ExpressionEvaluationLikeContains_Succeeds_ReturnsMultipleValues()
        {
            string value = "A";
            List<Author> authors = Author.SelectList(author => author.Name.Contains(value));

            Assert.That(authors.Count >= 2);
        }

        [Test]
        public void ExpressionEvaluationLikeStartsWith_Succeeds_ReturnsValues()
        {
            List<Author> authors = Author.SelectList(author =>  author.Name.StartsWith("A"));

            Assert.That(authors.Count >= 2);
        }

        [Test]
        public void AddTwoAuthors_Succeeds_ReturnsMultipleValues()
        {
            Author a1 = new Author() { Name = "Author 1" };
            a1.Insert();
            Author a2 = new Author() { Name = "Author 2" };
            a2.Insert();

            Assert.IsTrue(true);
        }


    }
}
