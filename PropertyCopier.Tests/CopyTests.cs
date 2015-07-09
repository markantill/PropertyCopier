﻿using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;

namespace PropertyCopier.Tests
{
    [TestFixture]
    public class CopyTests
    {
        //TODO: tests for updates
        //TODO: tests with multiple mappings e.g. Foo.BarID and Foo.Bar.ID test only sets prop once
        //TODO: tests for existing objects        

        [Test]
        public void CopyNumber()
        {
            var dto = Copy.PropertiesFrom(new EnitiyOne() { ID = 10 }).ToNew<DtoOne>();
            Assert.AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndString()
        {
            var dto = Copy.PropertiesFrom(new EnitiyOne() { ID = 10, Name = "Test" }).ToNew<DtoOne>();
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
            var dto = Copy.PropertiesFrom(new EnitiyOne() { ID = 10 }).ToExisting(new DtoOne() { ID = 0 });
            Assert.AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndStringToExisting()
        {
            var dto = Copy.PropertiesFrom(new EnitiyOne() { ID = 10, Name = "Test" }).ToExisting(new DtoOne { ID = 0, Name = "" });
            Assert.AreEqual(10, dto.ID);
            Assert.AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyExpressions()
        {
            var query = Builder<EnitiyOne>.CreateListOfSize(5).Build().AsQueryable();
            var result = query.Select(Copy.Expression<EnitiyOne, DtoOne>());
            Assert.AreEqual(5, result.OfType<DtoOne>().Count());
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }

        [Test]
        public void CopyEnumeration()
        {
            var enumeration = Builder<EnitiyOne>.CreateListOfSize(5).Build();
            var result = Copy.EnumerationFrom(enumeration).ToNew<DtoOne>().ToList();
            Assert.AreEqual(5, result.OfType<DtoOne>().Count());
            Assert.AreEqual(1, result.ElementAt(0).ID);
            Assert.AreEqual("Name1", result.ElementAt(0).Name);
            Assert.AreEqual(5, result.ElementAt(4).ID);
            Assert.AreEqual("Name5", result.ElementAt(4).Name);
        }
    }

    #region classes for tests

    public class EnitiyOne
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
