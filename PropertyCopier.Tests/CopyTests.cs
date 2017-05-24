using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using PropertyCopier.Extensions;

namespace PropertyCopier.Tests
{
    [TestFixture]
    public class CopyTests
    {
        //TODO: tests with multiple mappings e.g. Foo.BarID and Foo.Bar.ID test only sets prop once
        //TODO: tests for existing objects        

        [Test]
        public void CopyNumber()
        {
            var dto = Copy.From(new EntityOne() { ID = 10 }).To<DtoOne>();
            Assert.AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndString()
        {
            var dto = Copy.From(new EntityOne() { ID = 10, Name = "Test" }).To<DtoOne>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyNumberAndStringAndChildProperties()
        {
            var dto =
                Copy.From(
                    new EnitiyTwo() { ID = 10, Name = "Test", Child = new ChildEntityOne() { ID = 100, Name = "Child" } })
                    .To<DtoTwo>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
            Assert.AreEqual(100, dto.ChildID);
            Assert.AreEqual("Child", dto.ChildName);
        }

        [Test]
        public void CopyFlattenedProperty()
        {
            var dto =
                Copy.From(
                    new EnitiyTwo() { ID = 10, Name = "Test", Child = new ChildEntityOne() { ID = 100, Name = "Child" } })
                    .To<DtoTwo>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
            Assert.AreEqual(100, dto.ChildID);
            Assert.AreEqual("Child", dto.ChildName);
        }

        [Test]
        public void CopyNumberAndStringAndChild()
        {
            var dto =
                Copy.From(
                    new EnitiyTwo() { ID = 10, Name = "Test", Child = new ChildEntityOne() { ID = 100, Name = "Child" } })
                    .To<DtoThree>();
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
            Assert.AreEqual(100, dto.Child.ID);
            Assert.AreEqual("Child", dto.Child.Name);
        }

        [Test]
        public void CopyNumberAndStringAndChildren()
        {
            var dto =
                Copy.From(
                    new EnitiyThree()
                    {
                        ID = 10,
                        Name = "Test",
                        Children = new List<ChildEntityOne>()
                        {
                            new ChildEntityOne() { ID = 100, Name = "Child" }
                        }
                    })
                    .To<DtoFour>();
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
            var dto = Copy.From(new EntityOne() { ID = 10 }).To(new DtoOne() { ID = 0 });
            Assert.AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndStringToExisting()
        {
            var dto = Copy.From(new EntityOne() { ID = 10, Name = "Test" }).To(new DtoOne { ID = 0, Name = "" });
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyNumberToExistingOtherFieldsUnchanged()
        {
            var dto = Copy.From(new EntityOne() { ID = 10 }).To(new DtoTwo() { ID = 0, ChildName = "Child"});
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Child", dto.ChildName);
        }

        [Test]
        public void CopyFlattenedPropertyToExisting()
        {
            var dto = new DtoTwo();
            dto = Copy.From(
                    new EnitiyTwo() { ID = 10, Name = "Test", Child = new ChildEntityOne() { ID = 100, Name = "Child" } })
                    .To(dto);
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
            Assert.AreEqual(100, dto.ChildID);
            Assert.AreEqual("Child", dto.ChildName);
        }

        [Test]
        public void CopyExpressions()
        {
            var query = Builder<EntityOne>.CreateListOfSize(5).Build().AsQueryable();
            var result = query.Select(Copy.Expression<EntityOne, DtoOne>()).ToList();
            Assert.AreEqual(5, result.OfType<DtoOne>().Count());
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }

        [Test]
        public void CopyQueryableWithExpression()
        {
            var query = Builder<EntityOne>.CreateListOfSize(5).Build().AsQueryable();
            var result = query.Select(Copy.Expression<EntityOne, DtoOne>()).ToList();
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }

        [Test]
        public void CopyQueryable()
        {
            var query = Builder<EntityOne>.CreateListOfSize(5).Build().AsQueryable();
            var result = query.Copy().To<DtoOne>().ToList();
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }

        [Test]
        public void CopyEnumeration()
        {
            var enumeration = Builder<EntityOne>.CreateListOfSize(5).Build();
            var result = enumeration.Copy().To<DtoOne>();
            Assert.AreEqual(5, result.Count());
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }
    }

    #region classes for tests

    public class EntityOne
    {
        public byte ID { get; set; }

        public string Name { get; set; }
    }

    public class DtoOne
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }

    public class EnitiyTwo
    {
        public byte ID { get; set; }

        public string Name { get; set; }

        public ChildEntityOne Child { get; set; }
    }

    public class ChildEntityOne
    {
        public byte ID { get; set; }

        public string Name { get; set; }
    }

    public class DtoTwo
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int ChildID { get; set; }

        public string ChildName { get; set; }
    }

    public class ChildDtoOne
    {
        public byte ID { get; set; }

        public string Name { get; set; }
    }

    public class DtoThree
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public ChildDtoOne Child { get; set; }        
    }

    public class EnitiyThree
    {
        public byte ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<ChildEntityOne> Children { get; set; }
    }

    public class DtoFour
    {
        public byte ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<ChildDtoOne> Children { get; set; }
    }

    #endregion
}
