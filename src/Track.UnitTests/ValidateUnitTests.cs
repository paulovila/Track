using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Track.UnitTests
{
    [TestClass]
    public class ValidateUnitTests
    {
        [TestMethod]
        public void ShouldValidateRequired()
        {
            var sut = new E().ToTrack(q => q.IsRequired(w => w.P1));

            Assert.IsTrue(sut.HasErrors);

            sut.Modified.P1 = "A";
            Assert.IsFalse(sut.HasErrors);

            sut.Modified.P1 = null;
            Assert.IsTrue(sut.HasErrors);
        }

        [TestMethod]
        public void ShouldHaveItemsOnDetail()
        {
            var sut = new TestEntity().ToTrack(q =>
                q.HasItemsMessage(w => w.Items, "message1"));

            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);

            sut.Modified.Items = new List<string>();
            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);

            sut.Modified.Items = new List<string> { "item1" };
            Assert.AreEqual(0, sut.Validations.Count());
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void StringShouldBeRequired()
        {
            var sut = new TestEntity().ToTrack(t =>
            {
                t.IsRequired(w => w.Name);
                t.IsRequiredMessage(w => w.Name2, "message1");
            });

            Assert.AreEqual(2, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);

            sut.Modified.Name = "www";
            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);

            sut.Modified.Name2 = "www";
            Assert.AreEqual(0, sut.Validations.Count());
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void IntAndDecimalShouldBeNotNegative()
        {
            var sut = new TestEntity { IntId = -1, DecimalId = -1 }.ToTrack(t =>
            {
                t.IsNotNegative(w => w.IntId);
                t.IsNotNegative(w => w.DecimalId);
            }
            );

            Assert.AreEqual(2, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);
            sut.Modified.IntId = 1;
            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);
            sut.Modified.DecimalId = 0;
            Assert.AreEqual(0, sut.Validations.Count());
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void IntShouldBePositive()
        {
            var sut = new TestEntity().ToTrack(
                t => t.IsPositiveMessage(w => w.IntId, "Message1"));

            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);
            sut.Modified.IntId = 1;
            Assert.AreEqual(0, sut.Validations.Count());
            Assert.IsFalse(sut.HasErrors);
            sut.Modified.IntId = -1;
            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);
        }

        [TestMethod]
        public void TextShouldBeUpperCase()
        {
            var sut = new TestEntity().ToTrack(t => t.IsUpperCase(w => w.Name, "Message1"));

            Assert.AreEqual(0, sut.Validations.Count());
            Assert.IsFalse(sut.HasErrors);
            sut.Modified.Name = "ss";
            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);
            sut.Modified.Name = "SS";
            Assert.AreEqual(0, sut.Validations.Count());
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void ShouldValidateCustom()
        {
            var sut = new TestEntity().ToTrack(t=> 
                t.UpdateError("customRuleName", t.Modified.IntId % 2 == 0, "Id={0}, should be even", t.Modified.IntId));

            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);
            sut.Modified.IntId++;
            Assert.AreEqual(0, sut.Validations.Count());
            Assert.IsFalse(sut.HasErrors);
            sut.Modified.IntId++;
            Assert.AreEqual(1, sut.Validations.Count());
            Assert.IsTrue(sut.HasErrors);
        }

        [TestMethod]
        public void ShouldGetErrorsByProperty()
        {
            var sut = new TestEntity().ToTrack(t=> {
                t.HasItemsMessage(w => w.Items, "message1");
                t.IsRequired(w => w.Name);
                t.UpdateError("Name", t.Modified.Name == null || !t.Modified.Name.StartsWith("E"), "{0} should start with E", t.Modified.Name);
            });

            Assert.IsTrue(sut.HasErrors);
            Assert.AreEqual(3, sut.Validations.Count());
            Assert.AreEqual(1, sut.GetValidations("Items").Count());
            Assert.AreEqual(2, sut.GetValidations("Name").Count());

            sut.Modified.Items = new List<string> { "item" };
            Assert.IsTrue(sut.HasErrors);
            Assert.AreEqual(2, sut.Validations.Count());
            Assert.AreEqual(0, sut.GetValidations("Items").Count());
            Assert.AreEqual(2, sut.GetValidations("Name").Count());

            sut.Modified.Name = "aaa";
            Assert.IsTrue(sut.HasErrors);
            Assert.AreEqual(1, sut.Validations.Count());
            Assert.AreEqual(0, sut.GetValidations("Items").Count());
            Assert.AreEqual(1, sut.GetValidations("Name").Count());

            sut.Modified.Name = "Es";
            Assert.IsFalse(sut.HasErrors);
            Assert.AreEqual(0, sut.Validations.Count());
            Assert.AreEqual(0, sut.GetValidations("Items").Count());
            Assert.AreEqual(0, sut.GetValidations("Name").Count());
        }
    }
}