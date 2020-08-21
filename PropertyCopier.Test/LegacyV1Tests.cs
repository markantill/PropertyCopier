using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using PropertyCopier.Tests;

namespace PropertyCopier.Tests
{
    [TestFixture]
    // This test tests the old API
#pragma warning disable CS0618 // Type or member is obsolete
    public class LegacyV1Tests
    {   
        [Test]
        public void CopyNumber()
        {
            var dto = Copy.PropertiesFrom(new EntityOne() { ID = 10 }).ToNew<DtoOne>();
            Assert.AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndString()
        {
            var dto = Copy.PropertiesFrom(new EntityOne() { ID = 10, Name = "Test" }).ToNew<DtoOne>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyNumberAndStringAndChildProperties()
        {
            var dto =
                Copy.PropertiesFrom(
                        new EnitiyTwo() { ID = 10, Name = "Test", Child = new ChildEntityOne() { ID = 100, Name = "Child" } })
                    .ToNew<DtoTwo>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
            Assert.AreEqual(100, dto.ChildID);
            Assert.AreEqual("Child", dto.ChildName);
        }

        [Test]
        public void CopyNumberAndStringAndChild()
        {
            var dto =
                Copy.PropertiesFrom(
                        new EnitiyTwo() { ID = 10, Name = "Test", Child = new ChildEntityOne() { ID = 100, Name = "Child" } })
                    .ToNew<DtoThree>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
            Assert.AreEqual(100, dto.Child.ID);
            Assert.AreEqual("Child", dto.Child.Name);
        }

        [Test]
        public void CopyNumberAndStringAndChildren()
        {
            var dto =
                Copy.PropertiesFrom(
                        new EnitiyThree()
                        {
                            ID = 10,
                            Name = "Test",
                            Children = new List<ChildEntityOne>()
                            {
                                new ChildEntityOne() { ID = 100, Name = "Child" }
                            }
                        })
                    .ToNew<DtoFour>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
            Assert.IsNotNull(dto.Children);
            Assert.AreEqual(1, dto.Children.Count());
            Assert.AreEqual(100, dto.Children.Single().ID);
            Assert.AreEqual("Child", dto.Children.Single().Name);
        }

        [Test]
        public void CopyNumberToExisting()
        {
            var dto = Copy.PropertiesFrom(new EntityOne() { ID = 10 }).ToExisting(new DtoOne() { ID = 0 });
            Assert.AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndStringToExisting()
        {
            var dto = Copy.PropertiesFrom(new EntityOne() { ID = 10, Name = "Test" }).ToExisting(new DtoOne { ID = 0, Name = "" });
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyNumberToExistingOtherFieldsUnchanged()
        {
            var dto = Copy.PropertiesFrom(new EntityOne() { ID = 10 }).ToExisting(new DtoTwo() { ID = 0, ChildName = "Child" });
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Child", dto.ChildName);
        }

        [Test]
        public void CopyExpressions()
        {
            var query = Builder<EntityOne>.CreateListOfSize(5).Build().AsQueryable();
            var result = query.Select(Copy.Expression<EntityOne, DtoOne>());
            Assert.AreEqual(5, result.OfType<DtoOne>().Count());
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }

        [Test]
        public void CopyQueryable()
        {
            var query = Builder<EntityOne>.CreateListOfSize(5).Build().AsQueryable();
            var result = query.CopyEachTo<EntityOne, DtoOne>();
            Assert.AreEqual(5, result.OfType<DtoOne>().Count());
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }

        [Test]
        public void CopyEnumeration()
        {
            var enumeration = Builder<EntityOne>.CreateListOfSize(5).Build();
            var result = Copy.EnumerationFrom(enumeration).ToNew<DtoOne>().ToList();
            Assert.AreEqual(5, result.OfType<DtoOne>().Count());
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
