using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FieldMapper.Tests
{
    [TestClass]
    public class FieldMapper_Tests
    {
        [TestMethod]
        public void FieldMapper_NestedKeys_OnSameLine()
        {
            // Arrange
            var emailKey = "Email Name";
            var emailValue = "example@example.com";
            var phoneKey = "Phone";
            var phoneValue = "5555551234";
            var fullNameKey = "Name";
            var fullNameValue = "John Doe";
            var zipKey = "Zip Name";
            var zipValue = "55555";

            var content = string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", emailKey, emailValue, phoneKey, phoneValue, fullNameKey, fullNameValue, zipKey, zipValue);
            var mappings = new List<string>();
            mappings.Add(emailKey);
            mappings.Add(fullNameKey);
            mappings.Add(phoneKey);
            mappings.Add(zipKey);

            // Act
            var parser = new FieldMapper(content, mappings);
            var result = parser.Get();

            // Assert
            Assert.AreEqual(emailValue, result[emailKey]);
            Assert.AreEqual(fullNameValue, result[fullNameKey]);
            Assert.AreEqual(phoneValue, result[phoneKey]);
            Assert.AreEqual(zipValue, result[zipKey]);
        }
    }
}
