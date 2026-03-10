using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.Data.Enums
{
    [TestClass]
    public class RagRatingExtensionsTests
    {
        [TestMethod]
        [DataRow("R", RagRating.Red)]
        [DataRow("A", RagRating.Amber)]
        [DataRow("G", RagRating.Green)]
        [DataRow("R-M", RagRating.RedMedical)]
        [DataRow("A-M", RagRating.AmberMedical)]
        [DataRow("G-M", RagRating.GreenMedical)]
        public void ParseRag_ReturnsExpectedEnum(string input, RagRating expected)
        {
            var result = RagRatingExtensions.ParseRag(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow(RagRating.Red, "R")]
        [DataRow(RagRating.Amber, "A")]
        [DataRow(RagRating.Green, "G")]
        [DataRow(RagRating.RedMedical, "R-M")]
        [DataRow(RagRating.AmberMedical, "A-M")]
        [DataRow(RagRating.GreenMedical, "G-M")]
        public void ToDbValue_ReturnsExpectedString(RagRating rag, string expected)
        {
            var result = rag.ToDbValue();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("X")]
        [DataRow("RED")]
        [DataRow("RM")]
        public void ParseRag_InvalidValue_ThrowsArgumentException(string input)
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                RagRatingExtensions.ParseRag(input));
        }

        [TestMethod]
        public void ToDbValue_InvalidEnum_ThrowsArgumentException()
        {
            var invalid = (RagRating)999;

            Assert.ThrowsExactly<ArgumentException>(() =>
                invalid.ToDbValue());
        }
    }
}